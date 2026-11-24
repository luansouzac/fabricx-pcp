---
tema: autenticação com JWT e cache de consultas na API
fontes:
  - DIAGRAMA_ARQUITETURA.pdf (enunciado do trabalho prático, seções 4.4, 4.5, 6, 8 e 11)
  - https://learn.microsoft.com/pt-br/aspnet/core/security/authentication/ (autenticação no ASP.NET Core)
  - https://learn.microsoft.com/pt-br/aspnet/core/performance/caching/memory (cache em memória)
aplicado-em: AuthController, UsuariosController, GeradorDeTokenJwt, SenhaHasherPbkdf2, CacheDeConsultasMemoria, IndicadorService, CapacidadeService
relacionado: [arquitetura-limpa.md, injecao-de-dependencia.md, aspnet-core-web-api.md]
---

# JWT e cache

O enunciado trata os dois como estudo autônomo e pergunta pelas decisões na apresentação. Este arquivo registra o porquê de cada escolha.

## O que é um JWT

Três partes em Base64Url separadas por ponto: cabeçalho (algoritmo), corpo (claims) e assinatura. A assinatura é o HMAC-SHA256 de `cabeçalho.corpo` com a chave secreta do servidor. O token é **assinado, não criptografado**: qualquer um lê as claims, ninguém consegue alterá-las sem a chave. Por isso a API não coloca senha nem dado sensível no token.

Claims usadas (`GeradorDeTokenJwt`):

| Claim | Conteúdo | Para quê |
|---|---|---|
| `sub` | id do usuário | identifica quem chama |
| `email`, `name` | e-mail e nome | exibição |
| `role` | `Administrador` ou `Operador` | autorização por perfil |
| `jti` | GUID único por token | identifica o token (base para revogação futura) |
| `iss`, `aud` | `FabricX.Api`, `FabricX.Clientes` | a API só aceita token feito para ela |
| `exp`, `nbf`, `iat` | expiração, início e emissão | ciclo de vida |

## Ciclo de vida

1. `POST /api/auth/login` valida e-mail e senha e devolve o token com a expiração (`expiraEmUtc`).
2. O cliente guarda o token e o envia em `Authorization: Bearer <token>`.
3. A cada requisição o middleware `UseAuthentication` valida assinatura, emissor, audiência e validade (tolerância de 30 s). Sem token válido, 401.
4. `UseAuthorization` checa o perfil quando o endpoint exige. Perfil insuficiente, 403.
5. O token expira em 60 minutos (`Jwt:ExpiraEmMinutos`). Não há refresh token: ao expirar, novo login.

A validação é **sem estado**: a API não consulta o banco a cada requisição. Consequência assumida: inativar um usuário impede novos logins, mas um token já emitido vale até expirar.

## Decisões

| Decisão | Escolha | Motivo |
|---|---|---|
| Hash de senha | PBKDF2-HMAC-SHA256, 210 mil iterações, salt de 16 bytes, comparação em tempo constante | Vem na biblioteca padrão do .NET, sem pacote extra. Salt aleatório evita hashes iguais para senhas iguais. As iterações seguem a ordem de grandeza recomendada pela OWASP para SHA-256 |
| Mensagem de erro do login | A mesma para e-mail inexistente, senha errada e usuário inativo | Não revela quais e-mails existem |
| Status do login inválido | 401 (`NaoAutorizadoException`), não 400 | 400 é erro de regra de negócio; credencial inválida é falha de autenticação |
| Onde fica cada peça | `ISenhaHasher` e `IGeradorDeToken` na Application, implementações na Infrastructure | Services não conhecem biblioteca de criptografia nem de JWT, e os testes usam as implementações reais sem banco |
| Política padrão | Todo endpoint exige usuário autenticado (`FallbackPolicy`); só o login tem `[AllowAnonymous]` | Esquecer um `[Authorize]` deixa a rota fechada, não aberta |
| Perfis | `Administrador` gerencia usuários e inativa produto e recurso; `Operador` faz o restante | O enunciado pede autenticação **e autorização** |
| Sem cadastro público | Só um administrador cria usuários; o sistema recusa inativar ou rebaixar o último administrador ativo | Ninguém entra sem ser autorizado, e o sistema não fica sem administrador |
| Sessão revalidada | `OnTokenValidated` consulta o usuário a cada requisição e rejeita inativo ou inexistente | Inativar derruba o acesso na hora, sem esperar o token expirar |
| Login uniforme | E-mail inexistente também executa o PBKDF2, contra um hash fictício | O tempo de resposta não revela quais e-mails existem |
| Limite no login | Janela fixa de 1 minuto por IP (`RateLimit:LoginPorMinuto`, padrão 5), 429 no formato `WebResponse<T>` | Freia tentativa de adivinhar senha |
| Chave | `Jwt__Chave` por variável de ambiente ou `appsettings.Development.json` (só desenvolvimento). A API se recusa a subir sem chave de pelo menos 32 caracteres | O enunciado exige configuração sensível fora do código-fonte |
| `MapInboundClaims = false` | Claims chegam com o nome do token (`role`, `sub`) | Sem isso o framework renomeia `role` e a checagem de perfil falha |

## Cache

Cache em memória (`IMemoryCache`) nas consultas que somam muitas linhas e mudam pouco: `GET /api/indicadores`, `/api/indicadores/resumo` e `/api/capacidade`.

| Pergunta do enunciado | Resposta |
|---|---|
| Quando busca na fonte? | Na primeira chamada de cada chave, depois de expirar ou depois de invalidar. Log `CACHE MISS <chave>` |
| Quando vem do cache? | Chamada com a mesma chave dentro da expiração. Log `CACHE HIT <chave>` |
| Quando é invalidado? | Após qualquer `POST`, `PUT` ou `DELETE` bem-sucedido nos recursos da API. Log `CACHE INVALIDADO` |
| Expiração | 1 minuto (`Cache:Expiracao`), como rede de segurança |

Chave por consulta: os filtros fazem parte da chave (`indicadores:de:ate:produtoId:recursoId`, `capacidade:AAAA-MM`). Duas consultas com filtros diferentes não se misturam.

Invalidação: um filtro de ação (`InvalidarCacheAposEscritaFilter`) chama `InvalidarTudo()` quando a requisição não é de leitura e termina com sucesso. Escolha deliberada: os indicadores dependem de ordens, programação, apontamentos, estoque e demanda, então quase qualquer escrita pode mudá-los. Invalidar tudo é simples e nunca devolve dado velho; um cache com invalidação seletiva por entidade ganharia pouco neste volume e exigiria mapear cada dependência. Login e gestão de usuários não invalidam, porque não afetam as consultas.

Implementação: `CacheDeConsultasMemoria` guarda cada valor com expiração absoluta e um token de cancelamento compartilhado. `InvalidarTudo()` cancela o token e cria outro, expirando todas as entradas de uma vez. Se uma escrita acontece enquanto uma consulta está sendo calculada, o valor calculado já nasce expirado e não fica velho no cache.

Limites assumidos: o cache vive no processo (uma instância da API). Com várias instâncias seria preciso um cache distribuído (por exemplo Redis), mas o SQLite do projeto já impõe uma instância. Cache aplicado só a consultas, nunca a escritas.

## Onde olhar no código

| Assunto | Arquivo |
|---|---|
| Geração do token | `src/FabricX.Infrastructure/Seguranca/GeradorDeTokenJwt.cs` |
| Hash de senha | `src/FabricX.Infrastructure/Seguranca/SenhaHasherPbkdf2.cs` |
| Login | `src/FabricX.Application/Services/AuthService.cs` |
| Validação do token e política padrão | `src/FabricX.Api/Program.cs` |
| 401 e 403 no envelope | `src/FabricX.Api/Common/RespostasDeAutenticacao.cs` |
| Bearer no Swagger | `src/FabricX.Api/Common/BearerOpenApiTransformer.cs` |
| Cache | `src/FabricX.Infrastructure/Cache/CacheDeConsultasMemoria.cs` |
| Invalidação | `src/FabricX.Api/Filters/InvalidarCacheAposEscritaFilter.cs` |

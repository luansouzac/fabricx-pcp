# FabricX

API de PCP (Planejamento e Controle da Produção) em C# / .NET 10, com Arquitetura Limpa e banco SQLite. Documentação de domínio em `docs/`.

**Integrantes:** Francisco, Luan e Igor. **Disciplina:** Engenharia de Software (IF Sudeste MG, Prof. Paulo Dutra).

## Sistema e tecnologias

O FabricX controla produtos e estoque, estrutura de produto, ordens de produção, programação em recursos, apontamentos, indicadores de desempenho e planejamento de capacidade. Todas as funcionalidades saem por endpoints HTTP, protegidos por JWT. O aplicativo cliente (front-end Vue.js) é o trabalho de Web III e consome esta API em outro projeto. Até lá, o Swagger UI é a interface de uso.

| Tema | Tecnologia |
|---|---|
| Linguagem e plataforma | C# 14 / .NET 10, ASP.NET Core Web API (REST sobre HTTP/HTTPS) |
| Arquitetura | Clean Architecture em quatro projetos (Domain, Application, Infrastructure, Api) |
| Persistência | SQLite com EF Core, esquema criado por `database/script.sql` |
| Autenticação | JWT (Bearer) com senhas em PBKDF2 |
| Cache | `IMemoryCache` nas consultas de indicadores e capacidade |
| Padronização das respostas | `WebResponse<T>` (o ResultWrapper do enunciado) |
| Documentação da API | OpenAPI embutido + Swagger UI |
| Testes e execução | xUnit (unidade e integração), Docker |

## Requisitos

- [Docker](https://www.docker.com/products/docker-desktop) (recomendado, garante a mesma versão para todo o grupo), ou
- [.NET SDK 10](https://dotnet.microsoft.com/download)

## Rodar com Docker

```bash
docker compose up --build
```

A API sobe em `http://localhost:5168` já com o seed carregado e a chave de desenvolvimento do JWT (veja [Autenticação](#autenticação-jwt)). O banco fica no volume `fabricx-data` e persiste entre execuções.

```bash
docker compose down        # para o container, mantém os dados
docker compose down -v     # para e apaga o volume (o próximo up recarrega o seed)
```

Se o build falhar com `unexpected EOF` no `dotnet restore`, é instabilidade de rede ao baixar pacotes NuGet. Rode o comando de novo.

Os testes (`dotnet test`) e o desenvolvimento no dia a dia continuam usando o SDK local.

## Rodar a API sem Docker

Na raiz do projeto:

```bash
dotnet run --project src/FabricX.Api --launch-profile http
```

A API sobe em `http://localhost:5168`. O OpenAPI fica em `http://localhost:5168/openapi/v1.json`.

Use o perfil `http`. O perfil `https` exige o certificado de desenvolvimento (`dotnet dev-certs https --trust`).

## Banco de dados e seeder

O banco é o arquivo `src/FabricX.Api/fabricx.db`, criado na primeira execução (está no `.gitignore`). Na primeira execução a API cria as tabelas com `database/script.sql`. Quando o ambiente é Development, ela também carrega os dados de exemplo de `database/seed.sql`.

O seed só roda quando o banco é criado do zero. Ele traz 2 usuários de desenvolvimento (veja [Autenticação](#autenticação-jwt)), 11 produtos (três deles abaixo do estoque mínimo), estruturas de produto, 6 recursos, roteiros e movimentações iniciais. O exemplo da garrafa tem três níveis: tampinha e garrafa formam a garrafa d'água (`EP-003`, montadora, ciclo de 2 min para 10 unidades), e 6 garrafas d'água formam o kit (`PA-003`, linha de embalagem, 2 min por unidade).

O script e o seed só rodam quando o banco é criado do zero. Quem já tem o `fabricx.db`, ou o volume `fabricx-data` do Docker, precisa recriá-lo para receber a coluna nova do roteiro e o exemplo da garrafa.

Para resetar o banco e recarregar o seed:

```bash
rm src/FabricX.Api/fabricx.db
dotnet run --project src/FabricX.Api --launch-profile http
```

Pare a API antes de apagar o arquivo.

## Testar os endpoints

Suba a API com o perfil `http` (`dotnet run --project src/FabricX.Api --launch-profile http`) e abra http://localhost:5168/swagger. O Swagger UI lista todos os endpoints e executa as requisições. O JSON da especificação fica em `/openapi/v1.json`.

O Swagger UI só existe em Development.

Quase todos os endpoints exigem token. No Swagger UI: chame `POST /api/auth/login` com um dos usuários abaixo, copie o valor de `data.token`, clique em **Authorize** no topo da página, cole o token e confirme. Os cadeados das rotas passam a enviar o cabeçalho `Authorization: Bearer <token>`.

Endpoints atuais. Todos exigem token, exceto `POST /api/auth/registro` e `POST /api/auth/login`:

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/registro` | Cadastra usuário com perfil Operador (público) |
| POST | `/api/auth/login` | Valida e-mail e senha e devolve o token JWT (público) |
| GET | `/api/usuarios` | Lista usuários (Administrador) |
| GET | `/api/usuarios/{id}` | Busca usuário por id (Administrador) |
| POST | `/api/usuarios` | Cria usuário com o perfil informado (Administrador) |
| PUT | `/api/usuarios/{id}` | Altera nome e perfil (Administrador) |
| DELETE | `/api/usuarios/{id}` | Inativa usuário, que deixa de conseguir logar (Administrador) |
| GET | `/api/produtos?tipo=` | Lista produtos, com filtro opcional por tipo (`MateriaPrima`, `EmProcesso`, `Acabado`) |
| GET | `/api/produtos/estoque-baixo` | Produtos abaixo do estoque mínimo |
| GET | `/api/produtos/{id}` | Busca por id |
| POST | `/api/produtos` | Cria produto |
| PUT | `/api/produtos/{id}` | Atualiza produto |
| POST | `/api/produtos/{id}/movimentacoes` | Entrada ou saída de estoque, com motivo opcional |
| GET | `/api/produtos/{id}/movimentacoes` | Histórico de movimentações, da mais recente para a mais antiga |
| GET | `/api/produtos/{id}/estrutura` | Componentes do produto, com código, nome e quantidade |
| POST | `/api/produtos/{id}/estrutura` | Adiciona componente (recusa par repetido e ciclo) |
| PUT | `/api/produtos/{id}/estrutura/{componenteId}` | Altera a quantidade do componente |
| DELETE | `/api/produtos/{id}/estrutura/{componenteId}` | Remove o componente |
| GET | `/api/produtos/{id}/roteiro` | Etapas do roteiro, por sequência |
| POST | `/api/produtos/{id}/roteiro` | Adiciona etapa (recurso ativo, sequência única, ciclo e unidades por ciclo) |
| PUT | `/api/produtos/{id}/roteiro/{etapaId}` | Altera a etapa |
| DELETE | `/api/produtos/{id}/roteiro/{etapaId}` | Remove a etapa |
| POST | `/api/ordens-producao/{id}/programacao` | Programa uma etapa da ordem (início informado, fim calculado, sem sobreposição e dentro da capacidade) |
| GET | `/api/ordens-producao/{id}/programacao` | Etapas programadas da ordem |
| GET | `/api/programacao?recursoId=&de=&ate=` | Cronograma por recurso e período, ordenado por início |
| PUT | `/api/programacao/{id}` | Reprograma (só enquanto agendada) |
| DELETE | `/api/programacao/{id}` | Remove (só enquanto agendada) |
| POST | `/api/programacao/{id}/apontamentos` | Aponta produção real (produzida, defeituosa, tempo gasto) |
| GET | `/api/programacao/{id}/apontamentos` | Apontamentos da programação |
| GET | `/api/ordens-producao/{id}/apontamentos` | Apontamentos da ordem |
| GET | `/api/indicadores?de=&ate=&produtoId=&recursoId=` | Qualidade, eficiência, produtividade e cumprimento de prazo (em cache) |
| GET | `/api/indicadores/resumo` | Resumo para o dashboard: ordens por status, estoque baixo e indicadores (em cache) |
| GET | `/api/demanda-prevista?periodo=AAAA-MM` | Demanda prevista por produto e mês |
| POST | `/api/demanda-prevista` | Cria demanda prevista (um registro por produto e período) |
| PUT | `/api/demanda-prevista/{id}` | Altera período e quantidade |
| DELETE | `/api/demanda-prevista/{id}` | Remove a demanda |
| GET | `/api/capacidade?periodo=AAAA-MM` | Capacidade x demanda por recurso, com gargalo e ociosidade (em cache) |
| GET | `/api/recursos` | Lista recursos |
| GET | `/api/recursos/{id}` | Busca recurso por id |
| POST | `/api/recursos` | Cria recurso (nome único, capacidade em h/dia maior que zero) |
| PUT | `/api/recursos/{id}` | Atualiza recurso |
| DELETE | `/api/recursos/{id}` | Inativa recurso, não apaga (Administrador) |
| GET | `/api/ordens-producao?status=` | Lista ordens, com filtro opcional por status |
| GET | `/api/ordens-producao/{id}` | Busca ordem por id |
| POST | `/api/ordens-producao` | Cria ordem (número gerado, valida saldo dos componentes) |
| PUT | `/api/ordens-producao/{id}` | Altera quantidade, prazo e prioridade de ordem planejada |
| POST | `/api/ordens-producao/{id}/avancar` | Avança o status (ao iniciar baixa os componentes, ao concluir dá entrada no acabado) |
| POST | `/api/ordens-producao/{id}/cancelar` | Cancela ordem planejada ou programada |
| GET | `/api/integracoes/erp/pedidos` | Pedidos pendentes do ERP (implementação simulada) |
| POST | `/api/integracoes/sgq/resultados` | Envia resultado de qualidade ao SGQ (implementação simulada) |
| DELETE | `/api/produtos/{id}` | Inativa produto (Administrador) |

Todas as respostas seguem o formato `WebResponse<T>`: `data`, `message`, `isSuccess`, `statusCode`.

Exemplo de sucesso (`GET /api/recursos/1`):

```json
{ "data": { "id": 1, "nome": "Torno CNC", "tipo": "Maquina", "capacidadeHorasDia": 8, "ativo": true },
  "message": null, "isSuccess": true, "statusCode": 200 }
```

Exemplos de erro, no mesmo formato:

```json
{ "data": null, "message": "Autenticação necessária: envie um token JWT válido no cabeçalho Authorization.", "isSuccess": false, "statusCode": 401 }
{ "data": null, "message": "O perfil do usuário não permite esta operação.", "isSuccess": false, "statusCode": 403 }
{ "data": null, "message": "Recurso 99 não encontrado.", "isSuccess": false, "statusCode": 404 }
```

| Status | Quando |
|---|---|
| 400 | Dado inválido ou regra de negócio violada |
| 401 | Sem token, token inválido ou expirado, ou credencial errada no login |
| 403 | Token válido, mas o perfil não permite a operação |
| 404 | Recurso inexistente |

## Autenticação (JWT)

Fluxo: cadastro (`POST /api/auth/registro`) ou usuário do seed, login (`POST /api/auth/login`), token no cabeçalho `Authorization: Bearer <token>` nas demais rotas. O token é assinado com HMAC-SHA256, expira em 60 minutos e carrega `sub`, `email`, `name`, `role` e `jti`. Não há refresh token: ao expirar, faça login de novo. A explicação completa, com as decisões, está em [docs/conceitos/jwt-e-cache.md](docs/conceitos/jwt-e-cache.md).

Usuários criados pelo seed (só em Development, para teste):

| E-mail | Senha | Perfil |
|---|---|---|
| `admin@fabricx.com` | `Admin@123` | Administrador |
| `operador@fabricx.com` | `Operador@123` | Operador |

Perfis: o **Operador** usa todos os endpoints de produção, mas não gerencia usuários nem inativa produto e recurso. O **Administrador** faz tudo. O cadastro público sempre cria Operador.

Login:

```bash
curl -s -X POST http://localhost:5168/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fabricx.com","senha":"Admin@123"}'
```

```json
{ "data": { "token": "eyJhbGciOi...", "tipo": "Bearer", "expiraEmUtc": "2026-11-24T15:00:00Z",
            "usuario": { "id": 1, "nome": "Administrador", "email": "admin@fabricx.com", "perfil": "Administrador", "ativo": true } },
  "message": "Autenticado.", "isSuccess": true, "statusCode": 200 }
```

Usando o token:

```bash
curl -s http://localhost:5168/api/produtos -H "Authorization: Bearer $TOKEN"
```

Senhas ficam só como hash PBKDF2 (SHA-256, salt aleatório, 210 mil iterações). A senha em texto nunca é gravada nem devolvida.

### Configuração do JWT

A chave de assinatura não fica no código-fonte. Ela vem da configuração:

| Chave | Variável de ambiente | Observação |
|---|---|---|
| `Jwt:Chave` | `Jwt__Chave` | Mínimo de 32 caracteres. Em Development vem de `appsettings.Development.json` (chave só de desenvolvimento). A API não sobe sem chave válida |
| `Jwt:Emissor` | `Jwt__Emissor` | Padrão `FabricX.Api` |
| `Jwt:Audiencia` | `Jwt__Audiencia` | Padrão `FabricX.Clientes` |
| `Jwt:ExpiraEmMinutos` | `Jwt__ExpiraEmMinutos` | Padrão 60 |

Fora de Development (`ASPNETCORE_ENVIRONMENT=Production`), defina `Jwt__Chave` antes de subir. Com Docker, acrescente `Jwt__Chave` e `ASPNETCORE_ENVIRONMENT` em `environment:` no `docker-compose.yml`. Nesse caso o seed não roda e o primeiro administrador precisa ser inserido no banco.

## Cache

As consultas pesadas de leitura ficam em cache de memória por 1 minuto (`Cache:Expiracao`): `GET /api/indicadores`, `GET /api/indicadores/resumo` e `GET /api/capacidade`. Cada combinação de filtros tem a sua entrada.

O log da API mostra o que acontece:

```
CACHE MISS indicadores:resumo: buscando na fonte original     (1ª chamada: vai ao banco)
CACHE HIT indicadores:resumo: valor devolvido do cache        (2ª chamada: não toca o banco)
CACHE INVALIDADO: uma escrita bem-sucedida descartou ...      (um POST, PUT ou DELETE com sucesso)
CACHE MISS indicadores:resumo: buscando na fonte original     (volta a buscar, com dado novo)
```

Qualquer escrita bem-sucedida invalida todas as consultas em cache, então o resultado nunca fica velho. Login, cadastro e gestão de usuários não invalidam. Para ver ao vivo com Docker: `docker compose logs -f api`.

## Rodar os testes

```bash
dotnet test
```

Para rodar só um teste ou grupo:

```bash
dotnet test --filter "FullyQualifiedName~Produto"
```

Os testes ficam em `tests/FabricX.Tests` e cobrem domínio, serviços, repositório e a API (integração), incluindo autenticação, perfis (401 e 403) e cache.

## Estrutura

```
src/
  FabricX.Domain          entidades, enums e regras de negócio
  FabricX.Application     DTOs, interfaces e services (casos de uso)
  FabricX.Infrastructure  EF Core, SQLite e repositórios
  FabricX.Api             controllers, middleware e Program.cs
tests/FabricX.Tests
Dockerfile, docker-compose.yml
database/                 script.sql (schema) e seed.sql (dados de exemplo)
docs/                     documentação do domínio
```

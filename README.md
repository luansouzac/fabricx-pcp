# FabricX

API de PCP (Planejamento e Controle da Produção) em C# / .NET 10, com Arquitetura Limpa e banco SQLite. Documentação de domínio em `docs/`.

**Integrantes:** Francisco Carlos de Assis Santos Junior, Luan Souza Cunha e Igor Rosa. **Disciplina:** Engenharia de Software (IF Sudeste MG, Prof. Paulo Dutra).

## Início rápido

Com o [Docker](https://www.docker.com/products/docker-desktop) aberto, na raiz do projeto:

```bash
docker compose up --build
```

| O que | Endereço |
|---|---|
| Swagger UI (testar a API) | http://localhost:5168/swagger |
| Front-end | http://localhost:5173 |

O banco já sobe criado e populado com o seed. Para entrar, use `admin@fabricx.com` / `Admin@123` (Administrador) ou `operador@fabricx.com` / `Operador@123` (Operador). Sem Docker, veja [Rodar a API sem Docker](#rodar-a-api-sem-docker) e [Rodar o aplicativo (front-end)](#rodar-o-aplicativo-front-end).

## Onde está cada parte do trabalho

| Entregável | Onde está |
|---|---|
| Script DDL (`.sql`) | [database/script.sql](database/script.sql) |
| Dados de exemplo (seed) | [database/seed.sql](database/seed.sql), explicado em [Banco de dados e seeder](#banco-de-dados-e-seeder) |
| DER (imagem, PDF e fonte do diagrama) | [docs/der/der.png](docs/der/der.png), [docs/der/der.pdf](docs/der/der.pdf), [docs/der/der.mmd](docs/der/der.mmd) |
| Documentação do projeto (Processo Unificado) | [docs/processo-unificado/](docs/processo-unificado/01-iniciacao.md): iniciação, elaboração, construção e transição |
| Entidades e relacionamentos | [docs/03-entidades-e-relacionamentos.md](docs/03-entidades-e-relacionamentos.md) |
| Plano, arquitetura e decisões | [docs/02-plano.md](docs/02-plano.md) |
| Conceitos da disciplina aplicados | [docs/conceitos/](docs/conceitos/README.md) |
| Código da API (Arquitetura Limpa) | [src/](src/): `FabricX.Domain`, `FabricX.Application`, `FabricX.Infrastructure` e `FabricX.Api` |
| Testes (xUnit) | [tests/FabricX.Tests](tests/FabricX.Tests), rodam com `dotnet test` |
| Autenticação JWT | [Autenticação (JWT)](#autenticação-jwt) e [docs/conceitos/jwt-e-cache.md](docs/conceitos/jwt-e-cache.md) |
| Cache | [Cache](#cache) |
| Padronização das respostas (`WebResponse<T>`) | [Testar os endpoints](#testar-os-endpoints) e `src/FabricX.Api/Common/WebResponse.cs` |
| Interface gráfica | Front-end Vue em [web/](web/) e Swagger UI |
| Skills do Claude Code | [docs/04-skills.md](docs/04-skills.md) e `.claude/skills/` |

## Sistema e tecnologias

O FabricX controla produtos e estoque, estrutura de produto, ordens de produção, programação em recursos, apontamentos, indicadores de desempenho e planejamento de capacidade. Todas as funcionalidades saem por endpoints HTTP, protegidos por JWT. O aplicativo cliente é o front-end em Vue 3 + Vuetify da pasta `web/` (trabalho de Web III), que consome esta API com login, token JWT e tratamento das respostas padronizadas. O Swagger UI também serve para testar a API sem o front.

| Tema | Tecnologia |
|---|---|
| Linguagem e plataforma | C# 14 / .NET 10, ASP.NET Core Web API (REST sobre HTTP/HTTPS) |
| Arquitetura | Clean Architecture em quatro projetos (Domain, Application, Infrastructure, Api) |
| Persistência | SQLite com EF Core, esquema criado por `database/script.sql` |
| Autenticação | JWT (Bearer) com senhas em PBKDF2 |
| Cache | `IMemoryCache` nas consultas de indicadores e capacidade |
| Padronização das respostas | `WebResponse<T>` (o ResultWrapper do enunciado) |
| Aplicativo cliente | Vue 3, Vuetify 3, Pinia e Vue Router, em `web/` |
| Documentação da API | OpenAPI embutido + Swagger UI |
| Testes e execução | xUnit (unidade e integração), Docker |

## Requisitos

- [Docker](https://www.docker.com/products/docker-desktop) (recomendado, garante a mesma versão para todo o grupo), ou
- [.NET SDK 10](https://dotnet.microsoft.com/download)

## Rodar com Docker

```bash
docker compose up --build
```

Sobem dois serviços:

| Serviço | Endereço |
|---|---|
| Front (Vue, servido por nginx) | `http://localhost:5173` |
| API (com Swagger UI em `/swagger`) | `http://localhost:5168` |

A API já sobe com o seed carregado e a chave de desenvolvimento do JWT (veja [Autenticação](#autenticação-jwt)). O banco fica no volume `fabricx-data` e persiste entre execuções.

O front é compilado com a URL da API definida em `VITE_API_URL` (build arg no `docker-compose.yml`), e a API só aceita requisições vindas das origens listadas em `Cors__Origens__0`. Para publicar em outro endereço, mude os dois valores.

```bash
docker compose down        # para o container, mantém os dados
docker compose down -v     # para e apaga o volume (o próximo up recarrega o seed)
```

Se o build falhar com `unexpected EOF` no `dotnet restore`, é instabilidade de rede ao baixar pacotes NuGet. Rode o comando de novo.

Os testes (`dotnet test`) e o desenvolvimento no dia a dia continuam usando o SDK local.

### Windows

Os comandos `docker compose` funcionam igual no PowerShell, no CMD e no Git Bash. Antes de rodar:

1. Abra o **Docker Desktop** e espere o status "Engine running" (ele precisa do WSL 2 habilitado; o próprio instalador oferece).
2. Confirme que as portas `5168` (API) e `5173` (front) estão livres. Para ver quem usa uma porta: `netstat -ano | findstr :5173`.
3. Não rode o front com `npm run dev` ao mesmo tempo que o Docker: o Vite trocaria para a porta `5174`, que a API do Docker bloqueia por CORS.

Os exemplos de `curl` deste README usam sintaxe de bash. No PowerShell, `curl` é um apelido de `Invoke-WebRequest` e não aceita `-X`, `-H` nem `\` no fim da linha. O mesmo fluxo em PowerShell:

```powershell
$login = Invoke-RestMethod -Method Post -Uri http://localhost:5168/api/auth/login `
  -ContentType "application/json" `
  -Body '{"email":"admin@fabricx.com","senha":"Admin@123"}'
$token = $login.data.token

Invoke-RestMethod -Uri http://localhost:5168/api/produtos -Headers @{ Authorization = "Bearer $token" }
```

O caminho que funciona igual em qualquer sistema é o Swagger UI em `http://localhost:5168/swagger`.

## CORS

A origem do front vem de `Cors:Origens` (lista no `appsettings` ou variáveis `Cors__Origens__0`, `Cors__Origens__1`...). Em Development, sem nenhuma origem configurada, a API libera qualquer origem. Fora de Development, sem origem configurada, nenhuma origem é liberada.

## Rodar a API sem Docker

Na raiz do projeto:

```bash
dotnet run --project src/FabricX.Api --launch-profile http
```

A API sobe em `http://localhost:5168`. O OpenAPI fica em `http://localhost:5168/openapi/v1.json`.

Use o perfil `http`. O perfil `https` exige o certificado de desenvolvimento (`dotnet dev-certs https --trust`).

## Rodar o aplicativo (front-end)

O aplicativo está em `web/` (Vue 3 + Vuetify). Com a API no ar em `http://localhost:5168`:

```bash
cd web
npm install
npm run dev
```

O front sobe em `http://localhost:5173` e entra com os usuários do seed (veja [Autenticação](#autenticação-jwt)). A URL da API vem de `VITE_API_URL` (padrão `http://localhost:5168`; copie `web/.env.example` para `web/.env` para mudar). Requer Node.js 20.19 ou superior (ou 22.12+); o Docker usa o Node 22. Com Docker, o front já sobe junto, em `http://localhost:5173`.

## Banco de dados e seeder

O banco é o arquivo `src/FabricX.Api/fabricx.db` (SQLite), criado na primeira execução e ignorado pelo git. Não é preciso instalar nem configurar banco.

| Arquivo | Papel | Quando roda |
|---|---|---|
| `database/script.sql` | Cria todas as tabelas (DDL) | Na primeira execução, quando a tabela `produto` ainda não existe |
| `database/seed.sql` | Carrega os dados de exemplo | Logo depois do script, só em ambiente Development |

Os dois arquivos são copiados para a pasta de saída do projeto e lidos pelo `BancoInicializador` ao subir a API. Não há comando separado para o seeder: ele roda sozinho ao iniciar a API com o banco vazio, tanto com `dotnet run` quanto com Docker (que já usa Development). Em Production só o script roda, sem dados de exemplo.

O seed só roda quando o banco é criado do zero. Ele traz 2 usuários de desenvolvimento (veja [Autenticação](#autenticação-jwt)), 11 produtos (três deles abaixo do estoque mínimo), estruturas de produto, 6 recursos, roteiros e movimentações iniciais. O exemplo da garrafa tem três níveis: tampinha e garrafa formam a garrafa d'água (`EP-003`, montadora, ciclo de 2 min para 10 unidades), e 6 garrafas d'água formam o kit (`PA-003`, linha de embalagem, 2 min por unidade).

O script e o seed só rodam quando o banco é criado do zero. Quem já tem o `fabricx.db`, ou o volume `fabricx-data` do Docker, precisa recriá-lo para receber a coluna nova do roteiro e o exemplo da garrafa.

Para resetar o banco e recarregar o seed:

```bash
rm src/FabricX.Api/fabricx.db
dotnet run --project src/FabricX.Api --launch-profile http
```

No PowerShell ou no CMD, troque a primeira linha por `Remove-Item src\FabricX.Api\fabricx.db` (PowerShell) ou `del src\FabricX.Api\fabricx.db` (CMD).

Pare a API antes de apagar o arquivo.

Com Docker, o banco fica no volume `fabricx-data`. Para resetar:

```bash
docker compose down -v
docker compose up --build
```

Para só conferir o esquema ou os dados sem a API, abra o `fabricx.db` em qualquer cliente SQLite (DB Browser for SQLite, extensão do VS Code ou `sqlite3 src/FabricX.Api/fabricx.db`). O `script.sql` e o `seed.sql` também podem ser executados à mão num banco vazio.

## Testar os endpoints

Suba a API com o perfil `http` (`dotnet run --project src/FabricX.Api --launch-profile http`) e abra http://localhost:5168/swagger. O Swagger UI lista todos os endpoints e executa as requisições. O JSON da especificação fica em `/openapi/v1.json`.

O Swagger UI só existe em Development.

Quase todos os endpoints exigem token. No Swagger UI: chame `POST /api/auth/login` com um dos usuários abaixo, copie o valor de `data.token`, clique em **Authorize** no topo da página, cole o token e confirme. Os cadeados das rotas passam a enviar o cabeçalho `Authorization: Bearer <token>`.

Endpoints atuais. Todos exigem token, exceto `POST /api/auth/login`:

| Método | Rota | Descrição |
|---|---|---|
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

Todas as respostas seguem o formato `WebResponse<T>`: `data`, `message`, `isSuccess`, `statusCode` e `errors` (lista com os detalhes do erro; vazia no sucesso).

Exemplo de sucesso (`GET /api/recursos/1`):

```json
{ "data": { "id": 1, "nome": "Torno CNC", "tipo": "Maquina", "capacidadeHorasDia": 8, "ativo": true },
  "message": null, "isSuccess": true, "statusCode": 200, "errors": [] }
```

Criar produto (`POST /api/produtos`, exige token):

```bash
curl -s -X POST http://localhost:5168/api/produtos \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"codigo":"PA-900","nome":"Garrafa térmica","tipo":"Acabado","unidadeMedida":"UN","estoqueAtual":0,"estoqueMinimo":20,"custoUnitario":12.5}'
```

```json
{ "data": { "id": 27, "codigo": "PA-900", "nome": "Garrafa térmica", "descricao": null, "tipo": "Acabado", "unidadeMedida": "UN",
            "estoqueAtual": 0, "estoqueMinimo": 20, "custoUnitario": 12.5, "ativo": true, "estoqueAbaixoDoMinimo": true },
  "message": null, "isSuccess": true, "statusCode": 201, "errors": [] }
```

Criar ordem de produção (`POST /api/ordens-producao`). O número é gerado pela API:

```bash
curl -s -X POST http://localhost:5168/api/ordens-producao \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"produtoId":27,"quantidade":100,"prazoEntrega":"2026-12-30","prioridade":"Alta"}'
```

```json
{ "data": { "id": 15, "numero": "OP-2026-0015", "produtoId": 27, "produtoCodigo": "PA-900", "produtoNome": "Garrafa térmica",
            "quantidade": 100, "prazoEntrega": "2026-12-30", "prioridade": "Alta", "status": "Planejada",
            "criadaEm": "2026-11-25T09:35:26-03:00", "concluidaEm": null },
  "message": null, "isSuccess": true, "statusCode": 201, "errors": [] }
```

Regra de negócio violada (ordem sem saldo dos componentes), com o mesmo formato:

```json
{ "data": null, "message": "Estoque insuficiente de EP-003: disponível 0, necessário 600.", "isSuccess": false, "statusCode": 400,
  "errors": ["Estoque insuficiente de EP-003: disponível 0, necessário 600."] }
```

Exemplos de erro, no mesmo formato:

```json
{ "data": null, "message": "Autenticação necessária: envie um token JWT válido no cabeçalho Authorization.", "isSuccess": false, "statusCode": 401, "errors": ["Autenticação necessária: envie um token JWT válido no cabeçalho Authorization."] }
{ "data": null, "message": "O perfil do usuário não permite esta operação.", "isSuccess": false, "statusCode": 403, "errors": ["O perfil do usuário não permite esta operação."] }
{ "data": null, "message": "Recurso 99 não encontrado.", "isSuccess": false, "statusCode": 404, "errors": ["Recurso 99 não encontrado."] }
```

Erro de validação com mais de um problema (`POST /api/recursos` com corpo vazio), com um item em `errors` para cada problema:

```json
{ "data": null, "message": "O nome é obrigatório. A capacidade deve ser maior que zero e de no máximo 24 horas por dia.",
  "isSuccess": false, "statusCode": 400,
  "errors": ["O nome é obrigatório.", "A capacidade deve ser maior que zero e de no máximo 24 horas por dia."] }
```

| Status | Quando |
|---|---|
| 400 | Dado inválido ou regra de negócio violada |
| 401 | Sem token, token inválido ou expirado, ou credencial errada no login |
| 403 | Token válido, mas o perfil não permite a operação |
| 404 | Recurso inexistente |

## Autenticação (JWT)

Fluxo: usuário do seed ou criado por um administrador (`POST /api/usuarios`), login (`POST /api/auth/login`), token no cabeçalho `Authorization: Bearer <token>` nas demais rotas. O token é assinado com HMAC-SHA256, expira em 60 minutos e carrega `sub`, `email`, `name`, `role` e `jti`. Não há refresh token: ao expirar, faça login de novo. A explicação completa, com as decisões, está em [docs/conceitos/jwt-e-cache.md](docs/conceitos/jwt-e-cache.md).

Usuários criados pelo seed (só em Development, para teste):

| E-mail | Senha | Perfil |
|---|---|---|
| `admin@fabricx.com` | `Admin@123` | Administrador |
| `operador@fabricx.com` | `Operador@123` | Operador |

Perfis: o **Operador** usa todos os endpoints de produção, mas não gerencia usuários nem inativa produto e recurso. O **Administrador** faz tudo. Não há cadastro público: só o administrador cria usuários, e o sistema não deixa inativar ou rebaixar o último administrador ativo. Inativar um usuário derruba as sessões dele na hora: o token é revalidado contra o banco a cada requisição. O login tem limite de tentativas por IP (`RateLimit__LoginPorMinuto`, padrão 5) e responde 429 acima disso.

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

Qualquer escrita bem-sucedida invalida todas as consultas em cache, então o resultado nunca fica velho. Login e gestão de usuários não invalidam. Para ver ao vivo com Docker: `docker compose logs -f api`.

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

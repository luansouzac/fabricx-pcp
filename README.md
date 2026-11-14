# FabricX

API de PCP (Planejamento e Controle da Produção) em C# / .NET 10, com Arquitetura Limpa e banco SQLite. Documentação de domínio em `docs/`.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)

## Rodar a API

Na raiz do projeto:

```bash
dotnet run --project src/FabricX.Api --launch-profile http
```

A API sobe em `http://localhost:5168`. O OpenAPI fica em `http://localhost:5168/openapi/v1.json`.

Use o perfil `http`. O perfil `https` exige o certificado de desenvolvimento (`dotnet dev-certs https --trust`).

## Banco de dados e seeder

O banco é o arquivo `fabricx.db`, criado na pasta de onde a API é executada. Na primeira execução a API cria as tabelas com `database/script.sql`. Quando o ambiente é Development, ela também carrega os dados de exemplo de `database/seed.sql`.

O seed só roda quando o banco é criado do zero. Ele traz 7 produtos (três deles abaixo do estoque mínimo), estruturas de produto, 4 recursos e movimentações iniciais.

Para resetar o banco e recarregar o seed:

```bash
rm fabricx.db
dotnet run --project src/FabricX.Api --launch-profile http
```

Pare a API antes de apagar o arquivo. Se você rodou a API de dentro de `src/FabricX.Api`, o `fabricx.db` está nessa pasta.

## Testar os endpoints

Abra `teste-api.html` no navegador com a API rodando. A página tem um formulário para cada endpoint de `/api/produtos` e uma requisição livre. A URL base é editável no topo.

Em Development a API libera CORS para qualquer origem, o que permite abrir o HTML direto do disco.

Endpoints atuais:

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/produtos` | Lista produtos |
| GET | `/api/produtos/estoque-baixo` | Produtos abaixo do estoque mínimo |
| GET | `/api/produtos/{id}` | Busca por id |
| POST | `/api/produtos` | Cria produto |
| PUT | `/api/produtos/{id}` | Atualiza produto |
| POST | `/api/produtos/{id}/movimentacoes` | Entrada ou saída de estoque |
| DELETE | `/api/produtos/{id}` | Inativa produto |

Todas as respostas seguem o formato `WebResponse<T>`: `data`, `message`, `isSuccess`, `statusCode`.

## Rodar os testes

```bash
dotnet test
```

Para rodar só um teste ou grupo:

```bash
dotnet test --filter "FullyQualifiedName~Produto"
```

Os testes ficam em `tests/FabricX.Tests` e cobrem domínio, serviços, repositório e a API (integração).

## Estrutura

```
src/
  FabricX.Domain          entidades, enums e regras de negócio
  FabricX.Application     DTOs, interfaces e services (casos de uso)
  FabricX.Infrastructure  EF Core, SQLite e repositórios
  FabricX.Api             controllers, middleware e Program.cs
tests/FabricX.Tests
database/                 script.sql (schema) e seed.sql (dados de exemplo)
docs/                     documentação do domínio
```

# FabricX

API de PCP (Planejamento e Controle da Produção) em C# / .NET 10, com Arquitetura Limpa e banco SQLite. Documentação de domínio em `docs/`.

## Requisitos

- [Docker](https://www.docker.com/products/docker-desktop) (recomendado, garante a mesma versão para todo o grupo), ou
- [.NET SDK 10](https://dotnet.microsoft.com/download)

## Rodar com Docker

```bash
docker compose up --build
```

A API sobe em `http://localhost:5168` já com o seed carregado. O banco fica no volume `fabricx-data` e persiste entre execuções.

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

O seed só roda quando o banco é criado do zero. Ele traz 11 produtos (três deles abaixo do estoque mínimo), estruturas de produto, 6 recursos, roteiros e movimentações iniciais. O exemplo da garrafa tem três níveis: tampinha e garrafa formam a garrafa d'água (`EP-003`, montadora, ciclo de 2 min para 10 unidades), e 6 garrafas d'água formam o kit (`PA-003`, linha de embalagem, 2 min por unidade).

O script e o seed só rodam quando o banco é criado do zero. Quem já tem o `fabricx.db`, ou o volume `fabricx-data` do Docker, precisa recriá-lo para receber a coluna nova do roteiro e o exemplo da garrafa.

Para resetar o banco e recarregar o seed:

```bash
rm src/FabricX.Api/fabricx.db
dotnet run --project src/FabricX.Api --launch-profile http
```

Pare a API antes de apagar o arquivo.

## Testar os endpoints

Abra `teste-api.html` no navegador com a API rodando. A página tem um formulário para cada endpoint de `/api/produtos` e uma requisição livre. A URL base é editável no topo.

Em Development a API libera CORS para qualquer origem, o que permite abrir o HTML direto do disco.

Endpoints atuais:

| Método | Rota | Descrição |
|---|---|---|
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
| GET | `/api/recursos` | Lista recursos |
| GET | `/api/recursos/{id}` | Busca recurso por id |
| POST | `/api/recursos` | Cria recurso (nome único, capacidade em h/dia maior que zero) |
| PUT | `/api/recursos/{id}` | Atualiza recurso |
| DELETE | `/api/recursos/{id}` | Inativa recurso (não apaga) |
| GET | `/api/ordens-producao?status=` | Lista ordens, com filtro opcional por status |
| GET | `/api/ordens-producao/{id}` | Busca ordem por id |
| POST | `/api/ordens-producao` | Cria ordem (número gerado, valida saldo dos componentes) |
| PUT | `/api/ordens-producao/{id}` | Altera quantidade, prazo e prioridade de ordem planejada |
| POST | `/api/ordens-producao/{id}/avancar` | Avança o status (ao iniciar baixa os componentes, ao concluir dá entrada no acabado) |
| POST | `/api/ordens-producao/{id}/cancelar` | Cancela ordem planejada ou programada |
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
Dockerfile, docker-compose.yml
database/                 script.sql (schema) e seed.sql (dados de exemplo)
docs/                     documentação do domínio
```

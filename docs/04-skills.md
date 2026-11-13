# 04 — Skills do Claude Code

Ficam em `.claude/skills/` e são carregadas automaticamente pelo Claude Code ao trabalhar neste repositório.

## Prontas (repositório oficial da Microsoft)

Fonte: https://github.com/dotnet/skills (licença MIT, em `.claude/skills/LICENSE-dotnet-skills`).

| Skill | Uso |
|---|---|
| `csharp-expert` | Estilo e semântica de C#; indica o especialista certo para cada caso |
| `csharp-refactoring` | Refatorar sem mudar comportamento |
| `dotnet-webapi` | Controllers, OpenAPI/Swagger, arquivos `.http`, middleware de erros |
| `optimizing-ef-core-queries` | Consultas e repositórios com EF Core |
| `run-tests` | Comando correto de `dotnet test` |
| `test-anti-patterns` | Auditoria de testes fracos |

O repositório tem outras skills (MSTest, cobertura, upgrade de versão, Blazor). Não foram copiadas por não serem necessárias. Para usar todas em qualquer projeto: `/plugin marketplace add dotnet/skills`.

## Próprias

| Skill | Uso |
|---|---|
| `fabricx-arquitetura` | Onde cada classe fica, regra de dependência, fluxo `WebResponse<T>` e exceções, passo a passo para criar um módulo |

## Front-end futuro (Vue)

Quando o front-end em Vue for criado, vale adicionar uma skill própria de convenções do `web/` (estrutura de componentes, consumo do `WebResponse<T>`). Não foi criada agora.

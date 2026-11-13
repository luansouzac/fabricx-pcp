---
name: fabricx-arquitetura
description: >
  Como escrever código no FabricX (API de PCP em .NET 10, Arquitetura Limpa do
  professor Paulo Dutra). USE SEMPRE ao criar ou alterar entidade, DTO, Service,
  repositório, controller, middleware ou migração/DDL do FabricX, ou ao decidir em
  qual projeto uma classe deve ficar.
---

# FabricX: arquitetura e convenções

## Onde cada coisa fica

| Projeto | Conteúdo | Pode referenciar |
|---|---|---|
| `FabricX.Domain` | Entidades, value objects, enums, domain services, `DomainException` | nada |
| `FabricX.Application` | DTOs, interfaces de repositório, Services (casos de uso), `NotFoundException` | Domain |
| `FabricX.Infrastructure` | `AppDbContext` (EF Core, SQLite), repositórios, DI | Application |
| `FabricX.Api` | Controllers, `WebResponse<T>`, `ExceptionHandlingMiddleware`, `Program.cs` | Application, Infrastructure (só para DI) |

Se uma classe pede uma referência que inverte essa tabela, ela está no projeto errado.

## Domain

- Entidade com identidade, estado `private set` e comportamento. Regras de negócio no método da entidade, violação lança `DomainException`.
- Construtor protege invariantes (quantidade > 0, prazo futuro etc.). Nada de entidade anêmica.
- Value objects imutáveis, comparados por valor.
- Sem EF Core, sem atributos de persistência, sem referência a HTTP.

## Application

- Um Service por caso de uso ou por agregado, com interface `I...Service` e dependência só de interfaces (`IOrdemProducaoRepository`).
- Recebe DTO de entrada e devolve DTO de saída. Nunca devolve nem recebe entidade.
- Não conhece HTTP: não usa `StatusCode`, `IActionResult` nem `WebResponse<T>`.
- Registro não encontrado: `NotFoundException`. Regra violada: deixa a `DomainException` subir.
- Validação estrutural no DTO (`[Required]`, `[Range]`); regra de negócio na entidade.

## Infrastructure

- Só aqui existe EF Core e o provider (SQLite). Trocar de banco = trocar pacote e o registro no `Program.cs`.
- Repositório implementa a interface da Application. Mapeamento entidade ↔ tabela por Fluent API em `IEntityTypeConfiguration<T>`, não por atributos.
- O esquema oficial é o `database/script.sql`; o `DbContext` deve refletir esse esquema (nomes de tabela e coluna em snake_case).

## Api

- Controller fino: recebe DTO, chama o Service, monta `WebResponse<T>.Success(...)` e devolve com o status certo.
- Não usa try/catch para regra de negócio; o `ExceptionHandlingMiddleware` converte: `DomainException` 400, `NotFoundException` 404, demais 500.
- Toda resposta HTTP, de sucesso ou erro, usa `WebResponse<T>`.
- Rotas em `api/[controller]`, `[ProducesResponseType]` para cada status, `CancellationToken` nas ações assíncronas.

## Novo módulo, passo a passo

1. Tabela no `database/script.sql` e DER em `docs/der/der.mmd`.
2. Entidade e enums no Domain, com testes unitários.
3. Interface do repositório e DTOs na Application.
4. Service na Application, com testes usando repositório falso.
5. Repositório e configuração EF na Infrastructure.
6. Controller na Api e registro de DI.

## Estilo

Siga o `CLAUDE.md` da raiz: quase sem comentários, nomes claros. Para estilo geral de C# use a skill `csharp-expert`; para endpoints, `dotnet-webapi`; para EF Core, `optimizing-ef-core-queries`; para testes, `run-tests` e `test-anti-patterns`.

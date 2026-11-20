# Construção

Registra o que foi implementado por iteração, os testes e as decisões de design. O estado abaixo é o do repositório na data de escrita e deve ser atualizado na revisão final (issue #17).

## Módulos por iteração

| Iteração | Módulo | Endpoints | Estado |
|---|---|---|---|
| 1 | Swagger UI | `/swagger`, `/openapi/v1.json` (só em Development) | feito |
| 1 | Produtos e estoque | `/api/produtos`, `/api/produtos/{id}/movimentacoes` (POST e GET), filtro `?tipo=` | feito |
| 1 | Estrutura de produto | `/api/produtos/{id}/estrutura` (GET, POST, PUT e DELETE por componente) | feito |
| 1 | Ordens de produção | `/api/ordens-producao` (GET, POST, PUT), `/{id}/avancar`, `/{id}/cancelar` | feito |
| 2 | Recursos, roteiro, DDL com paralelismo | a definir nas issues #1, #10 e #11 | em andamento |
| 3 | Programação, apontamentos, indicadores | a definir nas issues #12 e #13 | planejado |
| 4 | Capacidade e ERP/SGQ | a definir nas issues #14 e #15 | planejado, opcional |

## Fluxo da ordem de produção

```
Planejada ──► Programada ──► Em produção ──► Concluída
    └───────────┴──► Cancelada
```

- **Criar:** valida produto ativo, prazo e quantidade, e o saldo dos componentes diretos.
- **Programada ──► Em produção:** confere o saldo de todos os componentes, baixa cada um e grava as movimentações de saída ligadas à ordem, em um único `SaveChanges`.
- **Em produção ──► Concluída:** dá entrada no produto acabado, grava a movimentação ligada à ordem e preenche `concluida_em`.
- **Cancelar:** só em Planejada e Programada.

Exemplo de aceitação coberto por teste: ordem de 60 garrafas d'água, com 1 tampinha e 1 garrafa por unidade. Ao concluir, tampinhas e garrafas caem 60 e as garrafas d'água sobem 60.

## Testes

Projeto `tests/FabricX.Tests` (xUnit), com quatro níveis:

| Nível | O que cobre | Como |
|---|---|---|
| Domain | Regras das entidades (`Produto`, `ItemEstrutura`, `OrdemProducao`, `MovimentacaoEstoque`) e o detector de ciclo | Sem banco |
| Application | Services com repositórios falsos (`Fakes/`) | Sem banco |
| Infrastructure | Repositórios contra o `database/script.sql` real | SQLite em memória (`BancoSqliteFixture`) |
| Api | Endpoints de ponta a ponta, com envelope e status HTTP | `WebApplicationFactory` (`ApiFactory`) com banco temporário |

Rodar: `dotnet test`.

## Decisões de design e referência à disciplina

| Decisão | Motivo | Conceito |
|---|---|---|
| Entidades com comportamento, sem setters públicos | Evita modelo anêmico e protege as regras | [engenharia-de-software.md](../conceitos/engenharia-de-software.md) |
| Interfaces de repositório na Application, implementação na Infrastructure | Inversão de dependência e banco trocável | [repositorio-dao-service.md](../conceitos/repositorio-dao-service.md) |
| Construtores injetados, tempo de vida Scoped | Testabilidade e coerência com o `DbContext` | [injecao-de-dependencia.md](../conceitos/injecao-de-dependencia.md) |
| Sem unidade de trabalho nem MediatR | Seria excesso para este tamanho de projeto | [quando-aplicar-e-overengineering.md](../conceitos/quando-aplicar-e-overengineering.md) |
| `async`/`await` ponta a ponta com `CancellationToken` | Escalabilidade sem bloquear threads | [async-await.md](../conceitos/async-await.md) |
| `ControllerBase`, `[ApiController]` e `ProducesResponseType` com o envelope | Contrato visível no OpenAPI | [aspnet-core-web-api.md](../conceitos/aspnet-core-web-api.md) |

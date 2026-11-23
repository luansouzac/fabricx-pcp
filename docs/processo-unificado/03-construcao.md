# Construção

Registra o que foi implementado por iteração, os testes e as decisões de design. Todos os módulos do ETS estão implementados; o ERP/SGQ e a capacidade são os opcionais.

## Módulos por iteração

| Iteração | Módulo | Endpoints | Estado |
|---|---|---|---|
| 1 | Swagger UI | `/swagger`, `/openapi/v1.json` (só em Development) | feito |
| 1 | Produtos e estoque | `/api/produtos`, `/api/produtos/{id}/movimentacoes` (POST e GET), filtro `?tipo=` | feito |
| 1 | Estrutura de produto | `/api/produtos/{id}/estrutura` (GET, POST, PUT e DELETE por componente) | feito |
| 1 | Ordens de produção | `/api/ordens-producao` (GET, POST, PUT), `/{id}/avancar`, `/{id}/cancelar` | feito |
| 2 | Recursos | `/api/recursos` (GET, POST, PUT e DELETE, que inativa) | feito |
| 2 | Roteiro de etapas com paralelismo | `/api/produtos/{id}/roteiro` (GET, POST, PUT e DELETE por etapa) | feito |
| 3 | Programação | `/api/ordens-producao/{id}/programacao` (POST e GET), `/api/programacao` (cronograma), `/api/programacao/{id}` (PUT e DELETE) | feito |
| 3 | Apontamentos e indicadores | `/api/programacao/{id}/apontamentos`, `/api/ordens-producao/{id}/apontamentos`, `/api/indicadores` e `/api/indicadores/resumo` | feito |
| 4 | Demanda prevista e capacidade | `/api/demanda-prevista` e `/api/capacidade?periodo=AAAA-MM` | feito, opcional |
| 4 | Integração ERP/SGQ simulada | `/api/integracoes/erp/pedidos` e `/api/integracoes/sgq/resultados` | feito, opcional |

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

## Programação, apontamento e indicadores

- **Programação:** o início é informado e o fim é calculado por `RoteiroEtapa.CalcularDuracaoMinutos` (teto da quantidade ÷ unidades por ciclo, vezes o tempo de ciclo). As regras ficam em `RegrasDeProgramacao`: cada etapa só começa depois do fim da anterior, sem sobreposição no mesmo recurso e dentro da capacidade diária (intervalo que cruza a meia-noite conta em cada dia). Ao programar a última etapa do roteiro, a ordem passa de Planejada para Programada.
- **Apontamento:** o primeiro apontamento coloca a programação em andamento; quando a soma produzida atinge a quantidade da ordem, ela é concluída. Apontamento e mudança de status são gravados juntos.
- **Indicadores** (`CalculadoraDeIndicadores`): qualidade = defeituosas ÷ (produzidas + defeituosas); eficiência = tempo padrão ÷ tempo gasto; produtividade = produzidas por hora apontada; cumprimento de prazo = fração das ordens concluídas no prazo. Sem dados o indicador volta nulo.
- **Capacidade** (`CalculadoraDeCapacidade`): por recurso, capacidade do mês, horas programadas e horas necessárias para a demanda prevista. A carga é o maior entre as duas: acima de 100% é gargalo e abaixo de 50% é ociosidade.

## Testes

Projeto `tests/FabricX.Tests` (xUnit), com quatro níveis:

| Nível | O que cobre | Como |
|---|---|---|
| Domain | Regras das entidades, o detector de ciclo, as regras de programação e as calculadoras de indicadores e de capacidade | Sem banco |
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

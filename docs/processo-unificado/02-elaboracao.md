# Elaboração

Detalha os requisitos, fixa a arquitetura e planeja as iterações.

## Regras de negócio

| Código | Regra |
|---|---|
| RN-01 | O código do produto é único |
| RN-02 | O estoque nunca fica negativo: saída maior que o saldo é recusada |
| RN-03 | Estoque abaixo do mínimo gera alerta de reposição |
| RN-04 | O produto não pode ser componente dele mesmo nem formar ciclo na estrutura |
| RN-05 | Pai e componente da estrutura precisam existir e estar ativos, e o par é único |
| RN-06 | Quantidade da ordem maior que zero e prazo de entrega não pode estar no passado |
| RN-07 | O número da ordem é gerado (`OP-2026-0001`) e é único |
| RN-08 | Criar ou alterar a ordem exige saldo de cada componente direto (quantidade do componente vezes a da ordem) |
| RN-09 | Fluxo da ordem: Planejada, Programada, Em produção, Concluída; só a ordem Planejada pode ser alterada |
| RN-10 | Iniciar a ordem baixa os componentes de uma vez (se faltar algum, nada é baixado); concluir só dá entrada no acabado e nunca falha por estoque |
| RN-11 | Ordem em produção não pode ser cancelada, porque o material já foi consumido |
| RN-12 | A duração da etapa é `teto(quantidade ÷ unidades por ciclo) × tempo de ciclo` |
| RN-13 | Programação: início informado, fim calculado, sem sobreposição no mesmo recurso, respeitando a capacidade diária e a sequência das etapas |
| RN-14 | Indicadores: qualidade, eficiência e produtividade calculados a partir dos apontamentos (fórmulas em [../03-entidades-e-relacionamentos.md](../03-entidades-e-relacionamentos.md)) |


## Arquitetura

Arquitetura Limpa em camadas, com dependência sempre de fora para dentro. Fundamentos em [../conceitos/arquitetura-limpa.md](../conceitos/arquitetura-limpa.md).

```
Api ──► Application ──► Domain
Infrastructure ──► Application ──► Domain
```

| Camada | Conteúdo | Exemplos |
|---|---|---|
| Domain | Entidades com regras, enums e `DomainException` | `Produto`, `ItemEstrutura`, `OrdemProducao`, `MovimentacaoEstoque`, `DetectorCicloEstrutura` |
| Application | DTOs, interfaces de repositório e Services (casos de uso) | `ProdutoService`, `ItemEstruturaService`, `OrdemProducaoService` |
| Infrastructure | EF Core, configurações de tabela e repositórios | `AppDbContext`, `ProdutoRepository`, `OrdemProducaoRepository` |
| Api | Controllers, `WebResponse<T>` e middleware de erros | `ProdutosController`, `EstruturaController`, `OrdensProducaoController` |

Decisões de design:

- **Services devolvem DTO** ou lançam `DomainException` (400) e `NotFoundException` (404). O `ExceptionHandlingMiddleware` converte em `WebResponse<T>.Failure`. Erro de validação de modelo (Data Annotations) também sai no envelope.
- **Atomicidade sem abstração extra:** os repositórios compartilham o `AppDbContext` (scoped). O Service altera as entidades já rastreadas e o último `SaveChanges` grava saldo, ordem e movimentações juntos. Evita criar uma camada de unidade de trabalho (ver [../conceitos/quando-aplicar-e-overengineering.md](../conceitos/quando-aplicar-e-overengineering.md)).
- **Regra na entidade:** quantidade positiva, saldo, cancelamento e transições de status ficam em `Produto`, `ItemEstrutura` e `OrdemProducao`. Os Services só orquestram.
- **Detecção de ciclo no Domain**, como função pura (`DetectorCicloEstrutura`), testável sem banco.
- **Banco trocável:** só a Infrastructure conhece o EF Core e o SQLite.
- **Estoque nunca negativo e sem reserva:** decisão de simplicidade. Duas ordens podem passar na criação e só a primeira conseguir iniciar.

## Modelo de dados

Um único cadastro de produto com `tipo`, e uma estrutura (`ITEM_ESTRUTURA`) que liga qualquer produto a qualquer componente, o que permite vários níveis (tampinha e garrafa formam a garrafa d'água, que forma o kit). O DER está em [../der/der.mmd](../der/der.mmd) e em [../03-entidades-e-relacionamentos.md](../03-entidades-e-relacionamentos.md); o DDL em `database/script.sql`.

Paralelismo do roteiro: cada etapa tem `tempo_ciclo_min` e `unidades_por_ciclo` (RN-12). Quantas unidades a etapa faz por ciclo vem do cadastro; o sistema não procura a melhor alocação.

## Protótipos

Como o ETS entrega só a API, o protótipo de uso é o **Swagger UI** em `/swagger`, gerado do OpenAPI. Os endpoints estão listados no [README](../../README.md). As telas de verdade (Vue.js) são do trabalho de Web III.

## Plano de iterações

| Iteração | Conteúdo | Issues |
|---|---|---|
| 1 | Swagger UI, estoque com movimentações, estrutura de produto, ordens ligadas ao estoque | #7, #8, #9, #2 |
| 2 | Recursos, DDL e DER com paralelismo, roteiro | #1, #10, #11 |
| 3 | Programação, apontamentos e indicadores | #12, #13 |
| 4 | Capacidade e integração ERP/SGQ (opcionais) | #14, #15 |
| 5 | Documentação, revisão final e entrega | #16, #17 |

Cada iteração usa branches `feat/...` com Pull Request. A entrega é uma tag (`ets-entrega`) em 25/11/2026; depois dela o ETS não é alterado.

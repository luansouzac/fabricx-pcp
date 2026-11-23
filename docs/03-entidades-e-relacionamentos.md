# 03 — Entidades e relacionamentos

Mapa dos requisitos do enunciado para as tabelas. O DER está em [der/der.mmd](der/der.mmd) e logo abaixo.

## Entidades

| Entidade | Papel | Requisito do enunciado |
|---|---|---|
| `PRODUTO` | Cadastro único de matéria-prima, produto em processo e acabado (campo `tipo`). Guarda saldo e estoque mínimo. | Estoque |
| `ITEM_ESTRUTURA` | Estrutura do produto: quanto de cada componente um produto pai consome. | Ordens de produção, Estoque |
| `RECURSO` | Máquina, operador ou linha, com `capacidade_horas_dia`. | Programação, Capacidade |
| `ROTEIRO_ETAPA` | Sequência de etapas de um produto, com recurso, tempo de um ciclo e unidades feitas por ciclo (paralelismo). | Programação |
| `ORDEM_PRODUCAO` | Produto, quantidade, prazo, prioridade e status. | Ordens de produção |
| `PROGRAMACAO` | Etapa de uma ordem alocada a um recurso com início e fim previstos (o cronograma). | Programação |
| `APONTAMENTO` | Produção real: quantidade boa, defeituosa e tempo gasto. | Desempenho |
| `MOVIMENTACAO_ESTOQUE` | Entradas e saídas de estoque, opcionalmente ligadas a uma ordem. | Estoque |
| `DEMANDA_PREVISTA` | Quantidade esperada por produto e mês. | Planejamento de capacidade |

## Relacionamentos

- `PRODUTO` 1—N `ITEM_ESTRUTURA` duas vezes: como **pai** e como **componente** (autorrelacionamento N—N).
- `PRODUTO` 1—N `ROTEIRO_ETAPA`, `ORDEM_PRODUCAO`, `MOVIMENTACAO_ESTOQUE`, `DEMANDA_PREVISTA`.
- `RECURSO` 1—N `ROTEIRO_ETAPA` e `PROGRAMACAO`.
- `ORDEM_PRODUCAO` 1—N `PROGRAMACAO`; 0..1—N `MOVIMENTACAO_ESTOQUE`.
- `ROTEIRO_ETAPA` 1—N `PROGRAMACAO`.
- `PROGRAMACAO` 1—N `APONTAMENTO`.

## Como saem os indicadores (sem tabela própria)

| Indicador | Cálculo |
|---|---|
| Qualidade | `sum(qtd_defeituosa) / sum(qtd_produzida + qtd_defeituosa)` |
| Eficiência | tempo padrão (`teto(qtd ÷ unidades_por_ciclo) × tempo_ciclo_min`) ÷ `tempo_gasto_min` |
| Produtividade | `qtd_produzida` por hora apontada |
| Cumprimento de prazo | ordens `CONCLUIDA` com `concluida_em <= prazo_entrega` |
| Gargalo / ociosidade | horas programadas no recurso ÷ (`capacidade_horas_dia` × dias do período) |
| Alerta de reposição | `estoque_atual < estoque_minimo` |

## Decisões de modelagem

- Um só cadastro de `PRODUTO` com `tipo`, em vez de três tabelas: a estrutura (BOM) liga qualquer produto a qualquer componente.
- `estoque_atual` é guardado no produto e atualizado a cada `MOVIMENTACAO_ESTOQUE` (o histórico fica na movimentação).
- Datas em `TEXT` ISO-8601 e booleanos em `INTEGER`, por serem os tipos nativos do SQLite.
- Integração com ERP/SGQ (desejável) não tem tabela: será uma interface na Application com implementação simulada.

## DER

```mermaid
erDiagram
    PRODUTO ||--o{ ITEM_ESTRUTURA : "e pai de"
    PRODUTO ||--o{ ITEM_ESTRUTURA : "e componente de"
    PRODUTO ||--o{ ROTEIRO_ETAPA : "possui roteiro"
    PRODUTO ||--o{ ORDEM_PRODUCAO : "e fabricado em"
    PRODUTO ||--o{ MOVIMENTACAO_ESTOQUE : "movimenta"
    PRODUTO ||--o{ DEMANDA_PREVISTA : "tem demanda"
    RECURSO ||--o{ ROTEIRO_ETAPA : "executa"
    RECURSO ||--o{ PROGRAMACAO : "e alocado em"
    ORDEM_PRODUCAO ||--o{ PROGRAMACAO : "e programada em"
    ORDEM_PRODUCAO |o--o{ MOVIMENTACAO_ESTOQUE : "gera"
    ROTEIRO_ETAPA ||--o{ PROGRAMACAO : "origina"
    PROGRAMACAO ||--o{ APONTAMENTO : "recebe"

    PRODUTO {
        INTEGER id PK
        TEXT codigo UK
        TEXT nome
        TEXT descricao
        TEXT tipo "MATERIA_PRIMA | EM_PROCESSO | ACABADO"
        TEXT unidade_medida
        REAL estoque_atual
        REAL estoque_minimo
        REAL custo_unitario
        INTEGER ativo
    }
    ITEM_ESTRUTURA {
        INTEGER id PK
        INTEGER produto_pai_id FK
        INTEGER componente_id FK
        REAL quantidade
    }
    RECURSO {
        INTEGER id PK
        TEXT nome
        TEXT tipo "MAQUINA | OPERADOR | LINHA"
        REAL capacidade_horas_dia
        INTEGER ativo
    }
    ROTEIRO_ETAPA {
        INTEGER id PK
        INTEGER produto_id FK
        INTEGER recurso_id FK
        INTEGER sequencia
        TEXT nome
        REAL tempo_ciclo_min
        INTEGER unidades_por_ciclo "padrao 1"
    }
    ORDEM_PRODUCAO {
        INTEGER id PK
        TEXT numero UK
        INTEGER produto_id FK
        REAL quantidade
        TEXT prazo_entrega
        TEXT prioridade "BAIXA | MEDIA | ALTA | URGENTE"
        TEXT status "PLANEJADA | PROGRAMADA | EM_PRODUCAO | CONCLUIDA | CANCELADA"
        TEXT criada_em
        TEXT concluida_em
    }
    PROGRAMACAO {
        INTEGER id PK
        INTEGER ordem_id FK
        INTEGER etapa_id FK
        INTEGER recurso_id FK
        TEXT inicio_previsto
        TEXT fim_previsto
        TEXT status "AGENDADA | EM_ANDAMENTO | CONCLUIDA"
    }
    APONTAMENTO {
        INTEGER id PK
        INTEGER programacao_id FK
        TEXT data_hora
        REAL qtd_produzida
        REAL qtd_defeituosa
        REAL tempo_gasto_min
        TEXT observacao
    }
    MOVIMENTACAO_ESTOQUE {
        INTEGER id PK
        INTEGER produto_id FK
        INTEGER ordem_id FK "opcional"
        TEXT tipo "ENTRADA | SAIDA"
        REAL quantidade
        TEXT motivo
        TEXT data_hora
    }
    DEMANDA_PREVISTA {
        INTEGER id PK
        INTEGER produto_id FK
        TEXT periodo "AAAA-MM"
        REAL quantidade
    }
```

PRAGMA foreign_keys = ON;

CREATE TABLE produto (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo          TEXT    NOT NULL UNIQUE,
    nome            TEXT    NOT NULL,
    descricao       TEXT,
    tipo            TEXT    NOT NULL
                    CHECK (tipo IN ('MATERIA_PRIMA', 'EM_PROCESSO', 'ACABADO')),
    unidade_medida  TEXT    NOT NULL DEFAULT 'UN',
    estoque_atual   REAL    NOT NULL DEFAULT 0 CHECK (estoque_atual >= 0),
    estoque_minimo  REAL    NOT NULL DEFAULT 0 CHECK (estoque_minimo >= 0),
    custo_unitario  REAL    NOT NULL DEFAULT 0 CHECK (custo_unitario >= 0),
    ativo           INTEGER NOT NULL DEFAULT 1 CHECK (ativo IN (0, 1))
);

CREATE TABLE item_estrutura (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    produto_pai_id  INTEGER NOT NULL REFERENCES produto (id),
    componente_id   INTEGER NOT NULL REFERENCES produto (id),
    quantidade      REAL    NOT NULL CHECK (quantidade > 0),
    UNIQUE (produto_pai_id, componente_id),
    CHECK (produto_pai_id <> componente_id)
);

CREATE TABLE recurso (
    id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    nome                 TEXT    NOT NULL UNIQUE,
    tipo                 TEXT    NOT NULL
                         CHECK (tipo IN ('MAQUINA', 'OPERADOR', 'LINHA')),
    capacidade_horas_dia REAL    NOT NULL CHECK (capacidade_horas_dia > 0),
    ativo                INTEGER NOT NULL DEFAULT 1 CHECK (ativo IN (0, 1))
);

CREATE TABLE roteiro_etapa (
    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    produto_id          INTEGER NOT NULL REFERENCES produto (id),
    recurso_id          INTEGER NOT NULL REFERENCES recurso (id),
    sequencia           INTEGER NOT NULL CHECK (sequencia > 0),
    nome                TEXT    NOT NULL,
    tempo_ciclo_min     REAL    NOT NULL CHECK (tempo_ciclo_min > 0),
    unidades_por_ciclo  INTEGER NOT NULL DEFAULT 1 CHECK (unidades_por_ciclo >= 1),
    UNIQUE (produto_id, sequencia)
);

CREATE TABLE ordem_producao (
    id             INTEGER PRIMARY KEY AUTOINCREMENT,
    numero         TEXT    NOT NULL UNIQUE,
    produto_id     INTEGER NOT NULL REFERENCES produto (id),
    quantidade     REAL    NOT NULL CHECK (quantidade > 0),
    prazo_entrega  TEXT    NOT NULL,
    prioridade     TEXT    NOT NULL DEFAULT 'MEDIA'
                   CHECK (prioridade IN ('BAIXA', 'MEDIA', 'ALTA', 'URGENTE')),
    status         TEXT    NOT NULL DEFAULT 'PLANEJADA'
                   CHECK (status IN ('PLANEJADA', 'PROGRAMADA', 'EM_PRODUCAO',
                                     'CONCLUIDA', 'CANCELADA')),
    criada_em      TEXT    NOT NULL DEFAULT (datetime('now')),
    concluida_em   TEXT
);

CREATE TABLE programacao (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    ordem_id         INTEGER NOT NULL REFERENCES ordem_producao (id),
    etapa_id         INTEGER NOT NULL REFERENCES roteiro_etapa (id),
    recurso_id       INTEGER NOT NULL REFERENCES recurso (id),
    inicio_previsto  TEXT    NOT NULL,
    fim_previsto     TEXT    NOT NULL,
    status           TEXT    NOT NULL DEFAULT 'AGENDADA'
                     CHECK (status IN ('AGENDADA', 'EM_ANDAMENTO', 'CONCLUIDA')),
    UNIQUE (ordem_id, etapa_id),
    CHECK (fim_previsto > inicio_previsto)
);

CREATE TABLE apontamento (
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    programacao_id   INTEGER NOT NULL REFERENCES programacao (id),
    data_hora        TEXT    NOT NULL DEFAULT (datetime('now')),
    qtd_produzida    REAL    NOT NULL DEFAULT 0 CHECK (qtd_produzida >= 0),
    qtd_defeituosa   REAL    NOT NULL DEFAULT 0 CHECK (qtd_defeituosa >= 0),
    tempo_gasto_min  REAL    NOT NULL CHECK (tempo_gasto_min > 0),
    observacao       TEXT
);

CREATE TABLE movimentacao_estoque (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    produto_id  INTEGER NOT NULL REFERENCES produto (id),
    ordem_id    INTEGER REFERENCES ordem_producao (id),
    tipo        TEXT    NOT NULL CHECK (tipo IN ('ENTRADA', 'SAIDA')),
    quantidade  REAL    NOT NULL CHECK (quantidade > 0),
    motivo      TEXT,
    data_hora   TEXT    NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE demanda_prevista (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    produto_id  INTEGER NOT NULL REFERENCES produto (id),
    periodo     TEXT    NOT NULL CHECK (periodo GLOB '[0-9][0-9][0-9][0-9]-[0-1][0-9]'),
    quantidade  REAL    NOT NULL CHECK (quantidade > 0),
    UNIQUE (produto_id, periodo)
);

CREATE INDEX ix_ordem_status_prazo   ON ordem_producao (status, prazo_entrega);
CREATE INDEX ix_programacao_recurso  ON programacao (recurso_id, inicio_previsto);
CREATE INDEX ix_apontamento_prog     ON apontamento (programacao_id);
CREATE INDEX ix_movimentacao_produto ON movimentacao_estoque (produto_id, data_hora);

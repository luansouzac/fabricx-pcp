INSERT INTO produto (codigo, nome, descricao, tipo, unidade_medida, estoque_atual, estoque_minimo, custo_unitario) VALUES
    ('MP-001', 'Chapa de aço 2mm', 'Chapa laminada a frio', 'MATERIA_PRIMA', 'UN', 100, 20, 12.50),
    ('MP-002', 'Parafuso M6', NULL, 'MATERIA_PRIMA', 'UN', 15, 200, 0.35),
    ('MP-003', 'Tinta preta', 'Tinta epóxi', 'MATERIA_PRIMA', 'L', 8, 10, 28.90),
    ('EP-001', 'Painel cortado', NULL, 'EM_PROCESSO', 'UN', 40, 10, 18.00),
    ('EP-002', 'Painel pintado', NULL, 'EM_PROCESSO', 'UN', 5, 10, 24.50),
    ('PA-001', 'Gabinete metálico', 'Produto acabado', 'ACABADO', 'UN', 12, 5, 95.00),
    ('PA-002', 'Caixa de passagem', NULL, 'ACABADO', 'UN', 3, 8, 42.00);

INSERT INTO item_estrutura (produto_pai_id, componente_id, quantidade) VALUES
    (4, 1, 1),
    (5, 4, 1),
    (5, 3, 0.2),
    (6, 5, 4),
    (6, 2, 16),
    (7, 5, 2),
    (7, 2, 8);

INSERT INTO recurso (nome, tipo, capacidade_horas_dia) VALUES
    ('Cortadora laser', 'MAQUINA', 8),
    ('Cabine de pintura', 'MAQUINA', 8),
    ('Operador João', 'OPERADOR', 8),
    ('Linha de montagem 1', 'LINHA', 16);

INSERT INTO movimentacao_estoque (produto_id, tipo, quantidade, motivo) VALUES
    (1, 'ENTRADA', 100, 'Estoque inicial'),
    (2, 'ENTRADA', 15, 'Estoque inicial'),
    (6, 'ENTRADA', 12, 'Estoque inicial');

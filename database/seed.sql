INSERT INTO produto (codigo, nome, descricao, tipo, unidade_medida, estoque_atual, estoque_minimo, custo_unitario) VALUES
    ('MP-001', 'Chapa de aço 2mm', 'Chapa laminada a frio', 'MATERIA_PRIMA', 'UN', 100, 20, 12.50),
    ('MP-002', 'Parafuso M6', NULL, 'MATERIA_PRIMA', 'UN', 15, 200, 0.35),
    ('MP-003', 'Tinta preta', 'Tinta epóxi', 'MATERIA_PRIMA', 'L', 8, 10, 28.90),
    ('EP-001', 'Painel cortado', NULL, 'EM_PROCESSO', 'UN', 40, 10, 18.00),
    ('EP-002', 'Painel pintado', NULL, 'EM_PROCESSO', 'UN', 5, 10, 24.50),
    ('PA-001', 'Gabinete metálico', 'Produto acabado', 'ACABADO', 'UN', 12, 5, 95.00),
    ('PA-002', 'Caixa de passagem', NULL, 'ACABADO', 'UN', 3, 8, 42.00),
    ('MP-004', 'Tampinha', 'Tampa plástica rosqueável', 'MATERIA_PRIMA', 'UN', 200, 50, 0.20),
    ('MP-005', 'Garrafa', 'Garrafa plástica de 500 ml', 'MATERIA_PRIMA', 'UN', 200, 50, 0.80),
    ('EP-003', 'Garrafa d''água', 'Garrafa com tampinha', 'EM_PROCESSO', 'UN', 0, 0, 1.00),
    ('PA-003', 'Kit de garrafas', 'Kit com 6 garrafas d''água', 'ACABADO', 'UN', 0, 0, 6.50);

INSERT INTO item_estrutura (produto_pai_id, componente_id, quantidade) VALUES
    (4, 1, 1),
    (5, 4, 1),
    (5, 3, 0.2),
    (6, 5, 4),
    (6, 2, 16),
    (7, 5, 2),
    (7, 2, 8),
    (10, 8, 1),
    (10, 9, 1),
    (11, 10, 6);

INSERT INTO recurso (nome, tipo, capacidade_horas_dia) VALUES
    ('Cortadora laser', 'MAQUINA', 8),
    ('Cabine de pintura', 'MAQUINA', 8),
    ('Operador João', 'OPERADOR', 8),
    ('Linha de montagem 1', 'LINHA', 16),
    ('Montadora', 'MAQUINA', 8),
    ('Linha de embalagem', 'LINHA', 8);

INSERT INTO roteiro_etapa (produto_id, recurso_id, sequencia, nome, tempo_ciclo_min, unidades_por_ciclo) VALUES
    (10, 5, 1, 'Montagem', 2, 10),
    (11, 6, 1, 'Embalagem', 2, 1),
    (6, 1, 1, 'Corte', 6, 1),
    (6, 2, 2, 'Pintura', 10, 1),
    (6, 4, 3, 'Montagem', 15, 1);

INSERT INTO demanda_prevista (produto_id, periodo, quantidade) VALUES
    (10, '2026-10', 3000),
    (11, '2026-10', 5000),
    (6, '2026-10', 700),
    (10, '2026-11', 3000),
    (11, '2026-11', 7500),
    (6, '2026-11', 900),
    (10, '2026-12', 600),
    (11, '2026-12', 8000);

INSERT INTO ordem_producao (numero, produto_id, quantidade, prazo_entrega, prioridade, status, criada_em, concluida_em) VALUES
    ('OP-2026-0001', 10, 300, '2026-11-10', 'ALTA', 'CONCLUIDA', '2026-11-02 09:00:00', '2026-11-09 16:00:00'),
    ('OP-2026-0002', 11, 1000, '2026-11-12', 'MEDIA', 'CONCLUIDA', '2026-11-03 09:00:00', '2026-11-14 11:00:00'),
    ('OP-2026-0003', 6, 50, '2026-12-05', 'ALTA', 'EM_PRODUCAO', '2026-11-18 09:00:00', NULL),
    ('OP-2026-0004', 10, 600, '2026-12-15', 'MEDIA', 'PROGRAMADA', '2026-11-20 09:00:00', NULL),
    ('OP-2026-0005', 11, 2000, '2026-12-20', 'BAIXA', 'PLANEJADA', '2026-11-22 09:00:00', NULL),
    ('OP-2026-0006', 7, 20, '2026-11-30', 'URGENTE', 'CANCELADA', '2026-11-15 09:00:00', NULL);

INSERT INTO programacao (ordem_id, etapa_id, recurso_id, inicio_previsto, fim_previsto, status) VALUES
    (1, 1, 5, '2026-11-03 08:00:00', '2026-11-03 09:30:00', 'CONCLUIDA'),
    (2, 2, 6, '2026-11-04 08:00:00', '2026-11-11 08:00:00', 'CONCLUIDA'),
    (3, 3, 1, '2026-11-19 08:00:00', '2026-11-19 14:00:00', 'CONCLUIDA'),
    (3, 4, 2, '2026-11-24 08:00:00', '2026-11-25 08:00:00', 'EM_ANDAMENTO'),
    (3, 5, 4, '2026-11-26 08:00:00', '2026-11-27 08:00:00', 'AGENDADA'),
    (4, 1, 5, '2026-12-01 08:00:00', '2026-12-01 20:00:00', 'AGENDADA');

INSERT INTO apontamento (programacao_id, data_hora, qtd_produzida, qtd_defeituosa, tempo_gasto_min, observacao) VALUES
    (1, '2026-11-03 09:20:00', 300, 6, 65, NULL),
    (2, '2026-11-06 17:00:00', 600, 12, 1300, 'Troca de turno durante a embalagem'),
    (2, '2026-11-10 17:00:00', 400, 4, 850, NULL),
    (3, '2026-11-19 13:40:00', 50, 1, 330, NULL),
    (4, '2026-11-24 11:00:00', 20, 0, 215, 'Cabine com fila de espera');

INSERT INTO movimentacao_estoque (produto_id, tipo, quantidade, motivo) VALUES
    (1, 'ENTRADA', 100, 'Estoque inicial'),
    (2, 'ENTRADA', 15, 'Estoque inicial'),
    (6, 'ENTRADA', 12, 'Estoque inicial'),
    (8, 'ENTRADA', 200, 'Estoque inicial'),
    (9, 'ENTRADA', 200, 'Estoque inicial');

INSERT INTO usuario (nome, email, senha_hash, perfil) VALUES
    ('Administrador', 'admin@fabricx.com', 'PBKDF2-SHA256$210000$fy/kB5knVfpQGkC7bnQ3Qg==$FNuZmsJsbGz/H//oZFm8QMzlirp0z+1K36xi35MJdtI=', 'ADMINISTRADOR'),
    ('Operador', 'operador@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR'),
    ('Maria Souza', 'maria@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR'),
    ('Carlos Lima', 'carlos@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR');

UPDATE usuario SET ativo = 0 WHERE email = 'carlos@fabricx.com';

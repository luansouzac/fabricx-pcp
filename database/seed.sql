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

INSERT INTO produto (codigo, nome, descricao, tipo, unidade_medida, estoque_atual, estoque_minimo, custo_unitario) VALUES
    ('MP-006', 'Tubo de aço 20mm', 'Tubo industrial de aço carbono', 'MATERIA_PRIMA', 'M', 300, 100, 9.80),
    ('MP-007', 'Porca M6', NULL, 'MATERIA_PRIMA', 'UN', 500, 300, 0.12),
    ('MP-008', 'MDF 15mm', 'Chapa de MDF cru', 'MATERIA_PRIMA', 'M2', 80, 30, 65.00),
    ('MP-009', 'Verniz', 'Verniz poliuretano', 'MATERIA_PRIMA', 'L', 25, 10, 34.00),
    ('MP-010', 'Dobradiça', NULL, 'MATERIA_PRIMA', 'UN', 40, 60, 3.50),
    ('MP-011', 'Puxador', NULL, 'MATERIA_PRIMA', 'UN', 120, 40, 5.20),
    ('MP-012', 'Cola PVA', 'Cola para madeira', 'MATERIA_PRIMA', 'L', 4, 5, 18.00),
    ('MP-013', 'Rodízio', 'Rodízio giratório com freio', 'MATERIA_PRIMA', 'UN', 60, 20, 8.90),
    ('MP-014', 'Tinta azul', 'Linha descontinuada', 'MATERIA_PRIMA', 'L', 0, 0, 26.00),
    ('EP-004', 'Painel de MDF cortado', NULL, 'EM_PROCESSO', 'UN', 30, 10, 38.00),
    ('EP-005', 'Porta montada', 'Porta com dobradiças e puxador', 'EM_PROCESSO', 'UN', 8, 5, 62.00),
    ('EP-006', 'Estrutura de tubo soldada', NULL, 'EM_PROCESSO', 'UN', 14, 6, 71.00),
    ('PA-004', 'Armário de escritório', 'Armário de MDF com duas portas', 'ACABADO', 'UN', 6, 4, 340.00),
    ('PA-005', 'Banco metálico', 'Banco de tubo com assento de MDF', 'ACABADO', 'UN', 10, 5, 150.00),
    ('PA-006', 'Carrinho de apoio', 'Carrinho com rodízios', 'ACABADO', 'UN', 2, 4, 210.00);

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

INSERT INTO item_estrutura (produto_pai_id, componente_id, quantidade) VALUES
    (21, 14, 1.5),
    (22, 21, 1),
    (22, 16, 2),
    (22, 17, 1),
    (22, 15, 0.3),
    (23, 12, 6),
    (23, 13, 8),
    (24, 22, 2),
    (24, 21, 3),
    (24, 2, 24),
    (24, 18, 0.2),
    (25, 23, 1),
    (25, 21, 1),
    (25, 3, 0.4),
    (25, 2, 4),
    (26, 23, 1),
    (26, 19, 4),
    (26, 21, 1),
    (26, 3, 0.3);

INSERT INTO recurso (nome, tipo, capacidade_horas_dia) VALUES
    ('Cortadora laser', 'MAQUINA', 8),
    ('Cabine de pintura', 'MAQUINA', 8),
    ('Operador João', 'OPERADOR', 8),
    ('Linha de montagem 1', 'LINHA', 16),
    ('Montadora', 'MAQUINA', 8),
    ('Linha de embalagem', 'LINHA', 8);

INSERT INTO recurso (nome, tipo, capacidade_horas_dia) VALUES
    ('Serra de esquadria', 'MAQUINA', 8),
    ('Estação de solda', 'MAQUINA', 8),
    ('Operadora Ana', 'OPERADOR', 8),
    ('Bancada de montagem', 'LINHA', 16),
    ('Estufa de secagem', 'MAQUINA', 8),
    ('Operador Pedro', 'OPERADOR', 8),
    ('Prensa antiga', 'MAQUINA', 8);

INSERT INTO roteiro_etapa (produto_id, recurso_id, sequencia, nome, tempo_ciclo_min, unidades_por_ciclo) VALUES
    (10, 5, 1, 'Montagem', 2, 10),
    (11, 6, 1, 'Embalagem', 2, 1),
    (6, 1, 1, 'Corte', 6, 1),
    (6, 2, 2, 'Pintura', 10, 1),
    (6, 4, 3, 'Montagem', 15, 1);

INSERT INTO roteiro_etapa (produto_id, recurso_id, sequencia, nome, tempo_ciclo_min, unidades_por_ciclo) VALUES
    (24, 7, 1, 'Corte do MDF', 20, 1),
    (24, 11, 2, 'Verniz e secagem', 30, 1),
    (24, 10, 3, 'Montagem', 45, 1),
    (25, 8, 1, 'Soldagem', 25, 1),
    (25, 2, 2, 'Pintura', 12, 1),
    (25, 10, 3, 'Montagem', 10, 1),
    (26, 8, 1, 'Soldagem', 20, 1),
    (26, 10, 2, 'Montagem', 18, 1),
    (22, 9, 1, 'Montagem da porta', 8, 1);

INSERT INTO demanda_prevista (produto_id, periodo, quantidade) VALUES
    (10, '2026-10', 3000),
    (11, '2026-10', 5000),
    (6, '2026-10', 700),
    (10, '2026-11', 3000),
    (11, '2026-11', 7500),
    (6, '2026-11', 900),
    (10, '2026-12', 600),
    (11, '2026-12', 8000);

INSERT INTO demanda_prevista (produto_id, periodo, quantidade) VALUES
    (24, '2026-10', 120),
    (24, '2026-11', 150),
    (24, '2026-12', 180),
    (25, '2026-10', 200),
    (25, '2026-11', 260),
    (25, '2026-12', 300),
    (26, '2026-11', 90),
    (26, '2026-12', 140);

INSERT INTO ordem_producao (numero, produto_id, quantidade, prazo_entrega, prioridade, status, criada_em, concluida_em) VALUES
    ('OP-2026-0001', 10, 300, '2026-11-10', 'ALTA', 'CONCLUIDA', '2026-11-02 09:00:00', '2026-11-09 16:00:00'),
    ('OP-2026-0002', 11, 1000, '2026-11-12', 'MEDIA', 'CONCLUIDA', '2026-11-03 09:00:00', '2026-11-14 11:00:00'),
    ('OP-2026-0003', 6, 50, '2026-12-05', 'ALTA', 'EM_PRODUCAO', '2026-11-18 09:00:00', NULL),
    ('OP-2026-0004', 10, 600, '2026-12-15', 'MEDIA', 'PROGRAMADA', '2026-11-20 09:00:00', NULL),
    ('OP-2026-0005', 11, 2000, '2026-12-20', 'BAIXA', 'PLANEJADA', '2026-11-22 09:00:00', NULL),
    ('OP-2026-0006', 7, 20, '2026-11-30', 'URGENTE', 'CANCELADA', '2026-11-15 09:00:00', NULL);

INSERT INTO ordem_producao (numero, produto_id, quantidade, prazo_entrega, prioridade, status, criada_em, concluida_em) VALUES
    ('OP-2026-0007', 24, 40, '2026-12-10', 'ALTA', 'EM_PRODUCAO', '2026-11-20 09:00:00', NULL),
    ('OP-2026-0008', 25, 60, '2026-12-12', 'MEDIA', 'PROGRAMADA', '2026-11-21 09:00:00', NULL),
    ('OP-2026-0009', 26, 30, '2026-11-28', 'URGENTE', 'EM_PRODUCAO', '2026-11-19 09:00:00', NULL),
    ('OP-2026-0010', 24, 25, '2026-11-12', 'MEDIA', 'CONCLUIDA', '2026-11-04 09:00:00', '2026-11-14 15:00:00'),
    ('OP-2026-0011', 25, 80, '2026-11-20', 'ALTA', 'CONCLUIDA', '2026-11-05 09:00:00', '2026-11-19 17:30:00'),
    ('OP-2026-0012', 26, 50, '2027-01-10', 'BAIXA', 'PLANEJADA', '2026-11-24 09:00:00', NULL),
    ('OP-2026-0013', 24, 100, '2026-12-22', 'URGENTE', 'PLANEJADA', '2026-11-24 10:00:00', NULL),
    ('OP-2026-0014', 25, 15, '2026-11-25', 'BAIXA', 'CANCELADA', '2026-11-12 09:00:00', NULL);

INSERT INTO programacao (ordem_id, etapa_id, recurso_id, inicio_previsto, fim_previsto, status) VALUES
    (1, 1, 5, '2026-11-03 08:00:00', '2026-11-03 09:30:00', 'CONCLUIDA'),
    (2, 2, 6, '2026-11-04 08:00:00', '2026-11-11 08:00:00', 'CONCLUIDA'),
    (3, 3, 1, '2026-11-19 08:00:00', '2026-11-19 14:00:00', 'CONCLUIDA'),
    (3, 4, 2, '2026-11-24 08:00:00', '2026-11-25 08:00:00', 'EM_ANDAMENTO'),
    (3, 5, 4, '2026-11-26 08:00:00', '2026-11-27 08:00:00', 'AGENDADA'),
    (4, 1, 5, '2026-12-01 08:00:00', '2026-12-01 20:00:00', 'AGENDADA');

INSERT INTO programacao (ordem_id, etapa_id, recurso_id, inicio_previsto, fim_previsto, status) VALUES
    (7, 6, 7, '2026-11-23 08:00:00', '2026-11-24 14:00:00', 'CONCLUIDA'),
    (7, 7, 11, '2026-11-25 08:00:00', '2026-11-27 12:00:00', 'EM_ANDAMENTO'),
    (7, 8, 10, '2026-11-30 08:00:00', '2026-12-03 14:00:00', 'AGENDADA'),
    (8, 9, 8, '2026-11-25 08:00:00', '2026-11-27 10:00:00', 'AGENDADA'),
    (8, 10, 2, '2026-11-30 08:00:00', '2026-12-01 12:00:00', 'AGENDADA'),
    (8, 11, 10, '2026-12-04 08:00:00', '2026-12-05 10:00:00', 'AGENDADA'),
    (9, 12, 8, '2026-11-24 08:00:00', '2026-11-25 12:00:00', 'EM_ANDAMENTO'),
    (9, 13, 10, '2026-11-27 08:00:00', '2026-11-27 17:00:00', 'AGENDADA'),
    (10, 6, 7, '2026-11-05 08:00:00', '2026-11-06 12:00:00', 'CONCLUIDA'),
    (10, 7, 11, '2026-11-09 08:00:00', '2026-11-10 14:00:00', 'CONCLUIDA'),
    (10, 8, 10, '2026-11-11 08:00:00', '2026-11-14 15:00:00', 'CONCLUIDA'),
    (11, 9, 8, '2026-11-06 08:00:00', '2026-11-12 10:00:00', 'CONCLUIDA'),
    (11, 10, 2, '2026-11-13 08:00:00', '2026-11-17 10:00:00', 'CONCLUIDA'),
    (11, 11, 10, '2026-11-17 08:00:00', '2026-11-19 17:30:00', 'CONCLUIDA');

INSERT INTO apontamento (programacao_id, data_hora, qtd_produzida, qtd_defeituosa, tempo_gasto_min, observacao) VALUES
    (1, '2026-11-03 09:20:00', 300, 6, 65, NULL),
    (2, '2026-11-06 17:00:00', 600, 12, 1300, 'Troca de turno durante a embalagem'),
    (2, '2026-11-10 17:00:00', 400, 4, 850, NULL),
    (3, '2026-11-19 13:40:00', 50, 1, 330, NULL),
    (4, '2026-11-24 11:00:00', 20, 0, 215, 'Cabine com fila de espera');

INSERT INTO apontamento (programacao_id, data_hora, qtd_produzida, qtd_defeituosa, tempo_gasto_min, observacao) VALUES
    (7, '2026-11-24 14:10:00', 40, 1, 810, 'Troca de lâmina da serra'),
    (13, '2026-11-24 17:00:00', 12, 1, 480, 'Eletrodo trocado'),
    (15, '2026-11-06 12:10:00', 25, 0, 505, NULL),
    (16, '2026-11-10 14:10:00', 25, 2, 760, 'Verniz com bolhas em duas peças'),
    (17, '2026-11-14 15:10:00', 25, 0, 1130, 'Atraso por falta de dobradiça'),
    (18, '2026-11-12 10:10:00', 80, 3, 2010, NULL),
    (19, '2026-11-17 10:10:00', 80, 5, 970, 'Retrabalho de pintura'),
    (20, '2026-11-19 17:40:00', 80, 0, 805, NULL);

INSERT INTO movimentacao_estoque (produto_id, tipo, quantidade, motivo) VALUES
    (1, 'ENTRADA', 100, 'Estoque inicial'),
    (2, 'ENTRADA', 15, 'Estoque inicial'),
    (6, 'ENTRADA', 12, 'Estoque inicial'),
    (8, 'ENTRADA', 200, 'Estoque inicial'),
    (9, 'ENTRADA', 200, 'Estoque inicial');

INSERT INTO movimentacao_estoque (produto_id, ordem_id, tipo, quantidade, motivo, data_hora) VALUES
    (12, NULL, 'ENTRADA', 300, 'Estoque inicial', '2026-11-01 08:00:00'),
    (13, NULL, 'ENTRADA', 500, 'Estoque inicial', '2026-11-01 08:00:00'),
    (14, NULL, 'ENTRADA', 120, 'Estoque inicial', '2026-11-01 08:00:00'),
    (15, NULL, 'ENTRADA', 25, 'Estoque inicial', '2026-11-01 08:00:00'),
    (16, NULL, 'ENTRADA', 100, 'Estoque inicial', '2026-11-01 08:00:00'),
    (17, NULL, 'ENTRADA', 120, 'Estoque inicial', '2026-11-01 08:00:00'),
    (18, NULL, 'ENTRADA', 8, 'Estoque inicial', '2026-11-01 08:00:00'),
    (19, NULL, 'ENTRADA', 60, 'Estoque inicial', '2026-11-01 08:00:00'),
    (14, 10, 'SAIDA', 40, 'Consumo na OP-2026-0010', '2026-11-05 08:00:00'),
    (16, 10, 'SAIDA', 50, 'Consumo na OP-2026-0010', '2026-11-11 08:00:00'),
    (24, 10, 'ENTRADA', 25, 'Produção concluída da OP-2026-0010', '2026-11-14 15:00:00'),
    (12, 11, 'SAIDA', 130, 'Consumo na OP-2026-0011', '2026-11-06 08:00:00'),
    (25, 11, 'ENTRADA', 80, 'Produção concluída da OP-2026-0011', '2026-11-19 17:30:00'),
    (18, NULL, 'SAIDA', 4, 'Uso na marcenaria', '2026-11-20 10:00:00');

INSERT INTO usuario (nome, email, senha_hash, perfil) VALUES
    ('Administrador', 'admin@fabricx.com', 'PBKDF2-SHA256$210000$fy/kB5knVfpQGkC7bnQ3Qg==$FNuZmsJsbGz/H//oZFm8QMzlirp0z+1K36xi35MJdtI=', 'ADMINISTRADOR'),
    ('Operador', 'operador@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR'),
    ('Maria Souza', 'maria@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR'),
    ('Carlos Lima', 'carlos@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR');

INSERT INTO usuario (nome, email, senha_hash, perfil) VALUES
    ('Ana Pereira', 'ana@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR'),
    ('Pedro Alves', 'pedro@fabricx.com', 'PBKDF2-SHA256$210000$tDXJoPEDDpLfnLpwF8FSOQ==$dGNu1vgXQfSaLiAT7J/ft8QxUjxFIGsnZSi5KAhAwfQ=', 'OPERADOR'),
    ('Gerente de Produção', 'gerente@fabricx.com', 'PBKDF2-SHA256$210000$fy/kB5knVfpQGkC7bnQ3Qg==$FNuZmsJsbGz/H//oZFm8QMzlirp0z+1K36xi35MJdtI=', 'ADMINISTRADOR');

UPDATE usuario SET ativo = 0 WHERE email = 'carlos@fabricx.com';
UPDATE produto SET ativo = 0 WHERE codigo = 'MP-014';
UPDATE recurso SET ativo = 0 WHERE nome = 'Prensa antiga';

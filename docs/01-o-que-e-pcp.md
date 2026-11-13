# 01 — O que é um sistema de PCP

**PCP = Planejamento e Controle da Produção.** É o sistema que uma fábrica usa para responder: *o que produzir, quanto, quando, com quê e em qual máquina*, e depois acompanhar se deu certo.

## Exemplo

Uma fábrica de móveis recebeu um pedido de 100 mesas para daqui a 20 dias.

1. **Ordem de produção:** registra-se "fabricar 100 mesas, prazo dia 30".
2. **Insumos:** cada mesa usa 4 pés, 1 tampo e 16 parafusos. O sistema confere o estoque e avisa o que falta comprar.
3. **Programação:** a ordem passa por etapas (corte, montagem, pintura). Cada etapa usa um recurso (máquina ou operador) com capacidade limitada. O sistema monta o cronograma, por exemplo: corte de 1 a 5, montagem de 6 a 12, pintura de 13 a 18.
4. **Execução e acompanhamento:** os operadores registram o que foi feito (50 mesas prontas, 3 com defeito). O sistema mostra se está no prazo.
5. **Indicadores:** eficiência, produtividade e taxa de defeitos.
6. **Capacidade:** com a demanda prevista, o sistema mostra o gargalo (a pintura só dá conta de 10 mesas por dia) e a ociosidade (máquina parada).

## Conceitos principais

| Conceito | O que guarda | Exemplo |
|---|---|---|
| **Produto** | O que entra e sai da fábrica (matéria-prima, em processo ou acabado) | Mesa, madeira, parafuso |
| **Estrutura do produto (BOM)** | De que componentes um produto é feito | 1 mesa = 4 pés + 1 tampo |
| **Roteiro** | Etapas para fabricar um produto e o recurso de cada uma | Corte → Montagem → Pintura |
| **Recurso** | Máquina ou pessoa com capacidade limitada | Serra, cabine de pintura |
| **Ordem de produção** | Pedido interno de fabricação | 100 mesas até dia 30 |
| **Programação** | Qual ordem usa qual recurso e quando | Pintura de 13 a 18 |
| **Apontamento** | O que foi realmente produzido | 50 prontas, 3 defeituosas |
| **Estoque** | Saldo e movimentações dos produtos | 400 pés em estoque |
| **Demanda prevista / capacidade** | Quanto se espera vender × quanto dá para produzir | 200 mesas em dezembro |

## Em termos de sistema

- **Cadastros:** produtos, recursos, estruturas e roteiros.
- **Fluxos com status:** a ordem passa por *Planejada → Programada → Em produção → Concluída* (ou *Cancelada*).
- **Regras:** não consumir mais estoque do que existe; alertar quando o estoque cai abaixo do mínimo; não alocar dois trabalhos no mesmo recurso ao mesmo tempo.
- **Cálculos e relatórios:** eficiência, produtividade, qualidade, gargalos e ociosidade.

Para o trabalho não é preciso algoritmo de otimização: basta programar ordem, etapas, recurso e datas, validando conflitos.

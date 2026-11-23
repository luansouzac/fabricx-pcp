# Iniciação

Fase do Processo Unificado que define o escopo, confere a viabilidade e levanta os requisitos e riscos. O que é PCP está em [../01-o-que-e-pcp.md](../01-o-que-e-pcp.md).

## Escopo

O FabricX é uma API de PCP (Planejamento e Controle da Produção) para uma fábrica pequena. Ele responde: o que produzir, quanto, quando, com quê e em qual recurso, e depois acompanha o que foi feito.

Faz:

- cadastro de produtos (matéria-prima, em processo e acabado), com estoque, histórico de movimentações e alerta de reposição;
- estrutura de produto (BOM) em vários níveis, com validação de ciclo;
- roteiro de fabricação com etapas, recurso e paralelismo;
- ordens de produção com fluxo de status ligado ao estoque;
- programação das etapas em recursos com capacidade;
- apontamento da produção real e indicadores de eficiência, produtividade e qualidade;
- planejamento de capacidade (demanda prevista contra capacidade) com gargalo e ociosidade;
- integração simulada com ERP/SGQ (desejável).

Não faz: otimização automática da programação, controle de custos, compras, controle de turnos e calendário de feriados, autenticação, reserva de material entre ordens.

Quem usa (stakeholders): o planejador da produção (ordens e programação), o supervisor de chão de fábrica (apontamentos), o almoxarife (estoque) e a gerência (indicadores e capacidade). O professor da disciplina valida o resultado como avaliador.

Entrega: somente a API, com o Swagger UI como interface de uso (`/swagger`). O front-end Vue.js é o trabalho de Web III e consome esta mesma API, em outro projeto.

## Viabilidade

- **Técnica:** C# / .NET 10, EF Core e SQLite, todos gratuitos e conhecidos pela equipe. O banco é trocável porque só a Infrastructure conhece o provider.
- **Prazo:** entrega em 25/11/2026, com a equipe de três pessoas dividida entre o backend (ETS) e o front (Web III). Por isso o escopo tem itens opcionais (ERP/SGQ e capacidade).
- **Custo:** nenhum. Roda em Docker (`docker-compose.yml`) ou direto com `dotnet run`.

## Requisitos funcionais

Prioridade MoSCoW: M (must), S (should), C (could).

| Código | Requisito | Prioridade |
|---|---|---|
| RF-01 | Cadastrar, listar, atualizar e inativar produtos, com filtro por tipo | M |
| RF-02 | Registrar entradas e saídas de estoque com motivo e consultar o histórico | M |
| RF-03 | Alertar produtos com estoque abaixo do mínimo | M |
| RF-04 | Cadastrar a estrutura do produto (componentes e quantidades) recusando ciclos | M |
| RF-05 | Cadastrar recursos com capacidade em horas por dia, ativando e inativando | M |
| RF-06 | Cadastrar o roteiro de fabricação com etapas, recurso, tempo de ciclo e unidades por ciclo | M |
| RF-07 | Criar ordens de produção (produto, quantidade, prazo e prioridade) com número gerado | M |
| RF-08 | Recusar a criação da ordem quando faltar saldo de componente | M |
| RF-09 | Avançar o status da ordem (Planejada, Programada, Em produção, Concluída) e cancelar antes da produção | M |
| RF-10 | Baixar os componentes ao iniciar a ordem e dar entrada no acabado ao concluir | M |
| RF-11 | Programar as etapas de uma ordem em recursos, com início informado, fim calculado e sem sobreposição | M |
| RF-12 | Apontar produção real (quantidade boa, defeituosa e tempo gasto) | M |
| RF-13 | Calcular indicadores de eficiência, produtividade, qualidade e cumprimento de prazo | M |
| RF-14 | Planejar capacidade: demanda prevista contra capacidade, com gargalo e ociosidade | S |
| RF-15 | Integrar com ERP/SGQ por interface, com implementação simulada | C |

## Requisitos não funcionais

| Código | Requisito |
|---|---|
| RNF-01 | Arquitetura Limpa em quatro projetos (Domain, Application, Infrastructure, Api) com dependências de fora para dentro |
| RNF-02 | Toda resposta HTTP usa o envelope `WebResponse<T>`, inclusive os erros |
| RNF-03 | Erro de regra de negócio devolve 400 e recurso inexistente 404 |
| RNF-04 | Persistência em SQLite criada a partir de `database/script.sql`, com banco trocável |
| RNF-05 | Operações que mexem em mais de uma tabela são atômicas (tudo ou nada) |
| RNF-06 | Código testado com xUnit em quatro níveis: Domain, Service, repositório e API |
| RNF-07 | API documentada em OpenAPI e usável pelo Swagger UI em Development |
| RNF-08 | Execução por contêiner Docker com volume para o arquivo do banco |

## Casos de uso principais

| Código | Caso de uso | Ator |
|---|---|---|
| UC-01 | Manter produtos e estoque (cadastrar, movimentar, consultar histórico) | Almoxarife |
| UC-02 | Manter a estrutura do produto | Planejador |
| UC-03 | Manter recursos e roteiros | Planejador |
| UC-04 | Criar e acompanhar ordem de produção | Planejador |
| UC-05 | Programar a ordem nos recursos | Planejador |
| UC-06 | Iniciar e concluir a ordem | Supervisor |
| UC-07 | Apontar a produção | Supervisor |
| UC-08 | Consultar indicadores e capacidade | Gerência |

## Riscos

| Risco | Probabilidade | Impacto | Resposta |
|---|---|---|---|
| Prazo curto (6 dias) para todos os módulos | Alta | Alto | Itens opcionais ficam por último; cadeias de tarefas independentes entre os dois desenvolvedores do backend |
| Banco de dados e DDL mudarem no meio do trabalho | Média | Médio | DDL e DER na issue #10, feita no início; testes de repositório rodam contra o `script.sql` real |
| Ordens concorrentes consumirem o mesmo material | Média | Baixo | Sem reserva de material: aceito como limitação e documentado na Transição |
| Programação com sobreposição de recurso | Média | Alto | Regra de não sobreposição no Service, com teste |
| Divergência entre documentação e código | Média | Médio | Revisão final cruzada na issue #17 |

## Plano inicial

Duas entregas na mesma base de código. ETS I (esta documentação e a API) em 25/11/2026 e Web III (front Vue.js) em 10/12/2026. As tarefas estão como issues no GitHub, agrupadas em dois épicos (#5 e #6) com sub-issues, e o plano de iterações está em [02-elaboracao.md](02-elaboracao.md).

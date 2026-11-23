# Transição

Como colocar o sistema para rodar, como validar a entrega e quais são as limitações conhecidas.

## Como rodar

Com .NET 10:

```
dotnet run --project src/FabricX.Api --launch-profile http
```

Com Docker:

```
docker compose up --build
```

O banco SQLite é criado a partir de `database/script.sql` na primeira execução. Em Development o `database/seed.sql` carrega dados de exemplo. A API fica em `http://localhost:5168` e o Swagger UI em `http://localhost:5168/swagger` (só em Development). No Docker o ambiente também é Development (seed e Swagger ativos) e o arquivo do banco fica em volume.

## Teste de aceitação: fluxo da garrafa d'água

Passo a passo no Swagger UI:

1. Cadastrar os produtos Tampinha (matéria-prima), Garrafa (matéria-prima) e Garrafa d'água (acabado) em `POST /api/produtos`.
2. Dar entrada de 100 unidades de tampinha e de garrafa em `POST /api/produtos/{id}/movimentacoes` (`tipo: Entrada`).
3. Montar a estrutura da garrafa d'água em `POST /api/produtos/{id}/estrutura`: 1 tampinha e 1 garrafa.
4. Criar a ordem de 60 garrafas d'água em `POST /api/ordens-producao`. Com saldo menor que 60 de qualquer componente a API responde 400 com "Estoque insuficiente de X".
5. Avançar três vezes em `POST /api/ordens-producao/{id}/avancar`: Programada, Em produção e Concluída.
6. Conferir os saldos em `GET /api/produtos`: tampinha e garrafa em 40, garrafa d'água em 60.
7. Conferir o histórico em `GET /api/produtos/{id}/movimentacoes`: as saídas e a entrada ligadas à ordem.
8. Tentar cancelar uma ordem em produção em `POST /api/ordens-producao/{id}/cancelar`: responde 400.

O mesmo fluxo é executado de forma automatizada em `tests/FabricX.Tests/Api/ProdutosControllerTests.cs`.

## Teste de aceitação: programação e desempenho

Com o seed carregado (garrafa d'água na Montadora, kit na Linha de embalagem):

1. Criar uma ordem de 60 garrafas d'água (`EP-003`) e programar a etapa em `POST /api/ordens-producao/{id}/programacao`. O fim volta calculado: 6 ciclos de 2 min, ou seja 12 minutos.
2. Programar uma segunda ordem no mesmo horário da Montadora: a API responde 400 com "já está ocupado".
3. Apontar a produção em `POST /api/programacao/{id}/apontamentos`. Ao somar 60 unidades produzidas a programação fica concluída.
4. Consultar `GET /api/indicadores` e `GET /api/indicadores/resumo`.
5. Consultar `GET /api/capacidade?periodo=2026-12`: a Montadora aparece ociosa e a Linha de embalagem em gargalo, com a demanda do seed.

## Limitações conhecidas

| Limitação | Efeito |
|---|---|
| Sem reserva de material entre ordens | Duas ordens criadas hoje podem passar na validação de saldo e só a primeira conseguir iniciar |
| Só componentes diretos são validados | Quem planeja produz primeiro o nível de baixo (garrafa d'água antes do kit); não há explosão da estrutura em vários níveis |
| Sem controle de sobreposição entre níveis | A programação do kit não considera o prazo da garrafa d'água |
| Sem setup de máquina nem calendário | Só a capacidade diária do recurso; não há feriados, turnos ou tempo de troca |
| Ordem em produção não pode ser cancelada | O material já foi consumido e o sistema não controla devolução |
| Sem autenticação | A API é aberta; adequado ao escopo acadêmico |
| Estoque não reserva nem aceita saldo negativo | Decisão de simplicidade |
| Integração com ERP/SGQ simulada | Não há sistema externo real |

## Entrega

Entregáveis do ETS no SIGAA até 25/11/2026 23:59: script DDL, documentação (esta pasta), projeto da API, DER em imagem ou PDF com o arquivo do diagrama e o link do repositório. A versão entregue é a tag `ets-entrega`; depois dela o ETS não é alterado.

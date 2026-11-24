---
name: fabricx-ui
description: >
  Identidade visual e regras de UI do front-end do FabricX (Vue 3 + Vuetify 3, em
  `web/`). USE SEMPRE ao criar ou alterar tela, tabela, formulário, menu, diálogo,
  toast, cor, ícone, logo ou mensagem ao usuário do FabricX.
---

# FabricX: identidade visual e UI

Sistema de PCP para planejadores e operadores de fábrica. Tom: profissional, sóbrio e claro. Visual minimalista, pensado para dados densos (tabelas e formulários).

## Arquivos da identidade

| Arquivo | Papel |
|---|---|
| `web/src/plugins/fabricx-theme.js` | Temas Vuetify (`fabricxTheme` claro e `fabricxDarkTheme` escuro) e padrões de componentes (`fabricxDefaults`: tabela compacta com hover, cards sem sombra, campos outlined compactos, botão flat sem caixa alta) |
| `web/src/styles/fabricx.css` | Fontes, classes de título, cabeçalho de tabela, cor dos links e estilo do item ativo do menu |
| `web/src/styles/tabela.css` | Zebra da tabela e classes `estoque-baixo`, `estoque-bom` e `linha-estoque-baixo`, só com tokens do tema |
| `web/src/assets/logo/fabricx-horizontal-claro.svg` | Logo horizontal, fundo claro (menu lateral) |
| `web/src/assets/logo/fabricx-horizontal-escuro.svg` | Logo horizontal, fundo escuro (usado no tema escuro) |
| `web/src/assets/logo/fabricx-simbolo-claro.svg` | Só o símbolo, fundo claro (menu recolhido). Cópia em `web/public/favicon.svg` é o favicon |
| `web/src/assets/logo/fabricx-simbolo-escuro.svg` | Só o símbolo, fundo escuro |

O `web/src/main.js` já registra os dois temas (`defaultTheme: localStorage.getItem('fx-tema') || 'fabricx'`, `themes: { fabricx: fabricxTheme, fabricxDark: fabricxDarkTheme }`), `defaults: fabricxDefaults` e importa `fabricx.css` e `tabela.css`. Sem alias `@/`: use caminhos relativos. Não existe cor fixa no projeto: toda cor vem de token do tema.

## Paleta

| Token | Hex | Uso |
|---|---|---|
| `primary` | `#4F6BED` | Botão principal, item ativo do menu, foco |
| `primary-darken-1` | `#4057C5` | **Texto e links** na cor da marca |
| `primary-lighten-4` | `#EDF0FD` | Fundo do item ativo do menu, linha selecionada |
| `secondary` | `#3A4250` | Ações secundárias, status neutro |
| `background` | `#F6F7F9` | Fundo da aplicação |
| `surface` | `#FFFFFF` | Cards, tabelas, menu lateral |
| `surface-variant` | `#EEF0F3` | Cabeçalho de tabela |
| `on-surface` | `#1B1F24` | Texto principal |
| `text-muted` | `#5E6470` | Legendas e texto auxiliar |
| `success` | `#2E7D32` | Estoque bom, concluído |
| `warning` | `#B23E00` | Estoque baixo |
| `error` | `#C62828` | Erro, cancelado |
| `info` | `#0B6BCB` | Em andamento, avisos |

Tons da primária: lighten-3 `#D3DAFB`, lighten-2 `#B0BCF7`, lighten-1 `#8497F2`, darken-2 `#32439D`, darken-3 `#232F75`.

### Contraste (WCAG AA), obrigatório

- `primary` não serve como texto pequeno sobre o fundo (4,21:1). Texto e links usam `primary-darken-1`.
- Botão `primary` com texto branco passa.
- Status em `v-chip` sempre `variant="flat"`, nunca `tonal`.
- Não usar `#E65100`, nem laranja claro, como texto. O laranja do tema é o `warning`.
- Não inventar cores. Se faltar uma, use um tom existente.
- Nada de hex solto em `.vue` e `.css`: use `color="<token>"` ou `rgb(var(--v-theme-<token>))`.

## Tema escuro

Nomes dos temas: `fabricx` (claro) e `fabricxDark` (escuro). A escolha fica em `localStorage` na chave `fx-tema`. O botão de alternar fica na barra superior do `MainLayout.vue` (`theme.change(...)` e `useTheme()`), com `mdi-weather-night` no claro e `mdi-weather-sunny` no escuro, sempre com `aria-label`.

| Token | Claro | Escuro |
|---|---|---|
| `background` | `#F6F7F9` | `#14171C` |
| `surface` | `#FFFFFF` | `#1C2027` |
| `surface-variant` | `#EEF0F3` | `#262B33` |
| `on-surface` | `#1B1F24` | `#E6E8EB` |
| `text-muted` | `#5E6470` | `#9BA2AE` |
| `primary` | `#4F6BED` | `#8497F2` |
| `primary-darken-1` (texto da marca) | `#4057C5` | `#A9B6F6` |
| `primary-lighten-4` (fundo do ativo) | `#EDF0FD` | `#2A3366` |
| `primary-darken-2` (texto do ativo) | `#32439D` | `#C9D1FA` |
| `secondary` | `#3A4250` | `#AEB5C1` |
| `success` | `#2E7D32` | `#66BB6A` |
| `warning` | `#B23E00` | `#FF9E5E` |
| `error` | `#C62828` | `#F26B6B` |
| `info` | `#0B6BCB` | `#5AA9F5` |

Regras:

- Os tokens guardam o **papel**, não o tom. `primary-darken-1` é sempre "texto na cor da marca" e `primary-lighten-4` é sempre "fundo do item ativo", mesmo que no escuro sejam outros tons. Escolha pelo papel, nunca pelo nome `darken` ou `lighten`. Assim nenhum componente precisa de `if` por tema.
- No escuro, botões e chips de cor cheia têm texto escuro (`on-primary`, `on-success` etc. = `#14171C`). Nunca force `color: white` em botão ou chip.
- Todas as combinações passam no AA.
- Logo do menu: `fabricx-horizontal-escuro.svg` quando `theme.current.value.dark`, senão o claro.
- Hex solto e `white` quebram o tema escuro: só tokens.

## Tipografia

- **Outfit** 500 nos títulos, **Inter** no resto (importadas em `fabricx.css`).
- Título da página `fx-titulo-pagina`, título de seção `fx-titulo-secao`, legenda `fx-legenda`.
- Corpo 14 px. Tabela 13 px, com números tabulares. Cabeçalho de tabela 12 px, peso 600, fundo `surface-variant`.

## Ícones

Só Material Design Icons (`@mdi/font`), de preferência a variante `-outline`. Tamanho 20 no menu e nos botões, 18 nas tabelas. Cor `secondary`, e `primary` só no item ativo. Botão só de ícone leva `aria-label`.

| Seção | Ícone |
|---|---|
| Início | `mdi-view-dashboard-outline` |
| Produtos | `mdi-package-variant-closed` |
| Estoque | `mdi-warehouse` |
| Recursos | `mdi-robot-industrial-outline` |
| Ordens de produção | `mdi-clipboard-text-outline` |
| Programação | `mdi-calendar-clock-outline` |
| Capacidade | `mdi-gauge-full` |
| Indicadores | `mdi-chart-line` |

## Status e estoque

Os nomes vêm da API (enums em texto). Exiba com rótulo em português e a cor do mapa:

| Status da ordem | Rótulo | Cor |
|---|---|---|
| `Planejada` | Planejada | `secondary` |
| `Programada` | Programada | `primary` |
| `EmProducao` | Em produção | `info` |
| `Concluida` | Concluída | `success` |
| `Cancelada` | Cancelada | `error` |

Estoque: acima do mínimo, número em `success`; abaixo do mínimo, em `warning`; sempre em negrito. Produto ativo `success`, inativo `grey`.

## Layout e componentes

- Menu lateral claro com borda à direita e o logo horizontal centralizado no topo (altura 32 px). Item ativo com fundo `primary-lighten-4` e texto `primary-darken-2`.
- Cards sem sombra, com borda e `rounded="lg"`.
- Tabelas `density="compact"`, com busca e filtros numa barra acima, dentro do mesmo card. **Cabeçalho e conteúdo centralizados em todas as colunas** (`align: 'center'`).
- Botões com rótulo de verbo, sem caixa alta (`text-none`). Um único botão `primary` por tela. O tema não define cor padrão para `v-btn`: a ação principal leva `color="primary"` explícito e as demais `secondary` ou `error`.
- Formulários `variant="outlined"`, `density="compact"`, com validação por campo em português.
- Sem sombras fortes, degradês, imagens pesadas nem emoji.

## Tom de voz (pt-BR)

Direto (começa pelo fato), específico (cita item, quantidade e unidade), calmo (não culpa o usuário) e orientado à ação. Sem "Ops!", sem exclamação, sem emoji.

| Situação | Assim | Não assim |
|---|---|---|
| Sucesso | Produto criado com sucesso. | Uhuul! Deu tudo certo! |
| Atenção | Estoque de Chapa de aço 2mm abaixo do mínimo: 120 de 300 kg. | Cuidado, o estoque está acabando!! |
| Erro | Não foi possível salvar. A quantidade precisa ser maior que zero. | Ops! Algo deu errado. |
| Confirmação | Inativar o produto Chapa de aço? Ele deixará de receber movimentações. [Inativar] [Cancelar] | Tem certeza? [Sim] [Não] |
| Lista vazia | Nenhum produto encontrado. Crie um produto ou mude o filtro. | Nada por aqui :( |

Formatos: datas `dd/mm/aaaa`, hora em 24 h, números e moeda com `Intl.NumberFormat('pt-BR')` e datas com `Intl.DateTimeFormat('pt-BR')`.

## Checklist antes de terminar uma tela

- [ ] Só tokens do tema, sem hex solto nem `white`. Conferir a tela nos dois temas.
- [ ] Texto na cor da marca usa `primary-darken-1`.
- [ ] Status em `v-chip` `flat` com a cor do mapa.
- [ ] Tabela compacta e centralizada, com unidade nos números.
- [ ] Carregamento, erro e lista vazia tratados, com mensagens no tom de voz.
- [ ] Botões de ícone com `aria-label`.
- [ ] Um único botão `primary` na tela.
- [ ] Sem comentários de código, conforme o `CLAUDE.md`.

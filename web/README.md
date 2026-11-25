# FabricX Web

Front-end do FabricX em Vue 3, consumindo a API .NET. Trabalho de Desenvolvimento Web III.

## Tecnologias

- Vue 3 e Vite, em JavaScript
- Vuetify 3 (componentes e tema)
- Vue Router (cada tela tem endereço próprio)
- Pinia (estado)

## Requisitos

- Node.js 20.19 ou superior (ou 22.12+)
- API do FabricX rodando (ver o README da raiz do repositório)

## Como rodar

```bash
cd web
npm install
npm run dev
```

O front sobe em `http://localhost:5173` (ou na próxima porta livre, que o Vite mostra no terminal).

A URL da API vem da variável `VITE_API_URL`. Sem ela, o padrão é `http://localhost:5168`. Para mudar, copie `.env.example` para `.env` e edite.

Em Development a API libera CORS para qualquer origem, então não é preciso configurar nada para rodar localmente.

## Comandos

| Comando | O que faz |
|---|---|
| `npm run dev` | Servidor de desenvolvimento |
| `npm run build` | Gera a versão de produção em `dist/` |
| `npm run preview` | Serve a versão gerada |

## Estrutura

```
src/
  api/          cliente HTTP que lê o WebResponse<T> da API
  components/   toast, diálogos e histórico de movimentações
  layouts/      menu lateral e área das telas
  router/       rotas
  stores/       Pinia: notificações e produtos
  styles/       estilo compartilhado das tabelas
  utils/        nomes e formatos de produto
  views/        telas
```

## Como o feedback funciona

Toda chamada à API passa por `src/api/cliente.js`. Ele lê o envelope `WebResponse<T>` (`data`, `message`, `isSuccess`, `statusCode`):

- No sucesso, devolve `data` e mostra um toast verde com o `message` da API ou, se ela não enviar, com a mensagem informada pela operação.
- No erro, mostra um toast vermelho com o `message` da API.
- Se a API estiver fora do ar, mostra uma mensagem de conexão.

O toast é controlado pela store `notificacoes` e exibido por `components/NotificacaoToast.vue`.

## Telas

| Endereço | Tela |
|---|---|
| `/` | Início |
| `/produtos` | Lista com busca, filtro por tipo e destaque de estoque baixo |
| `/produtos/novo` | Cadastro de produto |
| `/produtos/:id` | Detalhe, movimentação de estoque, inativação e histórico |
| `/produtos/:id/editar` | Edição de produto |

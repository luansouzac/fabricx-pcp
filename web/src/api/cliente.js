import { useNotificacoesStore } from '../stores/notificacoes'

const URL_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5168'

export class ApiError extends Error {
  constructor(message, statusCode) {
    super(message)
    this.statusCode = statusCode
  }
}

/**
 * @typedef {Object} WebResponse
 * @property {*} data
 * @property {string|null} message
 * @property {boolean} isSuccess
 * @property {number} statusCode
 */

async function lerEnvelope(resposta) {
  try {
    const envelope = await resposta.json()
    if (typeof envelope?.isSuccess === 'boolean') return envelope
  } catch {
    // o corpo não era JSON: cai na resposta inesperada abaixo
  }
  return {
    isSuccess: false,
    message: `Resposta inesperada da API (HTTP ${resposta.status}).`,
    statusCode: resposta.status,
  }
}

async function requisitar(metodo, caminho, { corpo, mensagemSucesso } = {}) {
  const notificacoes = useNotificacoesStore()

  let resposta
  try {
    resposta = await fetch(`${URL_BASE}${caminho}`, {
      method: metodo,
      headers: corpo === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: corpo === undefined ? undefined : JSON.stringify(corpo),
    })
  } catch {
    const erro = new ApiError('Não foi possível conectar à API. Verifique se ela está em execução.', 0)
    notificacoes.erro(erro.message)
    throw erro
  }

  /** @type {WebResponse} */
  const envelope = await lerEnvelope(resposta)

  if (!envelope.isSuccess) {
    const erro = new ApiError(envelope.message ?? 'Erro desconhecido.', envelope.statusCode)
    notificacoes.erro(erro.message)
    throw erro
  }

  const textoSucesso = envelope.message ?? mensagemSucesso
  if (textoSucesso) notificacoes.sucesso(textoSucesso)

  return envelope.data
}

export const api = {
  get: (caminho, opcoes) => requisitar('GET', caminho, opcoes),
  post: (caminho, corpo, opcoes) => requisitar('POST', caminho, { ...opcoes, corpo }),
  put: (caminho, corpo, opcoes) => requisitar('PUT', caminho, { ...opcoes, corpo }),
  delete: (caminho, opcoes) => requisitar('DELETE', caminho, opcoes),
}

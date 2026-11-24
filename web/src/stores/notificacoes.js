import { ref } from 'vue'
import { defineStore } from 'pinia'

const TEMPO_SUCESSO_MS = 3500
const TEMPO_ERRO_MS = 6000

export const useNotificacoesStore = defineStore('notificacoes', () => {
  const id = ref(0)
  const visivel = ref(false)
  const tipo = ref('success')
  const mensagem = ref('')
  const tempo = ref(TEMPO_SUCESSO_MS)

  function exibir(novoTipo, novaMensagem, novoTempo) {
    id.value += 1
    tipo.value = novoTipo
    mensagem.value = novaMensagem
    tempo.value = novoTempo
    visivel.value = true
  }

  const sucesso = (texto) => exibir('success', texto, TEMPO_SUCESSO_MS)
  const erro = (texto) => exibir('error', texto, TEMPO_ERRO_MS)

  return { id, visivel, tipo, mensagem, tempo, sucesso, erro }
})

import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useCapacidadeStore = defineStore('capacidade', () => {
  const dados = ref(null)
  const carregando = ref(false)
  const falhou = ref(false)

  let ultimaRequisicao = 0

  async function carregar(periodo) {
    const requisicao = ++ultimaRequisicao
    carregando.value = true
    falhou.value = false
    try {
      const resposta = await api.get(periodo ? `/api/capacidade?periodo=${periodo}` : '/api/capacidade')
      if (requisicao === ultimaRequisicao) dados.value = resposta
    } catch {
      if (requisicao === ultimaRequisicao) {
        dados.value = null
        falhou.value = true
      }
    } finally {
      if (requisicao === ultimaRequisicao) carregando.value = false
    }
  }

  return { dados, carregando, falhou, carregar }
})

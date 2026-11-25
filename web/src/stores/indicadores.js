import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useIndicadoresStore = defineStore('indicadores', () => {
  const dados = ref(null)
  const carregando = ref(false)
  const falhou = ref(false)

  let ultimaRequisicao = 0

  async function carregar(filtros = {}) {
    const requisicao = ++ultimaRequisicao
    carregando.value = true
    falhou.value = false
    const parametros = new URLSearchParams()
    for (const [chave, valor] of Object.entries(filtros)) {
      if (valor) parametros.set(chave, valor)
    }
    const consulta = parametros.toString()
    try {
      const resposta = await api.get(consulta ? `/api/indicadores?${consulta}` : '/api/indicadores')
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

import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useProgramacaoStore = defineStore('programacao', () => {
  const cronograma = ref([])
  const carregando = ref(false)
  const falhou = ref(false)

  let ultimaRequisicao = 0

  async function carregarCronograma(filtros = {}) {
    const requisicao = ++ultimaRequisicao
    carregando.value = true
    falhou.value = false
    const parametros = new URLSearchParams()
    for (const [chave, valor] of Object.entries(filtros)) {
      if (valor) parametros.set(chave, valor)
    }
    const consulta = parametros.toString()
    try {
      const lista = await api.get(consulta ? `/api/programacao?${consulta}` : '/api/programacao')
      if (requisicao === ultimaRequisicao) cronograma.value = lista
    } catch {
      if (requisicao === ultimaRequisicao) {
        cronograma.value = []
        falhou.value = true
      }
    } finally {
      if (requisicao === ultimaRequisicao) carregando.value = false
    }
  }

  const listarPorOrdem = (ordemId) => api.get(`/api/ordens-producao/${ordemId}/programacao`)

  const agendar = (ordemId, dados) =>
    api.post(`/api/ordens-producao/${ordemId}/programacao`, dados, { mensagemSucesso: 'Etapa programada com sucesso.' })

  const reprogramar = (id, dados) =>
    api.put(`/api/programacao/${id}`, dados, { mensagemSucesso: 'Etapa reprogramada com sucesso.' })

  const remover = (id) => api.delete(`/api/programacao/${id}`, { mensagemSucesso: 'Programação removida com sucesso.' })

  return { cronograma, carregando, falhou, carregarCronograma, listarPorOrdem, agendar, reprogramar, remover }
})

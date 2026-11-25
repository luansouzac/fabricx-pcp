import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useOrdensStore = defineStore('ordens', () => {
  const itens = ref([])
  const carregando = ref(false)
  const falhou = ref(false)

  let ultimaRequisicao = 0

  async function carregar(status) {
    const requisicao = ++ultimaRequisicao
    carregando.value = true
    falhou.value = false
    try {
      const lista = await api.get(status ? `/api/ordens-producao?status=${status}` : '/api/ordens-producao')
      if (requisicao === ultimaRequisicao) itens.value = lista
    } catch {
      if (requisicao === ultimaRequisicao) {
        itens.value = []
        falhou.value = true
      }
    } finally {
      if (requisicao === ultimaRequisicao) carregando.value = false
    }
  }

  const obter = (id) => api.get(`/api/ordens-producao/${id}`)

  const criar = (dados) =>
    api.post('/api/ordens-producao', dados, { mensagemSucesso: 'Ordem de produção criada com sucesso.' })

  const alterar = (id, dados) =>
    api.put(`/api/ordens-producao/${id}`, dados, { mensagemSucesso: 'Ordem de produção atualizada com sucesso.' })

  const avancar = (id, mensagemSucesso) => api.post(`/api/ordens-producao/${id}/avancar`, undefined, { mensagemSucesso })

  const cancelar = (id) =>
    api.post(`/api/ordens-producao/${id}/cancelar`, undefined, { mensagemSucesso: 'Ordem de produção cancelada.' })

  return { itens, carregando, falhou, carregar, obter, criar, alterar, avancar, cancelar }
})

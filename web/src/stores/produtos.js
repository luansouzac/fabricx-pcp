import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useProdutosStore = defineStore('produtos', () => {
  const itens = ref([])
  const carregando = ref(false)

  let ultimaRequisicao = 0

  async function carregar(tipo) {
    const requisicao = ++ultimaRequisicao
    carregando.value = true
    try {
      const lista = await api.get(tipo ? `/api/produtos?tipo=${tipo}` : '/api/produtos')
      if (requisicao === ultimaRequisicao) itens.value = lista
    } catch {
      if (requisicao === ultimaRequisicao) itens.value = []
    } finally {
      if (requisicao === ultimaRequisicao) carregando.value = false
    }
  }

  const obter = (id) => api.get(`/api/produtos/${id}`)

  const criar = (dados) => api.post('/api/produtos', dados, { mensagemSucesso: 'Produto criado com sucesso.' })

  const atualizar = (id, dados) =>
    api.put(`/api/produtos/${id}`, dados, { mensagemSucesso: 'Produto atualizado com sucesso.' })

  const movimentar = (id, dados) =>
    api.post(`/api/produtos/${id}/movimentacoes`, dados, { mensagemSucesso: 'Movimentação registrada com sucesso.' })

  const listarMovimentacoes = (id) => api.get(`/api/produtos/${id}/movimentacoes`)

  const inativar = (id) => api.delete(`/api/produtos/${id}`, { mensagemSucesso: 'Produto inativado com sucesso.' })

  return { itens, carregando, carregar, obter, criar, atualizar, movimentar, listarMovimentacoes, inativar }
})

import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useProdutosStore = defineStore('produtos', () => {
  const itens = ref([])
  const carregando = ref(false)
  const falhou = ref(false)

  let ultimaRequisicao = 0

  async function carregar(tipo) {
    const requisicao = ++ultimaRequisicao
    carregando.value = true
    falhou.value = false
    try {
      const lista = await api.get(tipo ? `/api/produtos?tipo=${tipo}` : '/api/produtos')
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

  const obter = (id) => api.get(`/api/produtos/${id}`)

  const criar = (dados) => api.post('/api/produtos', dados, { mensagemSucesso: 'Produto criado com sucesso.' })

  const atualizar = (id, dados) =>
    api.put(`/api/produtos/${id}`, dados, { mensagemSucesso: 'Produto atualizado com sucesso.' })

  const movimentar = (id, dados) =>
    api.post(`/api/produtos/${id}/movimentacoes`, dados, { mensagemSucesso: 'Movimentação registrada com sucesso.' })

  const listarMovimentacoes = (id) => api.get(`/api/produtos/${id}/movimentacoes`)

  const inativar = (id) => api.delete(`/api/produtos/${id}`, { mensagemSucesso: 'Produto inativado com sucesso.' })

  const listarEstrutura = (id) => api.get(`/api/produtos/${id}/estrutura`)

  const adicionarComponente = (id, dados) =>
    api.post(`/api/produtos/${id}/estrutura`, dados, { mensagemSucesso: 'Componente adicionado com sucesso.' })

  const alterarQuantidadeDoComponente = (id, componenteId, quantidade) =>
    api.put(`/api/produtos/${id}/estrutura/${componenteId}`, { quantidade }, { mensagemSucesso: 'Quantidade atualizada com sucesso.' })

  const removerComponente = (id, componenteId) =>
    api.delete(`/api/produtos/${id}/estrutura/${componenteId}`, { mensagemSucesso: 'Componente removido com sucesso.' })

  const listarRoteiro = (id) => api.get(`/api/produtos/${id}/roteiro`)

  const adicionarEtapa = (id, dados) =>
    api.post(`/api/produtos/${id}/roteiro`, dados, { mensagemSucesso: 'Etapa adicionada com sucesso.' })

  const atualizarEtapa = (id, etapaId, dados) =>
    api.put(`/api/produtos/${id}/roteiro/${etapaId}`, dados, { mensagemSucesso: 'Etapa atualizada com sucesso.' })

  const removerEtapa = (id, etapaId) =>
    api.delete(`/api/produtos/${id}/roteiro/${etapaId}`, { mensagemSucesso: 'Etapa removida com sucesso.' })

  return {
    itens, carregando, falhou,
    carregar,
    obter,
    criar,
    atualizar,
    movimentar,
    listarMovimentacoes,
    inativar,
    listarEstrutura,
    adicionarComponente,
    alterarQuantidadeDoComponente,
    removerComponente,
    listarRoteiro,
    adicionarEtapa,
    atualizarEtapa,
    removerEtapa,
  }
})

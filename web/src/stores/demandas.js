import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useDemandasStore = defineStore('demandas', () => {
  const itens = ref([])
  const carregando = ref(false)
  const falhou = ref(false)

  let ultimaRequisicao = 0

  async function carregar(periodo) {
    const requisicao = ++ultimaRequisicao
    carregando.value = true
    falhou.value = false
    try {
      const lista = await api.get(periodo ? `/api/demanda-prevista?periodo=${periodo}` : '/api/demanda-prevista')
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

  async function obter(id) {
    const lista = await api.get('/api/demanda-prevista')
    return lista.find((demanda) => demanda.id === Number(id)) ?? null
  }

  const criar = (dados) =>
    api.post('/api/demanda-prevista', dados, { mensagemSucesso: 'Demanda prevista criada com sucesso.' })

  const atualizar = (id, dados) =>
    api.put(`/api/demanda-prevista/${id}`, dados, { mensagemSucesso: 'Demanda prevista atualizada com sucesso.' })

  const remover = (id) =>
    api.delete(`/api/demanda-prevista/${id}`, { mensagemSucesso: 'Demanda prevista excluída com sucesso.' })

  return { itens, carregando, falhou, carregar, obter, criar, atualizar, remover }
})

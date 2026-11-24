import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useRecursosStore = defineStore('recursos', () => {
  const itens = ref([])
  const carregando = ref(false)

  async function carregar() {
    carregando.value = true
    try {
      itens.value = await api.get('/api/recursos')
    } catch {
      itens.value = []
    } finally {
      carregando.value = false
    }
  }

  const obter = (id) => api.get(`/api/recursos/${id}`)

  const criar = (dados) => api.post('/api/recursos', dados, { mensagemSucesso: 'Recurso criado com sucesso.' })

  const atualizar = (id, dados) =>
    api.put(`/api/recursos/${id}`, dados, { mensagemSucesso: 'Recurso atualizado com sucesso.' })

  const inativar = (id) => api.delete(`/api/recursos/${id}`, { mensagemSucesso: 'Recurso inativado com sucesso.' })

  return { itens, carregando, carregar, obter, criar, atualizar, inativar }
})

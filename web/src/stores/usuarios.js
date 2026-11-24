import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useUsuariosStore = defineStore('usuarios', () => {
  const itens = ref([])
  const carregando = ref(false)
  const falhou = ref(false)

  async function carregar() {
    carregando.value = true
    falhou.value = false
    try {
      itens.value = await api.get('/api/usuarios')
    } catch {
      itens.value = []
      falhou.value = true
    } finally {
      carregando.value = false
    }
  }

  const obter = (id) => api.get(`/api/usuarios/${id}`)

  const criar = (dados) => api.post('/api/usuarios', dados, { mensagemSucesso: 'Usuário criado com sucesso.' })

  const atualizar = (id, dados) =>
    api.put(`/api/usuarios/${id}`, dados, { mensagemSucesso: 'Usuário atualizado com sucesso.' })

  const inativar = (id) => api.delete(`/api/usuarios/${id}`, { mensagemSucesso: 'Usuário inativado com sucesso.' })

  return { itens, carregando, falhou, carregar, obter, criar, atualizar, inativar }
})

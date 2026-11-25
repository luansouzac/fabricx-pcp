import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useApontamentosStore = defineStore('apontamentos', () => {
  const listarPorOrdem = (ordemId) => api.get(`/api/ordens-producao/${ordemId}/apontamentos`)

  const registrar = (programacaoId, dados) =>
    api.post(`/api/programacao/${programacaoId}/apontamentos`, dados, { mensagemSucesso: 'Apontamento registrado com sucesso.' })

  return { listarPorOrdem, registrar }
})

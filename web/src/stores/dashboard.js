import { ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

export const useDashboardStore = defineStore('dashboard', () => {
  const resumo = ref(null)
  const capacidade = ref(null)
  const carregando = ref(false)

  async function carregar() {
    carregando.value = true
    const [respostaResumo, respostaCapacidade] = await Promise.allSettled([
      api.get('/api/indicadores/resumo'),
      api.get('/api/capacidade'),
    ])
    resumo.value = respostaResumo.status === 'fulfilled' ? respostaResumo.value : null
    capacidade.value = respostaCapacidade.status === 'fulfilled' ? respostaCapacidade.value : null
    carregando.value = false
  }

  return { resumo, capacidade, carregando, carregar }
})

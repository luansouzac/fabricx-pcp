import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../api/cliente'

const CHAVE = 'fx-auth'

function lerSessao() {
  try {
    const sessao = JSON.parse(sessionStorage.getItem(CHAVE))
    return sessao?.token && new Date(sessao.expiraEmUtc) > new Date() ? sessao : null
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const sessao = ref(lerSessao())

  const token = computed(() => sessao.value?.token ?? null)
  const usuario = computed(() => sessao.value?.usuario ?? null)
  const logado = computed(() => !!sessao.value && new Date(sessao.value.expiraEmUtc) > new Date())
  const ehAdministrador = computed(() => usuario.value?.perfil === 'Administrador')

  async function entrar(email, senha) {
    const dados = await api.post('/api/auth/login', { email, senha }, { publica: true })
    sessao.value = { token: dados.token, expiraEmUtc: dados.expiraEmUtc, usuario: dados.usuario }
    sessionStorage.setItem(CHAVE, JSON.stringify(sessao.value))
  }

  function sair() {
    sessao.value = null
    sessionStorage.removeItem(CHAVE)
  }

  return { token, usuario, logado, ehAdministrador, entrar, sair }
})

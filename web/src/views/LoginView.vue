<script setup>
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTheme } from 'vuetify'
import { useAuthStore } from '../stores/auth'
import logoClaro from '../assets/logo/fabricx-horizontal-claro.svg'
import logoEscuro from '../assets/logo/fabricx-horizontal-escuro.svg'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const theme = useTheme()

const email = ref('')
const senha = ref('')
const mostrarSenha = ref(false)
const enviando = ref(false)
const erro = ref('')

const logo = computed(() => (theme.current.value.dark ? logoEscuro : logoClaro))

const regras = {
  email: [(v) => !!v?.trim() || 'Informe o e-mail.', (v) => /^\S+@\S+\.\S+$/.test(v) || 'Informe um e-mail válido.'],
  senha: [(v) => !!v || 'Informe a senha.'],
}

async function entrar(evento) {
  const { valid } = await evento
  if (!valid) return

  enviando.value = true
  erro.value = ''
  try {
    await auth.entrar(email.value.trim(), senha.value)
    const destino = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/') ? route.query.redirect : '/'
    router.replace(destino)
  } catch (e) {
    erro.value = e.message
  } finally {
    enviando.value = false
  }
}
</script>

<template>
  <v-main class="d-flex align-center justify-center">
    <v-card width="400" max-width="92vw" border flat rounded="lg" class="pa-8">
      <div class="d-flex justify-center mb-6">
        <img :src="logo" alt="FabricX" height="36" />
      </div>
      <h1 class="fx-titulo-pagina mb-1">Entrar</h1>
      <p class="fx-legenda mb-6">Use o e-mail e a senha fornecidos pelo administrador.</p>

      <v-form @submit.prevent="entrar">
        <v-text-field
          v-model="email"
          label="E-mail"
          type="email"
          autocomplete="username"
          prepend-inner-icon="mdi-email-outline"
          :rules="regras.email"
          autofocus
        />
        <v-text-field
          v-model="senha"
          label="Senha"
          autocomplete="current-password"
          prepend-inner-icon="mdi-lock-outline"
          :type="mostrarSenha ? 'text' : 'password'"
          :append-inner-icon="mostrarSenha ? 'mdi-eye-off-outline' : 'mdi-eye-outline'"
          :rules="regras.senha"
          @click:append-inner="mostrarSenha = !mostrarSenha"
        />
        <v-alert v-if="erro" type="error" variant="flat" density="compact" class="mb-4">{{ erro }}</v-alert>
        <v-btn type="submit" color="primary" block :loading="enviando" class="text-none">Entrar</v-btn>
      </v-form>
    </v-card>
  </v-main>
</template>

<script setup>
import { computed } from 'vue'
import { useTheme } from 'vuetify'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import logoClaro from '../assets/logo/fabricx-horizontal-claro.svg'
import logoEscuro from '../assets/logo/fabricx-horizontal-escuro.svg'

const itensBase = [
  { titulo: 'Início', icone: 'mdi-view-dashboard-outline', rota: '/' },
  { titulo: 'Produtos', icone: 'mdi-package-variant-closed', rota: '/produtos' },
  { titulo: 'Recursos', icone: 'mdi-robot-industrial-outline', rota: '/recursos' },
  { titulo: 'Ordens de produção', icone: 'mdi-clipboard-text-outline', rota: '/ordens' },
  { titulo: 'Demanda prevista', icone: 'mdi-chart-timeline-variant', rota: '/demanda-prevista' },
  { titulo: 'Capacidade', icone: 'mdi-gauge-full', rota: '/capacidade' },
  { titulo: 'Indicadores', icone: 'mdi-chart-line', rota: '/indicadores' },
]

const itemUsuarios = { titulo: 'Usuários', icone: 'mdi-account-multiple-outline', rota: '/usuarios' }

const auth = useAuthStore()
const itensMenu = computed(() => (auth.ehAdministrador ? [...itensBase, itemUsuarios] : itensBase))
const router = useRouter()
const theme = useTheme()
const escuro = computed(() => theme.current.value.dark)
const logo = computed(() => (escuro.value ? logoEscuro : logoClaro))

function alternarTema() {
  const novo = escuro.value ? 'fabricx' : 'fabricxDark'
  theme.change(novo)
  localStorage.setItem('fx-tema', novo)
}

function sair() {
  auth.sair()
  router.push({ name: 'login' })
}
</script>

<template>
  <v-navigation-drawer permanent width="256">
    <div class="d-flex justify-center px-4 pt-5 pb-6">
      <img :src="logo" alt="FabricX" height="32" />
    </div>
    <v-list nav density="comfortable" color="primary">
      <v-list-item
        v-for="item in itensMenu"
        :key="item.rota"
        :to="item.rota"
        :prepend-icon="item.icone"
        :title="item.titulo"
        :exact="item.rota === '/'"
        rounded="lg"
      />
    </v-list>
  </v-navigation-drawer>

  <v-app-bar height="56">
    <v-spacer />
    <span class="fx-legenda mr-2">{{ auth.usuario?.nome }}</span>
    <v-btn
      :icon="escuro ? 'mdi-weather-sunny' : 'mdi-weather-night'"
      variant="text"
      color="secondary"
      class="mr-2"
      :aria-label="escuro ? 'Usar tema claro' : 'Usar tema escuro'"
      @click="alternarTema"
    />
    <v-btn
      icon="mdi-logout"
      variant="text"
      color="secondary"
      class="mr-2"
      aria-label="Sair"
      @click="sair"
    />
  </v-app-bar>

  <v-main>
    <v-container fluid class="pa-8">
      <router-view />
    </v-container>
  </v-main>
</template>

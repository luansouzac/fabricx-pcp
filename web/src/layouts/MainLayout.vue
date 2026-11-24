<script setup>
import { computed } from 'vue'
import { useTheme } from 'vuetify'
import logoClaro from '../assets/logo/fabricx-horizontal-claro.svg'
import logoEscuro from '../assets/logo/fabricx-horizontal-escuro.svg'

const itensMenu = [
  { titulo: 'Início', icone: 'mdi-view-dashboard-outline', rota: '/' },
  { titulo: 'Produtos', icone: 'mdi-package-variant-closed', rota: '/produtos' },
  { titulo: 'Recursos', icone: 'mdi-robot-industrial-outline', rota: '/recursos' },
  { titulo: 'Ordens de produção', icone: 'mdi-clipboard-text-outline', rota: '/ordens' },
]

const theme = useTheme()
const escuro = computed(() => theme.current.value.dark)
const logo = computed(() => (escuro.value ? logoEscuro : logoClaro))

function alternarTema() {
  const novo = escuro.value ? 'fabricx' : 'fabricxDark'
  theme.change(novo)
  localStorage.setItem('fx-tema', novo)
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
    <v-btn
      :icon="escuro ? 'mdi-weather-sunny' : 'mdi-weather-night'"
      variant="text"
      color="secondary"
      class="mr-2"
      :aria-label="escuro ? 'Usar tema claro' : 'Usar tema escuro'"
      @click="alternarTema"
    />
  </v-app-bar>

  <v-main>
    <v-container fluid class="pa-8">
      <router-view />
    </v-container>
  </v-main>
</template>

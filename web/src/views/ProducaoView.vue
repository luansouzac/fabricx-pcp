<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import CronogramaProducao from '../components/CronogramaProducao.vue'
import ListaOrdens from '../components/ListaOrdens.vue'

const route = useRoute()
const router = useRouter()

const aba = computed({
  get: () => (route.query.aba === 'cronograma' ? 'cronograma' : 'ordens'),
  set: (valor) => router.replace({ query: valor === 'ordens' ? {} : { aba: valor } }),
})
</script>

<template>
  <div class="d-flex align-center mb-4">
    <div>
      <h1 class="fx-titulo-pagina">Produção</h1>
      <div class="fx-legenda">Crie as ordens, programe as etapas nos recursos e acompanhe o cronograma.</div>
    </div>
    <v-spacer />
    <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" to="/ordens/nova">Nova ordem</v-btn>
  </div>

  <v-tabs v-model="aba" color="primary">
    <v-tab value="ordens" prepend-icon="mdi-clipboard-text-outline" class="text-none">Ordens</v-tab>
    <v-tab value="cronograma" prepend-icon="mdi-calendar-clock-outline" class="text-none">Cronograma</v-tab>
  </v-tabs>

  <div class="mt-4">
    <ListaOrdens v-if="aba === 'ordens'" />
    <CronogramaProducao v-else />
  </div>
</template>

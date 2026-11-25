<script setup>
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import CapacidadePeriodo from '../components/CapacidadePeriodo.vue'
import DemandaPrevista from '../components/DemandaPrevista.vue'

const hoje = new Date()
const periodoAtual = `${hoje.getFullYear()}-${String(hoje.getMonth() + 1).padStart(2, '0')}`
const periodoInformado = useRoute().query.periodo
const periodo = ref(/^\d{4}-\d{2}$/.test(periodoInformado ?? '') ? periodoInformado : periodoAtual)
const versao = ref(0)

watch(periodo, (valor) => {
  if (!valor) periodo.value = periodoAtual
})
</script>

<template>
  <div class="mb-4">
    <h1 class="fx-titulo-pagina">Planejamento</h1>
    <div class="fx-legenda">Informe a demanda do mês e confira se a capacidade dos recursos dá conta.</div>
  </div>

  <v-row>
    <v-col cols="12" sm="6" md="3">
      <v-text-field v-model="periodo" label="Mês de planejamento" type="month" hide-details />
    </v-col>
  </v-row>

  <v-card class="mt-4">
    <v-card-text>
      <DemandaPrevista :periodo="periodo" @alterada="versao++" />
    </v-card-text>
  </v-card>

  <v-card class="mt-4">
    <v-card-text>
      <CapacidadePeriodo :periodo="periodo" :versao="versao" />
    </v-card-text>
  </v-card>
</template>

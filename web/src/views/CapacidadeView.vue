<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useCapacidadeStore } from '../stores/capacidade'
import { formatarPeriodo } from '../utils/demanda'

const capacidade = useCapacidadeStore()

const hoje = new Date()
const periodo = ref(`${hoje.getFullYear()}-${String(hoje.getMonth() + 1).padStart(2, '0')}`)

const decimal = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 1 })

const classificacao = {
  Ocioso: { rotulo: 'Ocioso', cor: 'info', icone: 'mdi-sleep' },
  Normal: { rotulo: 'Normal', cor: 'success', icone: 'mdi-check-circle-outline' },
  Gargalo: { rotulo: 'Gargalo', cor: 'error', icone: 'mdi-alert-octagon-outline' },
}

const colunas = [
  { title: 'Recurso', key: 'recursoNome' },
  { title: 'Capacidade no período', key: 'capacidadeHorasPeriodo' },
  { title: 'Horas demandadas', key: 'horasNecessariasDemanda' },
  { title: 'Utilização', key: 'percentualCarga' },
  { title: 'Situação', key: 'classificacao' },
  { title: 'Dias no período', key: 'diasNoPeriodo', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const linhas = computed(() =>
  (capacidade.dados?.recursos ?? []).map((recurso) => ({
    ...recurso,
    diasNoPeriodo: capacidade.dados.diasNoPeriodo,
  })),
)

const gargalos = computed(() => linhas.value.filter((recurso) => recurso.classificacao === 'Gargalo'))

watch(periodo, (valor) => {
  if (valor) capacidade.carregar(valor)
})

onMounted(() => capacidade.carregar(periodo.value))
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Capacidade</h1>
  </div>

  <v-alert v-if="capacidade.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Não foi possível carregar a capacidade.
    <template #append>
      <v-btn variant="text" size="small" @click="capacidade.carregar(periodo)">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <v-alert
    v-else-if="gargalos.length"
    type="warning"
    variant="flat"
    density="compact"
    icon="mdi-alert-octagon-outline"
    class="mb-4"
  >
    Gargalo em {{ formatarPeriodo(capacidade.dados.periodo) }}: {{ gargalos.map((recurso) => recurso.recursoNome).join(', ') }}.
  </v-alert>

  <v-row>
    <v-col cols="12" md="3">
      <v-text-field v-model="periodo" label="Período" type="month" hide-details />
    </v-col>
  </v-row>

  <v-data-table
    :headers="colunas"
    :items="linhas"
    :loading="capacidade.carregando"
    item-value="recursoId"
    items-per-page-text="Itens por página"
    no-data-text="Nenhum recurso com carga para o período. Cadastre demanda prevista ou mude o período."
    loading-text="Carregando capacidade..."
    class="mt-4"
  >
    <template #item.capacidadeHorasPeriodo="{ item }">{{ decimal.format(item.capacidadeHorasPeriodo) }} h</template>
    <template #item.horasNecessariasDemanda="{ item }">{{ decimal.format(item.horasNecessariasDemanda) }} h</template>
    <template #item.percentualCarga="{ item }">
      <div class="d-flex align-center ga-2 justify-center">
        <v-progress-linear
          :model-value="Math.min(item.percentualCarga, 100)"
          :color="classificacao[item.classificacao].cor"
          height="8"
          rounded
          style="max-width: 120px"
          :aria-label="`Utilização de ${item.recursoNome}`"
        />
        <span class="font-weight-bold">{{ decimal.format(item.percentualCarga) }}%</span>
      </div>
    </template>
    <template #item.classificacao="{ item }">
      <v-chip :color="classificacao[item.classificacao].cor" variant="flat" size="small" :prepend-icon="classificacao[item.classificacao].icone">
        {{ classificacao[item.classificacao].rotulo }}
      </v-chip>
    </template>
  </v-data-table>
</template>

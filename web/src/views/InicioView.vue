<script setup>
import { computed, onMounted } from 'vue'
import { useDashboardStore } from '../stores/dashboard'
import { infoDoStatus, statusDaOrdem } from '../utils/ordem'

const dashboard = useDashboardStore()

const percentual = new Intl.NumberFormat('pt-BR', { style: 'percent', maximumFractionDigits: 1 })
const decimal = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 1 })

const formatar = (valor, formatador) => (valor === null || valor === undefined ? '—' : formatador.format(valor))

const ordensPorStatus = computed(() =>
  statusDaOrdem.map((status) => ({
    ...status,
    quantidade: dashboard.resumo?.ordensPorStatus.find((item) => item.status === status.value)?.quantidade ?? 0,
  })),
)

const indicadores = computed(() => {
  const dados = dashboard.resumo?.indicadores
  return [
    { titulo: 'Taxa de defeitos', valor: formatar(dados?.qualidade, percentual), icone: 'mdi-alert-circle-outline' },
    { titulo: 'Eficiência', valor: formatar(dados?.eficiencia, percentual), icone: 'mdi-speedometer' },
    { titulo: 'Produtividade', valor: dados?.produtividadePorHora == null ? '—' : `${decimal.format(dados.produtividadePorHora)} un/h`, icone: 'mdi-factory' },
    { titulo: 'Cumprimento de prazo', valor: formatar(dados?.cumprimentoDePrazo, percentual), icone: 'mdi-calendar-check-outline' },
  ]
})

const classificacao = {
  Ocioso: { rotulo: 'Ocioso', cor: 'info' },
  Normal: { rotulo: 'Normal', cor: 'success' },
  Gargalo: { rotulo: 'Gargalo', cor: 'error' },
}

const periodo = computed(() => {
  const [ano, mes] = (dashboard.capacidade?.periodo ?? '').split('-')
  return ano && mes ? `${mes}/${ano}` : ''
})

onMounted(() => dashboard.carregar())
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Início</h1>
    <v-spacer />
    <v-btn variant="outlined" color="secondary" prepend-icon="mdi-refresh" :loading="dashboard.carregando" @click="dashboard.carregar()">
      Atualizar
    </v-btn>
  </div>

  <div class="fx-titulo-secao mb-3">Ordens de produção</div>
  <v-row>
    <v-col v-for="status in ordensPorStatus" :key="status.value" cols="6" md>
      <v-card :loading="dashboard.carregando" :to="'/ordens'">
        <v-card-text>
          <div class="fx-legenda">{{ status.title }}</div>
          <div class="text-h4 mt-1">{{ status.quantidade }}</div>
          <v-chip :color="infoDoStatus[status.value].color" variant="flat" size="x-small" class="mt-2">{{ status.title }}</v-chip>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>

  <div class="fx-titulo-secao mt-6 mb-3">Indicadores</div>
  <v-row>
    <v-col v-for="indicador in indicadores" :key="indicador.titulo" cols="12" sm="6" md="3">
      <v-card :loading="dashboard.carregando">
        <v-card-text class="d-flex align-center ga-3">
          <v-icon :icon="indicador.icone" color="secondary" size="28" />
          <div>
            <div class="fx-legenda">{{ indicador.titulo }}</div>
            <div class="text-h5">{{ indicador.valor }}</div>
          </div>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>

  <v-row class="mt-2">
    <v-col cols="12" md="6">
      <div class="fx-titulo-secao mb-3">Produtos abaixo do estoque mínimo</div>
      <v-card :loading="dashboard.carregando">
        <v-table density="compact">
          <thead>
            <tr>
              <th class="text-center">Produto</th>
              <th class="text-center">Estoque atual</th>
              <th class="text-center">Estoque mínimo</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="produto in dashboard.resumo?.produtosComEstoqueBaixo ?? []" :key="produto.id">
              <td class="text-center">
                <router-link :to="`/produtos/${produto.id}`">{{ produto.codigo }} · {{ produto.nome }}</router-link>
              </td>
              <td class="text-center estoque-baixo">{{ produto.estoqueAtual }} {{ produto.unidadeMedida }}</td>
              <td class="text-center">{{ produto.estoqueMinimo }} {{ produto.unidadeMedida }}</td>
            </tr>
            <tr v-if="!dashboard.carregando && !dashboard.resumo?.produtosComEstoqueBaixo?.length">
              <td colspan="3" class="text-center text-medium-emphasis py-4">Nenhum produto abaixo do mínimo.</td>
            </tr>
          </tbody>
        </v-table>
      </v-card>
    </v-col>

    <v-col cols="12" md="6">
      <div class="fx-titulo-secao mb-3">Carga por recurso <span v-if="periodo" class="fx-legenda">· {{ periodo }}</span></div>
      <v-card :loading="dashboard.carregando">
        <v-card-text>
          <div v-for="recurso in dashboard.capacidade?.recursos ?? []" :key="recurso.recursoId" class="mb-4">
            <div class="d-flex align-center mb-1">
              <span>{{ recurso.recursoNome }}</span>
              <v-spacer />
              <span class="mr-2">{{ decimal.format(recurso.percentualCarga) }}%</span>
              <v-chip :color="classificacao[recurso.classificacao].cor" variant="flat" size="x-small">
                {{ classificacao[recurso.classificacao].rotulo }}
              </v-chip>
            </div>
            <v-progress-linear
              :model-value="Math.min(recurso.percentualCarga, 100)"
              :color="classificacao[recurso.classificacao].cor"
              height="8"
              rounded
              :aria-label="`Carga de ${recurso.recursoNome}`"
            />
          </div>
          <div
            v-if="!dashboard.carregando && !dashboard.capacidade?.recursos?.length"
            class="text-center text-medium-emphasis py-4"
          >
            Nenhum recurso com carga para o período.
          </div>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

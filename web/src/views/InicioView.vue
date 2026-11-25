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

const hoje = new Date().toISOString().slice(0, 10)
const statusAbertos = ['Planejada', 'Programada', 'EmProducao']

const pontosDeAtencao = computed(() => {
  const ordensAbertas = dashboard.ordens.filter((ordem) => statusAbertos.includes(ordem.status))
  const atrasadas = ordensAbertas.filter((ordem) => ordem.prazoEntrega < hoje).length
  const aguardando = dashboard.ordens.filter((ordem) => ordem.status === 'Planejada').length
  const gargalos = (dashboard.capacidade?.recursos ?? []).filter((recurso) => recurso.classificacao === 'Gargalo')
  const estoqueBaixo = dashboard.resumo?.produtosComEstoqueBaixo?.length ?? 0

  const pontos = []
  if (atrasadas) {
    pontos.push({ icone: 'mdi-clock-alert-outline', cor: 'error', rota: '/producao', texto: `${atrasadas} ${atrasadas === 1 ? 'ordem passou' : 'ordens passaram'} do prazo de entrega.`, acao: 'Ver ordens' })
  }
  if (aguardando) {
    pontos.push({ icone: 'mdi-calendar-plus', cor: 'info', rota: '/producao', texto: `${aguardando} ${aguardando === 1 ? 'ordem planejada aguarda' : 'ordens planejadas aguardam'} programação.`, acao: 'Programar' })
  }
  if (gargalos.length) {
    pontos.push({ icone: 'mdi-alert-octagon-outline', cor: 'error', rota: '/planejamento', texto: `Gargalo no mês: ${gargalos.map((recurso) => recurso.recursoNome).join(', ')}.`, acao: 'Ver capacidade' })
  }
  if (estoqueBaixo) {
    pontos.push({ icone: 'mdi-warehouse', cor: 'warning', rota: '/produtos', texto: `${estoqueBaixo} ${estoqueBaixo === 1 ? 'produto está abaixo' : 'produtos estão abaixo'} do estoque mínimo.`, acao: 'Ver produtos' })
  }
  return pontos
})

const etapasDoFluxo = [
  { titulo: 'Produtos', texto: 'Cadastre produtos, estrutura (BOM) e roteiro.', rota: '/produtos', icone: 'mdi-package-variant-closed' },
  { titulo: 'Recursos', texto: 'Cadastre máquinas, operadores e linhas.', rota: '/recursos', icone: 'mdi-robot-industrial-outline' },
  { titulo: 'Planejamento', texto: 'Informe a demanda e veja a capacidade.', rota: '/planejamento', icone: 'mdi-chart-timeline-variant' },
  { titulo: 'Produção', texto: 'Crie ordens e programe as etapas.', rota: '/producao', icone: 'mdi-clipboard-text-outline' },
  { titulo: 'Apontamentos', texto: 'Registre o que foi produzido na ordem.', rota: '/producao', icone: 'mdi-clipboard-edit-outline' },
  { titulo: 'Indicadores', texto: 'Acompanhe qualidade e eficiência.', rota: '/indicadores', icone: 'mdi-chart-line' },
]

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

  <v-alert v-if="dashboard.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Parte dos dados não pôde ser carregada. Os números abaixo podem estar incompletos.
    <template #append>
      <v-btn variant="text" size="small" @click="dashboard.carregar()">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <div class="fx-titulo-secao mb-3">O que fazer agora</div>
  <v-card :loading="dashboard.carregando">
    <v-list v-if="pontosDeAtencao.length" lines="one">
      <v-list-item v-for="ponto in pontosDeAtencao" :key="ponto.texto" :to="ponto.rota">
        <template #prepend>
          <v-icon :icon="ponto.icone" :color="ponto.cor" />
        </template>
        <v-list-item-title class="text-wrap">{{ ponto.texto }}</v-list-item-title>
        <template #append>
          <span class="fx-legenda">{{ ponto.acao }}</span>
        </template>
      </v-list-item>
    </v-list>
    <v-card-text v-else-if="!dashboard.carregando" class="text-medium-emphasis">
      Nada pendente agora. Siga o fluxo abaixo para planejar o próximo mês.
    </v-card-text>
  </v-card>

  <div class="fx-titulo-secao mt-6 mb-3">Fluxo de trabalho</div>
  <v-row>
    <v-col v-for="(etapa, indice) in etapasDoFluxo" :key="etapa.titulo" cols="12" sm="6" md="4" lg="2">
      <v-card :to="etapa.rota" class="h-100">
        <v-card-text>
          <div class="d-flex align-center ga-2 mb-2">
            <v-avatar size="24" color="secondary" class="text-caption">{{ indice + 1 }}</v-avatar>
            <v-icon :icon="etapa.icone" color="secondary" size="20" />
          </div>
          <div class="font-weight-bold">{{ etapa.titulo }}</div>
          <div class="fx-legenda mt-1">{{ etapa.texto }}</div>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>

  <div class="fx-titulo-secao mt-6 mb-3">Ordens de produção</div>
  <v-row>
    <v-col v-for="status in ordensPorStatus" :key="status.value" cols="6" md>
      <v-card :loading="dashboard.carregando" :to="'/producao'">
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

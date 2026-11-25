<script setup>
import { computed, onMounted, reactive, watch } from 'vue'
import { useIndicadoresStore } from '../stores/indicadores'
import { useProdutosStore } from '../stores/produtos'
import { useRecursosStore } from '../stores/recursos'

const indicadores = useIndicadoresStore()
const produtos = useProdutosStore()
const recursos = useRecursosStore()

const filtros = reactive({ de: '', ate: '', produtoId: null, recursoId: null })

const percentual = new Intl.NumberFormat('pt-BR', { style: 'percent', maximumFractionDigits: 1 })
const decimal = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 1 })
const inteiro = new Intl.NumberFormat('pt-BR')

const opcoesDeProduto = computed(() =>
  produtos.itens.map((produto) => ({ title: `${produto.codigo} · ${produto.nome}`, value: produto.id })),
)
const opcoesDeRecurso = computed(() => recursos.itens.map((recurso) => ({ title: recurso.nome, value: recurso.id })))

const periodoInvalido = computed(() => Boolean(filtros.de && filtros.ate && filtros.de > filtros.ate))
const filtrado = computed(() => Boolean(filtros.de || filtros.ate || filtros.produtoId || filtros.recursoId))

const semResultado = computed(() => {
  const dados = indicadores.dados
  return Boolean(dados) && dados.totalProduzido === 0 && dados.tempoApontadoMin === 0 && dados.ordensConcluidas === 0
})

const formatar = (valor, formatador, unidade = '') => (valor == null ? '—' : `${formatador.format(valor)}${unidade}`)

const cartoes = computed(() => {
  const dados = indicadores.dados
  return [
    {
      titulo: 'Taxa de defeitos',
      valor: formatar(dados?.qualidade, percentual),
      legenda: 'Parcela do que foi processado que saiu com defeito.',
      icone: 'mdi-alert-circle-outline',
    },
    {
      titulo: 'Eficiência',
      valor: formatar(dados?.eficiencia, percentual),
      legenda: 'Tempo padrão previsto em relação ao tempo realmente apontado.',
      icone: 'mdi-speedometer',
    },
    {
      titulo: 'Produtividade',
      valor: formatar(dados?.produtividadePorHora, decimal, ' un/h'),
      legenda: 'Unidades produzidas por hora de trabalho apontada.',
      icone: 'mdi-factory',
    },
    {
      titulo: 'Cumprimento de prazo',
      valor: formatar(dados?.cumprimentoDePrazo, percentual),
      legenda: 'Ordens concluídas dentro do prazo.',
      icone: 'mdi-calendar-check-outline',
    },
  ]
})

const totais = computed(() => {
  const dados = indicadores.dados
  return [
    { titulo: 'Total produzido', valor: formatar(dados?.totalProduzido, inteiro, ' un'), icone: 'mdi-package-variant-closed' },
    { titulo: 'Total defeituoso', valor: formatar(dados?.totalDefeituoso, inteiro, ' un'), icone: 'mdi-close-octagon-outline' },
    { titulo: 'Tempo apontado', valor: formatar(dados?.tempoApontadoMin, inteiro, ' min'), icone: 'mdi-timer-outline' },
    { titulo: 'Ordens concluídas', valor: formatar(dados?.ordensConcluidas, inteiro), icone: 'mdi-clipboard-check-outline' },
  ]
})

function recarregar() {
  if (periodoInvalido.value) return
  indicadores.carregar(filtros)
}

function limpar() {
  Object.assign(filtros, { de: '', ate: '', produtoId: null, recursoId: null })
}

watch(filtros, recarregar)

onMounted(() => {
  produtos.carregar()
  recursos.carregar()
  recarregar()
})
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Indicadores</h1>
    <v-spacer />
    <v-btn variant="outlined" color="secondary" prepend-icon="mdi-refresh" :loading="indicadores.carregando" @click="recarregar">
      Atualizar
    </v-btn>
  </div>

  <v-alert v-if="indicadores.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Não foi possível carregar os indicadores.
    <template #append>
      <v-btn variant="text" size="small" @click="recarregar">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <v-row>
    <v-col cols="12" sm="6" md="3">
      <v-text-field v-model="filtros.de" label="De" type="date" hide-details="auto" :error="periodoInvalido" />
    </v-col>
    <v-col cols="12" sm="6" md="3">
      <v-text-field
        v-model="filtros.ate"
        label="Até"
        type="date"
        :error-messages="periodoInvalido ? 'A data final precisa ser igual ou posterior à inicial.' : ''"
        hide-details="auto"
      />
    </v-col>
    <v-col cols="12" sm="6" md="3">
      <v-autocomplete
        v-model="filtros.produtoId"
        :items="opcoesDeProduto"
        label="Produto"
        no-data-text="Nenhum produto encontrado."
        clearable
        hide-details
      />
    </v-col>
    <v-col cols="12" sm="6" md="3">
      <v-autocomplete
        v-model="filtros.recursoId"
        :items="opcoesDeRecurso"
        label="Recurso"
        no-data-text="Nenhum recurso encontrado."
        clearable
        hide-details
      />
    </v-col>
  </v-row>
  <div class="mt-3">
    <v-btn variant="text" color="secondary" prepend-icon="mdi-filter-remove-outline" :disabled="!filtrado" @click="limpar">
      Limpar filtros
    </v-btn>
  </div>

  <v-alert v-if="semResultado && !indicadores.carregando" type="info" variant="flat" density="compact" class="mt-4">
    Nenhum apontamento encontrado para os filtros escolhidos. Mude o filtro ou registre apontamentos.
  </v-alert>

  <v-row class="mt-2">
    <v-col v-for="cartao in cartoes" :key="cartao.titulo" cols="12" sm="6" md="3">
      <v-card :loading="indicadores.carregando" class="h-100">
        <v-card-text>
          <div class="d-flex align-center ga-3">
            <v-icon :icon="cartao.icone" color="secondary" size="28" />
            <div>
              <div class="fx-legenda">{{ cartao.titulo }}</div>
              <div class="text-h5">{{ cartao.valor }}</div>
            </div>
          </div>
          <div class="fx-legenda mt-3">{{ cartao.legenda }}</div>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>

  <div class="fx-titulo-secao mt-6 mb-3">Totais do período</div>
  <v-row>
    <v-col v-for="total in totais" :key="total.titulo" cols="12" sm="6" md="3">
      <v-card :loading="indicadores.carregando">
        <v-card-text class="d-flex align-center ga-3">
          <v-icon :icon="total.icone" color="secondary" size="28" />
          <div>
            <div class="fx-legenda">{{ total.titulo }}</div>
            <div class="text-h5">{{ total.valor }}</div>
          </div>
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

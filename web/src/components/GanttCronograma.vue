<script setup>
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { horaCurta, statusDaProgramacao } from '../utils/programacao'

const props = defineProps({ itens: { type: Array, required: true } })

const ALTURA_CABECALHO = 48
const ALTURA_BARRA = 30
const ESPACO_ENTRE_BARRAS = 6
const MARGEM_LINHA = 10
const LARGURA_ROTULOS = 170
const HORA_MS = 3600000

const niveisDeZoom = [
  { titulo: 'Compacto', pxPorHora: 24, passoHoras: 6 },
  { titulo: 'Normal', pxPorHora: 48, passoHoras: 3 },
  { titulo: 'Detalhado', pxPorHora: 96, passoHoras: 1 },
]

const router = useRouter()
const zoom = ref(1)
const selecionada = ref(null)

const nivel = computed(() => niveisDeZoom[zoom.value])
const formatadorDia = new Intl.DateTimeFormat('pt-BR', { weekday: 'short', day: '2-digit', month: '2-digit' })

const limites = computed(() => {
  const inicios = props.itens.map((item) => new Date(item.inicioPrevisto).getTime())
  const fins = props.itens.map((item) => new Date(item.fimPrevisto).getTime())
  const inicio = new Date(Math.min(...inicios))
  inicio.setHours(0, 0, 0, 0)
  const fim = new Date(Math.max(...fins))
  fim.setHours(0, 0, 0, 0)
  fim.setDate(fim.getDate() + 1)
  return { inicio, fim }
})

const posicaoX = (valor) => ((new Date(valor).getTime() - limites.value.inicio.getTime()) / HORA_MS) * nivel.value.pxPorHora

const largura = computed(() => ((limites.value.fim - limites.value.inicio) / HORA_MS) * nivel.value.pxPorHora)

const linhas = computed(() => {
  const grupos = new Map()
  const ordenados = [...props.itens].sort((a, b) => a.inicioPrevisto.localeCompare(b.inicioPrevisto))
  for (const item of ordenados) {
    if (!grupos.has(item.recursoId)) grupos.set(item.recursoId, { id: item.recursoId, nome: item.recursoNome, faixas: [], itens: [] })
    const grupo = grupos.get(item.recursoId)
    const inicio = new Date(item.inicioPrevisto).getTime()
    let faixa = grupo.faixas.findIndex((fimDaFaixa) => fimDaFaixa <= inicio)
    if (faixa === -1) faixa = grupo.faixas.length
    grupo.faixas[faixa] = new Date(item.fimPrevisto).getTime()
    grupo.itens.push({ ...item, faixa })
  }

  let topo = 0
  return [...grupos.values()]
    .sort((a, b) => a.nome.localeCompare(b.nome))
    .map((grupo) => {
      const altura = grupo.faixas.length * (ALTURA_BARRA + ESPACO_ENTRE_BARRAS) - ESPACO_ENTRE_BARRAS + MARGEM_LINHA * 2
      const linha = { ...grupo, topo, altura }
      topo += altura
      return linha
    })
})

const alturaTotal = computed(() => linhas.value.reduce((soma, linha) => soma + linha.altura, 0))

const dias = computed(() => {
  const lista = []
  const cursor = new Date(limites.value.inicio)
  while (cursor < limites.value.fim) {
    lista.push({
      chave: cursor.getTime(),
      x: posicaoX(cursor),
      titulo: formatadorDia.format(cursor).replace('.', ''),
      fimDeSemana: cursor.getDay() === 0 || cursor.getDay() === 6,
    })
    cursor.setDate(cursor.getDate() + 1)
  }
  return lista
})

const horas = computed(() => {
  const lista = []
  const totalHoras = (limites.value.fim - limites.value.inicio) / HORA_MS
  for (let hora = 0; hora < totalHoras; hora += nivel.value.passoHoras) {
    const data = new Date(limites.value.inicio.getTime() + hora * HORA_MS)
    lista.push({ chave: hora, x: hora * nivel.value.pxPorHora, rotulo: `${String(data.getHours()).padStart(2, '0')}h`, inicioDeDia: data.getHours() === 0 })
  }
  return lista
})

const agora = computed(() => {
  const instante = new Date()
  return instante >= limites.value.inicio && instante <= limites.value.fim ? posicaoX(instante) : null
})

function barra(item, linha) {
  const x = posicaoX(item.inicioPrevisto)
  return {
    x,
    y: linha.topo + MARGEM_LINHA + item.faixa * (ALTURA_BARRA + ESPACO_ENTRE_BARRAS),
    largura: Math.max(posicaoX(item.fimPrevisto) - x, 6),
  }
}

const corDoStatus = (status) => statusDaProgramacao[status].color
const abrir = (item) => router.push(`/ordens/${item.ordemId}`)
const descricao = (item) =>
  `${item.ordemNumero} · ${item.etapaNome}\n${item.recursoNome}\n${horaCurta(item.inicioPrevisto)} – ${horaCurta(item.fimPrevisto)} · ${statusDaProgramacao[item.status].title}`
</script>

<template>
  <v-card class="mt-4">
    <v-card-text>
      <div class="d-flex flex-wrap align-center ga-4 mb-4">
        <span v-for="(status, chave) in statusDaProgramacao" :key="chave" class="d-flex align-center ga-1 text-caption">
          <span class="legenda" :style="{ background: `rgb(var(--v-theme-${status.color}))` }" />{{ status.title }}
        </span>
        <span v-if="agora !== null" class="d-flex align-center ga-1 text-caption">
          <span class="legenda-agora" />Agora
        </span>
        <v-spacer />
        <v-btn-toggle v-model="zoom" mandatory density="compact" variant="outlined" color="primary" divided>
          <v-btn v-for="(opcao, indice) in niveisDeZoom" :key="opcao.titulo" :value="indice" class="text-none" size="small">
            {{ opcao.titulo }}
          </v-btn>
        </v-btn-toggle>
      </div>

      <div class="gantt">
        <div class="gantt-rotulos" :style="{ width: `${LARGURA_ROTULOS}px` }">
          <div class="gantt-rotulos-cabecalho" :style="{ height: `${ALTURA_CABECALHO}px` }">Recurso</div>
          <div
            v-for="linha in linhas"
            :key="linha.id"
            class="gantt-rotulo"
            :style="{ height: `${linha.altura}px` }"
            :title="linha.nome"
          >
            {{ linha.nome }}
          </div>
        </div>

        <div class="gantt-area">
          <svg
            :width="largura"
            :height="ALTURA_CABECALHO + alturaTotal"
            role="img"
            aria-label="Gantt da programação por recurso"
          >
            <g v-for="dia in dias" :key="dia.chave">
              <rect
                v-if="dia.fimDeSemana"
                :x="dia.x"
                :y="ALTURA_CABECALHO"
                :width="24 * nivel.pxPorHora"
                :height="alturaTotal"
                class="fim-de-semana"
              />
              <text :x="dia.x + 8" y="18" class="dia-texto">{{ dia.titulo }}</text>
            </g>

            <g v-for="hora in horas" :key="hora.chave">
              <line
                :x1="hora.x"
                :x2="hora.x"
                :y1="hora.inicioDeDia ? 0 : 26"
                :y2="ALTURA_CABECALHO + alturaTotal"
                :class="hora.inicioDeDia ? 'linha-dia' : 'linha-hora'"
              />
              <text :x="hora.x + 4" y="40" class="hora-texto">{{ hora.rotulo }}</text>
            </g>

            <line x1="0" :x2="largura" :y1="ALTURA_CABECALHO" :y2="ALTURA_CABECALHO" class="linha-dia" />
            <line
              v-for="linha in linhas"
              :key="`l-${linha.id}`"
              x1="0"
              :x2="largura"
              :y1="ALTURA_CABECALHO + linha.topo + linha.altura"
              :y2="ALTURA_CABECALHO + linha.topo + linha.altura"
              class="linha-dia"
            />

            <template v-for="linha in linhas" :key="`b-${linha.id}`">
              <svg
                v-for="item in linha.itens"
                :key="item.id"
                :x="barra(item, linha).x"
                :y="ALTURA_CABECALHO + barra(item, linha).y"
                :width="barra(item, linha).largura"
                :height="ALTURA_BARRA"
                class="barra"
                :class="{ apagada: selecionada !== null && selecionada !== item.ordemId }"
                tabindex="0"
                role="link"
                :aria-label="descricao(item)"
                @mouseenter="selecionada = item.ordemId"
                @mouseleave="selecionada = null"
                @focus="selecionada = item.ordemId"
                @blur="selecionada = null"
                @click="abrir(item)"
                @keyup.enter="abrir(item)"
              >
                <rect width="100%" :height="ALTURA_BARRA" rx="5" :fill="`rgb(var(--v-theme-${corDoStatus(item.status)}))`" class="barra-caixa" />
                <text x="8" y="19" :fill="`rgb(var(--v-theme-on-${corDoStatus(item.status)}))`" class="barra-texto">
                  {{ item.ordemNumero }} · {{ item.etapaNome }}
                </text>
                <title>{{ descricao(item) }}</title>
              </svg>
            </template>

            <line
              v-if="agora !== null"
              :x1="agora"
              :x2="agora"
              y1="0"
              :y2="ALTURA_CABECALHO + alturaTotal"
              class="linha-agora"
            />
          </svg>
        </div>
      </div>

      <div class="fx-legenda mt-2">Passe o mouse numa barra para destacar as etapas da mesma ordem. Clique para abrir a ordem.</div>
    </v-card-text>
  </v-card>
</template>

<style scoped>
.gantt {
  display: flex;
  border: 1px solid rgb(var(--v-theme-surface-variant));
  border-radius: 8px;
  overflow: hidden;
}

.gantt-rotulos {
  flex: none;
  border-right: 1px solid rgb(var(--v-theme-surface-variant));
  background: rgb(var(--v-theme-surface));
}

.gantt-rotulos-cabecalho {
  display: flex;
  align-items: flex-end;
  padding: 0 12px 8px;
  background: rgb(var(--v-theme-surface-variant));
  font-size: 12px;
  font-weight: 600;
}

.gantt-rotulo {
  display: flex;
  align-items: center;
  padding: 0 12px;
  border-bottom: 1px solid rgb(var(--v-theme-surface-variant));
  font-size: 13px;
  font-weight: 600;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.gantt-area {
  flex: 1;
  min-width: 0;
  overflow-x: auto;
}

.gantt-area svg {
  display: block;
}

.fim-de-semana {
  fill: rgb(var(--v-theme-surface-variant));
  fill-opacity: 0.55;
}

.linha-dia {
  stroke: rgb(var(--v-theme-secondary));
  stroke-opacity: 0.3;
}

.linha-hora {
  stroke: rgb(var(--v-theme-secondary));
  stroke-opacity: 0.1;
}

.linha-agora {
  stroke: rgb(var(--v-theme-error));
  stroke-width: 2;
  pointer-events: none;
}

.dia-texto {
  fill: rgb(var(--v-theme-on-surface));
  font-size: 12px;
  font-weight: 600;
  text-transform: capitalize;
}

.hora-texto {
  fill: rgb(var(--v-theme-text-muted));
  font-size: 10px;
  font-variant-numeric: tabular-nums;
}

.barra {
  cursor: pointer;
  outline: none;
  transition: opacity 0.15s;
}

.barra-caixa {
  stroke: transparent;
  stroke-width: 2;
}

.barra:hover .barra-caixa,
.barra:focus-visible .barra-caixa {
  stroke: rgb(var(--v-theme-on-surface));
}

.barra-texto {
  font-size: 12px;
  font-weight: 600;
  pointer-events: none;
}

.apagada {
  opacity: 0.3;
}

.legenda {
  display: inline-block;
  width: 10px;
  height: 10px;
  border-radius: 2px;
}

.legenda-agora {
  display: inline-block;
  width: 2px;
  height: 12px;
  background: rgb(var(--v-theme-error));
}
</style>

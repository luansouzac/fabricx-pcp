<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useProdutosStore } from '../stores/produtos'

const props = defineProps({ produto: { type: Object, required: true } })

const LARGURA = 200
const ALTURA = 56
const ESPACO_X = 90
const ESPACO_Y = 18
const PROFUNDIDADE_MAXIMA = 6

const router = useRouter()
const produtos = useProdutosStore()

const raiz = ref(null)
const carregando = ref(false)
const selecionado = ref(null)

const numero = new Intl.NumberFormat('pt-BR')

async function montarNo(dados, quantidade, ancestrais) {
  const no = { ...dados, quantidade, filhos: [], truncado: false }
  if (ancestrais.length >= PROFUNDIDADE_MAXIMA) {
    no.truncado = true
    return no
  }
  let estrutura = []
  try {
    estrutura = await produtos.listarEstrutura(dados.id)
  } catch {
    return no
  }
  const caminho = [...ancestrais, dados.id]
  no.filhos = await Promise.all(
    estrutura
      .filter((item) => !caminho.includes(item.componenteId))
      .map((item) => {
        const completo = produtos.itens.find((p) => p.id === item.componenteId)
        return montarNo(
          {
            id: item.componenteId,
            codigo: item.componenteCodigo,
            nome: item.componenteNome,
            unidadeMedida: item.unidadeMedida,
            estoqueAtual: completo?.estoqueAtual,
            estoqueAbaixoDoMinimo: completo?.estoqueAbaixoDoMinimo,
          },
          item.quantidade,
          caminho,
        )
      }),
  )
  return no
}

async function carregar() {
  carregando.value = true
  try {
    await produtos.carregar()
    raiz.value = await montarNo(props.produto, null, [])
  } finally {
    carregando.value = false
  }
}

const layout = computed(() => {
  if (!raiz.value) return { nos: [], ligacoes: [], largura: 0, altura: 0 }

  const nos = []
  const ligacoes = []
  let proximaLinha = 0

  function posicionar(no, profundidade) {
    no.x = profundidade * (LARGURA + ESPACO_X)
    if (no.filhos.length === 0) {
      no.y = proximaLinha * (ALTURA + ESPACO_Y)
      proximaLinha += 1
    } else {
      no.filhos.forEach((filho) => posicionar(filho, profundidade + 1))
      no.y = (no.filhos[0].y + no.filhos[no.filhos.length - 1].y) / 2
    }
    nos.push(no)
  }

  posicionar(raiz.value, 0)

  nos.forEach((no) =>
    no.filhos.forEach((filho) => {
      const x1 = no.x + LARGURA
      const y1 = no.y + ALTURA / 2
      const x2 = filho.x
      const y2 = filho.y + ALTURA / 2
      const meio = (x1 + x2) / 2
      ligacoes.push({
        chave: `${no.id}-${filho.id}-${filho.y}`,
        d: `M ${x1} ${y1} C ${meio} ${y1}, ${meio} ${y2}, ${x2} ${y2}`,
        rotuloX: meio,
        rotuloY: (y1 + y2) / 2,
        rotulo: `${numero.format(filho.quantidade)} ${filho.unidadeMedida}`,
        origem: no,
        destino: filho,
      })
    }),
  )

  const profundidadeMaxima = Math.max(...nos.map((n) => n.x)) / (LARGURA + ESPACO_X)
  return {
    nos,
    ligacoes,
    largura: (profundidadeMaxima + 1) * LARGURA + profundidadeMaxima * ESPACO_X,
    altura: Math.max(proximaLinha, 1) * (ALTURA + ESPACO_Y) - ESPACO_Y,
  }
})

const semComponentes = computed(() => raiz.value && raiz.value.filhos.length === 0)

function classeDoNo(no) {
  if (no.truncado) return 'no-truncado'
  if (no.estoqueAbaixoDoMinimo === true) return 'no-baixo'
  if (no.estoqueAbaixoDoMinimo === false) return 'no-bom'
  return ''
}

function destacado(no) {
  return selecionado.value === null || selecionado.value === no.id
}

function ligacaoDestacada(ligacao) {
  return selecionado.value === null || ligacao.origem.id === selecionado.value || ligacao.destino.id === selecionado.value
}

function cortar(texto, limite) {
  return texto.length > limite ? `${texto.slice(0, limite - 1)}…` : texto
}

function abrir(no) {
  if (no.id !== props.produto.id) router.push(`/produtos/${no.id}/diagrama`)
}

onMounted(carregar)
watch(() => props.produto.id, carregar)
</script>

<template>
  <v-card class="mt-4" title="Diagrama da estrutura" subtitle="Cada linha mostra quanto do componente é usado para produzir uma unidade do produto.">
    <v-card-text>
      <div class="d-flex flex-wrap ga-4 mb-4 text-caption">
        <span class="d-flex align-center ga-1"><span class="legenda legenda-bom" />Estoque acima do mínimo</span>
        <span class="d-flex align-center ga-1"><span class="legenda legenda-baixo" />Estoque abaixo do mínimo</span>
        <span class="text-medium-emphasis">Passe o mouse para destacar um caminho. Clique num componente para abrir o diagrama dele.</span>
      </div>

      <v-progress-linear v-if="carregando" indeterminate color="primary" />

      <v-alert v-else-if="semComponentes" type="info" variant="tonal" density="compact">
        Este produto ainda não tem componentes. Cadastre-os na aba Estrutura.
      </v-alert>

      <div v-else-if="raiz" class="diagrama-area">
        <svg
          :width="layout.largura + 4"
          :height="layout.altura + 4"
          :viewBox="`-2 -2 ${layout.largura + 4} ${layout.altura + 4}`"
          role="img"
          :aria-label="`Diagrama da estrutura de ${produto.nome}`"
        >
          <path
            v-for="ligacao in layout.ligacoes"
            :key="ligacao.chave"
            :d="ligacao.d"
            class="ligacao"
            :class="{ apagado: !ligacaoDestacada(ligacao) }"
          />
          <g v-for="ligacao in layout.ligacoes" :key="`r-${ligacao.chave}`" :class="{ apagado: !ligacaoDestacada(ligacao) }">
            <rect :x="ligacao.rotuloX - 34" :y="ligacao.rotuloY - 10" width="68" height="20" rx="10" class="rotulo-fundo" />
            <text :x="ligacao.rotuloX" :y="ligacao.rotuloY + 4" text-anchor="middle" class="rotulo-texto">{{ ligacao.rotulo }}</text>
          </g>

          <g
            v-for="no in layout.nos"
            :key="`${no.id}-${no.x}-${no.y}`"
            :transform="`translate(${no.x}, ${no.y})`"
            class="no"
            :class="[classeDoNo(no), { raiz: no === raiz, apagado: !destacado(no) }]"
            tabindex="0"
            role="link"
            :aria-label="`${no.codigo} ${no.nome}`"
            @mouseenter="selecionado = no.id"
            @mouseleave="selecionado = null"
            @focus="selecionado = no.id"
            @blur="selecionado = null"
            @click="abrir(no)"
            @keyup.enter="abrir(no)"
          >
            <rect :width="LARGURA" :height="ALTURA" rx="8" class="no-caixa" />
            <rect width="5" :height="ALTURA" rx="2" class="no-faixa" />
            <text x="16" y="22" class="no-nome">{{ cortar(no.nome, 24) }}</text>
            <text x="16" y="40" class="no-detalhe">
              {{ no.codigo }}<template v-if="no.estoqueAtual !== undefined"> · {{ numero.format(no.estoqueAtual) }} {{ no.unidadeMedida }}</template>
              <template v-if="no.truncado"> · …</template>
            </text>
            <title>{{ no.codigo }} · {{ no.nome }}</title>
          </g>
        </svg>
      </div>
    </v-card-text>
  </v-card>
</template>

<style scoped>
.diagrama-area {
  overflow: auto;
  padding-bottom: 8px;
}

.ligacao {
  fill: none;
  stroke: rgb(var(--v-theme-secondary));
  stroke-opacity: 0.45;
  stroke-width: 1.5;
  transition: opacity 0.15s;
}

.rotulo-fundo {
  fill: rgb(var(--v-theme-surface-variant));
}

.rotulo-texto {
  fill: rgb(var(--v-theme-on-surface));
  font-size: 11px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.no {
  cursor: pointer;
  outline: none;
  transition: opacity 0.15s;
}

.no-caixa {
  fill: rgb(var(--v-theme-surface));
  stroke: rgb(var(--v-theme-secondary));
  stroke-opacity: 0.4;
  stroke-width: 1;
}

.no:hover .no-caixa,
.no:focus-visible .no-caixa {
  stroke: rgb(var(--v-theme-primary));
  stroke-opacity: 1;
  stroke-width: 2;
}

.no-faixa {
  fill: rgb(var(--v-theme-secondary));
}

.no-bom .no-faixa {
  fill: rgb(var(--v-theme-success));
}

.no-baixo .no-faixa {
  fill: rgb(var(--v-theme-warning));
}

.raiz .no-caixa {
  fill: rgb(var(--v-theme-primary-lighten-4));
  stroke: rgb(var(--v-theme-primary));
  stroke-opacity: 1;
}

.raiz .no-faixa {
  fill: rgb(var(--v-theme-primary));
}

.no-nome {
  fill: rgb(var(--v-theme-on-surface));
  font-size: 13px;
  font-weight: 600;
}

.no-detalhe {
  fill: rgb(var(--v-theme-text-muted));
  font-size: 11px;
  font-variant-numeric: tabular-nums;
}

.apagado {
  opacity: 0.25;
}

.legenda {
  display: inline-block;
  width: 10px;
  height: 10px;
  border-radius: 2px;
}

.legenda-bom {
  background: rgb(var(--v-theme-success));
}

.legenda-baixo {
  background: rgb(var(--v-theme-warning));
}
</style>

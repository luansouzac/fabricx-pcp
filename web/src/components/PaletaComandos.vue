<script setup>
import { computed, nextTick, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api/cliente'
import { useAuthStore } from '../stores/auth'

const aberta = defineModel({ type: Boolean, default: false })
const emit = defineEmits(['alternar-tema'])

const MAXIMO_POR_GRUPO = 6

const router = useRouter()
const auth = useAuthStore()

const busca = ref('')
const indice = ref(0)
const carregando = ref(false)
const campo = ref(null)
const lista = ref(null)
const produtos = ref([])
const ordens = ref([])
const recursos = ref([])

const normalizar = (texto) => String(texto ?? '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase()

const comandosFixos = computed(() => {
  const itens = [
    { grupo: 'Ir para', titulo: 'Início', icone: 'mdi-view-dashboard-outline', rota: '/' },
    { grupo: 'Ir para', titulo: 'Planejamento', icone: 'mdi-chart-timeline-variant', rota: '/planejamento' },
    { grupo: 'Ir para', titulo: 'Produção', icone: 'mdi-clipboard-text-outline', rota: '/producao' },
    { grupo: 'Ir para', titulo: 'Cronograma (Gantt)', icone: 'mdi-chart-gantt', rota: '/producao?aba=cronograma' },
    { grupo: 'Ir para', titulo: 'Indicadores', icone: 'mdi-chart-line', rota: '/indicadores' },
    { grupo: 'Ir para', titulo: 'Produtos', icone: 'mdi-package-variant-closed', rota: '/produtos' },
    { grupo: 'Ir para', titulo: 'Recursos', icone: 'mdi-robot-industrial-outline', rota: '/recursos' },
    { grupo: 'Ações', titulo: 'Nova ordem de produção', icone: 'mdi-plus', rota: '/ordens/nova' },
    { grupo: 'Ações', titulo: 'Novo produto', icone: 'mdi-plus', rota: '/produtos/novo' },
    { grupo: 'Ações', titulo: 'Novo recurso', icone: 'mdi-plus', rota: '/recursos/novo' },
    { grupo: 'Ações', titulo: 'Alternar tema claro e escuro', icone: 'mdi-theme-light-dark', acao: () => emit('alternar-tema') },
  ]
  if (auth.ehAdministrador) {
    itens.splice(7, 0, { grupo: 'Ir para', titulo: 'Usuários', icone: 'mdi-account-multiple-outline', rota: '/usuarios' })
  }
  return itens
})

const itensDeDados = computed(() => [
  ...ordens.value.map((ordem) => ({
    grupo: 'Ordens',
    titulo: `${ordem.numero} · ${ordem.produtoNome}`,
    subtitulo: ordem.status,
    icone: 'mdi-clipboard-text-outline',
    rota: `/ordens/${ordem.id}`,
    texto: `${ordem.numero} ${ordem.produtoCodigo} ${ordem.produtoNome}`,
  })),
  ...produtos.value.flatMap((produto) => [
    {
      grupo: 'Produtos',
      titulo: `${produto.codigo} · ${produto.nome}`,
      icone: 'mdi-package-variant-closed',
      rota: `/produtos/${produto.id}`,
      texto: `${produto.codigo} ${produto.nome}`,
    },
    {
      grupo: 'Produtos',
      titulo: `Diagrama · ${produto.nome}`,
      subtitulo: produto.codigo,
      icone: 'mdi-family-tree',
      rota: `/produtos/${produto.id}/diagrama`,
      texto: `diagrama estrutura ${produto.codigo} ${produto.nome}`,
    },
  ]),
  ...recursos.value.map((recurso) => ({
    grupo: 'Recursos',
    titulo: recurso.nome,
    icone: 'mdi-robot-industrial-outline',
    rota: `/recursos/${recurso.id}/editar`,
    texto: recurso.nome,
  })),
])

const resultados = computed(() => {
  const termos = normalizar(busca.value).split(/\s+/).filter(Boolean)
  const fontes = termos.length ? [...comandosFixos.value, ...itensDeDados.value] : comandosFixos.value
  const contagem = {}
  return fontes.filter((item) => {
    const alvo = normalizar(item.texto ?? item.titulo)
    if (!termos.every((termo) => alvo.includes(termo))) return false
    contagem[item.grupo] = (contagem[item.grupo] ?? 0) + 1
    return contagem[item.grupo] <= MAXIMO_POR_GRUPO || !termos.length
  })
})

const grupos = computed(() => {
  const mapa = new Map()
  resultados.value.forEach((item, posicao) => {
    if (!mapa.has(item.grupo)) mapa.set(item.grupo, [])
    mapa.get(item.grupo).push({ ...item, posicao })
  })
  return [...mapa].map(([nome, itens]) => ({ nome, itens }))
})

async function carregarDados() {
  carregando.value = true
  const [listaProdutos, listaOrdens, listaRecursos] = await Promise.all([
    api.get('/api/produtos').catch(() => []),
    api.get('/api/ordens-producao').catch(() => []),
    api.get('/api/recursos').catch(() => []),
  ])
  produtos.value = listaProdutos
  ordens.value = listaOrdens
  recursos.value = listaRecursos
  carregando.value = false
}

function executar(item) {
  aberta.value = false
  if (item.acao) item.acao()
  else router.push(item.rota)
}

function aoTeclar(evento) {
  if (!resultados.value.length) return
  if (evento.key === 'ArrowDown') {
    evento.preventDefault()
    indice.value = (indice.value + 1) % resultados.value.length
  } else if (evento.key === 'ArrowUp') {
    evento.preventDefault()
    indice.value = (indice.value - 1 + resultados.value.length) % resultados.value.length
  } else if (evento.key === 'Enter') {
    evento.preventDefault()
    executar(resultados.value[indice.value])
  }
}

watch(busca, () => (indice.value = 0))

watch(indice, async () => {
  await nextTick()
  lista.value?.querySelector('[data-ativo="true"]')?.scrollIntoView({ block: 'nearest' })
})

watch(aberta, async (valor) => {
  if (!valor) return
  busca.value = ''
  indice.value = 0
  carregarDados()
  await nextTick()
  campo.value?.focus()
})
</script>

<template>
  <v-dialog v-model="aberta" max-width="600" location="top" class="paleta" scrim>
    <v-card rounded="lg">
      <v-text-field
        ref="campo"
        v-model="busca"
        placeholder="Buscar páginas, produtos, ordens e recursos"
        prepend-inner-icon="mdi-magnify"
        variant="solo"
        flat
        hide-details
        autocomplete="off"
        aria-label="Buscar"
        @keydown="aoTeclar"
      />
      <v-progress-linear v-if="carregando" indeterminate color="primary" height="2" />
      <v-divider v-else />

      <div ref="lista" class="paleta-lista" role="listbox">
        <div v-if="!resultados.length" class="text-center text-medium-emphasis pa-6">
          Nenhum resultado para "{{ busca }}". Tente outro termo.
        </div>

        <template v-for="grupo in grupos" :key="grupo.nome">
          <div class="fx-legenda px-4 pt-3 pb-1">{{ grupo.nome }}</div>
          <v-list-item
            v-for="item in grupo.itens"
            :key="item.posicao"
            :prepend-icon="item.icone"
            :title="item.titulo"
            :subtitle="item.subtitulo"
            :active="item.posicao === indice"
            :data-ativo="item.posicao === indice"
            color="primary"
            density="compact"
            role="option"
            :aria-selected="item.posicao === indice"
            @mousemove="indice = item.posicao"
            @click="executar(item)"
          />
        </template>
      </div>

      <v-divider />
      <div class="d-flex ga-4 px-4 py-2 text-caption text-medium-emphasis">
        <span><kbd>↑</kbd> <kbd>↓</kbd> navegar</span>
        <span><kbd>Enter</kbd> abrir</span>
        <span><kbd>Esc</kbd> fechar</span>
      </div>
    </v-card>
  </v-dialog>
</template>

<style scoped>
.paleta-lista {
  max-height: 380px;
  overflow-y: auto;
  padding-bottom: 8px;
}

kbd {
  padding: 1px 6px;
  border: 1px solid rgb(var(--v-theme-surface-variant));
  border-radius: 4px;
  background: rgb(var(--v-theme-surface-variant));
  font-family: inherit;
  font-size: 11px;
}
</style>

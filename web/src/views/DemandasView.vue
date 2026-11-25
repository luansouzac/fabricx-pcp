<script setup>
import { onMounted, ref, watch } from 'vue'
import ConfirmacaoDialog from '../components/ConfirmacaoDialog.vue'
import { useDemandasStore } from '../stores/demandas'
import { useProdutosStore } from '../stores/produtos'
import { formatarPeriodo, formatarQuantidade } from '../utils/demanda'

const demandas = useDemandasStore()
const produtos = useProdutosStore()

const busca = ref('')
const periodo = ref('')
const demandaParaExcluir = ref(null)
const confirmandoExclusao = ref(false)
const excluindo = ref(false)

const colunas = [
  { title: 'Produto', key: 'produtoNome' },
  { title: 'Período', key: 'periodo' },
  { title: 'Quantidade', key: 'quantidade' },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const filtrar = (valor, consulta, item) => {
  if (!consulta) return true
  const termo = consulta.toLowerCase()
  return item.raw.produtoNome.toLowerCase().includes(termo) || item.raw.produtoCodigo.toLowerCase().includes(termo)
}

const unidadeDe = (produtoId) => produtos.itens.find((produto) => produto.id === produtoId)?.unidadeMedida ?? ''

function pedirExclusao(demanda) {
  demandaParaExcluir.value = demanda
  confirmandoExclusao.value = true
}

async function excluir() {
  excluindo.value = true
  try {
    await demandas.remover(demandaParaExcluir.value.id)
    confirmandoExclusao.value = false
    await demandas.carregar(periodo.value)
  } catch {
    confirmandoExclusao.value = false
  } finally {
    excluindo.value = false
  }
}

watch(periodo, (valor) => demandas.carregar(valor))

onMounted(() => {
  produtos.carregar()
  demandas.carregar(periodo.value)
})
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Demanda prevista</h1>
    <v-spacer />
    <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" to="/demanda-prevista/nova">Nova demanda</v-btn>
  </div>

  <v-alert v-if="demandas.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Não foi possível carregar a demanda prevista.
    <template #append>
      <v-btn variant="text" size="small" @click="demandas.carregar(periodo)">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <v-row>
    <v-col cols="12" md="6">
      <v-text-field v-model="busca" label="Buscar por produto" prepend-inner-icon="mdi-magnify" clearable hide-details />
    </v-col>
    <v-col cols="12" md="3">
      <v-text-field v-model="periodo" label="Período" type="month" clearable hide-details />
    </v-col>
  </v-row>

  <v-data-table
    :headers="colunas"
    :items="demandas.itens"
    :loading="demandas.carregando"
    :search="busca ?? ''"
    :custom-filter="filtrar"
    items-per-page-text="Itens por página"
    no-data-text="Nenhuma demanda encontrada. Crie uma demanda ou mude o filtro."
    loading-text="Carregando demanda prevista..."
    class="mt-4"
  >
    <template #item.produtoNome="{ item }">{{ item.produtoCodigo }} · {{ item.produtoNome }}</template>
    <template #item.periodo="{ item }">{{ formatarPeriodo(item.periodo) }}</template>
    <template #item.quantidade="{ item }">{{ formatarQuantidade(item.quantidade) }} {{ unidadeDe(item.produtoId) }}</template>
    <template #item.acoes="{ item }">
      <v-btn icon="mdi-pencil-outline" variant="text" size="small" color="secondary" aria-label="Editar demanda" title="Editar" :to="`/demanda-prevista/${item.id}/editar`" />
      <v-btn icon="mdi-delete-outline" variant="text" size="small" color="error" aria-label="Excluir demanda" title="Excluir" @click="pedirExclusao(item)" />
    </template>
  </v-data-table>

  <ConfirmacaoDialog
    v-model="confirmandoExclusao"
    titulo="Excluir demanda prevista"
    :mensagem="`Excluir a demanda de ${demandaParaExcluir?.produtoNome ?? ''} em ${demandaParaExcluir ? formatarPeriodo(demandaParaExcluir.periodo) : ''}? Ela deixará de entrar no cálculo de capacidade.`"
    texto-confirmar="Excluir"
    :carregando="excluindo"
    @confirmar="excluir"
  />
</template>

<script setup>
import { ref, watch } from 'vue'
import { useProdutosStore } from '../stores/produtos'

const props = defineProps({
  produtoId: { type: String, required: true },
  versao: { type: Number, default: 0 },
})

const produtos = useProdutosStore()

const movimentacoes = ref([])
const carregando = ref(false)

const colunas = [
  { title: 'Data e hora', key: 'dataHora' },
  { title: 'Tipo', key: 'tipo' },
  { title: 'Quantidade', key: 'quantidade' },
  { title: 'Motivo', key: 'motivo' },
].map((coluna) => ({ ...coluna, align: 'center' }))

const formatadorDataHora = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

// a API devolve a data em UTC sem o sufixo Z
const formatarDataHora = (valor) => formatadorDataHora.format(new Date(/Z$|[+-]\d\d:\d\d$/.test(valor) ? valor : `${valor}Z`))

async function carregar() {
  carregando.value = true
  try {
    movimentacoes.value = await produtos.listarMovimentacoes(props.produtoId)
  } catch {
    movimentacoes.value = []
  } finally {
    carregando.value = false
  }
}

watch(() => [props.produtoId, props.versao], carregar, { immediate: true })
</script>

<template>
  <v-card class="mt-6" title="Histórico de movimentações">
    <v-data-table
      :headers="colunas"
      :items="movimentacoes"
      :loading="carregando"
      :sort-by="[{ key: 'dataHora', order: 'desc' }]"
      items-per-page-text="Itens por página"
      no-data-text="Nenhuma movimentação registrada."
      loading-text="Carregando histórico..."
    >
      <template #item.dataHora="{ item }">{{ formatarDataHora(item.dataHora) }}</template>
      <template #item.tipo="{ item }">
        <v-chip
          :color="item.tipo === 'Entrada' ? 'success' : 'warning'"
          :prepend-icon="item.tipo === 'Entrada' ? 'mdi-arrow-down-bold' : 'mdi-arrow-up-bold'"
          size="small"
        >
          {{ item.tipo === 'Entrada' ? 'Entrada' : 'Saída' }}
        </v-chip>
      </template>
      <template #item.motivo="{ item }">{{ item.motivo || '—' }}</template>
    </v-data-table>
  </v-card>
</template>

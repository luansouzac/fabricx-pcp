<script setup>
import { onMounted, ref, watch } from 'vue'
import { useOrdensStore } from '../stores/ordens'
import { dataCurta, infoDaPrioridade, infoDoStatus, statusDaOrdem } from '../utils/ordem'

const ordens = useOrdensStore()

const busca = ref('')
const statusSelecionado = ref(null)

const colunas = [
  { title: 'Número', key: 'numero' },
  { title: 'Produto', key: 'produtoNome' },
  { title: 'Quantidade', key: 'quantidade' },
  { title: 'Prazo de entrega', key: 'prazoEntrega' },
  { title: 'Prioridade', key: 'prioridade' },
  { title: 'Status', key: 'status' },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const filtrar = (valor, consulta, item) => {
  const termo = (consulta ?? '').toLowerCase()
  return !termo || `${item.raw.numero} ${item.raw.produtoCodigo} ${item.raw.produtoNome}`.toLowerCase().includes(termo)
}

onMounted(() => ordens.carregar(statusSelecionado.value))
watch(statusSelecionado, (status) => ordens.carregar(status))
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Ordens de produção</h1>
    <v-spacer />
    <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" to="/ordens/nova">Nova ordem</v-btn>
  </div>

  <v-row>
    <v-col cols="12" md="6">
      <v-text-field
        v-model="busca"
        label="Buscar por número ou produto"
        prepend-inner-icon="mdi-magnify"
        clearable
        hide-details
      />
    </v-col>
    <v-col cols="12" md="3">
      <v-select v-model="statusSelecionado" :items="statusDaOrdem" label="Status" clearable hide-details />
    </v-col>
  </v-row>

  <v-data-table
    :headers="colunas"
    :items="ordens.itens"
    :loading="ordens.carregando"
    :search="busca ?? ''"
    :custom-filter="filtrar"
    items-per-page-text="Itens por página"
    no-data-text="Nenhuma ordem encontrada. Crie uma ordem ou mude o filtro."
    loading-text="Carregando ordens..."
    class="mt-4"
  >
    <template #item.produtoNome="{ item }">{{ item.produtoCodigo }} · {{ item.produtoNome }}</template>
    <template #item.prazoEntrega="{ item }">{{ dataCurta(item.prazoEntrega) }}</template>
    <template #item.prioridade="{ item }">
      <v-chip :color="infoDaPrioridade[item.prioridade].color" variant="flat" size="small">
        {{ infoDaPrioridade[item.prioridade].title }}
      </v-chip>
    </template>
    <template #item.status="{ item }">
      <v-chip :color="infoDoStatus[item.status].color" variant="flat" size="small">
        {{ infoDoStatus[item.status].title }}
      </v-chip>
    </template>
    <template #item.acoes="{ item }">
      <v-btn icon="mdi-eye-outline" variant="text" size="small" color="secondary" aria-label="Ver detalhes" title="Ver detalhes" :to="`/ordens/${item.id}`" />
    </template>
  </v-data-table>
</template>

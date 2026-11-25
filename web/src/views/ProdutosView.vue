<script setup>
import { onMounted, ref, watch } from 'vue'
import { useProdutosStore } from '../stores/produtos'
import { moeda, nomeDoTipo, tiposDeProduto as tipos } from '../utils/produto'

const produtos = useProdutosStore()

const busca = ref('')
const tipoSelecionado = ref(null)

const colunas = [
  { title: 'Código', key: 'codigo' },
  { title: 'Nome', key: 'nome' },
  { title: 'Tipo', key: 'tipo' },
  { title: 'Unidade', key: 'unidadeMedida' },
  { title: 'Estoque atual', key: 'estoqueAtual' },
  { title: 'Estoque mínimo', key: 'estoqueMinimo' },
  { title: 'Custo unitário', key: 'custoUnitario' },
  { title: 'Situação', key: 'ativo' },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const propriedadesDaLinha = ({ item }) => (item.estoqueAbaixoDoMinimo ? { class: 'linha-estoque-baixo' } : {})

onMounted(() => produtos.carregar(tipoSelecionado.value))
watch(tipoSelecionado, (tipo) => produtos.carregar(tipo))
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Produtos</h1>
    <v-spacer />
    <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" to="/produtos/novo">Novo produto</v-btn>
  </div>

  <v-alert v-if="produtos.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Não foi possível carregar os produtos.
    <template #append>
      <v-btn variant="text" size="small" @click="produtos.carregar(tipoSelecionado)">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <v-row>
    <v-col cols="12" md="6">
      <v-text-field
        v-model="busca"
        label="Buscar por código ou nome"
        prepend-inner-icon="mdi-magnify"
        clearable
        hide-details
      />
    </v-col>
    <v-col cols="12" md="3">
      <v-select
        v-model="tipoSelecionado"
        :items="tipos"
        label="Tipo"
        clearable
        hide-details
      />
    </v-col>
  </v-row>

  <v-data-table
    :headers="colunas"
    :items="produtos.itens"
    :loading="produtos.carregando"
    :search="busca"
    :row-props="propriedadesDaLinha"
    items-per-page-text="Itens por página"
    no-data-text="Nenhum produto encontrado."
    loading-text="Carregando produtos..."
    class="mt-4"
  >
    <template #item.tipo="{ item }">{{ nomeDoTipo[item.tipo] }}</template>
    <template #item.estoqueAtual="{ item }">
      <span :class="item.estoqueAbaixoDoMinimo ? 'estoque-baixo' : 'estoque-bom'">{{ item.estoqueAtual }}</span>
    </template>
    <template #item.custoUnitario="{ item }">{{ moeda.format(item.custoUnitario) }}</template>
    <template #item.ativo="{ item }">
      <v-chip :color="item.ativo ? 'success' : 'grey'" variant="flat" size="small">{{ item.ativo ? 'Ativo' : 'Inativo' }}</v-chip>
    </template>
    <template #item.acoes="{ item }">
      <v-btn icon="mdi-eye-outline" variant="text" size="small" color="secondary" aria-label="Ver detalhes" :to="`/produtos/${item.id}`" title="Ver detalhes" />
    </template>
  </v-data-table>
</template>

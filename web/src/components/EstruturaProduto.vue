<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import ConfirmacaoDialog from './ConfirmacaoDialog.vue'
import { useProdutosStore } from '../stores/produtos'

const props = defineProps({ produtoId: { type: String, required: true } })

const produtos = useProdutosStore()

const itens = ref([])
const carregando = ref(false)
const adicionando = ref(false)
const formulario = reactive({ componenteId: null, quantidade: 1 })
const referenciaFormulario = ref(null)

const itemEmEdicao = ref(null)
const quantidadeEditada = ref(null)
const salvandoEdicao = ref(false)
const editando = ref(false)

const itemParaRemover = ref(null)
const confirmandoRemocao = ref(false)
const removendo = ref(false)

const colunas = [
  { title: 'Componente', key: 'componenteNome' },
  { title: 'Quantidade', key: 'quantidade' },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const opcoes = computed(() =>
  produtos.itens
    .filter((p) => p.ativo && String(p.id) !== props.produtoId && !itens.value.some((i) => i.componenteId === p.id))
    .map((p) => ({ title: `${p.codigo} · ${p.nome}`, value: p.id })),
)

const unidade = computed(() => produtos.itens.find((p) => p.id === formulario.componenteId)?.unidadeMedida)

const regras = {
  componente: [(valor) => !!valor || 'Selecione o componente.'],
  quantidade: [(valor) => Number(valor) > 0 || 'Informe uma quantidade maior que zero.'],
}

async function carregar() {
  carregando.value = true
  try {
    itens.value = await produtos.listarEstrutura(props.produtoId)
  } catch {
    itens.value = []
  } finally {
    carregando.value = false
  }
}

async function adicionar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  adicionando.value = true
  try {
    await produtos.adicionarComponente(props.produtoId, {
      componenteId: formulario.componenteId,
      quantidade: Number(formulario.quantidade),
    })
    Object.assign(formulario, { componenteId: null, quantidade: 1 })
    referenciaFormulario.value.resetValidation()
    await carregar()
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    adicionando.value = false
  }
}

function abrirEdicao(item) {
  itemEmEdicao.value = item
  quantidadeEditada.value = item.quantidade
  editando.value = true
}

async function salvarEdicao() {
  if (!(Number(quantidadeEditada.value) > 0)) return

  salvandoEdicao.value = true
  try {
    await produtos.alterarQuantidadeDoComponente(props.produtoId, itemEmEdicao.value.componenteId, Number(quantidadeEditada.value))
    editando.value = false
    await carregar()
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    salvandoEdicao.value = false
  }
}

function pedirRemocao(item) {
  itemParaRemover.value = item
  confirmandoRemocao.value = true
}

async function remover() {
  removendo.value = true
  try {
    await produtos.removerComponente(props.produtoId, itemParaRemover.value.componenteId)
    confirmandoRemocao.value = false
    await carregar()
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    removendo.value = false
  }
}

onMounted(async () => {
  await Promise.all([produtos.carregar(), carregar()])
})
</script>

<template>
  <v-card class="mt-4" title="Estrutura do produto" subtitle="Componentes e quantidades necessárias para produzir uma unidade.">
    <v-card-text>
      <v-form ref="referenciaFormulario" @submit.prevent="adicionar">
        <v-row align="start">
          <v-col cols="12" md="6">
            <v-autocomplete
              v-model="formulario.componenteId"
              :items="opcoes"
              label="Componente"
              no-data-text="Nenhum produto disponível."
              :rules="regras.componente"
            />
          </v-col>
          <v-col cols="12" md="3">
            <v-text-field
              v-model.number="formulario.quantidade"
              label="Quantidade"
              type="number"
              min="0"
              :suffix="unidade"
              :rules="regras.quantidade"
            />
          </v-col>
          <v-col cols="12" md="3">
            <v-btn type="submit" color="primary" variant="flat" prepend-icon="mdi-plus" :loading="adicionando" height="40">
              Adicionar
            </v-btn>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>

    <v-data-table
      :headers="colunas"
      :items="itens"
      :loading="carregando"
      items-per-page-text="Itens por página"
      no-data-text="Nenhum componente na estrutura. Adicione o primeiro acima."
      loading-text="Carregando estrutura..."
    >
      <template #item.componenteNome="{ item }">{{ item.componenteCodigo }} · {{ item.componenteNome }}</template>
      <template #item.quantidade="{ item }">{{ item.quantidade }} {{ item.unidadeMedida }}</template>
      <template #item.acoes="{ item }">
        <v-btn icon="mdi-pencil-outline" variant="text" size="small" color="secondary" aria-label="Alterar quantidade" title="Alterar quantidade" @click="abrirEdicao(item)" />
        <v-btn icon="mdi-delete-outline" variant="text" size="small" color="error" aria-label="Remover componente" title="Remover" @click="pedirRemocao(item)" />
      </template>
    </v-data-table>
  </v-card>

  <v-dialog v-model="editando" max-width="400" :persistent="salvandoEdicao">
    <v-card :title="`Alterar quantidade`" :subtitle="itemEmEdicao ? `${itemEmEdicao.componenteCodigo} · ${itemEmEdicao.componenteNome}` : ''">
      <v-card-text>
        <v-text-field
          v-model.number="quantidadeEditada"
          label="Quantidade"
          type="number"
          min="0"
          :suffix="itemEmEdicao?.unidadeMedida"
          :error-messages="Number(quantidadeEditada) > 0 ? [] : ['Informe uma quantidade maior que zero.']"
          autofocus
          @keyup.enter="salvarEdicao"
        />
      </v-card-text>
      <v-card-actions class="px-4 pb-4">
        <v-spacer />
        <v-btn variant="text" color="secondary" :disabled="salvandoEdicao" @click="editando = false">Cancelar</v-btn>
        <v-btn variant="flat" color="primary" :loading="salvandoEdicao" @click="salvarEdicao">Salvar</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>

  <ConfirmacaoDialog
    v-model="confirmandoRemocao"
    titulo="Remover componente"
    :mensagem="`Remover ${itemParaRemover?.componenteNome ?? ''} da estrutura deste produto?`"
    texto-confirmar="Remover"
    :carregando="removendo"
    @confirmar="remover"
  />
</template>

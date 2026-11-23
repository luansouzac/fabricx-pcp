<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import ConfirmacaoDialog from '../components/ConfirmacaoDialog.vue'
import HistoricoMovimentacoes from '../components/HistoricoMovimentacoes.vue'
import MovimentacaoDialog from '../components/MovimentacaoDialog.vue'
import { useProdutosStore } from '../stores/produtos'
import { moeda, nomeDoTipo } from '../utils/produto'

const props = defineProps({ id: { type: String, required: true } })

const router = useRouter()
const produtos = useProdutosStore()

const produto = ref(null)
const carregando = ref(true)
const movimentando = ref(false)
const confirmandoInativacao = ref(false)
const inativando = ref(false)
const versaoHistorico = ref(0)

function aoMovimentar(produtoAtualizado) {
  produto.value = produtoAtualizado
  versaoHistorico.value += 1
}

onMounted(async () => {
  try {
    produto.value = await produtos.obter(props.id)
  } catch {
    router.replace('/produtos')
  } finally {
    carregando.value = false
  }
})

async function inativar() {
  inativando.value = true
  try {
    produto.value = await produtos.inativar(props.id)
    confirmandoInativacao.value = false
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    inativando.value = false
  }
}
</script>

<template>
  <div class="d-flex align-center mb-4">
    <v-btn icon="mdi-arrow-left" variant="text" color="secondary" to="/produtos" title="Voltar" aria-label="Voltar" class="mr-2" />
    <h1 class="fx-titulo-pagina">Detalhe do produto</h1>
  </div>

  <v-card :loading="carregando">
    <template v-if="produto">
      <v-card-title class="d-flex align-center flex-wrap ga-2">
        {{ produto.nome }}
        <v-chip :color="produto.ativo ? 'success' : 'grey'" size="small">{{ produto.ativo ? 'Ativo' : 'Inativo' }}</v-chip>
      </v-card-title>
      <v-card-subtitle>{{ produto.codigo }}</v-card-subtitle>

      <v-card-text>
        <v-row>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Tipo</div>
            <div>{{ nomeDoTipo[produto.tipo] }}</div>
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Unidade</div>
            <div>{{ produto.unidadeMedida }}</div>
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Custo unitário</div>
            <div>{{ moeda.format(produto.custoUnitario) }}</div>
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Estoque atual</div>
            <div :class="produto.estoqueAbaixoDoMinimo ? 'estoque-baixo' : 'estoque-bom'">{{ produto.estoqueAtual }}</div>
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Estoque mínimo</div>
            <div>{{ produto.estoqueMinimo }}</div>
          </v-col>
          <v-col v-if="produto.descricao" cols="12">
            <div class="text-caption text-medium-emphasis">Descrição</div>
            <div>{{ produto.descricao }}</div>
          </v-col>
        </v-row>
      </v-card-text>

      <v-card-actions class="px-4 pb-4 flex-wrap ga-2">
        <v-btn color="primary" variant="flat" prepend-icon="mdi-swap-vertical" :disabled="!produto.ativo" @click="movimentando = true">
          Movimentar estoque
        </v-btn>
        <v-btn variant="outlined" color="secondary" prepend-icon="mdi-pencil-outline" :to="`/produtos/${produto.id}/editar`">Editar</v-btn>
        <v-spacer />
        <v-btn
          variant="outlined"
          color="error"
          prepend-icon="mdi-cancel"
          :disabled="!produto.ativo"
          @click="confirmandoInativacao = true"
        >
          Inativar
        </v-btn>
      </v-card-actions>
    </template>
  </v-card>

  <HistoricoMovimentacoes v-if="produto" :produto-id="id" :versao="versaoHistorico" />

  <MovimentacaoDialog v-if="produto" v-model="movimentando" :produto="produto" @movimentado="aoMovimentar" />
  <ConfirmacaoDialog
    v-model="confirmandoInativacao"
    titulo="Inativar produto"
    :mensagem="`Deseja inativar o produto ${produto?.nome ?? ''}? Ele deixará de receber movimentações.`"
    texto-confirmar="Inativar"
    :carregando="inativando"
    @confirmar="inativar"
  />
</template>

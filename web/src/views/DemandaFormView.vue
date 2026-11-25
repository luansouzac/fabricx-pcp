<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useDemandasStore } from '../stores/demandas'
import { useProdutosStore } from '../stores/produtos'

const props = defineProps({ id: { type: String, default: null } })

const router = useRouter()
const demandas = useDemandasStore()
const produtos = useProdutosStore()

const editando = computed(() => props.id !== null)

const formulario = reactive({ produtoId: null, periodo: '', quantidade: 1 })

const referenciaFormulario = ref(null)
const carregando = ref(false)
const salvando = ref(false)

const opcoesDeProduto = computed(() =>
  produtos.itens
    .filter((produto) => produto.ativo || produto.id === formulario.produtoId)
    .map((produto) => ({ title: `${produto.codigo} · ${produto.nome}`, value: produto.id })),
)

const unidade = computed(() => produtos.itens.find((produto) => produto.id === formulario.produtoId)?.unidadeMedida)

const regras = {
  produto: [(valor) => !!valor || 'Selecione o produto.'],
  periodo: [(valor) => /^\d{4}-\d{2}$/.test(valor ?? '') || 'Informe o período.'],
  quantidade: [(valor) => (valor !== '' && valor !== null && Number(valor) > 0) || 'Informe uma quantidade maior que zero.'],
}

onMounted(async () => {
  carregando.value = true
  try {
    await produtos.carregar()
    if (!editando.value) return

    const demanda = await demandas.obter(props.id)
    if (!demanda) {
      router.replace('/demanda-prevista')
      return
    }
    Object.assign(formulario, {
      produtoId: demanda.produtoId,
      periodo: demanda.periodo,
      quantidade: demanda.quantidade,
    })
  } catch {
    if (editando.value) router.replace('/demanda-prevista')
  } finally {
    carregando.value = false
  }
})

async function salvar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  const dados = { periodo: formulario.periodo, quantidade: Number(formulario.quantidade) }

  salvando.value = true
  try {
    if (editando.value) {
      await demandas.atualizar(props.id, dados)
    } else {
      await demandas.criar({ ...dados, produtoId: formulario.produtoId })
    }
    router.push({ path: '/planejamento', query: { periodo: formulario.periodo } })
  } catch {
    salvando.value = false
  }
}
</script>

<template>
  <h1 class="fx-titulo-pagina mb-1">{{ editando ? 'Editar demanda prevista' : 'Nova demanda prevista' }}</h1>
  <p class="fx-legenda mb-4">Quanto se espera produzir de cada produto por mês. Alimenta o planejamento de capacidade.</p>

  <v-card max-width="800" :loading="carregando">
    <v-form ref="referenciaFormulario" @submit.prevent="salvar">
      <v-card-text>
        <v-row>
          <v-col cols="12">
            <v-autocomplete
              v-model="formulario.produtoId"
              :items="opcoesDeProduto"
              label="Produto"
              no-data-text="Nenhum produto encontrado."
              :rules="regras.produto"
              :disabled="editando"
              :autofocus="!editando"
            />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field v-model="formulario.periodo" label="Período" type="month" :rules="regras.periodo" />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              v-model.number="formulario.quantidade"
              label="Quantidade"
              type="number"
              min="0"
              :suffix="unidade"
              :rules="regras.quantidade"
            />
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions class="px-4 pb-4">
        <v-btn variant="text" color="secondary" to="/demanda-prevista">Cancelar</v-btn>
        <v-spacer />
        <v-btn type="submit" color="primary" variant="flat" :loading="salvando">Salvar</v-btn>
      </v-card-actions>
    </v-form>
  </v-card>
</template>

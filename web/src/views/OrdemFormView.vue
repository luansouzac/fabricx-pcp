<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useOrdensStore } from '../stores/ordens'
import { useProdutosStore } from '../stores/produtos'
import { prioridades } from '../utils/ordem'

const props = defineProps({ id: { type: String, default: null } })

const router = useRouter()
const ordens = useOrdensStore()
const produtos = useProdutosStore()

const editando = computed(() => props.id !== null)

const formulario = reactive({ produtoId: null, quantidade: 1, prazoEntrega: '', prioridade: 'Media' })

const referenciaFormulario = ref(null)
const carregando = ref(false)
const salvando = ref(false)
const ordem = ref(null)

const opcoesDeProduto = computed(() =>
  produtos.itens
    .filter((produto) => produto.ativo || produto.id === formulario.produtoId)
    .map((produto) => ({ title: `${produto.codigo} · ${produto.nome}`, value: produto.id })),
)

const unidade = computed(() => produtos.itens.find((produto) => produto.id === formulario.produtoId)?.unidadeMedida)

const regras = {
  produto: [(valor) => !!valor || 'Selecione o produto.'],
  quantidade: [(valor) => (valor !== '' && valor !== null && Number(valor) > 0) || 'Informe uma quantidade maior que zero.'],
  prazo: [(valor) => !!valor || 'Informe o prazo de entrega.'],
}

onMounted(async () => {
  carregando.value = true
  try {
    await produtos.carregar()
    if (!editando.value) return

    ordem.value = await ordens.obter(props.id)
    if (ordem.value.status !== 'Planejada') {
      router.replace(`/ordens/${props.id}`)
      return
    }
    Object.assign(formulario, ordem.value)
  } catch {
    if (editando.value) router.replace('/ordens')
  } finally {
    carregando.value = false
  }
})

async function salvar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  const dados = {
    quantidade: Number(formulario.quantidade),
    prazoEntrega: formulario.prazoEntrega,
    prioridade: formulario.prioridade,
  }

  salvando.value = true
  try {
    if (editando.value) {
      await ordens.alterar(props.id, dados)
      router.push(`/ordens/${props.id}`)
    } else {
      const criada = await ordens.criar({ ...dados, produtoId: formulario.produtoId })
      router.push(`/ordens/${criada.id}`)
    }
  } catch {
    salvando.value = false
  }
}
</script>

<template>
  <h1 class="fx-titulo-pagina mb-1">{{ editando ? `Editar ordem ${ordem?.numero ?? ''}` : 'Nova ordem de produção' }}</h1>
  <p class="fx-legenda mb-4">
    {{ editando ? 'A ordem só pode ser alterada enquanto estiver Planejada.' : 'A ordem nasce Planejada e segue para programação, produção e conclusão.' }}
  </p>

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
          <v-col cols="12" md="4">
            <v-text-field
              v-model.number="formulario.quantidade"
              label="Quantidade"
              type="number"
              min="0"
              :suffix="unidade"
              :rules="regras.quantidade"
              :autofocus="editando"
            />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field v-model="formulario.prazoEntrega" label="Prazo de entrega" type="date" :rules="regras.prazo" />
          </v-col>
          <v-col cols="12" md="4">
            <v-select v-model="formulario.prioridade" :items="prioridades" label="Prioridade" />
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions class="px-4 pb-4">
        <v-btn variant="text" color="secondary" :to="editando ? `/ordens/${id}` : '/ordens'">Cancelar</v-btn>
        <v-spacer />
        <v-btn type="submit" color="primary" variant="flat" :loading="salvando">Salvar</v-btn>
      </v-card-actions>
    </v-form>
  </v-card>
</template>

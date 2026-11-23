<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useProdutosStore } from '../stores/produtos'
import { tiposDeProduto as tipos } from '../utils/produto'

const props = defineProps({ id: { type: String, default: null } })

const router = useRouter()
const produtos = useProdutosStore()

const editando = computed(() => props.id !== null)

const formulario = reactive({
  codigo: '',
  nome: '',
  descricao: '',
  tipo: null,
  unidadeMedida: 'UN',
  estoqueAtual: 0,
  estoqueMinimo: 0,
  custoUnitario: 0,
})

const referenciaFormulario = ref(null)
const carregando = ref(false)
const salvando = ref(false)

const obrigatorio = (rotulo) => (valor) => !!String(valor ?? '').trim() || `${rotulo} é obrigatório.`
const limite = (maximo) => (valor) => String(valor ?? '').length <= maximo || `Use no máximo ${maximo} caracteres.`
const naoNegativo = (valor) => (valor !== '' && valor !== null && Number(valor) >= 0) || 'Informe um valor igual ou maior que zero.'

const regras = {
  codigo: [obrigatorio('O código'), limite(50)],
  nome: [obrigatorio('O nome'), limite(150)],
  tipo: [(valor) => !!valor || 'Selecione o tipo.'],
  unidadeMedida: [obrigatorio('A unidade'), limite(10)],
  descricao: [limite(500)],
  numero: [naoNegativo],
}

onMounted(async () => {
  if (!editando.value) return

  carregando.value = true
  try {
    Object.assign(formulario, await produtos.obter(props.id))
    formulario.descricao ??= ''
  } catch {
    router.replace('/produtos')
  } finally {
    carregando.value = false
  }
})

async function salvar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  const dados = {
    nome: formulario.nome.trim(),
    descricao: formulario.descricao?.trim() || null,
    unidadeMedida: formulario.unidadeMedida.trim(),
    estoqueMinimo: Number(formulario.estoqueMinimo),
    custoUnitario: Number(formulario.custoUnitario),
  }

  salvando.value = true
  try {
    if (editando.value) {
      await produtos.atualizar(props.id, dados)
    } else {
      await produtos.criar({
        ...dados,
        codigo: formulario.codigo.trim(),
        tipo: formulario.tipo,
        estoqueAtual: Number(formulario.estoqueAtual),
      })
    }
    router.push(editando.value ? `/produtos/${props.id}` : '/produtos')
  } catch {
    salvando.value = false
  }
}
</script>

<template>
  <h1 class="fx-titulo-pagina mb-4">{{ editando ? 'Editar produto' : 'Novo produto' }}</h1>

  <v-card max-width="800" :loading="carregando">
    <v-form ref="referenciaFormulario" @submit.prevent="salvar">
      <v-card-text>
        <v-row>
          <v-col cols="12" md="4">
            <v-text-field
              v-model="formulario.codigo"
              label="Código"
              :rules="regras.codigo"
              :disabled="editando"
              autofocus
            />
          </v-col>
          <v-col cols="12" md="8">
            <v-text-field v-model="formulario.nome" label="Nome" :rules="regras.nome" :autofocus="editando" />
          </v-col>
          <v-col cols="12" md="6">
            <v-select
              v-model="formulario.tipo"
              :items="tipos"
              label="Tipo"
              :rules="regras.tipo"
              :disabled="editando"
            />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field v-model="formulario.unidadeMedida" label="Unidade de medida" :rules="regras.unidadeMedida" />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-model.number="formulario.estoqueAtual"
              label="Estoque atual"
              type="number"
              min="0"
              :rules="regras.numero"
              :disabled="editando"
              :hint="editando ? 'O estoque muda por movimentação.' : ''"
              persistent-hint
            />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-model.number="formulario.estoqueMinimo"
              label="Estoque mínimo"
              type="number"
              min="0"
              :rules="regras.numero"
            />
          </v-col>
          <v-col cols="12" md="4">
            <v-text-field
              v-model.number="formulario.custoUnitario"
              label="Custo unitário (R$)"
              type="number"
              min="0"
              step="0.01"
              :rules="regras.numero"
            />
          </v-col>
          <v-col cols="12">
            <v-textarea
              v-model="formulario.descricao"
              label="Descrição"
              variant="outlined"
              rows="2"
              counter="500"
              :rules="regras.descricao"
            />
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions class="px-4 pb-4">
        <v-btn variant="text" color="secondary" :to="editando ? `/produtos/${id}` : '/produtos'">Cancelar</v-btn>
        <v-spacer />
        <v-btn type="submit" color="primary" variant="flat" :loading="salvando">Salvar</v-btn>
      </v-card-actions>
    </v-form>
  </v-card>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useRecursosStore } from '../stores/recursos'
import { tiposDeRecurso as tipos } from '../utils/recurso'

const props = defineProps({ id: { type: String, default: null } })

const router = useRouter()
const recursos = useRecursosStore()

const editando = computed(() => props.id !== null)

const formulario = reactive({ nome: '', tipo: null, capacidadeHorasDia: 8 })

const referenciaFormulario = ref(null)
const carregando = ref(false)
const salvando = ref(false)

const regras = {
  nome: [
    (valor) => !!String(valor ?? '').trim() || 'O nome é obrigatório.',
    (valor) => String(valor ?? '').length <= 100 || 'Use no máximo 100 caracteres.',
  ],
  tipo: [(valor) => !!valor || 'Selecione o tipo.'],
  capacidade: [
    (valor) =>
      (valor !== '' && valor !== null && Number(valor) > 0 && Number(valor) <= 24) ||
      'Informe uma capacidade maior que zero e de até 24 horas por dia.',
  ],
}

onMounted(async () => {
  if (!editando.value) return

  carregando.value = true
  try {
    Object.assign(formulario, await recursos.obter(props.id))
  } catch {
    router.replace('/recursos')
  } finally {
    carregando.value = false
  }
})

async function salvar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  const dados = {
    nome: formulario.nome.trim(),
    tipo: formulario.tipo,
    capacidadeHorasDia: Number(formulario.capacidadeHorasDia),
  }

  salvando.value = true
  try {
    if (editando.value) {
      await recursos.atualizar(props.id, dados)
    } else {
      await recursos.criar(dados)
    }
    router.push('/recursos')
  } catch {
    salvando.value = false
  }
}
</script>

<template>
  <h1 class="fx-titulo-pagina mb-1">{{ editando ? 'Editar recurso' : 'Novo recurso' }}</h1>
  <p class="fx-legenda mb-4">Máquinas, operadores e linhas que recebem a programação da produção.</p>

  <v-card max-width="800" :loading="carregando">
    <v-form ref="referenciaFormulario" @submit.prevent="salvar">
      <v-card-text>
        <v-row>
          <v-col cols="12">
            <v-text-field v-model="formulario.nome" label="Nome" placeholder="Ex.: Injetora 01" :rules="regras.nome" autofocus />
          </v-col>
          <v-col cols="12" md="6">
            <v-select v-model="formulario.tipo" :items="tipos" label="Tipo" :rules="regras.tipo" />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              v-model.number="formulario.capacidadeHorasDia"
              label="Capacidade"
              type="number"
              min="0"
              max="24"
              step="0.5"
              suffix="h/dia"
              :rules="regras.capacidade"
            />
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions class="px-4 pb-4">
        <v-btn variant="text" color="secondary" to="/recursos">Cancelar</v-btn>
        <v-spacer />
        <v-btn type="submit" color="primary" variant="flat" :loading="salvando">Salvar</v-btn>
      </v-card-actions>
    </v-form>
  </v-card>
</template>

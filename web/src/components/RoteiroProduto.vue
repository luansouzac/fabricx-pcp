<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import ConfirmacaoDialog from './ConfirmacaoDialog.vue'
import { useProdutosStore } from '../stores/produtos'
import { useRecursosStore } from '../stores/recursos'

const props = defineProps({ produtoId: { type: String, required: true } })

const produtos = useProdutosStore()
const recursos = useRecursosStore()

const etapas = ref([])
const carregando = ref(false)
const quantidadeExemplo = ref(10)

const dialogoAberto = ref(false)
const etapaEmEdicao = ref(null)
const salvando = ref(false)
const referenciaFormulario = ref(null)
const formulario = reactive({ recursoId: null, sequencia: 1, nome: '', tempoCicloMin: null, unidadesPorCiclo: 1 })

const etapaParaRemover = ref(null)
const confirmandoRemocao = ref(false)
const removendo = ref(false)

const colunas = [
  { title: 'Sequência', key: 'sequencia' },
  { title: 'Etapa', key: 'nome' },
  { title: 'Recurso', key: 'recursoNome' },
  { title: 'Tempo de ciclo (min)', key: 'tempoCicloMin' },
  { title: 'Unidades por ciclo', key: 'unidadesPorCiclo' },
  { title: 'Duração para a quantidade', key: 'duracao', sortable: false },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const opcoesDeRecurso = computed(() =>
  recursos.itens
    .filter((r) => r.ativo || r.id === formulario.recursoId)
    .map((r) => ({ title: r.nome, value: r.id })),
)

const duracaoDaEtapa = (etapa) =>
  Number(quantidadeExemplo.value) > 0
    ? Math.ceil(Number(quantidadeExemplo.value) / etapa.unidadesPorCiclo) * etapa.tempoCicloMin
    : null

const duracaoTotal = computed(() =>
  etapas.value.reduce((soma, etapa) => soma + (duracaoDaEtapa(etapa) ?? 0), 0),
)

const formatarMinutos = (minutos) => (minutos === null ? '—' : `${new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 1 }).format(minutos)} min`)

const regras = {
  recurso: [(valor) => !!valor || 'Selecione o recurso.'],
  sequencia: [(valor) => (Number.isInteger(Number(valor)) && Number(valor) >= 1) || 'Informe um número inteiro a partir de 1.'],
  nome: [
    (valor) => !!String(valor ?? '').trim() || 'O nome é obrigatório.',
    (valor) => String(valor ?? '').length <= 100 || 'Use no máximo 100 caracteres.',
  ],
  tempo: [(valor) => Number(valor) > 0 || 'Informe um tempo maior que zero.'],
  unidades: [(valor) => (Number.isInteger(Number(valor)) && Number(valor) >= 1) || 'Informe um número inteiro a partir de 1.'],
}

async function carregar() {
  carregando.value = true
  try {
    etapas.value = await produtos.listarRoteiro(props.produtoId)
  } catch {
    etapas.value = []
  } finally {
    carregando.value = false
  }
}

function abrirDialogo(etapa) {
  etapaEmEdicao.value = etapa ?? null
  Object.assign(
    formulario,
    etapa
      ? { ...etapa }
      : {
          recursoId: null,
          sequencia: Math.max(0, ...etapas.value.map((e) => e.sequencia)) + 1,
          nome: '',
          tempoCicloMin: null,
          unidadesPorCiclo: 1,
        },
  )
  dialogoAberto.value = true
  referenciaFormulario.value?.resetValidation()
}

async function salvar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  const dados = {
    recursoId: formulario.recursoId,
    sequencia: Number(formulario.sequencia),
    nome: formulario.nome.trim(),
    tempoCicloMin: Number(formulario.tempoCicloMin),
    unidadesPorCiclo: Number(formulario.unidadesPorCiclo),
  }

  salvando.value = true
  try {
    if (etapaEmEdicao.value) {
      await produtos.atualizarEtapa(props.produtoId, etapaEmEdicao.value.id, dados)
    } else {
      await produtos.adicionarEtapa(props.produtoId, dados)
    }
    dialogoAberto.value = false
    await carregar()
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    salvando.value = false
  }
}

function pedirRemocao(etapa) {
  etapaParaRemover.value = etapa
  confirmandoRemocao.value = true
}

async function remover() {
  removendo.value = true
  try {
    await produtos.removerEtapa(props.produtoId, etapaParaRemover.value.id)
    confirmandoRemocao.value = false
    await carregar()
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    removendo.value = false
  }
}

onMounted(async () => {
  await Promise.all([recursos.carregar(), carregar()])
})
</script>

<template>
  <v-card class="mt-4" title="Roteiro de produção" subtitle="Etapas na ordem em que o produto é fabricado.">
    <v-card-text>
      <v-row align="center">
        <v-col cols="12" sm="6" md="4">
          <v-text-field
            v-model.number="quantidadeExemplo"
            label="Quantidade para calcular a duração"
            type="number"
            min="0"
            hide-details
          />
        </v-col>
        <v-col cols="12" sm="6" md="8" class="d-flex align-center">
          <span v-if="etapas.length" class="fx-legenda">Duração total: {{ formatarMinutos(duracaoTotal) }}</span>
          <v-spacer />
          <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" @click="abrirDialogo()">Nova etapa</v-btn>
        </v-col>
      </v-row>
    </v-card-text>

    <v-data-table
      :headers="colunas"
      :items="etapas"
      :loading="carregando"
      :sort-by="[{ key: 'sequencia', order: 'asc' }]"
      items-per-page-text="Itens por página"
      no-data-text="Nenhuma etapa no roteiro. Crie a primeira etapa."
      loading-text="Carregando roteiro..."
    >
      <template #item.duracao="{ item }">{{ formatarMinutos(duracaoDaEtapa(item)) }}</template>
      <template #item.acoes="{ item }">
        <v-btn icon="mdi-pencil-outline" variant="text" size="small" color="secondary" aria-label="Editar etapa" title="Editar" @click="abrirDialogo(item)" />
        <v-btn icon="mdi-delete-outline" variant="text" size="small" color="error" aria-label="Remover etapa" title="Remover" @click="pedirRemocao(item)" />
      </template>
    </v-data-table>
  </v-card>

  <v-dialog v-model="dialogoAberto" max-width="560" :persistent="salvando">
    <v-card :title="etapaEmEdicao ? 'Editar etapa' : 'Nova etapa'">
      <v-form ref="referenciaFormulario" @submit.prevent="salvar">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="3">
              <v-text-field v-model.number="formulario.sequencia" label="Sequência" type="number" min="1" :rules="regras.sequencia" />
            </v-col>
            <v-col cols="12" md="9">
              <v-text-field v-model="formulario.nome" label="Nome da etapa" placeholder="Ex.: Montagem" :rules="regras.nome" autofocus />
            </v-col>
            <v-col cols="12">
              <v-select v-model="formulario.recursoId" :items="opcoesDeRecurso" label="Recurso" :rules="regras.recurso" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model.number="formulario.tempoCicloMin" label="Tempo de ciclo" type="number" min="0" suffix="min" :rules="regras.tempo" />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field v-model.number="formulario.unidadesPorCiclo" label="Unidades por ciclo" type="number" min="1" :rules="regras.unidades" />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="px-4 pb-4">
          <v-spacer />
          <v-btn variant="text" color="secondary" :disabled="salvando" @click="dialogoAberto = false">Cancelar</v-btn>
          <v-btn type="submit" variant="flat" color="primary" :loading="salvando">Salvar</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>

  <ConfirmacaoDialog
    v-model="confirmandoRemocao"
    titulo="Remover etapa"
    :mensagem="`Remover a etapa ${etapaParaRemover?.nome ?? ''} do roteiro?`"
    texto-confirmar="Remover"
    :carregando="removendo"
    @confirmar="remover"
  />
</template>

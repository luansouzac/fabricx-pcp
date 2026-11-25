<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import ConfirmacaoDialog from './ConfirmacaoDialog.vue'
import { useApontamentosStore } from '../stores/apontamentos'
import { useProdutosStore } from '../stores/produtos'
import { useProgramacaoStore } from '../stores/programacao'
import { useRecursosStore } from '../stores/recursos'
import { dataHora } from '../utils/ordem'
import {
  duracaoDaEtapa,
  formatarMinutos,
  paraCampoDataHora,
  somarMinutos,
  statusDaProgramacao,
} from '../utils/programacao'

const props = defineProps({ ordem: { type: Object, required: true } })
const emit = defineEmits(['alterado'])

const programacao = useProgramacaoStore()
const apontamentos = useApontamentosStore()
const produtos = useProdutosStore()
const recursos = useRecursosStore()

const itensProgramados = ref([])
const itensApontados = ref([])
const roteiro = ref([])
const carregando = ref(false)

const ordemAberta = computed(() => props.ordem.status === 'Planejada' || props.ordem.status === 'Programada')
const aceitaApontamento = computed(() => props.ordem.status === 'Programada' || props.ordem.status === 'EmProducao')

const colunasProgramacao = [
  { title: 'Seq.', key: 'sequencia' },
  { title: 'Etapa', key: 'etapaNome' },
  { title: 'Recurso', key: 'recursoNome' },
  { title: 'Início previsto', key: 'inicioPrevisto' },
  { title: 'Fim previsto', key: 'fimPrevisto' },
  { title: 'Situação', key: 'status' },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const colunasApontamento = [
  { title: 'Data e hora', key: 'dataHora' },
  { title: 'Etapa', key: 'etapa' },
  { title: 'Produzida', key: 'quantidadeProduzida' },
  { title: 'Defeituosa', key: 'quantidadeDefeituosa' },
  { title: 'Tempo gasto', key: 'tempoGastoMin' },
  { title: 'Observação', key: 'observacao', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const decimal = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 })

const etapasLivres = computed(() =>
  roteiro.value
    .filter((etapa) => !itensProgramados.value.some((item) => item.etapaId === etapa.id))
    .map((etapa) => ({ title: `${etapa.sequencia}. ${etapa.nome} (${etapa.recursoNome})`, value: etapa.id })),
)

const opcoesDeRecurso = computed(() => recursos.itens.filter((r) => r.ativo).map((r) => ({ title: r.nome, value: r.id })))

const nomeDaEtapa = (programacaoId) =>
  itensProgramados.value.find((item) => item.id === programacaoId)?.etapaNome ?? '—'

const dialogoProgramar = ref(false)
const emEdicao = ref(null)
const salvando = ref(false)
const referenciaProgramar = ref(null)
const formularioProgramar = reactive({ etapaId: null, inicio: '', recursoId: null })

const etapaEscolhida = computed(() => {
  const id = emEdicao.value?.etapaId ?? formularioProgramar.etapaId
  return roteiro.value.find((etapa) => etapa.id === id) ?? null
})

const duracaoPrevista = computed(() =>
  etapaEscolhida.value ? duracaoDaEtapa(etapaEscolhida.value, props.ordem.quantidade) : null,
)

const fimCalculado = computed(() =>
  formularioProgramar.inicio && duracaoPrevista.value !== null
    ? dataHora(somarMinutos(formularioProgramar.inicio, duracaoPrevista.value))
    : null,
)

const regrasProgramar = {
  etapa: [(valor) => !!valor || 'Selecione a etapa.'],
  inicio: [(valor) => !!valor || 'Informe a data e a hora de início.'],
}

const dialogoApontar = ref(false)
const programacaoApontada = ref(null)
const registrando = ref(false)
const referenciaApontar = ref(null)
const formularioApontar = reactive({ quantidadeProduzida: 0, quantidadeDefeituosa: 0, tempoGastoMin: null, observacao: '' })

const regrasApontar = {
  quantidade: [(valor) => (valor !== '' && valor !== null && Number(valor) >= 0) || 'Informe zero ou mais.'],
  tempo: [(valor) => Number(valor) > 0 || 'Informe um tempo maior que zero.'],
  observacao: [(valor) => String(valor ?? '').length <= 500 || 'Use no máximo 500 caracteres.'],
}

const apontamentoSemQuantidade = computed(
  () => !(Number(formularioApontar.quantidadeProduzida) > 0 || Number(formularioApontar.quantidadeDefeituosa) > 0),
)

const programacaoParaRemover = ref(null)
const confirmandoRemocao = ref(false)
const removendo = ref(false)

async function carregar() {
  carregando.value = true
  try {
    const [programadas, apontadas] = await Promise.all([
      programacao.listarPorOrdem(props.ordem.id),
      apontamentos.listarPorOrdem(props.ordem.id),
    ])
    itensProgramados.value = programadas
    itensApontados.value = apontadas
  } catch {
    itensProgramados.value = []
    itensApontados.value = []
  } finally {
    carregando.value = false
  }
}

function abrirProgramar(item) {
  emEdicao.value = item ?? null
  Object.assign(formularioProgramar, {
    etapaId: item?.etapaId ?? null,
    inicio: item ? paraCampoDataHora(item.inicioPrevisto) : '',
    recursoId: item?.recursoId ?? null,
  })
  dialogoProgramar.value = true
  referenciaProgramar.value?.resetValidation()
}

async function salvarProgramacao() {
  const { valid } = await referenciaProgramar.value.validate()
  if (!valid) return

  salvando.value = true
  try {
    if (emEdicao.value) {
      await programacao.reprogramar(emEdicao.value.id, {
        inicio: formularioProgramar.inicio,
        recursoId: formularioProgramar.recursoId,
      })
    } else {
      await programacao.agendar(props.ordem.id, {
        etapaId: formularioProgramar.etapaId,
        inicio: formularioProgramar.inicio,
        recursoId: formularioProgramar.recursoId,
      })
    }
    dialogoProgramar.value = false
    await carregar()
    emit('alterado')
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    salvando.value = false
  }
}

function pedirRemocao(item) {
  programacaoParaRemover.value = item
  confirmandoRemocao.value = true
}

async function remover() {
  removendo.value = true
  try {
    await programacao.remover(programacaoParaRemover.value.id)
    confirmandoRemocao.value = false
    await carregar()
    emit('alterado')
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    removendo.value = false
  }
}

function abrirApontar(item) {
  programacaoApontada.value = item
  Object.assign(formularioApontar, { quantidadeProduzida: 0, quantidadeDefeituosa: 0, tempoGastoMin: null, observacao: '' })
  dialogoApontar.value = true
  referenciaApontar.value?.resetValidation()
}

async function registrarApontamento() {
  const { valid } = await referenciaApontar.value.validate()
  if (!valid || apontamentoSemQuantidade.value) return

  registrando.value = true
  try {
    await apontamentos.registrar(programacaoApontada.value.id, {
      quantidadeProduzida: Number(formularioApontar.quantidadeProduzida),
      quantidadeDefeituosa: Number(formularioApontar.quantidadeDefeituosa),
      tempoGastoMin: Number(formularioApontar.tempoGastoMin),
      observacao: formularioApontar.observacao.trim() || null,
    })
    dialogoApontar.value = false
    await carregar()
    emit('alterado')
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    registrando.value = false
  }
}

onMounted(async () => {
  recursos.carregar()
  try {
    roteiro.value = await produtos.listarRoteiro(props.ordem.produtoId)
  } catch {
    roteiro.value = []
  }
  await carregar()
})
</script>

<template>
  <v-card class="mt-4" title="Programação" subtitle="Etapas do roteiro agendadas nos recursos.">
    <template #append>
      <v-btn
        v-if="ordemAberta"
        color="secondary"
        variant="outlined"
        prepend-icon="mdi-calendar-plus"
        :disabled="!etapasLivres.length"
        @click="abrirProgramar()"
      >
        Programar etapa
      </v-btn>
    </template>

    <v-data-table
      :headers="colunasProgramacao"
      :items="itensProgramados"
      :loading="carregando"
      :sort-by="[{ key: 'sequencia', order: 'asc' }]"
      items-per-page-text="Itens por página"
      no-data-text="Nenhuma etapa programada. Programe a primeira etapa do roteiro."
      loading-text="Carregando programação..."
    >
      <template #item.inicioPrevisto="{ item }">{{ dataHora(item.inicioPrevisto) }}</template>
      <template #item.fimPrevisto="{ item }">{{ dataHora(item.fimPrevisto) }}</template>
      <template #item.status="{ item }">
        <v-chip :color="statusDaProgramacao[item.status].color" variant="flat" size="small">
          {{ statusDaProgramacao[item.status].title }}
        </v-chip>
      </template>
      <template #item.acoes="{ item }">
        <v-btn
          v-if="aceitaApontamento && item.status !== 'Concluida'"
          icon="mdi-clipboard-edit-outline"
          variant="text"
          size="small"
          color="primary"
          aria-label="Registrar apontamento"
          title="Apontar"
          @click="abrirApontar(item)"
        />
        <v-btn
          v-if="item.status === 'Agendada'"
          icon="mdi-calendar-edit"
          variant="text"
          size="small"
          color="secondary"
          aria-label="Reprogramar etapa"
          title="Reprogramar"
          @click="abrirProgramar(item)"
        />
        <v-btn
          v-if="item.status === 'Agendada'"
          icon="mdi-delete-outline"
          variant="text"
          size="small"
          color="error"
          aria-label="Remover programação"
          title="Remover"
          @click="pedirRemocao(item)"
        />
      </template>
    </v-data-table>
  </v-card>

  <v-card class="mt-4" title="Apontamentos" subtitle="Produção registrada em cada etapa da ordem.">
    <v-data-table
      :headers="colunasApontamento"
      :items="itensApontados"
      :loading="carregando"
      :sort-by="[{ key: 'dataHora', order: 'desc' }]"
      items-per-page-text="Itens por página"
      no-data-text="Nenhum apontamento registrado. Aponte a produção a partir da programação."
      loading-text="Carregando apontamentos..."
    >
      <template #item.dataHora="{ item }">{{ dataHora(item.dataHora) }}</template>
      <template #item.etapa="{ item }">{{ nomeDaEtapa(item.programacaoId) }}</template>
      <template #item.quantidadeProduzida="{ item }">{{ decimal.format(item.quantidadeProduzida) }}</template>
      <template #item.quantidadeDefeituosa="{ item }">{{ decimal.format(item.quantidadeDefeituosa) }}</template>
      <template #item.tempoGastoMin="{ item }">{{ formatarMinutos(item.tempoGastoMin) }}</template>
      <template #item.observacao="{ item }">{{ item.observacao ?? '—' }}</template>
    </v-data-table>
  </v-card>

  <v-dialog v-model="dialogoProgramar" max-width="520" :persistent="salvando">
    <v-card :title="emEdicao ? `Reprogramar ${emEdicao.etapaNome}` : 'Programar etapa'">
      <v-form ref="referenciaProgramar" @submit.prevent="salvarProgramacao">
        <v-card-text>
          <v-row>
            <v-col v-if="!emEdicao" cols="12">
              <v-select v-model="formularioProgramar.etapaId" :items="etapasLivres" label="Etapa" :rules="regrasProgramar.etapa" autofocus />
            </v-col>
            <v-col cols="12">
              <v-text-field
                v-model="formularioProgramar.inicio"
                label="Início"
                type="datetime-local"
                :rules="regrasProgramar.inicio"
                :autofocus="!!emEdicao"
              />
            </v-col>
            <v-col cols="12">
              <v-select
                v-model="formularioProgramar.recursoId"
                :items="opcoesDeRecurso"
                label="Recurso"
                clearable
                :hint="etapaEscolhida ? `Sem escolha, usa o recurso do roteiro: ${etapaEscolhida.recursoNome}.` : ''"
                persistent-hint
              />
            </v-col>
            <v-col v-if="fimCalculado" cols="12">
              <div class="fx-legenda">Duração: {{ formatarMinutos(duracaoPrevista) }}</div>
              <div class="fx-legenda">Fim previsto: {{ fimCalculado }}</div>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="px-6 pb-4">
          <v-spacer />
          <v-btn variant="text" color="secondary" :disabled="salvando" @click="dialogoProgramar = false">Cancelar</v-btn>
          <v-btn type="submit" color="primary" variant="flat" :loading="salvando">{{ emEdicao ? 'Reprogramar' : 'Programar' }}</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>

  <v-dialog v-model="dialogoApontar" max-width="520" :persistent="registrando">
    <v-card :title="`Apontar ${programacaoApontada?.etapaNome ?? ''}`">
      <v-form ref="referenciaApontar" @submit.prevent="registrarApontamento">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model.number="formularioApontar.quantidadeProduzida"
                label="Quantidade produzida"
                type="number"
                min="0"
                :rules="regrasApontar.quantidade"
                autofocus
              />
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model.number="formularioApontar.quantidadeDefeituosa"
                label="Quantidade defeituosa"
                type="number"
                min="0"
                :rules="regrasApontar.quantidade"
              />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model.number="formularioApontar.tempoGastoMin" label="Tempo gasto" type="number" min="0" suffix="min" :rules="regrasApontar.tempo" />
            </v-col>
            <v-col cols="12">
              <v-textarea v-model="formularioApontar.observacao" label="Observação" rows="2" auto-grow counter="500" :rules="regrasApontar.observacao" />
            </v-col>
            <v-col v-if="apontamentoSemQuantidade" cols="12">
              <div class="fx-legenda">Informe ao menos uma quantidade produzida ou defeituosa maior que zero.</div>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="px-6 pb-4">
          <v-spacer />
          <v-btn variant="text" color="secondary" :disabled="registrando" @click="dialogoApontar = false">Cancelar</v-btn>
          <v-btn type="submit" color="primary" variant="flat" :loading="registrando" :disabled="apontamentoSemQuantidade">Registrar</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>

  <ConfirmacaoDialog
    v-model="confirmandoRemocao"
    titulo="Remover programação"
    :mensagem="`Remover a programação de ${programacaoParaRemover?.etapaNome ?? ''}? A etapa volta a ficar sem agendamento.`"
    texto-confirmar="Remover"
    :carregando="removendo"
    @confirmar="remover"
  />
</template>

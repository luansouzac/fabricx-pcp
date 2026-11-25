<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import ConfirmacaoDialog from '../components/ConfirmacaoDialog.vue'
import ProgramacaoOrdem from '../components/ProgramacaoOrdem.vue'
import { useOrdensStore } from '../stores/ordens'
import { acaoDeAvanco, dataCurta, dataHora, infoDaPrioridade, infoDoStatus, podeCancelar } from '../utils/ordem'

const props = defineProps({ id: { type: String, required: true } })

const router = useRouter()
const ordens = useOrdensStore()

const ordem = ref(null)
const carregando = ref(true)
const avancando = ref(false)
const cancelando = ref(false)
const confirmandoCancelamento = ref(false)

const resumo = ref({ etapasTotal: 0, etapasProgramadas: 0, apontamentos: 0 })

const passos = [
  { title: 'Planejada', value: 'Planejada' },
  { title: 'Programada', value: 'Programada' },
  { title: 'Em produção', value: 'EmProducao' },
  { title: 'Concluída', value: 'Concluida' },
]

const itensDoFluxo = passos.map((passo, indice) => ({ title: passo.title, value: indice + 1 }))

const passoAtual = computed(() => passos.findIndex((passo) => passo.value === ordem.value?.status) + 1)

const proximoPasso = computed(() => {
  const atual = ordem.value
  if (!atual) return null
  const { etapasTotal, etapasProgramadas, apontamentos } = resumo.value
  const faltam = etapasTotal - etapasProgramadas

  if (atual.status === 'Cancelada') return { tipo: 'warning', texto: 'Ordem cancelada. Ela não pode mais ser programada nem apontada.' }
  if (atual.status === 'Planejada' && etapasTotal === 0) {
    return { tipo: 'warning', texto: 'O produto não tem roteiro. Cadastre as etapas em Produtos antes de programar.', link: { rota: `/produtos/${atual.produtoId}/roteiro`, rotulo: 'Abrir roteiro' } }
  }
  if (atual.status === 'Planejada' && faltam > 0) {
    return { tipo: 'info', texto: `Programe ${faltam === 1 ? 'a etapa restante' : `as ${faltam} etapas restantes`} do roteiro. Com todas agendadas, a ordem passa para Programada.` }
  }
  if (atual.status === 'Planejada') return { tipo: 'info', texto: 'Todas as etapas estão agendadas. Use Programar para liberar a ordem.' }
  if (atual.status === 'Programada') return { tipo: 'info', texto: 'Ordem programada. Inicie a produção para começar a registrar apontamentos.' }
  if (atual.status === 'EmProducao') {
    return {
      tipo: 'info',
      texto: apontamentos === 0
        ? 'Em produção. Registre o primeiro apontamento em uma etapa da programação.'
        : 'Em produção. Continue apontando as etapas e conclua a ordem quando terminar.',
    }
  }
  return { tipo: 'success', texto: 'Ordem concluída. Veja o resultado em Indicadores.', link: { rota: '/indicadores', rotulo: 'Abrir indicadores' } }
})

const acaoAvanco = computed(() => (ordem.value ? acaoDeAvanco[ordem.value.status] : null))

const mensagemDoAvanco = {
  Planejada: 'Ordem programada com sucesso.',
  Programada: 'Produção iniciada com sucesso.',
  EmProducao: 'Ordem concluída com sucesso.',
}

onMounted(async () => {
  try {
    ordem.value = await ordens.obter(props.id)
  } catch {
    router.replace('/ordens')
  } finally {
    carregando.value = false
  }
})

async function atualizarOrdem() {
  try {
    ordem.value = await ordens.obter(props.id)
  } catch {
    // o toast de erro já foi exibido pelo cliente
  }
}

async function avancar() {
  avancando.value = true
  try {
    ordem.value = await ordens.avancar(props.id, mensagemDoAvanco[ordem.value.status])
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    avancando.value = false
  }
}

async function cancelar() {
  cancelando.value = true
  try {
    ordem.value = await ordens.cancelar(props.id)
    confirmandoCancelamento.value = false
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    cancelando.value = false
  }
}
</script>

<template>
  <div class="d-flex align-center mb-4">
    <v-btn icon="mdi-arrow-left" variant="text" color="secondary" to="/ordens" title="Voltar" aria-label="Voltar" class="mr-2" />
    <h1 class="fx-titulo-pagina">Detalhe da ordem</h1>
  </div>

  <template v-if="ordem && ordem.status !== 'Cancelada'">
    <v-stepper :model-value="passoAtual" :items="itensDoFluxo" hide-actions flat class="mb-4 fx-fluxo" />
  </template>

  <v-alert
    v-if="proximoPasso"
    :type="proximoPasso.tipo"
    variant="flat"
    density="compact"
    class="mb-4"
    :icon="proximoPasso.tipo === 'info' ? 'mdi-arrow-right-circle-outline' : undefined"
  >
    <strong>Próximo passo:</strong> {{ proximoPasso.texto }}
    <template v-if="proximoPasso.link" #append>
      <v-btn variant="text" size="small" :to="proximoPasso.link.rota">{{ proximoPasso.link.rotulo }}</v-btn>
    </template>
  </v-alert>

  <v-card :loading="carregando">
    <template v-if="ordem">
      <v-card-title class="d-flex align-center flex-wrap ga-2">
        {{ ordem.numero }}
        <v-chip :color="infoDoStatus[ordem.status].color" variant="flat" size="small">{{ infoDoStatus[ordem.status].title }}</v-chip>
      </v-card-title>
      <v-card-subtitle>{{ ordem.produtoCodigo }} · {{ ordem.produtoNome }}</v-card-subtitle>

      <v-card-text>
        <v-row>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Quantidade</div>
            <div>{{ ordem.quantidade }}</div>
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Prazo de entrega</div>
            <div>{{ dataCurta(ordem.prazoEntrega) }}</div>
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Prioridade</div>
            <v-chip :color="infoDaPrioridade[ordem.prioridade].color" variant="flat" size="small">
              {{ infoDaPrioridade[ordem.prioridade].title }}
            </v-chip>
          </v-col>
          <v-col cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Criada em</div>
            <div>{{ dataHora(ordem.criadaEm) }}</div>
          </v-col>
          <v-col v-if="ordem.concluidaEm" cols="12" sm="6" md="4">
            <div class="text-caption text-medium-emphasis">Concluída em</div>
            <div>{{ dataHora(ordem.concluidaEm) }}</div>
          </v-col>
        </v-row>
      </v-card-text>

      <v-card-actions class="px-4 pb-4 flex-wrap ga-2">
        <v-btn
          v-if="acaoAvanco"
          color="primary"
          variant="flat"
          :prepend-icon="acaoAvanco.icone"
          :loading="avancando"
          @click="avancar"
        >
          {{ acaoAvanco.rotulo }}
        </v-btn>
        <v-btn
          v-if="ordem.status === 'Planejada'"
          variant="outlined"
          color="secondary"
          prepend-icon="mdi-pencil-outline"
          :to="`/ordens/${ordem.id}/editar`"
        >
          Editar
        </v-btn>
        <v-spacer />
        <v-btn
          v-if="podeCancelar(ordem.status)"
          variant="outlined"
          color="error"
          prepend-icon="mdi-cancel"
          @click="confirmandoCancelamento = true"
        >
          Cancelar ordem
        </v-btn>
      </v-card-actions>
    </template>
  </v-card>

  <ProgramacaoOrdem v-if="ordem" :ordem="ordem" @alterado="atualizarOrdem" @resumo="resumo = $event" />

  <ConfirmacaoDialog
    v-model="confirmandoCancelamento"
    titulo="Cancelar ordem"
    :mensagem="`Cancelar a ordem ${ordem?.numero ?? ''}? Ela não poderá ser retomada.`"
    texto-confirmar="Cancelar ordem"
    :carregando="cancelando"
    @confirmar="cancelar"
  />
</template>

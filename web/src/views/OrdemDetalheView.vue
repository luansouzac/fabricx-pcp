<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import ConfirmacaoDialog from '../components/ConfirmacaoDialog.vue'
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

  <ConfirmacaoDialog
    v-model="confirmandoCancelamento"
    titulo="Cancelar ordem"
    :mensagem="`Cancelar a ordem ${ordem?.numero ?? ''}? Ela não poderá ser retomada.`"
    texto-confirmar="Cancelar ordem"
    :carregando="cancelando"
    @confirmar="cancelar"
  />
</template>

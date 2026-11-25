<script setup>
import { computed, onMounted, ref } from 'vue'
import ConfirmacaoDialog from '../components/ConfirmacaoDialog.vue'
import { useRecursosStore } from '../stores/recursos'
import { nomeDoTipoRecurso, tiposDeRecurso as tipos } from '../utils/recurso'

const recursos = useRecursosStore()

const busca = ref('')
const tipoSelecionado = ref(null)
const recursoParaInativar = ref(null)
const confirmandoInativacao = ref(false)
const inativando = ref(false)

const colunas = [
  { title: 'Nome', key: 'nome' },
  { title: 'Tipo', key: 'tipo' },
  { title: 'Capacidade (h/dia)', key: 'capacidadeHorasDia' },
  { title: 'Situação', key: 'ativo' },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const recursosFiltrados = computed(() => {
  const consulta = (busca.value ?? '').trim().toLowerCase()
  return recursos.itens.filter(
    (recurso) =>
      (!tipoSelecionado.value || recurso.tipo === tipoSelecionado.value) &&
      (!consulta || recurso.nome.toLowerCase().includes(consulta)),
  )
})

function pedirInativacao(recurso) {
  recursoParaInativar.value = recurso
  confirmandoInativacao.value = true
}

async function inativar() {
  inativando.value = true
  try {
    await recursos.inativar(recursoParaInativar.value.id)
    confirmandoInativacao.value = false
    await recursos.carregar()
  } catch {
    // o toast de erro já foi exibido pelo cliente
  } finally {
    inativando.value = false
  }
}

onMounted(() => recursos.carregar())
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Recursos</h1>
    <v-spacer />
    <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" to="/recursos/novo">Novo recurso</v-btn>
  </div>

  <v-alert v-if="recursos.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Não foi possível carregar os recursos.
    <template #append>
      <v-btn variant="text" size="small" @click="recursos.carregar()">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <v-row>
    <v-col cols="12" md="6">
      <v-text-field v-model="busca" label="Buscar por nome" prepend-inner-icon="mdi-magnify" clearable hide-details />
    </v-col>
    <v-col cols="12" md="3">
      <v-select v-model="tipoSelecionado" :items="tipos" label="Tipo" clearable hide-details />
    </v-col>
  </v-row>

  <v-data-table
    :headers="colunas"
    :items="recursosFiltrados"
    :loading="recursos.carregando"
    items-per-page-text="Itens por página"
    no-data-text="Nenhum recurso encontrado. Crie um recurso ou mude o filtro."
    loading-text="Carregando recursos..."
    class="mt-4"
  >
    <template #item.tipo="{ item }">{{ nomeDoTipoRecurso[item.tipo] }}</template>
    <template #item.ativo="{ item }">
      <v-chip :color="item.ativo ? 'success' : 'grey'" variant="flat" size="small">{{ item.ativo ? 'Ativo' : 'Inativo' }}</v-chip>
    </template>
    <template #item.acoes="{ item }">
      <v-btn icon="mdi-pencil-outline" variant="text" size="small" color="secondary" aria-label="Editar recurso" title="Editar" :to="`/recursos/${item.id}/editar`" />
      <v-btn
        icon="mdi-cancel"
        variant="text"
        size="small"
        color="error"
        aria-label="Inativar recurso"
        title="Inativar"
        :disabled="!item.ativo"
        @click="pedirInativacao(item)"
      />
    </template>
  </v-data-table>

  <ConfirmacaoDialog
    v-model="confirmandoInativacao"
    titulo="Inativar recurso"
    :mensagem="`Inativar o recurso ${recursoParaInativar?.nome ?? ''}? Ele deixará de receber programação.`"
    texto-confirmar="Inativar"
    :carregando="inativando"
    @confirmar="inativar"
  />
</template>

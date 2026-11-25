<script setup>
import { onMounted, ref } from 'vue'
import ConfirmacaoDialog from '../components/ConfirmacaoDialog.vue'
import { useUsuariosStore } from '../stores/usuarios'
import { nomeDoPerfil } from '../utils/usuario'

const usuarios = useUsuariosStore()

const busca = ref('')
const usuarioParaInativar = ref(null)
const confirmandoInativacao = ref(false)
const inativando = ref(false)

const colunas = [
  { title: 'Nome', key: 'nome' },
  { title: 'E-mail', key: 'email' },
  { title: 'Perfil', key: 'perfil' },
  { title: 'Situação', key: 'ativo' },
  { title: 'Ações', key: 'acoes', sortable: false },
].map((coluna) => ({ ...coluna, align: 'center' }))

const filtrar = (valor, consulta, item) => {
  if (!consulta) return true
  const termo = consulta.toLowerCase()
  return item.raw.nome.toLowerCase().includes(termo) || item.raw.email.toLowerCase().includes(termo)
}

function pedirInativacao(usuario) {
  usuarioParaInativar.value = usuario
  confirmandoInativacao.value = true
}

async function inativar() {
  inativando.value = true
  try {
    await usuarios.inativar(usuarioParaInativar.value.id)
    confirmandoInativacao.value = false
    await usuarios.carregar()
  } catch {
    confirmandoInativacao.value = false
  } finally {
    inativando.value = false
  }
}

onMounted(() => usuarios.carregar())
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Usuários</h1>
    <v-spacer />
    <v-btn color="primary" variant="flat" prepend-icon="mdi-plus" to="/usuarios/novo">Novo usuário</v-btn>
  </div>

  <v-alert v-if="usuarios.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Não foi possível carregar os usuários.
    <template #append>
      <v-btn variant="text" size="small" @click="usuarios.carregar()">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <v-row>
    <v-col cols="12" md="6">
      <v-text-field v-model="busca" label="Buscar por nome ou e-mail" prepend-inner-icon="mdi-magnify" clearable hide-details />
    </v-col>
  </v-row>

  <v-data-table
    :headers="colunas"
    :items="usuarios.itens"
    :loading="usuarios.carregando"
    :search="busca ?? ''"
    :custom-filter="filtrar"
    items-per-page-text="Itens por página"
    no-data-text="Nenhum usuário encontrado. Crie um usuário ou mude a busca."
    loading-text="Carregando usuários..."
    class="mt-4"
  >
    <template #item.perfil="{ item }">{{ nomeDoPerfil[item.perfil] }}</template>
    <template #item.ativo="{ item }">
      <v-chip :color="item.ativo ? 'success' : 'grey'" variant="flat" size="small">{{ item.ativo ? 'Ativo' : 'Inativo' }}</v-chip>
    </template>
    <template #item.acoes="{ item }">
      <v-btn icon="mdi-pencil-outline" variant="text" size="small" color="secondary" aria-label="Editar usuário" title="Editar" :to="`/usuarios/${item.id}/editar`" />
      <v-btn
        icon="mdi-cancel"
        variant="text"
        size="small"
        color="error"
        aria-label="Inativar usuário"
        title="Inativar"
        :disabled="!item.ativo"
        @click="pedirInativacao(item)"
      />
    </template>
  </v-data-table>

  <ConfirmacaoDialog
    v-model="confirmandoInativacao"
    titulo="Inativar usuário"
    :mensagem="`Inativar o usuário ${usuarioParaInativar?.nome ?? ''}? Ele deixará de conseguir entrar no sistema.`"
    texto-confirmar="Inativar"
    :carregando="inativando"
    @confirmar="inativar"
  />
</template>

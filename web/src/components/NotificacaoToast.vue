<script setup>
import { useNotificacoesStore } from '../stores/notificacoes'

const notificacoes = useNotificacoesStore()
</script>

<template>
  <v-snackbar
    :key="notificacoes.id"
    v-model="notificacoes.visivel"
    :color="notificacoes.tipo"
    :timeout="notificacoes.tempo"
    location="bottom right"
  >
    <v-icon :icon="notificacoes.tipo === 'error' ? 'mdi-alert-circle' : 'mdi-check-circle'" class="mr-2" />
    {{ notificacoes.mensagem }}
    <template #actions>
      <v-btn variant="text" @click="notificacoes.visivel = false">Fechar</v-btn>
    </template>
  </v-snackbar>
</template>

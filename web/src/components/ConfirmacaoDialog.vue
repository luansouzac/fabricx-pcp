<script setup>
defineProps({
  titulo: { type: String, required: true },
  mensagem: { type: String, required: true },
  textoConfirmar: { type: String, default: 'Confirmar' },
  carregando: { type: Boolean, default: false },
})

const aberto = defineModel({ type: Boolean, default: false })
const emit = defineEmits(['confirmar'])
</script>

<template>
  <v-dialog v-model="aberto" max-width="440" :persistent="carregando">
    <v-card :title="titulo" :text="mensagem">
      <v-card-actions class="px-4 pb-4">
        <v-spacer />
        <v-btn variant="text" color="secondary" :disabled="carregando" @click="aberto = false">Cancelar</v-btn>
        <v-btn variant="flat" color="error" :loading="carregando" @click="emit('confirmar')">{{ textoConfirmar }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

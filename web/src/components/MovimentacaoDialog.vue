<script setup>
import { reactive, ref, watch } from 'vue'
import { useProdutosStore } from '../stores/produtos'

const props = defineProps({ produto: { type: Object, required: true } })

const aberto = defineModel({ type: Boolean, default: false })
const emit = defineEmits(['movimentado'])

const produtos = useProdutosStore()

const formulario = reactive({ tipo: 'Entrada', quantidade: null, motivo: '' })
const referenciaFormulario = ref(null)
const salvando = ref(false)

const regras = {
  quantidade: [(valor) => Number(valor) > 0 || 'Informe uma quantidade maior que zero.'],
  motivo: [(valor) => String(valor ?? '').length <= 200 || 'Use no máximo 200 caracteres.'],
}

watch(aberto, (estaAberto) => {
  if (!estaAberto) return
  Object.assign(formulario, { tipo: 'Entrada', quantidade: null, motivo: '' })
  referenciaFormulario.value?.resetValidation()
})

async function salvar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  salvando.value = true
  try {
    const atualizado = await produtos.movimentar(props.produto.id, {
      tipo: formulario.tipo,
      quantidade: Number(formulario.quantidade),
      motivo: formulario.motivo?.trim() || null,
    })
    aberto.value = false
    emit('movimentado', atualizado)
  } catch {
    // o toast de erro já foi exibido pelo cliente; o diálogo continua aberto
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <v-dialog v-model="aberto" max-width="480" :persistent="salvando">
    <v-card :title="`Movimentar estoque`" :subtitle="`${produto.codigo} · ${produto.nome} · saldo ${produto.estoqueAtual} ${produto.unidadeMedida}`">
      <v-form ref="referenciaFormulario" @submit.prevent="salvar">
        <v-card-text>
          <v-btn-toggle v-model="formulario.tipo" mandatory divided color="primary" class="mb-4">
            <v-btn value="Entrada" prepend-icon="mdi-arrow-down-bold">Entrada</v-btn>
            <v-btn value="Saida" prepend-icon="mdi-arrow-up-bold">Saída</v-btn>
          </v-btn-toggle>
          <v-text-field
            v-model.number="formulario.quantidade"
            label="Quantidade"
            type="number"
            min="0"
            :rules="regras.quantidade"
            autofocus
          />
          <v-text-field v-model="formulario.motivo" label="Motivo (opcional)" :rules="regras.motivo" counter="200" />
        </v-card-text>
        <v-card-actions class="px-4 pb-4">
          <v-spacer />
          <v-btn variant="text" color="secondary" :disabled="salvando" @click="aberto = false">Cancelar</v-btn>
          <v-btn type="submit" color="primary" variant="flat" :loading="salvando">Registrar</v-btn>
        </v-card-actions>
      </v-form>
    </v-card>
  </v-dialog>
</template>

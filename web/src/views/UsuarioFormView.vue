<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUsuariosStore } from '../stores/usuarios'
import { perfisDeUsuario as perfis } from '../utils/usuario'

const props = defineProps({ id: { type: String, default: null } })

const router = useRouter()
const usuarios = useUsuariosStore()

const editando = computed(() => props.id !== null)

const formulario = reactive({ nome: '', email: '', senha: '', perfil: 'Operador' })

const referenciaFormulario = ref(null)
const carregando = ref(false)
const salvando = ref(false)

const regras = {
  nome: [
    (valor) => !!String(valor ?? '').trim() || 'O nome é obrigatório.',
    (valor) => String(valor ?? '').length <= 100 || 'Use no máximo 100 caracteres.',
  ],
  email: [
    (valor) => !!String(valor ?? '').trim() || 'O e-mail é obrigatório.',
    (valor) => /^\S+@\S+\.\S+$/.test(String(valor ?? '').trim()) || 'Informe um e-mail válido.',
    (valor) => String(valor ?? '').length <= 200 || 'Use no máximo 200 caracteres.',
  ],
  senha: [
    (valor) => !!valor || 'A senha é obrigatória.',
    (valor) => (String(valor ?? '').length >= 8 && String(valor).length <= 100) || 'A senha deve ter de 8 a 100 caracteres.',
  ],
  perfil: [(valor) => !!valor || 'Selecione o perfil.'],
}

onMounted(async () => {
  if (!editando.value) return

  carregando.value = true
  try {
    const usuario = await usuarios.obter(props.id)
    formulario.nome = usuario.nome
    formulario.email = usuario.email
    formulario.perfil = usuario.perfil
  } catch {
    router.replace('/usuarios')
  } finally {
    carregando.value = false
  }
})

async function salvar() {
  const { valid } = await referenciaFormulario.value.validate()
  if (!valid) return

  salvando.value = true
  try {
    if (editando.value) {
      await usuarios.atualizar(props.id, { nome: formulario.nome.trim(), perfil: formulario.perfil })
    } else {
      await usuarios.criar({
        nome: formulario.nome.trim(),
        email: formulario.email.trim(),
        senha: formulario.senha,
        perfil: formulario.perfil,
      })
    }
    router.push('/usuarios')
  } catch {
    salvando.value = false
  }
}
</script>

<template>
  <h1 class="fx-titulo-pagina mb-1">{{ editando ? 'Editar usuário' : 'Novo usuário' }}</h1>
  <p class="fx-legenda mb-4">Quem acessa o sistema e o que cada perfil pode fazer.</p>

  <v-card max-width="800" :loading="carregando">
    <v-form ref="referenciaFormulario" @submit.prevent="salvar">
      <v-card-text>
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field v-model="formulario.nome" label="Nome" :rules="regras.nome" autofocus />
          </v-col>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="formulario.email"
              label="E-mail"
              type="email"
              autocomplete="off"
              :readonly="editando"
              :rules="editando ? [] : regras.email"
            />
          </v-col>
          <v-col v-if="!editando" cols="12" md="6">
            <v-text-field
              v-model="formulario.senha"
              label="Senha"
              type="password"
              autocomplete="new-password"
              hint="De 8 a 100 caracteres."
              :rules="regras.senha"
            />
          </v-col>
          <v-col cols="12" md="6">
            <v-select v-model="formulario.perfil" :items="perfis" label="Perfil" :rules="regras.perfil" />
          </v-col>
        </v-row>
      </v-card-text>
      <v-card-actions class="px-4 pb-4">
        <v-btn variant="text" color="secondary" to="/usuarios">Cancelar</v-btn>
        <v-spacer />
        <v-btn type="submit" color="primary" variant="flat" :loading="salvando">Salvar</v-btn>
      </v-card-actions>
    </v-form>
  </v-card>
</template>

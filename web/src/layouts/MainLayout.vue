<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useDisplay, useTheme } from 'vuetify'
import { useRoute, useRouter } from 'vue-router'
import PaletaComandos from '../components/PaletaComandos.vue'
import { useAuthStore } from '../stores/auth'
import logoClaro from '../assets/logo/fabricx-horizontal-claro.svg'
import logoEscuro from '../assets/logo/fabricx-horizontal-escuro.svg'

const itensPrincipais = [
  { titulo: 'Início', icone: 'mdi-view-dashboard-outline', rota: '/', exato: true },
  { titulo: 'Planejamento', icone: 'mdi-chart-timeline-variant', rota: '/planejamento', prefixos: ['/planejamento', '/demanda-prevista'] },
  { titulo: 'Produção', icone: 'mdi-clipboard-text-outline', rota: '/producao', prefixos: ['/producao', '/ordens'] },
  { titulo: 'Indicadores', icone: 'mdi-chart-line', rota: '/indicadores' },
]

const itensCadastros = [
  { titulo: 'Produtos', icone: 'mdi-package-variant-closed', rota: '/produtos' },
  { titulo: 'Recursos', icone: 'mdi-robot-industrial-outline', rota: '/recursos' },
]

const itemUsuarios = { titulo: 'Usuários', icone: 'mdi-account-multiple-outline', rota: '/usuarios' }

const auth = useAuthStore()
const route = useRoute()
const cadastros = computed(() => (auth.ehAdministrador ? [...itensCadastros, itemUsuarios] : itensCadastros))

const ativo = (item) => (item.prefixos ?? [item.rota]).some((prefixo) => route.path === prefixo || route.path.startsWith(`${prefixo}/`))
const cadastrosAtivo = computed(() => cadastros.value.some(ativo))
const grupoAberto = ref(['cadastros'])
watch(cadastrosAtivo, (valor) => valor && !grupoAberto.value.includes('cadastros') && grupoAberto.value.push('cadastros'), { immediate: true })
const router = useRouter()
const { mdAndUp } = useDisplay()
const menuAberto = ref(mdAndUp.value)
watch(mdAndUp, (largo) => (menuAberto.value = largo))
const theme = useTheme()
const escuro = computed(() => theme.current.value.dark)
const logo = computed(() => (escuro.value ? logoEscuro : logoClaro))

function alternarTema() {
  const novo = escuro.value ? 'fabricx' : 'fabricxDark'
  theme.change(novo)
  localStorage.setItem('fx-tema', novo)
}

const paletaAberta = ref(false)
const ehMac = /Mac|iPhone|iPad/.test(navigator.userAgentData?.platform ?? navigator.platform)
const atalho = ehMac ? '⌘K' : 'Ctrl K'

function aoTeclar(evento) {
  if ((evento.metaKey || evento.ctrlKey) && evento.key.toLowerCase() === 'k') {
    evento.preventDefault()
    paletaAberta.value = !paletaAberta.value
  }
}

onMounted(() => window.addEventListener('keydown', aoTeclar))
onBeforeUnmount(() => window.removeEventListener('keydown', aoTeclar))

function sair() {
  auth.sair()
  router.push({ name: 'login' })
}
</script>

<template>
  <v-navigation-drawer v-model="menuAberto" :permanent="mdAndUp" :temporary="!mdAndUp" width="256">
    <div class="d-flex justify-center px-4 pt-5 pb-6">
      <img :src="logo" alt="FabricX" height="32" />
    </div>
    <div class="px-3 pb-3">
      <v-btn
        block
        variant="outlined"
        color="secondary"
        class="text-none justify-start"
        prepend-icon="mdi-magnify"
        aria-label="Buscar"
        @click="paletaAberta = true"
      >
        Buscar
        <v-spacer />
        <span class="atalho">{{ atalho }}</span>
      </v-btn>
    </div>
    <v-list v-model:opened="grupoAberto" nav density="comfortable" color="primary">
      <v-list-item
        v-for="item in itensPrincipais"
        :key="item.rota"
        :to="item.rota"
        :prepend-icon="item.icone"
        :title="item.titulo"
        :active="item.exato ? route.path === item.rota : ativo(item)"
        rounded="lg"
      />

      <v-list-group value="cadastros">
        <template #activator="{ props: propriedades }">
          <v-list-item
            v-bind="propriedades"
            prepend-icon="mdi-database-cog-outline"
            title="Cadastros"
            rounded="lg"
            :active="cadastrosAtivo && !grupoAberto.includes('cadastros')"
          />
        </template>
        <v-list-item
          v-for="item in cadastros"
          :key="item.rota"
          :to="item.rota"
          :prepend-icon="item.icone"
          :title="item.titulo"
          :active="ativo(item)"
          rounded="lg"
        />
      </v-list-group>
    </v-list>
  </v-navigation-drawer>

  <v-app-bar height="56">
    <v-app-bar-nav-icon v-if="!mdAndUp" color="secondary" aria-label="Abrir menu" @click="menuAberto = !menuAberto" />
    <v-spacer />
    <span class="fx-legenda mr-2">{{ auth.usuario?.nome }}</span>
    <v-btn
      :icon="escuro ? 'mdi-weather-sunny' : 'mdi-weather-night'"
      variant="text"
      color="secondary"
      class="mr-2"
      :aria-label="escuro ? 'Usar tema claro' : 'Usar tema escuro'"
      @click="alternarTema"
    />
    <v-btn
      icon="mdi-logout"
      variant="text"
      color="secondary"
      class="mr-2"
      aria-label="Sair"
      @click="sair"
    />
  </v-app-bar>

  <PaletaComandos v-model="paletaAberta" @alternar-tema="alternarTema" />

  <v-main>
    <v-container fluid class="pa-4 pa-md-8">
      <router-view />
    </v-container>
  </v-main>
</template>

<style scoped>
.atalho {
  padding: 0 6px;
  border-radius: 4px;
  background: rgb(var(--v-theme-surface-variant));
  font-size: 11px;
}
</style>

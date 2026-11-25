<script setup>
import { computed, onMounted, reactive, watch } from 'vue'
import { useProgramacaoStore } from '../stores/programacao'
import { useRecursosStore } from '../stores/recursos'
import { chaveDoDia, diaPorExtenso, horaCurta, statusDaProgramacao } from '../utils/programacao'

const programacao = useProgramacaoStore()
const recursos = useRecursosStore()

const filtros = reactive({ recursoId: null, de: '', ate: '' })

const opcoesDeRecurso = computed(() => recursos.itens.map((recurso) => ({ title: recurso.nome, value: recurso.id })))
const periodoInvalido = computed(() => Boolean(filtros.de && filtros.ate && filtros.de > filtros.ate))
const filtrado = computed(() => Boolean(filtros.recursoId || filtros.de || filtros.ate))

const porRecurso = computed(() => {
  const grupos = new Map()
  for (const item of [...programacao.cronograma].sort((a, b) => a.inicioPrevisto.localeCompare(b.inicioPrevisto))) {
    if (!grupos.has(item.recursoId)) grupos.set(item.recursoId, { id: item.recursoId, nome: item.recursoNome, dias: new Map() })
    const dias = grupos.get(item.recursoId).dias
    const dia = chaveDoDia(item.inicioPrevisto)
    if (!dias.has(dia)) dias.set(dia, { chave: dia, titulo: diaPorExtenso(item.inicioPrevisto), itens: [] })
    dias.get(dia).itens.push(item)
  }
  return [...grupos.values()]
    .sort((a, b) => a.nome.localeCompare(b.nome))
    .map((grupo) => ({ ...grupo, dias: [...grupo.dias.values()] }))
})

function recarregar() {
  if (periodoInvalido.value) return
  programacao.carregarCronograma(filtros)
}

function limpar() {
  Object.assign(filtros, { recursoId: null, de: '', ate: '' })
}

watch(filtros, recarregar)

onMounted(() => {
  recursos.carregar()
  recarregar()
})
</script>

<template>
  <div class="d-flex align-center mb-4">
    <h1 class="fx-titulo-pagina">Programação</h1>
    <v-spacer />
    <v-btn variant="outlined" color="secondary" prepend-icon="mdi-refresh" :loading="programacao.carregando" @click="recarregar">
      Atualizar
    </v-btn>
  </div>

  <v-alert v-if="programacao.falhou" type="error" variant="flat" density="compact" class="mb-4">
    Não foi possível carregar o cronograma.
    <template #append>
      <v-btn variant="text" size="small" @click="recarregar">Tentar novamente</v-btn>
    </template>
  </v-alert>

  <v-row>
    <v-col cols="12" md="4">
      <v-autocomplete
        v-model="filtros.recursoId"
        :items="opcoesDeRecurso"
        label="Recurso"
        no-data-text="Nenhum recurso encontrado."
        clearable
        hide-details
      />
    </v-col>
    <v-col cols="12" sm="6" md="3">
      <v-text-field v-model="filtros.de" label="De" type="date" hide-details="auto" :error="periodoInvalido" />
    </v-col>
    <v-col cols="12" sm="6" md="3">
      <v-text-field
        v-model="filtros.ate"
        label="Até"
        type="date"
        :error-messages="periodoInvalido ? 'A data final precisa ser igual ou posterior à inicial.' : ''"
        hide-details="auto"
      />
    </v-col>
    <v-col cols="12" md="2" class="d-flex align-center">
      <v-btn variant="text" color="secondary" prepend-icon="mdi-filter-remove-outline" :disabled="!filtrado" @click="limpar">
        Limpar filtros
      </v-btn>
    </v-col>
  </v-row>

  <v-progress-linear v-if="programacao.carregando" indeterminate color="primary" class="mt-4" aria-label="Carregando cronograma" />

  <div v-else-if="!porRecurso.length && !programacao.falhou" class="text-center text-medium-emphasis py-8">
    Nenhuma etapa programada para os filtros escolhidos. Programe etapas pelo detalhe de uma ordem ou mude o filtro.
  </div>

  <v-card v-for="recurso in porRecurso" :key="recurso.id" class="mt-4" :title="recurso.nome">
    <v-card-text>
      <div v-for="dia in recurso.dias" :key="dia.chave" class="mb-4">
        <div class="fx-legenda text-capitalize mb-2">{{ dia.titulo }}</div>
        <v-list density="compact" class="pa-0">
          <v-list-item v-for="item in dia.itens" :key="item.id" :to="`/ordens/${item.ordemId}`" rounded="lg">
            <div class="d-flex align-center flex-wrap ga-3">
              <span class="font-weight-bold">{{ horaCurta(item.inicioPrevisto) }} – {{ horaCurta(item.fimPrevisto) }}</span>
              <span>{{ item.ordemNumero }} · {{ item.etapaNome }}</span>
              <v-spacer />
              <v-chip :color="statusDaProgramacao[item.status].color" variant="flat" size="small">
                {{ statusDaProgramacao[item.status].title }}
              </v-chip>
            </div>
          </v-list-item>
        </v-list>
      </div>
    </v-card-text>
  </v-card>
</template>

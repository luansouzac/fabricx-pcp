export const statusDaOrdem = [
  { title: 'Planejada', value: 'Planejada', color: 'secondary' },
  { title: 'Programada', value: 'Programada', color: 'primary' },
  { title: 'Em produção', value: 'EmProducao', color: 'info' },
  { title: 'Concluída', value: 'Concluida', color: 'success' },
  { title: 'Cancelada', value: 'Cancelada', color: 'error' },
]

export const prioridades = [
  { title: 'Baixa', value: 'Baixa', color: 'secondary' },
  { title: 'Média', value: 'Media', color: 'info' },
  { title: 'Alta', value: 'Alta', color: 'warning' },
  { title: 'Urgente', value: 'Urgente', color: 'error' },
]

export const infoDoStatus = Object.fromEntries(statusDaOrdem.map((status) => [status.value, status]))
export const infoDaPrioridade = Object.fromEntries(prioridades.map((prioridade) => [prioridade.value, prioridade]))

export const acaoDeAvanco = {
  Planejada: { rotulo: 'Programar', icone: 'mdi-calendar-check-outline' },
  Programada: { rotulo: 'Iniciar produção', icone: 'mdi-play-circle-outline' },
  EmProducao: { rotulo: 'Concluir', icone: 'mdi-check-circle-outline' },
}

export const podeCancelar = (status) => status === 'Planejada' || status === 'Programada'

export const dataCurta = (data) => data.split('-').reverse().join('/')

const formatadorDataHora = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
export const dataHora = (valor) => formatadorDataHora.format(new Date(valor))

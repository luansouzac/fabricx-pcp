export const statusDaProgramacao = {
  Agendada: { title: 'Agendada', color: 'secondary' },
  EmAndamento: { title: 'Em andamento', color: 'info' },
  Concluida: { title: 'Concluída', color: 'success' },
}

export const duracaoDaEtapa = (etapa, quantidade) => Math.ceil(quantidade / etapa.unidadesPorCiclo) * etapa.tempoCicloMin

export const somarMinutos = (inicio, minutos) => new Date(new Date(inicio).getTime() + minutos * 60000)

const doisDigitos = (numero) => String(numero).padStart(2, '0')

export function paraCampoDataHora(valor) {
  const data = new Date(valor)
  return `${data.getFullYear()}-${doisDigitos(data.getMonth() + 1)}-${doisDigitos(data.getDate())}T${doisDigitos(data.getHours())}:${doisDigitos(data.getMinutes())}`
}

const formatadorHora = new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' })
const formatadorDia = new Intl.DateTimeFormat('pt-BR', { weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric' })

export const horaCurta = (valor) => formatadorHora.format(new Date(valor))
export const diaPorExtenso = (valor) => formatadorDia.format(new Date(valor))
export const chaveDoDia = (valor) => paraCampoDataHora(valor).slice(0, 10)

const formatadorMinutos = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 1 })
export const formatarMinutos = (minutos) => `${formatadorMinutos.format(minutos)} min`

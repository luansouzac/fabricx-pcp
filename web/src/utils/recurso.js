export const tiposDeRecurso = [
  { title: 'Máquina', value: 'Maquina' },
  { title: 'Operador', value: 'Operador' },
  { title: 'Linha', value: 'Linha' },
]

export const nomeDoTipoRecurso = Object.fromEntries(tiposDeRecurso.map((tipo) => [tipo.value, tipo.title]))

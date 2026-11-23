export const tiposDeProduto = [
  { title: 'Matéria-prima', value: 'MateriaPrima' },
  { title: 'Em processo', value: 'EmProcesso' },
  { title: 'Acabado', value: 'Acabado' },
]

export const nomeDoTipo = Object.fromEntries(tiposDeProduto.map((tipo) => [tipo.value, tipo.title]))

export const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

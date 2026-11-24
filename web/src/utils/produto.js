export const tiposDeProduto = [
  { title: 'Matéria-prima', value: 'MateriaPrima' },
  { title: 'Em processo', value: 'EmProcesso' },
  { title: 'Acabado', value: 'Acabado' },
]

export const unidadesDeMedida = [
  { title: 'Unidade (UN)', value: 'UN' },
  { title: 'Peça (PC)', value: 'PC' },
  { title: 'Caixa (CX)', value: 'CX' },
  { title: 'Quilograma (KG)', value: 'KG' },
  { title: 'Grama (G)', value: 'G' },
  { title: 'Tonelada (T)', value: 'T' },
  { title: 'Litro (L)', value: 'L' },
  { title: 'Mililitro (ML)', value: 'ML' },
  { title: 'Metro (M)', value: 'M' },
  { title: 'Centímetro (CM)', value: 'CM' },
  { title: 'Metro quadrado (M2)', value: 'M2' },
  { title: 'Metro cúbico (M3)', value: 'M3' },
]

export const nomeDoTipo = Object.fromEntries(tiposDeProduto.map((tipo) => [tipo.value, tipo.title]))

export const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

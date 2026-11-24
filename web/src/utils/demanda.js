const formatadorQuantidade = new Intl.NumberFormat('pt-BR')

export function formatarPeriodo(periodo) {
  const [ano, mes] = periodo.split('-')
  return `${mes}/${ano}`
}

export const formatarQuantidade = (quantidade) => formatadorQuantidade.format(quantidade)

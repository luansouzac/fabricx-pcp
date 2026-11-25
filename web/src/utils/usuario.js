export const perfisDeUsuario = [
  { title: 'Administrador', value: 'Administrador' },
  { title: 'Operador', value: 'Operador' },
]

export const nomeDoPerfil = Object.fromEntries(perfisDeUsuario.map((perfil) => [perfil.value, perfil.title]))

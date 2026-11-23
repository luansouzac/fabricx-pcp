import { createRouter, createWebHistory } from 'vue-router'
import InicioView from '../views/InicioView.vue'
import ProdutosView from '../views/ProdutosView.vue'
import ProdutoFormView from '../views/ProdutoFormView.vue'
import ProdutoDetalheView from '../views/ProdutoDetalheView.vue'

const routes = [
  { path: '/', name: 'inicio', component: InicioView, meta: { titulo: 'Início' } },
  { path: '/produtos', name: 'produtos', component: ProdutosView, meta: { titulo: 'Produtos' } },
  { path: '/produtos/novo', name: 'produto-novo', component: ProdutoFormView, meta: { titulo: 'Novo produto' } },
  {
    path: '/produtos/:id(\\d+)',
    name: 'produto-detalhe',
    component: ProdutoDetalheView,
    props: true,
    meta: { titulo: 'Detalhe do produto' },
  },
  {
    path: '/produtos/:id(\\d+)/editar',
    name: 'produto-editar',
    component: ProdutoFormView,
    props: true,
    meta: { titulo: 'Editar produto' },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.afterEach((to) => {
  document.title = to.meta.titulo ? `${to.meta.titulo} · FabricX` : 'FabricX'
})

export default router

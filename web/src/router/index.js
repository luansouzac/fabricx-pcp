import { createRouter, createWebHistory } from 'vue-router'
import InicioView from '../views/InicioView.vue'
import ProdutosView from '../views/ProdutosView.vue'
import ProdutoFormView from '../views/ProdutoFormView.vue'
import ProdutoDetalheView from '../views/ProdutoDetalheView.vue'
import RecursosView from '../views/RecursosView.vue'
import RecursoFormView from '../views/RecursoFormView.vue'
import OrdensView from '../views/OrdensView.vue'
import OrdemFormView from '../views/OrdemFormView.vue'
import OrdemDetalheView from '../views/OrdemDetalheView.vue'

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
    path: '/produtos/:id(\\d+)/estrutura',
    name: 'produto-estrutura',
    component: ProdutoDetalheView,
    props: (rota) => ({ id: rota.params.id, aba: 'estrutura' }),
    meta: { titulo: 'Estrutura do produto' },
  },
  {
    path: '/produtos/:id(\\d+)/roteiro',
    name: 'produto-roteiro',
    component: ProdutoDetalheView,
    props: (rota) => ({ id: rota.params.id, aba: 'roteiro' }),
    meta: { titulo: 'Roteiro do produto' },
  },
  {
    path: '/produtos/:id(\\d+)/editar',
    name: 'produto-editar',
    component: ProdutoFormView,
    props: true,
    meta: { titulo: 'Editar produto' },
  },
  { path: '/recursos', name: 'recursos', component: RecursosView, meta: { titulo: 'Recursos' } },
  { path: '/recursos/novo', name: 'recurso-novo', component: RecursoFormView, meta: { titulo: 'Novo recurso' } },
  {
    path: '/recursos/:id(\\d+)/editar',
    name: 'recurso-editar',
    component: RecursoFormView,
    props: true,
    meta: { titulo: 'Editar recurso' },
  },
  { path: '/ordens', name: 'ordens', component: OrdensView, meta: { titulo: 'Ordens de produção' } },
  { path: '/ordens/nova', name: 'ordem-nova', component: OrdemFormView, meta: { titulo: 'Nova ordem' } },
  {
    path: '/ordens/:id(\\d+)',
    name: 'ordem-detalhe',
    component: OrdemDetalheView,
    props: true,
    meta: { titulo: 'Detalhe da ordem' },
  },
  {
    path: '/ordens/:id(\\d+)/editar',
    name: 'ordem-editar',
    component: OrdemFormView,
    props: true,
    meta: { titulo: 'Editar ordem' },
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

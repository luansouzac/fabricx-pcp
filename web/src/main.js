import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createVuetify } from 'vuetify'
import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import './styles/fabricx.css'
import './styles/tabela.css'
import { fabricxTheme, fabricxDarkTheme, fabricxDefaults } from './plugins/fabricx-theme'
import App from './App.vue'
import router from './router'

const vuetify = createVuetify({
  theme: {
    defaultTheme: localStorage.getItem('fx-tema') || 'fabricx',
    themes: { fabricx: fabricxTheme, fabricxDark: fabricxDarkTheme },
  },
  defaults: fabricxDefaults,
  icons: { defaultSet: 'mdi' },
})

createApp(App).use(createPinia()).use(router).use(vuetify).mount('#app')

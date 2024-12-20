import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import axios from 'axios'
import { config } from './config'

// Configurar axios
axios.defaults.baseURL = config.apiBaseUrl

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')

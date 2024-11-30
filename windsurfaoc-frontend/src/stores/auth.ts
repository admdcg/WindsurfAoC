import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { User } from '../types'
import axios from 'axios'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(loadUser())
  const loading = ref(false)
  const error = ref('')

  // Cargar usuario y token del localStorage
  function loadUser(): User | null {
    const userJson = localStorage.getItem('user')
    if (userJson) {
      const userData = JSON.parse(userJson)
      // Configurar el token en axios
      if (userData.token) {
        axios.defaults.headers.common['Authorization'] = `Bearer ${userData.token}`
      }
      return userData
    }
    return null
  }

  async function login(email: string, password: string) {
    try {
      loading.value = true
      error.value = ''
      const response = await axios.post('/api/auth/login', { email, password })
      user.value = response.data
      // Guardar en localStorage y configurar axios
      localStorage.setItem('user', JSON.stringify(response.data))
      axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`
    } catch (e) {
      error.value = 'Error al iniciar sesión'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function register(email: string, password: string) {
    try {
      loading.value = true
      error.value = ''
      const response = await axios.post('/api/auth/register', { email, password })
      user.value = response.data
      // Guardar en localStorage y configurar axios
      localStorage.setItem('user', JSON.stringify(response.data))
      axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`
    } catch (e) {
      error.value = 'Error al registrar usuario'
      throw e
    } finally {
      loading.value = false
    }
  }

  function logout() {
    user.value = null
    // Limpiar localStorage y quitar token de axios
    localStorage.removeItem('user')
    delete axios.defaults.headers.common['Authorization']
  }

  return {
    user,
    loading,
    error,
    login,
    register,
    logout
  }
})

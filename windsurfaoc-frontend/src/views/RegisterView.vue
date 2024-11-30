<template>
  <div class="register-container">
    <h2>Registro</h2>
    <form @submit.prevent="handleSubmit" class="register-form">
      <div class="form-group">
        <label for="email">Email:</label>
        <input 
          type="email" 
          id="email" 
          v-model="email" 
          required 
          class="form-control"
        >
      </div>
      
      <div class="form-group">
        <label for="password">Contraseña:</label>
        <input 
          type="password" 
          id="password" 
          v-model="password" 
          required 
          class="form-control"
        >
      </div>

      <div class="form-group">
        <label for="confirmPassword">Confirmar Contraseña:</label>
        <input 
          type="password" 
          id="confirmPassword" 
          v-model="confirmPassword" 
          required 
          class="form-control"
        >
      </div>

      <button type="submit" class="btn-primary" :disabled="authStore.loading || !isValid">
        {{ authStore.loading ? 'Registrando...' : 'Registrarse' }}
      </button>

      <p v-if="authStore.error" class="error-message">
        {{ authStore.error }}
      </p>
    </form>

    <p class="login-link">
      ¿Ya tienes cuenta? 
      <router-link to="/login">Inicia sesión aquí</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')

const isValid = computed(() => {
  return password.value === confirmPassword.value && password.value.length >= 6
})

async function handleSubmit() {
  if (!isValid.value) {
    return
  }

  try {
    await authStore.register(email.value, password.value)
    router.push('/')
  } catch (error) {
    console.error('Error en el registro:', error)
  }
}
</script>

<style scoped>
.register-container {
  max-width: 400px;
  margin: 2rem auto;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  background-color: white;
}

.register-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-control {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.btn-primary {
  padding: 0.75rem;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.btn-primary:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}

.error-message {
  color: red;
  margin-top: 1rem;
}

.login-link {
  margin-top: 1rem;
  text-align: center;
}

.login-link a {
  color: #4CAF50;
  text-decoration: none;
}

.login-link a:hover {
  text-decoration: underline;
}
</style>

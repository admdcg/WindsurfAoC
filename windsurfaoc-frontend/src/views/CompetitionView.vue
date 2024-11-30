<template>
  <div class="competition-container">
    <div v-if="competitionStore.loading" class="loading">
      Cargando detalles de la competición...
    </div>

    <div v-else-if="competitionStore.error" class="error-message">
      {{ competitionStore.error }}
    </div>

    <div v-else-if="competitionStore.currentCompetition" class="competition-details">
      <h1>{{ competitionStore.currentCompetition.name }}</h1>
      
      <div class="competition-info">
        <p>Estado: {{ competitionStore.currentCompetition.isActive ? 'Activa' : 'Finalizada' }}</p>
        <p>Inicio: {{ new Date(competitionStore.currentCompetition.startDate).toLocaleDateString() }}</p>
        <p>Fin: {{ new Date(competitionStore.currentCompetition.endDate).toLocaleDateString() }}</p>
      </div>

      <div class="actions">
        <router-link 
          :to="{ name: 'ranking', params: { id: competitionStore.currentCompetition.id }}"
          class="btn-primary"
        >
          Ver Ranking
        </router-link>
      </div>

      <div v-if="!authStore.user" class="login-prompt">
        <p>Debes iniciar sesión para ver los detalles de la competición.</p>
        <router-link :to="{ name: 'login' }">Iniciar sesión</router-link>
      </div>

      <div v-else class="challenge-grid">
        <div class="day-column" v-for="day in days" :key="day">
          <div class="day-header">Día {{ day }}</div>
          <div class="stars">
            <button 
              class="star-button"
              :class="{ 'completed': isCompleted(day, 1), 'pending': !isCompleted(day, 1) }"
              :disabled="isCompleted(day, 1)"
              @click="handleCompletion(day, 1)"
              :title="isCompleted(day, 1) ? 'Completado' : 'Pendiente'"
            >
              ★
            </button>
            <button 
              class="star-button"
              :class="{ 'completed': isCompleted(day, 2), 'pending': !isCompleted(day, 2) }"
              :disabled="isCompleted(day, 2)"
              @click="handleCompletion(day, 2)"
              :title="isCompleted(day, 2) ? 'Completado' : 'Pendiente'"
            >
              ★
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useCompetitionStore } from '../stores/competition'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const competitionStore = useCompetitionStore()
const authStore = useAuthStore()

onMounted(async () => {
  const competitionId = Number(route.params.id)
  await competitionStore.fetchCompetitionDetails(competitionId)
})

function isCompleted(day: number, part: number): boolean {
  if (!authStore.user || !competitionStore.completions) return false
  
  return competitionStore.completions.some(completion => 
    completion.userId === authStore.user?.id && 
    completion.dayNumber === day && 
    completion.partNumber === part
  )
}

async function handleCompletion(day: number, part: number) {
  if (!competitionStore.currentCompetition || isCompleted(day, part)) return

  try {
    await competitionStore.submitCompletion(
      competitionStore.currentCompetition.id,
      day,
      part
    )
  } catch (error) {
    console.error('Error al completar la parte:', error)
  }
}

// Computed property para generar el array de días
const days = computed(() => {
  if (!competitionStore.currentCompetition) return []
  
  const start = new Date(competitionStore.currentCompetition.startDate)
  const end = new Date(competitionStore.currentCompetition.endDate)
  const dayCount = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1
  return Array.from({ length: dayCount }, (_, i) => i + 1)
})
</script>

<style scoped>
.competition-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.competition-info {
  margin: 2rem 0;
  padding: 1rem;
  background-color: #f5f5f5;
  border-radius: 8px;
}

.challenge-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  margin-top: 2rem;
}

.day-column {
  background-color: white;
  border-radius: 8px;
  padding: 1rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.day-header {
  font-weight: bold;
  margin-bottom: 0.5rem;
}

.stars {
  display: flex;
  gap: 0.5rem;
}

.star-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  padding: 0.25rem;
}

.star-button.completed {
  color: #FFD700; /* Dorado */
  text-shadow: 0 0 5px rgba(255, 215, 0, 0.5);
  cursor: default;
}

.star-button.pending {
  color: #C0C0C0; /* Plateado */
  cursor: pointer;
}

.star-button.pending:hover:not(:disabled) {
  color: #FFD700; /* Dorado al hover */
  transform: scale(1.1);
}

.star-button:disabled {
  cursor: not-allowed;
  opacity: 1; /* Mantenemos la opacidad completa */
}

.actions {
  margin-top: 2rem;
  text-align: center;
}

.btn-primary {
  padding: 0.75rem 1.5rem;
  background-color: #2196F3;
  color: white;
  text-decoration: none;
  border-radius: 4px;
  display: inline-block;
}

.login-prompt {
  text-align: center;
  margin: 2rem 0;
  padding: 2rem;
  background-color: #f5f5f5;
  border-radius: 8px;
}

.login-prompt p {
  margin-bottom: 1rem;
  color: #666;
}

.loading {
  text-align: center;
  margin-top: 2rem;
  font-size: 1.2rem;
  color: #666;
}

.error-message {
  color: red;
  text-align: center;
  margin-top: 2rem;
}
</style>

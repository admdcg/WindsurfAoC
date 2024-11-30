<template>
  <div class="ranking-container">
    <div v-if="competitionStore.loading" class="loading">
      Cargando ranking...
    </div>

    <div v-else-if="competitionStore.error" class="error-message">
      {{ competitionStore.error }}
    </div>

    <div v-else-if="competitionStore.currentCompetition" class="ranking-content">
      <h1>Ranking - {{ competitionStore.currentCompetition.name }}</h1>

      <div class="ranking-table">
        <table>
          <thead>
            <tr>
              <th>Posición</th>
              <th>Usuario</th>
              <th>Puntos</th>
              <th>Desafíos Completados</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(participant, index) in sortedParticipants" :key="participant.id">
              <td>{{ index + 1 }}</td>
              <td>{{ participant.email }}</td>
              <td>{{ participant.totalPoints }}</td>
              <td class="challenges">
                <div class="challenge-grid">
                  <template v-for="day in 25" :key="day">
                    <div class="challenge-day">
                      <span 
                        class="star"
                        :class="{
                          'completed': isPartCompleted(participant.userId, day, 1),
                          'gold': isPartCompleted(participant.userId, day, 1)
                        }"
                      >★</span>
                      <span 
                        class="star"
                        :class="{
                          'completed': isPartCompleted(participant.userId, day, 2),
                          'gold': isPartCompleted(participant.userId, day, 2)
                        }"
                      >★</span>
                    </div>
                  </template>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useCompetitionStore } from '../stores/competition'

const route = useRoute()
const competitionStore = useCompetitionStore()

const sortedParticipants = computed(() => {
  return [...competitionStore.participants].sort((a, b) => b.totalPoints - a.totalPoints)
})

function isPartCompleted(userId: number, day: number, part: number) {
  return competitionStore.completions.some(c => 
    c.userId === userId && 
    c.dayNumber === day && 
    c.partNumber === part
  )
}

onMounted(async () => {
  const competitionId = Number(route.params.id)
  await competitionStore.fetchCompetitionDetails(competitionId)
})
</script>

<style scoped>
.ranking-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.ranking-table {
  margin-top: 2rem;
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  background-color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

th, td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

th {
  background-color: #f5f5f5;
  font-weight: bold;
}

.challenges {
  padding: 0.5rem;
}

.challenge-grid {
  display: grid;
  grid-template-columns: repeat(25, 1fr);
  gap: 0.25rem;
  align-items: center;
}

.challenge-day {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
}

.star {
  font-size: 12px;
  color: #ddd;
}

.star.completed {
  color: #ffd700;
}

.star.gold {
  color: #ffd700;
}

.loading {
  text-align: center;
  padding: 2rem;
}

.error-message {
  color: red;
  text-align: center;
  padding: 2rem;
}
</style>

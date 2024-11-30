<template>
  <div class="home-container">
    <h1>Competiciones</h1>
    <div v-if="authStore.user?.isAdmin" class="admin-actions">
      <button 
        @click="showCreateModal = true" 
        class="btn-create"
      >
        <span class="icon">+</span>
        Nueva Competición
      </button>
    </div>

    <div v-if="competitionStore.loading" class="loading">
      Cargando competiciones...
    </div>

    <div v-else-if="competitionStore.error" class="error-message">
      {{ competitionStore.error }}
    </div>

    <div v-else class="competitions-grid">
      <div v-for="competition in competitionStore.competitions" :key="competition.id" class="competition-card">
        <h2>{{ competition.name }}</h2>
        <div class="competition-info">
          <p>Estado: {{ competition.isActive ? 'Activa' : 'Finalizada' }}</p>
          <p>Inicio: {{ new Date(competition.startDate).toLocaleDateString() }}</p>
          <p>Fin: {{ new Date(competition.endDate).toLocaleDateString() }}</p>
          <p>Participantes: {{ competition.participants.length }}</p>
        </div>
        <div class="competition-actions">
          <!-- Opciones de administrador -->
          <template v-if="authStore.user?.isAdmin">
            <button 
              @click="showEditModal(competition)"
              class="btn-edit"
            >
              Editar
            </button>
          </template>
          <!-- Si el usuario está autenticado y no es participante -->
          <template v-else-if="authStore.user && !isParticipant(competition)">
            <button 
              @click="showJoinConfirmation(competition)"
              class="btn-join"
              :disabled="!competition.isActive"
            >
              Unirse
            </button>
          </template>
          <!-- Si el usuario es participante -->
          <template v-else-if="authStore.user && isParticipant(competition)">
            <router-link 
              :to="{ name: 'competition', params: { id: competition.id }}"
              class="btn-primary"
            >
              Detalles
            </router-link>
          </template>
          <router-link 
            :to="{ name: 'ranking', params: { id: competition.id }}"
            class="btn-primary"
          >
            Ranking
          </router-link>
        </div>
      </div>
    </div>

    <!-- Modal de creación -->
    <div v-if="showCreateModal" class="modal-overlay">
      <div class="modal">
        <h2>Crear Nueva Competición</h2>
        <form @submit.prevent="handleCreateCompetition" class="competition-form">
          <div class="form-group">
            <label for="name">Nombre:</label>
            <input 
              type="text" 
              id="name" 
              v-model="newCompetition.name" 
              required 
              class="form-control"
            >
          </div>
          
          <div class="form-group">
            <label for="startDate">Fecha de inicio:</label>
            <input 
              type="date" 
              id="startDate" 
              v-model="newCompetition.startDate" 
              required 
              class="form-control"
            >
          </div>

          <div class="form-group">
            <label for="endDate">Fecha de fin:</label>
            <input 
              type="date" 
              id="endDate" 
              v-model="newCompetition.endDate" 
              required 
              class="form-control"
            >
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn-primary">Crear</button>
            <button type="button" @click="showCreateModal = false" class="btn-secondary">Cancelar</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal de edición -->
    <div v-if="editingCompetition" class="modal-overlay">
      <div class="modal">
        <h2>Editar Competición</h2>
        <form @submit.prevent="handleEditCompetition" class="competition-form">
          <div class="form-group">
            <label for="editName">Nombre:</label>
            <input 
              type="text" 
              id="editName" 
              v-model="editingCompetition.name" 
              required 
              class="form-control"
            >
          </div>
          
          <div class="form-group">
            <label for="editStartDate">Fecha de inicio:</label>
            <input 
              type="date" 
              id="editStartDate" 
              v-model="editingCompetition.startDate" 
              required 
              class="form-control"
            >
          </div>

          <div class="form-group">
            <label for="editEndDate">Fecha de fin:</label>
            <input 
              type="date" 
              id="editEndDate" 
              v-model="editingCompetition.endDate" 
              required 
              class="form-control"
            >
          </div>

          <div class="form-group">
            <label>
              <input 
                type="checkbox" 
                v-model="editingCompetition.isActive"
              >
              Competición activa
            </label>
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn-primary">Guardar</button>
            <button type="button" @click="editingCompetition = null" class="btn-secondary">Cancelar</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal de confirmación -->
    <div v-if="selectedCompetition" class="modal-overlay">
      <div class="modal">
        <h2>Confirmar participación</h2>
        <p>¿Estás seguro de que quieres unirte a la competición "{{ selectedCompetition.name }}"?</p>
        <p>Al unirte, podrás:</p>
        <ul>
          <li>Marcar los desafíos como completados</li>
          <li>Aparecer en el ranking de la competición</li>
          <li>Competir con otros participantes</li>
        </ul>
        <div class="modal-actions">
          <button @click="handleJoinCompetition" class="btn-primary">Sí, unirme</button>
          <button @click="closeJoinModal" class="btn-secondary">Cancelar</button>
        </div>
      </div>
    </div>

    <!-- Modal de login requerido -->
    <div v-if="showLoginPrompt" class="modal-overlay">
      <div class="modal">
        <h2>Inicio de sesión requerido</h2>
        <p>Debes iniciar sesión para unirte a una competición.</p>
        <div class="modal-actions">
          <router-link to="/login" class="btn-primary">Ir a iniciar sesión</router-link>
          <button @click="showLoginPrompt = false" class="btn-secondary">Cancelar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useCompetitionStore } from '../stores/competition'
import { useAuthStore } from '../stores/auth'

const competitionStore = useCompetitionStore()
const authStore = useAuthStore()
const selectedCompetition = ref(null)
const showLoginPrompt = ref(false)
const showCreateModal = ref(false)
const newCompetition = ref({
  name: '',
  startDate: '',
  endDate: ''
})
const editingCompetition = ref(null)

onMounted(async () => {
  await competitionStore.fetchCompetitions()
})

function isParticipant(competition) {
  if (!authStore.user) return false
  return competition.participants.some(p => p.userId === authStore.user.id)
}

function showJoinConfirmation(competition) {
  if (!authStore.user) {
    showLoginPrompt.value = true
    return
  }
  selectedCompetition.value = competition
}

function closeJoinModal() {
  selectedCompetition.value = null
}

async function handleJoinCompetition() {
  if (!selectedCompetition.value) return

  try {
    await competitionStore.joinCompetition(selectedCompetition.value.id)
    await competitionStore.fetchCompetitions() // Recargar la lista
    closeJoinModal()
  } catch (error) {
    console.error('Error al unirse a la competición:', error)
  }
}

function showEditModal(competition) {
  // Convertir las fechas al formato YYYY-MM-DD para los inputs date
  const startDate = new Date(competition.startDate)
  const endDate = new Date(competition.endDate)
  
  editingCompetition.value = {
    ...competition,
    startDate: startDate.toISOString().split('T')[0],
    endDate: endDate.toISOString().split('T')[0]
  }
}

async function handleEditCompetition() {
  if (!editingCompetition.value) return

  try {
    await competitionStore.updateCompetition(
      editingCompetition.value.id,
      editingCompetition.value.name,
      new Date(editingCompetition.value.startDate),
      new Date(editingCompetition.value.endDate),
      editingCompetition.value.isActive
    )
    await competitionStore.fetchCompetitions() // Recargar la lista
    editingCompetition.value = null
  } catch (error) {
    console.error('Error al editar la competición:', error)
  }
}

async function handleCreateCompetition() {
  try {
    await competitionStore.createCompetition(
      newCompetition.value.name,
      new Date(newCompetition.value.startDate),
      new Date(newCompetition.value.endDate)
    )
    showCreateModal.value = false
    newCompetition.value = {
      name: '',
      startDate: '',
      endDate: ''
    }
  } catch (error) {
    console.error('Error al crear la competición:', error)
  }
}
</script>

<style scoped>
.home-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.admin-actions {
  margin: 1rem 0 2rem;
}

.btn-create {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.3s ease;
}

.btn-create:hover {
  background-color: #45a049;
}

.btn-create .icon {
  font-size: 1.2rem;
  font-weight: bold;
}

.competitions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.competition-card {
  background-color: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.competition-info {
  margin: 1rem 0;
  color: #666;
}

.competition-info p {
  margin: 0.5rem 0;
}

.competition-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.btn-primary, .btn-join, .btn-edit {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  font-size: 1rem;
  flex: 1;
  text-align: center;
}

.btn-primary {
  background-color: #2196F3;
  color: white;
}

.btn-join {
  background-color: #4CAF50;
  color: white;
}

.btn-join:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}

.btn-edit {
  background-color: #FF9800;
  color: white;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background-color: white;
  padding: 2rem;
  border-radius: 8px;
  max-width: 500px;
  width: 90%;
}

.modal h2 {
  margin-bottom: 1rem;
  color: #333;
}

.modal p {
  margin-bottom: 1rem;
  color: #666;
}

.modal ul {
  margin: 1rem 0;
  padding-left: 1.5rem;
  color: #666;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.btn-secondary {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  flex: 1;
  background-color: #f5f5f5;
  color: #333;
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

.competition-form {
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
  padding: 0.75rem 1rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 1rem;
}
</style>

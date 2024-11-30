import { defineStore } from 'pinia'
import axios from 'axios'
import { ref } from 'vue'

export const useCompetitionStore = defineStore('competition', () => {
  const competitions = ref<any[]>([])
  const currentCompetition = ref<any>(null)
  const participants = ref<any[]>([])
  const completions = ref<any[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchCompetitions() {
    try {
      loading.value = true
      error.value = null
      const response = await axios.get('http://localhost:5071/api/competitions')
      competitions.value = response.data
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al cargar las competiciones'
      console.error('Error fetching competitions:', err)
    } finally {
      loading.value = false
    }
  }

  async function fetchCompetitionDetails(id: number) {
    try {
      loading.value = true
      error.value = null
      const [competitionResponse, participantsResponse, completionsResponse] = await Promise.all([
        axios.get(`http://localhost:5071/api/competitions/${id}`),
        axios.get(`http://localhost:5071/api/competitions/${id}/participants`),
        axios.get(`http://localhost:5071/api/competitions/${id}/completions`)
      ])
      currentCompetition.value = competitionResponse.data
      participants.value = participantsResponse.data
      completions.value = completionsResponse.data
      
      // Log para depuración
      console.log('Completions recibidas:', completionsResponse.data)
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al cargar los detalles de la competición'
      console.error('Error fetching competition details:', err)
    } finally {
      loading.value = false
    }
  }

  async function joinCompetition(id: number) {
    try {
      loading.value = true
      error.value = null
      await axios.post(`http://localhost:5071/api/competitions/${id}/join`)
      // Recargar los detalles de la competición para actualizar la lista de participantes
      await fetchCompetitionDetails(id)
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al unirse a la competición'
      console.error('Error joining competition:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function submitCompletion(competitionId: number, day: number, part: number) {
    try {
      loading.value = true
      error.value = null
      await axios.post(`http://localhost:5071/api/competitions/${competitionId}/days/${day}/complete`, {
        partNumber: part
      })
      // Recargar los detalles de la competición para actualizar el estado
      await fetchCompetitionDetails(competitionId)
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al marcar la parte como completada'
      console.error('Error submitting completion:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function createCompetition(name: string, startDate: Date, endDate: Date) {
    try {
      loading.value = true
      error.value = null
      const response = await axios.post('http://localhost:5071/api/competitions', {
        name,
        startDate,
        endDate
      })
      await fetchCompetitions() // Recargar la lista después de crear
      return response.data
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al crear la competición'
      console.error('Error creating competition:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function updateCompetition(id: number, name: string, startDate: Date, endDate: Date, isActive: boolean) {
    try {
      loading.value = true
      error.value = null
      await axios.put(`http://localhost:5071/api/competitions/${id}`, {
        name,
        startDate,
        endDate,
        isActive
      })
      await fetchCompetitions() // Recargar la lista después de actualizar
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al actualizar la competición'
      console.error('Error updating competition:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    competitions,
    currentCompetition,
    participants,
    completions,
    loading,
    error,
    fetchCompetitions,
    fetchCompetitionDetails,
    joinCompetition,
    submitCompletion,
    createCompetition,
    updateCompetition
  }
})

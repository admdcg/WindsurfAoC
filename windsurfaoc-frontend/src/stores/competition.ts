import { defineStore } from 'pinia'
import axios from 'axios'
import { ref } from 'vue'
import type { Competition, CreateCompetitionRequest } from '../types/competition'

export const useCompetitionStore = defineStore('competition', () => {
  const competitions = ref<Competition[]>([])
  const currentCompetition = ref<Competition | null>(null)
  const participants = ref<any[]>([])
  const completions = ref<any[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchCompetitions() {
    try {
      loading.value = true
      error.value = null
      const response = await axios.get('/competitions')
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
        axios.get(`/competitions/${id}`),
        axios.get(`/competitions/${id}/participants`),
        axios.get(`/competitions/${id}/completions`)
      ])
      currentCompetition.value = competitionResponse.data
      participants.value = participantsResponse.data
      completions.value = completionsResponse.data
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
      await axios.post(`/competitions/${id}/join`)
      await fetchCompetitions()
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
      await axios.post(`/competitions/${competitionId}/completions`, {
        day,
        part
      })
      await fetchCompetitionDetails(competitionId)
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al enviar la solución'
      console.error('Error submitting completion:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function createCompetition(request: CreateCompetitionRequest) {
    try {
      loading.value = true
      error.value = null
      await axios.post('/competitions', request)
      await fetchCompetitions()
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Error al crear la competición'
      console.error('Error creating competition:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function updateCompetition(competition: Competition) {
    try {
      loading.value = true
      error.value = null
      await axios.put(`/competitions/${competition.id}`, competition)
      await fetchCompetitions()
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

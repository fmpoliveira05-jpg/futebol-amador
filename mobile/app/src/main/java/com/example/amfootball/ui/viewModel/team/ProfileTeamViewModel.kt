package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.UiState
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.services.TeamService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Perfil de Equipa.
 *
 * Este ViewModel coordena a obtenção de dados detalhados de uma equipa através do [TeamService]
 * e expõe esses dados e o estado da interface (Loading/Erro) para a UI.
 *
 * @property savedStateHandle O manipulador de estado para recuperar argumentos de navegação (ex: teamId).
 * @property teamRepository O repositório responsável pela comunicação com a API de equipas.
 * @property sessionManager para obter o id da team quando se entra atraves do login
 */
@HiltViewModel
class ProfileTeamViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val teamRepository: TeamService,
    private val sessionManager: SessionManager
): ViewModel() {
    /**
     * LiveData que contém os dados da equipa ([ProfileTeamDto]) quando carregados com sucesso.
     * A UI observa esta variável para preencher os campos do perfil.
     */
    private val infoTeam: MutableStateFlow<ProfileTeamDto> = MutableStateFlow(ProfileTeamDto())
    val uiInfoTeam: StateFlow<ProfileTeamDto> = infoTeam

    /**
     * O ID da equipa recuperado dos argumentos da navegação.
     * Pode ser nulo se a navegação não fornecer um ID (caso em que se usa um fallback).
     */
    private var teamId: String? = savedStateHandle["teamId"]

    /**
     * StateFlow que gere os estados visuais da UI:
     * - `isLoading`: Se a app está à espera da resposta da API.
     * - `errorMessage`: Se ocorreu algum erro (Rede ou HTTP) para mostrar ao utilizador.
     */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState

    init {
        val teamIdSessionManager = sessionManager.getUserProfile()?.idTeam

        if (teamId.isNullOrBlank() && !teamIdSessionManager.isNullOrBlank()) {
            teamId = teamIdSessionManager
        }

        if (!teamId.isNullOrBlank()) {
            loadTeamProfile()
        } else {
            _uiState.update { it.copy(isLoading = false, errorMessage = "ID da equipa não encontrado.") }
        }
    }

    /**
     * Ação pública para tentar recarregar os dados.
     */
    fun retry() {
        loadTeamProfile()
    }

    /**
     * Realiza a chamada assíncrona à API para obter os detalhes da equipa.
     *
     * Fluxo de execução:
     * 1. Define `isLoading = true` no [_uiState].
     * 2. Chama o repositório.
     * 3. Se sucesso: Atualiza [infoTeam] com os dados e remove o loading.
     * 4. Se erro: Atualiza `errorMessage` no [_uiState] e remove o loading.
     *
     * @param teamId O identificador único (GUID/String) da equipa a carregar.
     */
    private fun loadTeamProfile() {
        val idToLoad = teamId
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }
            try {
                if(!idToLoad.isNullOrBlank()) {
                    val profile = teamRepository.getTeamProfile(teamId = idToLoad)

                    infoTeam.value = profile
                    _uiState.update { it.copy(isLoading = false) }
                } else {
                    _uiState.update { it.copy(isLoading = true, errorMessage = null) }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem conexão: ${e.localizedMessage}")
                }
            }
        }
    }
}
package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.UiState
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.repository.TeamRepository
import com.example.amfootball.utils.NetworkUtils
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Perfil de Equipa.
 *
 * Este ViewModel coordena a obtenção de dados detalhados de uma equipa através do [TeamRepository]
 * e expõe esses dados e o estado da interface (Loading/Erro) para a UI.
 *
 * @property savedStateHandle O manipulador de estado para recuperar argumentos de navegação (ex: teamId).
 * @property repository O repositório responsável pela comunicação com a API de equipas.
 */
@HiltViewModel
class ProfileTeamViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val repository: TeamRepository
): ViewModel() {
    /**
     * LiveData que contém os dados da equipa ([ProfileTeamDto]) quando carregados com sucesso.
     * A UI observa esta variável para preencher os campos do perfil.
     */
    private val infoTeam: MutableLiveData<ProfileTeamDto> = MutableLiveData(null)
    val uiInfoTeam: LiveData<ProfileTeamDto> = infoTeam

    /**
     * O ID da equipa recuperado dos argumentos da navegação.
     * Pode ser nulo se a navegação não fornecer um ID (caso em que se usa um fallback).
     */
    val teamId: String? = savedStateHandle["teamId"]

    /**
     * StateFlow que gere os estados visuais da UI:
     * - `isLoading`: Se a app está à espera da resposta da API.
     * - `errorMessage`: Se ocorreu algum erro (Rede ou HTTP) para mostrar ao utilizador.
     */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState

    init {
        if (teamId != null) {
            // Carrega a equipa específica passada pela navegação
            loadTeamProfile("B5225BE4-8336-4CC3-BB7A-E695A39A4FD0")
        } else {
            //Carrega a equipa do utilizador
            //TODO: Carregar com base no nome da team
            loadTeamProfile("B5225BE4-8336-4CC3-BB7A-E695A39A4FD0")
        }
        //infoTeam.value = ProfileTeamInfoDto.profileExempleTeam()
    }

    /**
     * Ação pública para tentar recarregar os dados.
     */
    fun retry() {
        val id = teamId ?: "B5225BE4-8336-4CC3-BB7A-E695A39A4FD0"
        loadTeamProfile(teamId = id)
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
    private fun loadTeamProfile(teamId: String) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }
            try {
                val response = repository.getTeamProfile(teamId = teamId)

                if (response.isSuccessful && response.body() != null) {
                    infoTeam.value = response.body()

                    _uiState.update { it.copy(isLoading = false) }
                } else {
                    val errorRaw = response.errorBody()?.string()
                    val errorMsg = NetworkUtils.parseBackendError(errorRaw)
                        ?: "Erro desconhecido: ${response.code()}"

                    _uiState.update {
                        it.copy(isLoading = false, errorMessage = errorMsg)
                    }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem conexão: ${e.localizedMessage}")
                }
            }
        }
    }
}
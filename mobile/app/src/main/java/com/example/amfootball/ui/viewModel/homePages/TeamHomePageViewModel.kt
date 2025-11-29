package com.example.amfootball.ui.viewModel.homePages

import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.repository.TeamRepository
import com.example.amfootball.ui.viewModel.abstracts.BaseViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

//TODO: Meter um endPoint na API, que carrega os dados da teamDto + próximos 3 jogos da equipa + sequencia de resultados 5 próximos jogos (W, L, D), depois trocar o TeamDto, por isso
/**
 * ViewModel responsável pela lógica de negócio e gestão de estado da Home Page da Equipa.
 *
 * Este ViewModel atua como o controlador central para:
 * 1. Carregar as informações da equipa do utilizador logado.
 * 2. Determinar os privilégios do utilizador (Admin vs Membro) para exibir/ocultar funcionalidades.
 * 3. Gerir a navegação segura, garantindo que o dispositivo tem conectividade antes de avançar.
 *
 * Herda de [BaseViewModel] para gestão automática de Loading, Erros e Conectividade.
 *
 * @property teamRepository Repositório para buscar dados da equipa à API.
 * @property networkObserver Observador de rede para validações de conectividade.
 * @property sessionManager Gestor de sessão local para recuperar o ID da equipa e status de admin.
 */
@HiltViewModel
class TeamHomePageViewModel @Inject constructor(
    private val teamRepository: TeamRepository,
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager
): BaseViewModel(
    networkObserver = networkObserver,
    needObserverNetwork = true
) {
    /**
     * Estado interno mutável contendo os dados da equipa.
     * Inicializado com um objeto [TeamDto] vazio.
     */
    private val teamInfo: MutableStateFlow<TeamDto> = MutableStateFlow(TeamDto())

    /**
     * Fluxo público imutável com os dados da equipa (Nome, Logo, etc.).
     * Observado pela UI para renderizar o cabeçalho e informações.
     */
    val team: StateFlow<TeamDto> = teamInfo.asStateFlow()

    /**
     * Estado interno mutável que armazena o estatuto de administrador do utilizador.
     * * É mantido como **privado** (`private`) para garantir que apenas este ViewModel
     * tem permissão para alterar o seu valor (Encapsulamento), prevenindo
     * modificações acidentais vindas da UI.
     */
    private val _isAdmin: MutableStateFlow<Boolean> = MutableStateFlow(false)

    /**
     * Fluxo público imutável (Read-only) que indica se o utilizador atual é administrador da equipa.
     * * A UI observa este fluxo para adaptar o layout dinamicamente:
     * - `true`: Mostra funcionalidades de gestão (Match Center, Agendar jogos).
     * - `false`: Mostra apenas funcionalidades de visualização (Membros, Calendário).
     */
    val isAdmin: StateFlow<Boolean> = _isAdmin.asStateFlow()

    init {
        loadInfoTeam()

        val isAdminValue = sessionManager.getUserProfile()?.isAdmin
        if(isAdminValue != null) {
            _isAdmin.value = isAdminValue
        }
    }

    /**
     * Obtém o ID da equipa da sessão local e solicita os dados atualizados à API.
     *
     * Utiliza [launchDataLoad] para gerir automaticamente o estado de `isLoading` na UI
     * e capturar possíveis exceções de rede.
     */
    private fun loadInfoTeam() {
        val teamId = sessionManager.getUserProfile()?.idTeam

        if (teamId == null) {
            return
        }

        launchDataLoad {
            val teamData = teamRepository.getNameTeam(teamId = teamId)

            teamInfo.value = teamData
        }
    }

    //TODO: Falta validar autorização
    /**
     * Tenta navegar para o ecrã de agendamento de partida Casual.
     *
     * Verifica a conexão à internet antes de permitir a navegação.
     *
     * @param onSucess Callback executada se as condições (internet) forem cumpridas.
     */
    fun onNavigateCasualMatch(onSucess: () -> Unit) {

        onlineFunctionality(
            action = onSucess,
            toastMessage = "Para visualizar as equipas disponiveis para uma partida precisa estar conectado há internet"
        )
    }

    //TODO: Falta validar autorização
    /**
     * Tenta navegar para o ecrã de agendamento de partida Rankeada (Competitiva).
     *
     * Verifica a conexão à internet antes de permitir a navegação.
     *
     * @param onSucess Callback executada se as condições (internet) forem cumpridas.
     */
    fun onNavigateRankedMatch(onSucess: () -> Unit) {

        onlineFunctionality(
            action = onSucess,
            toastMessage = "Para marcar uma partida competitiva, necessita estar online"
        )
    }

    /**
     * Tenta navegar para a lista de membros da equipa.
     *
     * @param onSucess Callback de navegação.
     */
    fun onNavigateMembers(onSucess: () -> Unit) {
        onlineFunctionality(
            action = onSucess,
            toastMessage = "Para visualizar a lista de membros precisa ter internet"
        )
    }

    /**
     * Tenta navegar para o calendário da equipa.
     *
     * @param onSucess Callback de navegação.
     */
    fun onNavigateCalendar(onSucess: () -> Unit) {
        onlineFunctionality(
            action = onSucess,
            toastMessage = "Para visualizar o calendário de jogo precisa de ter internet"
        )
    }
}
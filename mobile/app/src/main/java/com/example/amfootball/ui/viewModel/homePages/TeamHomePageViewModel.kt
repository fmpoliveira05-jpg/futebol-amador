package com.example.amfootball.ui.viewModel.homePages

import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.enums.UserRole
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.TeamService
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
    private val teamRepository: TeamService,
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager
) : BaseViewModel(
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
     * Estado interno mutável do Role do utilizador.
     */
    private val roleState: MutableStateFlow<UserRole> = MutableStateFlow(UserRole.MEMBER_TEAM)

    /**
     * Fluxo público imutável que indica o nível de permissão do utilizador na equipa.
     *
     * A UI deve observar este estado para decidir quais cartões mostrar:
     * - [UserRole.ADMIN_TEAM]: Mostra tudo (Agendar, Gerir).
     * - [UserRole.MEMBER_TEAM]: Mostra apenas visualização (Calendário, Lista).
     *
     * Valor por defeito seguro: [UserRole.MEMBER_TEAM].
     */

    val role: StateFlow<UserRole> = roleState.asStateFlow()

    init {
        loadInfoTeam()
        loadUserRole()
    }

    /**
     * Obtém o ID da equipa da sessão local e solicita os dados atualizados à API.
     *
     * Utiliza [launchDataLoad] para gerir automaticamente o estado de `isLoading` na UI
     * e capturar possíveis exceções de rede.
     */
    private fun loadInfoTeam() {
        val teamId = sessionManager.getUserProfile()?.effectiveTeamId

        if (teamId.isNullOrEmpty()) {
            return
        }

        launchDataLoad {
            val teamData = teamRepository.getNameTeam(teamId = teamId)

            teamInfo.value = teamData
        }
    }

    /**
     * Tenta navegar para o ecrã de agendamento de partida Casual.
     *
     * Verifica a conexão à internet antes de permitir a navegação.
     *
     * @param onSucess Callback executada se as condições (internet) forem cumpridas.
     */
    fun onNavigateCasualMatch(onSucess: () -> Unit) {
        if (roleState.value != UserRole.ADMIN_TEAM) {
            updateToast("Apenas adminsitradores de equipa podem agendar partidas casuais")
            return
        }

        onlineFunctionality(
            action = onSucess,
            toastMessage = "Para visualizar as equipas disponiveis para uma partida precisa estar conectado há internet"
        )
    }

    /**
     * Tenta navegar para o ecrã de agendamento de partida Rankeada (Competitiva).
     *
     * Verifica a conexão à internet antes de permitir a navegação.
     *
     * @param onSucess Callback executada se as condições (internet) forem cumpridas.
     */
    fun onNavigateRankedMatch(onSucess: () -> Unit) {
        if (roleState.value != UserRole.ADMIN_TEAM) {
            updateToast("Apenas adminsitradores de equipa podem agendar partidas casuais")
            return
        }

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

    /**
     * Carrega o Role do utilizador a partir da sessão local.
     * Deve ser chamado na inicialização para configurar a UI imediatamente.
     */
    private fun loadUserRole() {
        val profile = sessionManager.getUserProfile()
        roleState.value = profile?.role ?: UserRole.MEMBER_TEAM
    }
}
package com.example.amfootball.ui.viewModel.homePages

import androidx.lifecycle.viewModelScope
import com.example.amfootball.R
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.enums.UserRole
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.PlayerService
import com.example.amfootball.data.services.TeamService
import com.example.amfootball.ui.viewModel.abstracts.BaseViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
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
    private val playerService: PlayerService,
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
        //refreshUserProfile()
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
            updateToast(message = R.string.toast_admin_only_casual)
            return
        }

        onlineFunctionality(
            action = onSucess,
            toastMessage = R.string.toast_offline_casual_teams
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
            updateToast(message = R.string.toast_admin_only_ranked)
            return
        }

        onlineFunctionality(
            action = onSucess,
            toastMessage = R.string.toast_offline_ranked_match
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
            toastMessage = R.string.toast_offline_members_list
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
            toastMessage = R.string.toast_offline_calendar
        )
    }

    //TODO: Testar
    fun onLeaveTeam(onSucess: () -> Unit) {
        launchDataLoad {
            val userId = sessionManager.getUserProfile()?.loginResponseDto?.localId

            if(userId == null) {
                return@launchDataLoad
            }

            val updatedUser = playerService.leaveTeam(playerId = userId)

            if (updatedUser != null) {
                sessionManager.updateTeamIdUser(null)
            }

            onSucess()
        }
    }

    /*
    Metodo para apos o user ser promovido ou despromovido ao entrar na HomePage, o mesmo ter a role atualizada
    fun refreshUserProfile() {
        viewModelScope.launch {
            try {
                // Chama a API para obter o perfil atualizado
                // Nota: Não precisas de ativar 'isLoading' visual que bloqueie o ecrã
                val response = userRepository.getUserProfile()

                if (response.isSuccessful && response.body() != null) {
                    val freshProfile = response.body()!!

                    // 1. Verificar se houve mudanças críticas
                    val oldProfile = sessionManager.getUserProfile()

                    // Se o user foi expulso da equipa (ex: teamId veio null ou diferente)
                    if (oldProfile?.idTeam != null && freshProfile.idTeam == null) {
                        // Lógica para redirecionar para ecrã de "Sem Equipa"
                        // _navigationEvent.emit(NavigateToNoTeam)
                    }

                    sessionManager.saveUserProfile(freshProfile)
                    loadUserRole()
                }
            } catch (e: Exception) {
                // Se falhar (sem net), não faz mal.
                // O utilizador continua a ver os dados em cache.
                // Podes mostrar um pequeno Toast ou ignorar.
            }
        }
    }

     */


    /**
     * Carrega o Role do utilizador a partir da sessão local.
     * Deve ser chamado na inicialização para configurar a UI imediatamente.
     */
    private fun loadUserRole() {
        val profile = sessionManager.getUserProfile()
        roleState.value = profile?.role ?: UserRole.MEMBER_TEAM
    }
}
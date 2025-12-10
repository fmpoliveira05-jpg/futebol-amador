package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.SavedStateHandle
import com.example.amfootball.R
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.enums.UserRole
import com.example.amfootball.data.events.AppEvent
import com.example.amfootball.data.events.GlobalEventBus
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.TeamService
import com.example.amfootball.ui.viewModel.abstracts.BaseViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
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
    private val sessionManager: SessionManager,
    private val networkObserver: NetworkConnectivityObserver,
    private val globalEventBus: GlobalEventBus
) : BaseViewModel(networkObserver = networkObserver) {
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
        val profile = sessionManager.getUserProfile()
        val teamIdSessionManager = profile?.effectiveTeamId

        if (teamId.isNullOrBlank() && !teamIdSessionManager.isNullOrBlank()) {
            teamId = teamIdSessionManager
        }

        roleState.value = profile?.role ?: UserRole.MEMBER_TEAM

        loadTeamProfile()
    }

    fun updateTeam(onSucess: () -> Unit) {
        if(sessionManager.getUserProfile()?.role != UserRole.ADMIN_TEAM) {
            updateToast(R.string.toast_admin_only_edit)
            return
        }

        onlineFunctionality(
            action = onSucess,
            toastMessage = R.string.toast_offline_edit_team,
        )
    }

    fun deleteTeam(onSucess: () -> Unit) {
        val profile = sessionManager.getUserProfile()
        if (profile?.role != UserRole.ADMIN_TEAM) {
            updateToast(R.string.toast_admin_only_delete)
            return
        }

        val teamIdFinal: String?
        if (teamId != null) {
            teamIdFinal = teamId
        }
        else if (profile.effectiveTeamId.isNotEmpty()) {
            teamIdFinal = profile.effectiveTeamId
        } else {
            updateToast(R.string.toast_admin_only_delete)
            return
        }

        launchDataLoad {
            teamRepository.deleteTeam(teamId = teamIdFinal!!)

            //Atualiza a role do admin mas preciso de atualizar de todos os membros.
            sessionManager.updateTeamIdUser(teamId = null)

            //Emite o evento para remover os players todos da equipa
            globalEventBus.emitEvent(AppEvent.TeamDeleted(message = "Equipa eliminada com sucesso!"))

            //onSucess()
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

        launchDataLoad {
            if (idToLoad.isNullOrBlank()) {
                return@launchDataLoad
            }

            val profile = teamRepository.getTeamProfile(teamId = idToLoad)

            infoTeam.value = profile
        }
    }
}
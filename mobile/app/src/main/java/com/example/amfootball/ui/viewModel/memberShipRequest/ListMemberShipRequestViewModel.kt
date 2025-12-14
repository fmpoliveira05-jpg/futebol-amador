package com.example.amfootball.ui.viewModel.memberShipRequest

import androidx.lifecycle.SavedStateHandle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.core.extensions.toLocalDateTime
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.ListsSizesConst
import com.example.amfootball.core.utils.UserConst
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.filters.FilterMemberShipRequest
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.remote.services.PlayerService
import com.example.amfootball.data.remote.services.TeamService
import com.example.amfootball.domains.enums.pages.ListMembershipRequestMode
import com.example.amfootball.domains.enums.pages.ListPlayerMode
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.filtersError.FilterMemberShipRequestError
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Listagem de Pedidos de Adesão.
 *
 * Este componente gere o estado dos filtros (pesquisa por remetente, datas) e coordena as ações
 * de decisão (Aceitar/Rejeitar) sobre os pedidos de adesão recebidos/enviados.
 *
 * Herda de [ListsViewModels] para obter funcionalidades de gestão de UI State, Loading, e da lista principal.
 *
 * @property networkObserver Observador de conectividade injetado no [ListsViewModels].
 */
@HiltViewModel
class ListMemberShipRequestViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val teamService: TeamService,
    private val playerService: PlayerService,
    private val savedStateHandle: SavedStateHandle,
    private val sessionManager: SessionManager
) : ListsViewModels<MembershipRequestInfoDto>(networkObserver = networkObserver) {

    private val teamId: MutableStateFlow<String> = MutableStateFlow("")

    private val playerId: MutableStateFlow<String> = MutableStateFlow("")

    /**
     * Estado interno mutável contendo os critérios de filtro atuais.
     */
    private val filterState: MutableStateFlow<FilterMemberShipRequest> = MutableStateFlow(FilterMemberShipRequest())

    /**
     * Fluxo público de leitura dos filtros observados pela UI.
     */
    val uiFilterState: StateFlow<FilterMemberShipRequest> = filterState.asStateFlow()

    /**
     * Estado interno mutável contendo os erros de validação de filtros.
     */
    private val filterErrorState: MutableStateFlow<FilterMemberShipRequestError> =
        MutableStateFlow(FilterMemberShipRequestError())

    /**
     * Fluxo público de leitura dos erros de filtro.
     */
    val uiFilterErrorState: StateFlow<FilterMemberShipRequestError> = filterErrorState.asStateFlow()

    private val modeStr = savedStateHandle.get<String>(Arguments.LIST_MEMBERSHIP_REQUEST_MODE)

    /**
     * O modo de operação da página. Define o tipo de lista a ser carregada (Geral vs. Recrutamento).
     * Valor por defeito: [ListPlayerMode.PLAYER_LIST].
     */
    val mode: ListMembershipRequestMode = try {
        if (modeStr != null) {
            ListMembershipRequestMode.valueOf(modeStr)
        } else {
            ListMembershipRequestMode.MEMBERSHIP_PLAYER
        }
    } catch (e: Exception) {
        ListMembershipRequestMode.MEMBERSHIP_PLAYER
    }

    //Inicializador
    init {
        teamId.value = sessionManager.fetchTeamId() ?: ""
        playerId.value = sessionManager.fetchUserId() ?: ""

        loadListMemberShipRequest()
    }

    //Metodos
    /**
     * Atualiza o campo de filtro com o nome do remetente (equipa ou jogador).
     *
     * @param newName O novo texto para o filtro de nome. Se vazio, o valor é armazenado como `null`.
     */
    fun onSenderNameChanged(newName: String) {
        filterState.value = filterState.value.copy(senderName = newName.ifEmpty { null })
    }

    /**
     * Atualiza o campo de filtro para a data mínima do pedido de adesão.
     *
     * @param newMinDate Timestamp (Long) da nova data mínima.
     */
    fun onMinDateSelected(newMinDate: Long) {
        filterState.value = filterState.value.copy(minDate = newMinDate.toLocalDateTime())
    }

    /**
     * Atualiza o campo de filtro para a data máxima do pedido de adesão.
     *
     * @param newMaxDate Timestamp (Long) da nova data máxima.
     */
    fun onMaxDateSelected(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxDate = newMaxDate.toLocalDateTime())
    }

    /**
     * Aumenta o número de itens exibidos na lista (usado para paginação local/Load More).
     */
    fun loadMoreTeams() {
        inicialSizeList.value = inicialSizeList.value.plus(10)
    }

    /**
     * Executa a lógica de aplicação de filtros.
     *
     * 1. Executa a validação síncrona dos filtros ([validateFilter]).
     * 2. Se for válido: Faz uma chamada assíncrona para o backend para obter a lista filtrada.
     */
    fun applyFilters() {
        if (!validateFilter()) {
            return
        }

        if(isNetworkAvailable()) {
            loadListMemberShipRequest()
        } else {
            listState.value = offlineFilter(originalList = originalList, filter = filterState.value)
        }
    }

    fun clearFilters() {
        filterState.value = FilterMemberShipRequest()
        filterErrorState.value = FilterMemberShipRequestError()
        inicialSizeList.value = ListsSizesConst.INICIAL_SIZE

        if (networkObserver.isOnlineOneShot()) {
            loadListMemberShipRequest()
        } else {
            listState.value = originalList
        }
    }

    /**
     * Aceita um pedido de adesão (Join Request ou Recrutamento).
     *
     * A lógica decide qual endpoint chamar (Player ou Team) com base em [isPlayerSender].
     *
     * @param idRequest O ID único do pedido de adesão.
     * @param onSucess Callback a ser executado após a operação bem-sucedida.
     */
    fun acceptMemberShipRequest(idRequest: String, idSender: String, onSucess: () -> Unit) {
        launchDataLoad {
            when(mode) {
                ListMembershipRequestMode.MEMBERSHIP_PLAYER -> {
                    playerService.acceptMemberShipRequest(playerId = playerId.value, requestId = idRequest)
                    sessionManager.updateTeamIdUser(idSender)

                    onSucess()
                }
                ListMembershipRequestMode.MEMBERSHIP_TEAM -> {
                    teamService.acceptMemberShipRequest(teamId = teamId.value, requestId = idRequest)
                }
            }
        }
    }

    /**
     * Rejeita um pedido de adesão (Join Request ou Recrutamento).
     *
     * Remove o pedido da lista local e envia o pedido de rejeição para a API.
     *
     * @param idRequest O ID único do pedido de adesão.
     */
    fun rejectMemberShipRequest(idRequest: String) {
        launchDataLoad {
            when(mode) {
                ListMembershipRequestMode.MEMBERSHIP_PLAYER -> {
                    playerService.rejectMemberShipRequest(playerId = playerId.value, requestId = idRequest)
                }
                ListMembershipRequestMode.MEMBERSHIP_TEAM -> {
                    teamService.rejectMemberShipRequest(teamId = teamId.value, requestId = idRequest)
                }
            }
        }

        listState.value = listState.value.filterNot { it.id == idRequest }
        originalList = originalList.filterNot { it.id == idRequest }
    }

    /**
     * Navega para o ecrã de perfil do remetente (Equipa ou Jogador).
     *
     * O método decide a rota ([Routes.UserRoutes.PROFILE] ou [Routes.TeamRoutes.TEAM_PROFILE])
     * com base em [isPlayerSender].
     *
     * @param idSender O ID da entidade que enviou o pedido.
     * @param isPlayerSender Indica se o remetente é um jogador (para decidir a rota de destino).
     * @param navHostController Controlador de navegação.
     */
    fun showMore(idSender: String, navHostController: NavHostController) {
        var route = Routes.UserRoutes.PROFILE.route

        if (modeStr == ListMembershipRequestMode.MEMBERSHIP_PLAYER.name) {
            route = Routes.TeamRoutes.TEAM_PROFILE.route
        }

        navHostController.navigate("$route/$idSender") {
            launchSingleTop = true
        }
    }

    //Private Methods
    private fun loadListMemberShipRequest() {
        launchDataLoad {
            when(mode) {
                ListMembershipRequestMode.MEMBERSHIP_PLAYER -> {
                    if(playerId.value.isEmpty()) {
                        updateToast(R.string.toast_autenticate_people)
                        return@launchDataLoad
                    }

                    listState.value = playerService.listMemberShipRequest(playerId = playerId.value, filter = filterState.value)
                }
                ListMembershipRequestMode.MEMBERSHIP_TEAM -> {
                    if(teamId.value.isEmpty()) {
                        updateToast(R.string.toast_autenticate_people)
                        return@launchDataLoad
                    }

                    listState.value = teamService.getListMemberShipRequest(teamId = teamId.value, filter = filterState.value)
                }
            }

            if(filterState.value == FilterMemberShipRequest()) {
                originalList = listState.value
            }
        }
    }

    private fun offlineFilter(
        originalList: List<MembershipRequestInfoDto>,
        filter: FilterMemberShipRequest
    ): List<MembershipRequestInfoDto> {
        return originalList.filter { item ->
            val nameSender = if (mode == ListMembershipRequestMode.MEMBERSHIP_PLAYER) {
                item.team.name
            } else {
                item.player.name
            }

            val senderName = filter.senderName.isNullOrBlank()
                    || nameSender.contains(filter.senderName, ignoreCase = true)


            val minDate = filter.minDate == null
                    || item.requestDate.toLocalDate() >= filter.minDate.toLocalDate()

            val maxDate = filter.maxDate == null
                    || item.requestDate.toLocalDate() <= filter.maxDate.toLocalDate()

            senderName && minDate && maxDate
        }
    }
    /**
     * Validação síncrona dos critérios de filtro.
     *
     * Verifica o comprimento do nome do remetente e a ordem das datas (Min Date vs Max Date).
     *
     * @return `true` se todos os filtros forem válidos, `false` caso contrário.
     */
    private fun validateFilter(): Boolean {
        val name = filterState.value.senderName
        val minDate = filterState.value.minDate
        val maxDate = filterState.value.maxDate

        var nameError: ErrorMessage? = null
        var minDateError: ErrorMessage? = null
        var maxDateError: ErrorMessage? = null

        if (name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            nameError = ErrorMessage(
                messageId = R.string.error_max_name_sender,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        if (minDate != null && maxDate != null && minDate > maxDate) {
            minDateError = ErrorMessage(
                messageId = R.string.error_min_date_after,
            )

            maxDateError = ErrorMessage(
                messageId = R.string.error_max_date_before
            )
        }

        filterErrorState.value = FilterMemberShipRequestError(
            senderNameError = nameError,
            minDateError = minDateError,
            maxDateError = maxDateError
        )

        val isValid = listOf(nameError, minDateError, maxDateError).all {
            it == null
        }

        return isValid
    }
}
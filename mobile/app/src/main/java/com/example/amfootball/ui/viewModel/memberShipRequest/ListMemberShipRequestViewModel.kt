package com.example.amfootball.ui.viewModel.memberShipRequest

import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMemberShipRequestError
import com.example.amfootball.data.filters.FilterMemberShipRequest
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import com.example.amfootball.utils.UserConst
import com.example.amfootball.utils.extensions.toLocalDateTime
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
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
    private val networkObserver: NetworkConnectivityObserver
) : ListsViewModels<MembershipRequestInfoDto>(networkObserver = networkObserver) {

    /**
     * Estado interno mutável contendo os critérios de filtro atuais.
     */
    private val filterState: MutableStateFlow<FilterMemberShipRequest> = MutableStateFlow(FilterMemberShipRequest())

    /**
     * Fluxo público de leitura dos filtros observados pela UI.
     */
    val uiFilterState: StateFlow<FilterMemberShipRequest> = filterState

    /**
     * Estado interno mutável contendo os erros de validação de filtros.
     */
    private val filterErrorState: MutableStateFlow<FilterMemberShipRequestError> = MutableStateFlow(FilterMemberShipRequestError())

    /**
     * Fluxo público de leitura dos erros de filtro.
     */
    val uiFilterErrorState: StateFlow<FilterMemberShipRequestError> = filterErrorState

    //Inicializador
    init {
        // TODO: Meter aqui para logo que ele for chamado/criado o viewModel carregar os dados da lista
        val initialValue = MembershipRequestInfoDto.generateMemberShipRequestTeam()

        listState.value = initialValue
        stopLoading()
    }

    //Metodos
    /**
     * Atualiza o campo de filtro com o nome do remetente (equipa ou jogador).
     *
     * @param newName O novo texto para o filtro de nome. Se vazio, o valor é armazenado como `null`.
     */
    fun onSenderNameChanged(newName: String) {
        filterState.value = filterState.value.copy(
            senderName = newName.ifEmpty { null } //Caso esteja vazio guarda null
        )
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
        // TODO: Quando tiver a API, é aqui que a vai chamar:
    }


    private fun filter() {
        /*
               val currentFilters = filterState.value!!

        println("A aplicar filtros (sem API): $currentFilters")

       val masterList = listState.value

       val filteredList = masterList.filter { item ->
           val matchesName = if (currentFilters.senderName.isNullOrBlank()) {
               true
           } else {
               item.sender.name.contains(currentFilters.senderName, ignoreCase = true)
           }

           val matchesMinDate =
               if (currentFilters.minDate == null) {
               true
           } else {
               !item.dateSend.isBefore(currentFilters.minDate)
           }

           val matchesMaxDate = if (currentFilters.maxDate == null) {
               true
           } else {
               !item.dateSend.isAfter(currentFilters.maxDate)
           }

           matchesName && matchesMinDate && matchesMaxDate
       }
       listState.value = filteredList
       * */
    }

    //TODO: Aqui seria feito um novo pedido há API com os dados atualizados
    fun clearFilters() {
        filterState.value = FilterMemberShipRequest()
        //listState.value = originalList
    }

    /**
     * TODO: O metodo recebe por parametro o recetor, emissor e o boolean que indica
     * TODO: Depois dependendo do valor do if ou manda o pedido à API como se fosse a team ou o pedido como se fosse o player
     * */
    /**
     * Aceita um pedido de adesão (Join Request ou Recrutamento).
     *
     * A lógica decide qual endpoint chamar (Player ou Team) com base em [isPlayerSender].
     *
     * @param idReceiver O ID do recetor (Equipa se Jogador for o remetente, ou vice-versa).
     * @param idRequest O ID único do pedido de adesão.
     * @param isPlayerSender `true` se o pedido partiu de um jogador (Join Request).
     * @param navHostController Controlador para navegação pós-ação (ex: Home).
     */
    fun acceptMemberShipRequest(
        idReceiver: String,
        idRequest: String,
        isPlayerSender: Boolean,
        navHostController: NavHostController,
    ) {
        if (isPlayerSender) {
            //TODO: Fazer pedido ao endpoint do Player accept da API
        } else {
            //TODO: Fazer pedido ao endpoint da Team accept da API
        }

        navHostController.navigate(Routes.TeamRoutes.HOMEPAGE.route)
    }

    /**
     * TODO: Igual ao Accept mas para reject memberShipRequest
     * */
    /**
     * Rejeita um pedido de adesão (Join Request ou Recrutamento).
     *
     * Remove o pedido da lista local e envia o pedido de rejeição para a API.
     *
     * @param idReceiver O ID do recetor.
     * @param idRequest O ID único do pedido de adesão.
     * @param isPlayerSender `true` se o pedido partiu de um jogador.
     */
    fun rejectMemberShipRequest(idReceiver: String, idRequest: String, isPlayerSender: Boolean) {
        if (isPlayerSender) {
            //TODO: Fazer pedido há API, ao endpoint do player
        } else {
            //TODO:Fazer pedido há API, ao endpoint da team
        }

        val updatedList = listState.value.filterNot { it.id == idRequest }

        listState.value = updatedList
    }

    /**
     * TODO: Falta apenas nas paginas de perfil carregar os dados e de seguida enviar esse dados aqui
     * */
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
    fun showMore(idSender: String, isPlayerSender: Boolean, navHostController: NavHostController) {
        var route = Routes.UserRoutes.PROFILE.route

        if (isPlayerSender) {
            route = Routes.TeamRoutes.TEAM_PROFILE.route
        }

        navHostController.navigate(route) {
            launchSingleTop = true
        }
    }

    //Private Methods
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
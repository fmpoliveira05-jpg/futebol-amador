package com.example.amfootball.ui.viewModel.memberShipRequest

import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.filters.FilterMemberShipRequest
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.utils.extensions.toLocalDateTime
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.navigation.objects.pages.CrudTeamRoutes
import java.time.Instant
import java.time.LocalDateTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter

class ListMemberShipRequestViewModel: ViewModel() {

    private val filterState = MutableStateFlow(value = FilterMemberShipRequest())
    val uiFilterState = filterState.asStateFlow()

    private val listState = MutableStateFlow(value= emptyList<MembershipRequestInfoDto>())
    private var originalList: List<MembershipRequestInfoDto> = emptyList()
    val uiListState = listState.asStateFlow()

    //Inicializador
    init {
        // TODO: Meter aqui para logo que ele for chamado/criado o viewModel carregar os dados da lista
        val initialValue = MembershipRequestInfoDto.generateMemberShipRequestTeam()

        listState.value = initialValue
        originalList = initialValue
    }

    //Metodos
    fun onSenderNameChanged(newName: String) {
        filterState.value = filterState.value.copy(
            senderName = newName.ifEmpty { null } //Caso esteja vazio guarda null
        )
    }

    fun onMinDateSelected(newMinDate: Long) {
        filterState.value = filterState.value.copy(minDate = newMinDate.toLocalDateTime())
    }

    fun onMaxDateSelected(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxDate = newMaxDate.toLocalDateTime())
    }

    /**
     * Função que permite chamar o endPoint da BD para consutlar a lista com os filtros aplicados
     * */
    fun applyFilters() {
        val currentFilters = filterState.value
        // TODO: Quando tiver a API, é aqui que a vai chamar:

        println("A aplicar filtros (sem API): $currentFilters")

        val masterList = listState.value

        val filteredList = masterList.filter { item ->
            val matchesName = if (currentFilters.senderName.isNullOrBlank()) {
                true
            } else {
                item.Sender.contains(currentFilters.senderName, ignoreCase = true)
            }

            val matchesMinDate =
                if (currentFilters.minDate == null) {
                true
            } else {
                !item.DateSend.isBefore(currentFilters.minDate)
            }

            val matchesMaxDate = if (currentFilters.maxDate == null) {
                true
            } else {
                !item.DateSend.isAfter(currentFilters.maxDate)
            }

            matchesName && matchesMinDate && matchesMaxDate
        }

        listState.value = filteredList
    }


    //TODO: Aqui seria feito um novo pedido há API com os dados atualizados
    fun clearFilters() {
        filterState.value = FilterMemberShipRequest()
        listState.value = originalList
    }

    /**
     * TODO: O metodo recebe por parametro o recetor, emissor e o boolean que indica
     * se foi o player a aceitar ou a Team
     * TODO: Depois dependendo do valor do if ou manda o pedido à API como se fosse a team ou o pedido como se fosse o player
     * */
    fun AcceptMemberShipRequest(
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
    fun RejectMemberShipRequest(
        idReceiver: String,
        idRequest: String,
        isPlayerSender: Boolean,
    ) {
        if (isPlayerSender) {
            //TODO: Fazer pedido há API, ao endpoint do player
        } else {
            //TODO:Fazer pedido há API, ao endpoint da team
        }

        val updatedList = listState.value.filterNot { it.Id == idRequest }

        listState.value = updatedList
    }


    /**
     * TODO: Falta apenas nas paginas de perfil carregar os dados e de seguida enviar esse dados aqui
     * */
    fun ShowMore(
        isPlayerSender: Boolean,
        IdSender: String,
        navHostController: NavHostController,
    ) {
        var route = Routes.UserRoutes.PROFILE.route

        if (isPlayerSender) {
            route = CrudTeamRoutes.PROFILE_TEAM
        }

        navHostController.navigate(route) {
            launchSingleTop = true
        }
    }
}
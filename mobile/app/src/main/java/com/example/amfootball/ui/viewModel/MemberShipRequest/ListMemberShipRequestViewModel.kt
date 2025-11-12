package com.example.amfootball.ui.viewModel.MemberShipRequest

import androidx.compose.runtime.State
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.Filters.FilterMemberShipRequest
import com.example.amfootball.data.dtos.MembershipRequest.MembershipRequestInfoDto
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.navigation.Objects.Pages.CrudTeamRoutes
import com.example.amfootball.navigation.Objects.RotasUser
import com.example.amfootball.utils.Patterns
import java.time.Instant
import java.time.LocalDateTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter

class ListMemberShipRequestViewModel: ViewModel() {
    private val dateFormatter = DateTimeFormatter.ofPattern(Patterns.Date)

    //Estados
    //Filtros aplicados
    private val filterState = mutableStateOf(value = FilterMemberShipRequest())

    private val listState = mutableStateOf(value= emptyList<MembershipRequestInfoDto>())

    //Getters (que a UI vai utilizar para mostrar os dados)
    val senderName: State<String> = derivedStateOf {
        filterState.value.senderName ?: ""
    }

    val minDateDisplayString: State<String> = derivedStateOf {
        filterState.value.minDate?.format(dateFormatter) ?: ""
    }

    val maxDateDisplayString: State<String> = derivedStateOf {
        filterState.value.maxDate?.format(dateFormatter) ?: ""
    }

    val listMemberShipRequest: State<List<MembershipRequestInfoDto>> = listState

    //Setters (Vão pegar nos dados introduzidos pelo utilizador e guardar no filtro)
    fun onSenderNameChanged(newName: String) {
        filterState.value = filterState.value.copy(
            senderName = newName.ifEmpty { null } //Caso esteja vazio guarda null
        )
    }

    fun onMinDateSelected(millis: Long) {
        filterState.value = filterState.value.copy(minDate = newDateTime(millis = millis))
    }

    fun onMaxDateSelected(millis: Long) {
        filterState.value = filterState.value.copy(maxDate = newDateTime(millis = millis))
    }

    //Inicializador
    init {
        // TODO: Meter aqui para logo que ele for chamado/criado o viewModel carregar os dados da lista
        listState.value = MembershipRequestInfoDto.generateMemberShipRequestTeam()
    }

    //Metodos
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

    fun clearFilters() {
        filterState.value = FilterMemberShipRequest()
        listState.value = listState.value
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

        navHostController.navigate(RoutesNavBarTeam.HOME_PAGE_TEAM)
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
        var route = RotasUser.USER_PROFILE

        if (isPlayerSender) {
            route = CrudTeamRoutes.PROFILE_TEAM
        }

        navHostController.navigate(route) {
            launchSingleTop = true
        }
    }

    /**
     * Metodos privados
     * */
    private fun newDateTime(millis: Long): LocalDateTime {
        return Instant.ofEpochMilli(millis)
            .atZone(ZoneId.systemDefault())
            .toLocalDateTime()
    }
}
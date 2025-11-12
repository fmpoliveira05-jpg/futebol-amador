package com.example.amfootball.ui.viewModel.MemberShipRequest

import androidx.compose.runtime.State
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.Filters.FilterMemberShipRequest
import java.time.Instant
import java.time.LocalDateTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter

class ListMemberShipRequestViewModel: ViewModel() {
    private val dateFormatter = DateTimeFormatter.ofPattern("dd/MM/yyyy")

    //Estados
    //Filtros aplicados
    private val filterState = mutableStateOf(FilterMemberShipRequest())

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

    //Metodos
    init {
        // TODO: Meter aqui para logo que ele for chamado/criado o viewModel carregar os dados da lista
    }

    /**
     * Função que permite chamar o endPoint da BD para consutlar a lista com os filtros aplicados
     * */
    fun applyFilters() {
        val currentFilters = filterState.value
        // TODO: Quando tiver a API, é aqui que a vai chamar:

        println("A aplicar filtros (sem API): $currentFilters")
    }

    /**
     * TODO: O metodo recebe por parametro o recetor, emissor e o boolean que indica
     * se foi o player a aceitar ou a Team
     * TODO: Depois dependendo do valor do if ou manda o pedido à API como se fosse a team ou o pedido como se fosse o player
     * */
    fun AcceptMemberShipRequest() {
    }

    /**
     * TODO: Igual ao Accept mas para reject memberShipRequest
     * */
    fun RejectMemberShipRequest() {
    }


    /**
     * Todo: Recebe o id do sender e também o boolean e se for uma team leva para a pagina da team se for player para o profile do player
     * */
    fun NavigateShowMore() {
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
package com.example.amfootball.ui.viewModel.memberShipRequest

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterMemberShipRequest
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMemberShipRequestError
import com.example.amfootball.utils.extensions.toLocalDateTime
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.navigation.objects.page.CrudTeamRoutes
import com.example.amfootball.utils.UserConst
import com.example.amfootball.R

class ListMemberShipRequestViewModel: ViewModel() {

    private val filterState: MutableLiveData<FilterMemberShipRequest> = MutableLiveData(FilterMemberShipRequest())
    val uiFilterState: LiveData<FilterMemberShipRequest> = filterState
    private val filterErrorState: MutableLiveData<FilterMemberShipRequestError> = MutableLiveData(FilterMemberShipRequestError())
    val uiFilterErrorState: LiveData<FilterMemberShipRequestError> = filterErrorState
    private val listState: MutableLiveData<List<MembershipRequestInfoDto>> = MutableLiveData(emptyList<MembershipRequestInfoDto>())
    private var originalList: List<MembershipRequestInfoDto> = emptyList()
    val uiListState: LiveData<List<MembershipRequestInfoDto>> = listState

    //Inicializador
    init {
        // TODO: Meter aqui para logo que ele for chamado/criado o viewModel carregar os dados da lista
        val initialValue = MembershipRequestInfoDto.generateMemberShipRequestTeam()

        listState.value = initialValue
        originalList = initialValue
    }

    //Metodos
    fun onSenderNameChanged(newName: String) {
        filterState.value = filterState.value!!.copy(
            senderName = newName.ifEmpty { null } //Caso esteja vazio guarda null
        )
    }

    fun onMinDateSelected(newMinDate: Long) {
        filterState.value = filterState.value!!.copy(minDate = newMinDate.toLocalDateTime())
    }

    fun onMaxDateSelected(newMaxDate: Long) {
        filterState.value = filterState.value!!.copy(maxDate = newMaxDate.toLocalDateTime())
    }

    /**
     * Função que permite chamar o endPoint da BD para consutlar a lista com os filtros aplicados
     * */
    fun applyFilters() {
        if(!validateFilter()) {
            return
        }
        // TODO: Quando tiver a API, é aqui que a vai chamar:

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
        listState.value = originalList
    }

    /**
     * TODO: O metodo recebe por parametro o recetor, emissor e o boolean que indica
     * se foi o player a aceitar ou a Team
     * TODO: Depois dependendo do valor do if ou manda o pedido à API como se fosse a team ou o pedido como se fosse o player
     * */
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
    fun rejectMemberShipRequest(
        idReceiver: String,
        idRequest: String,
        isPlayerSender: Boolean,
    ) {
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
    fun showMore(
        isPlayerSender: Boolean,
        idSender: String,
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

    //Private Methods
    private fun validateFilter(): Boolean{
        val name = filterState.value?.senderName
        val minDate = filterState.value?.minDate
        val maxDate = filterState.value?.maxDate

        var nameError: ErrorMessage? = null
        var minDateError: ErrorMessage? = null
        var maxDateError: ErrorMessage? = null

        if(name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            nameError = ErrorMessage(
                messageId = R.string.error_max_name_sender,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        if(minDate != null && maxDate != null && minDate > maxDate) {
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
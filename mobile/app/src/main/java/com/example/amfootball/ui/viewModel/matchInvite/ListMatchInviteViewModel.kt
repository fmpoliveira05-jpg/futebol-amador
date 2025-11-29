package com.example.amfootball.ui.viewModel.matchInvite

import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMatchInviteError
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.UserConst
import com.example.amfootball.utils.extensions.toLocalDateTime
import com.example.amfootball.R
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import javax.inject.Inject

@HiltViewModel
class ListMatchInviteViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver
): ListsViewModels<InfoMatchInviteDto>(networkObserver = networkObserver) {

    private val filtersState: MutableStateFlow<FilterMatchInvite> = MutableStateFlow(FilterMatchInvite())
    val uiFilters: StateFlow<FilterMatchInvite> = filtersState

    private val filtersErrorState: MutableStateFlow<FilterMatchInviteError> = MutableStateFlow(FilterMatchInviteError())
    val filterError: StateFlow<FilterMatchInviteError> = filtersErrorState

    //Initializor
    init {
        val initList = InfoMatchInviteDto.generatePreviewList()
        listState.value = initList
        originalList = initList
    }

    fun onNameSenderChange(newSenderName: String) {
        filtersState.value = filtersState.value.copy(senderName = newSenderName)
    }

    fun onMinDateChange(newMinDate: Long) {
        filtersState.value = filtersState.value.copy(minDate = newMinDate.toLocalDateTime())
    }

    fun onMaxDateChange(newMaxDate: Long) {
        filtersState.value = filtersState.value.copy(maxDate = newMaxDate.toLocalDateTime())
    }

    fun onApplyFilter() {
        if (!validateFitler()) {
            return
        }
        //TODO: Fazer pedido há API para carregar os novos dados

    }

    fun onFilterClear() {
        //TODO: Fazer pedido há API para recarregar a lista (mas depois meter aqui if, para ver se os filtros tinham dados, se não não faz o pedido)
        filtersState.value = FilterMatchInvite()
        listState.value = originalList
    }

    fun acceptMatchInvite(idMatchInvite: String) {
        //TODO: Remover da lista e fazer pedido há API para aceitar
    }

    //TODO: Passar o idMatch por parametro
    fun negociateMatchInvite (idMatchInvite: String, navHostController: NavHostController) {
        navHostController.navigate(route = Routes.TeamRoutes.NEGOCIATE_MATCH_INVITE.route) {
            launchSingleTop = true
        }
    }

    fun rejectMatchInvite (idMatchInvite: String) {
        //TODO: Mandar pedido há API para rejeitar
    }

    //TODO: Passar o id na rota
    fun showMoreDetails(idMatchInvite: String, navHostController: NavHostController) {
        navHostController.navigate(route = Routes.TeamRoutes.TEAM_PROFILE.route) {
            launchSingleTop = true
        }
    }

    private fun validateFitler(): Boolean {
        val nameSender = filtersState.value.senderName
        val minDate = filtersState.value.minDate
        val maxDate = filtersState.value.maxDate

        var nameSenderError: ErrorMessage? = null
        var minDateError: ErrorMessage? = null
        var maxDateError: ErrorMessage? = null

        if(nameSender != null && nameSender.length > UserConst.MAX_NAME_LENGTH) {
            nameSenderError = ErrorMessage(
                messageId = R.string.error_max_name_sender,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        if (minDate != null && maxDate != null && minDate > maxDate) {
            minDateError = ErrorMessage(
                messageId = R.string.error_min_date_after,
                args = listOf(R.string.error_date_sende)
            )

            maxDateError = ErrorMessage(
                messageId = R.string.error_max_date_before,
                args = listOf(R.string.error_date_sende)
            )
        }

        filtersErrorState.value = FilterMatchInviteError(
            senderNameError = nameSenderError,
            minDateError = minDateError,
            maxDateError = maxDateError
        )

        val isValid = listOf(nameSender, minDateError, maxDateError).all { it == null }

        return isValid
    }
}
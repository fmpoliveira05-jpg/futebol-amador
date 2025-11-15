package com.example.amfootball.ui.viewModel.matchInvite

import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.filters.FilterMatchInvite
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.navigation.Objects.page.CrudTeamRoutes
import com.example.amfootball.utils.extensions.toLocalDateTime
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow

class ListMatchInviteViewModel: ViewModel() {

    private val filtersState = MutableStateFlow(FilterMatchInvite())
    val uiFilters = filtersState.asStateFlow()

    private val listState = MutableStateFlow(emptyList<InfoMatchInviteDto>())
    private var originalList: List<InfoMatchInviteDto> = emptyList()
    val uiList = listState.asStateFlow()

    //Initializor
    init {
        var initList = InfoMatchInviteDto.generatePreviewList()
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
        //TODO: Criar validações dos fitlros
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

    fun negociateMatchInvite (
        idMatchInvite: String,
        navHostController: NavHostController
    ) {
        //TODO: Chamar pagina de negociar matchInivte (Form)
    }

    fun rejectMatchInvite (idMatchInvite: String) {
        //TODO: Mandar pedido há API para rejeitar
    }

    fun showMoreDetails(
        idMatchInvite: String,
        navHostController: NavHostController
    ) {
        //TODO: Corrigir deve levar para a pagina da matchInivte
        navHostController.navigate(route = "${CrudTeamRoutes.PROFILE_TEAM}/${idMatchInvite}") {
            launchSingleTop = true
        }
    }
}
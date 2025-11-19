package com.example.amfootball.ui.viewModel.matchInvite

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.utils.extensions.toLocalDateTime

class ListMatchInviteViewModel(): ViewModel() {

    private val filtersState: MutableLiveData<FilterMatchInvite> = MutableLiveData(FilterMatchInvite())
    val uiFilters: LiveData<FilterMatchInvite> = filtersState

    private val listState:MutableLiveData<List<InfoMatchInviteDto>> = MutableLiveData(emptyList<InfoMatchInviteDto>())
    val uiList = listState
    private var originalList: List<InfoMatchInviteDto> = emptyList()


    //Initializor
    init {
        val initList = InfoMatchInviteDto.generatePreviewList()
        listState.value = initList
        originalList = initList
    }

    fun onNameSenderChange(newSenderName: String) {
        filtersState.value = filtersState.value!!.copy(senderName = newSenderName)
    }

    fun onMinDateChange(newMinDate: Long) {
        filtersState.value = filtersState.value!!.copy(minDate = newMinDate.toLocalDateTime())
    }

    fun onMaxDateChange(newMaxDate: Long) {
        filtersState.value = filtersState.value!!.copy(maxDate = newMaxDate.toLocalDateTime())
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

    //TODO: Passar o idMatch por parametro
    fun negociateMatchInvite (
        idMatchInvite: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(route = Routes.TeamRoutes.NEGOCIATE_MATCH_INVITE.route) {
            launchSingleTop = true
        }
    }

    fun rejectMatchInvite (idMatchInvite: String) {
        //TODO: Mandar pedido há API para rejeitar
    }

    //TODO: Passar o id na rota
    fun showMoreDetails(
        idMatchInvite: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(route = Routes.TeamRoutes.TEAM_PROFILE.route) {
            launchSingleTop = true
        }
    }
}
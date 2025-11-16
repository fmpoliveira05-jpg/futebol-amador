package com.example.amfootball.ui.viewModel.lists

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.filters.FilterListPlayerDto
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.navigation.Objects.Routes

class ListPlayerViewModel(): ViewModel() {
    private val filterState: MutableLiveData<FilterListPlayerDto> = MutableLiveData(FilterListPlayerDto())
    val uiFilters: LiveData<FilterListPlayerDto> = filterState

    private val listState: MutableLiveData<List<InfoPlayerDto>> = MutableLiveData(emptyList<InfoPlayerDto>())
    val uiList: LiveData<List<InfoPlayerDto>> = listState

    private val listPositions: MutableLiveData<List<Position?>> = MutableLiveData(emptyList())
    val uiListPositions: LiveData<List<Position?>> = listPositions

    init {
        //TODO: Carregar a lista da API
        listState.value = InfoPlayerDto.createExamplePlayerList()
        listPositions.value = listOf(
            null,
            Position.FORWARD,
            Position.MIDFIELDER,
            Position.DEFENDER,
            Position.GOALKEEPER
        )
    }

    //Metodos
    fun onNameChange(name: String) {
        filterState.value = filterState.value!!.copy(name = name)
    }

    fun onCityChange(city: String) {
        filterState.value = filterState.value!!.copy(city = city)
    }

    fun onMinAgeChange(minAge: Int?) {
        filterState.value = filterState.value!!.copy(minAge = minAge)
    }

    fun onMaxAgeChange(maxAge: Int?) {
        filterState.value = filterState.value!!.copy(maxAge = maxAge)
    }

    fun onPositionChange(position: Position?) {
        filterState.value = filterState.value!!.copy(position = position)
    }

    fun onMinSizeChange(minSize: Int?) {
        filterState.value = filterState.value!!.copy(minSize = minSize)
    }

    fun onMaxSizeChange(maxSize: Int?) {
        filterState.value = filterState.value!!.copy(maxSize = maxSize)
    }

    fun filterApply() {
        //TODO: Carregar a lista filtrada da API
    }

    fun cleanFilters() {
        filterState.value = FilterListPlayerDto()
        //TODO: Carregar a lista da API
    }

    fun sendMembershipRequest(idPlayer: String) {
        //TODO: Fazer pedido há API, para a Team mandar o pedido de adesão
    }

    //TODO: Mandar o id na Rota
    fun showMore(
        idPlayer: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(Routes.UserRoutes.PROFILE.route) {
            launchSingleTop = true
        }
    }
}
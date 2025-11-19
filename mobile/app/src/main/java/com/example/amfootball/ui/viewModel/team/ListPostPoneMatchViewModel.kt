package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterPostPoneMatch
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.utils.extensions.toLocalDateTime

//TODO: Implementar os metodos todos com as chamadas há API (se necessário)
class ListPostPoneMatchViewModel: ViewModel() {
    private val filterState: MutableLiveData<FilterPostPoneMatch> = MutableLiveData<FilterPostPoneMatch>(FilterPostPoneMatch())
    val filter: LiveData<FilterPostPoneMatch> = filterState

    private val listState: MutableLiveData<List<PostPoneMatchDto>> = MutableLiveData<List<PostPoneMatchDto>>(emptyList())
    val list: LiveData<List<PostPoneMatchDto>> = listState

    init {
        //TODO: Carregar a lista da API
        listState.value = PostPoneMatchDto.createExamplePostPoneMatchList()
    }

    fun onOpponentNameChange(newName: String) {
        filterState.value = filterState.value!!.copy(nameOpponent = newName)
    }

    fun onIsHomeChange(isHome: Boolean?) {
        filterState.value = filterState.value!!.copy(isHome = isHome)
    }

    fun onMinDateGameChange(newMinDate: Long) {
        filterState.value = filterState.value!!.copy(minDataGame = newMinDate.toLocalDateTime())
    }

    fun onMaxDateChange(newMaxDate: Long) {
        filterState.value = filterState.value!!.copy(maxDateGame = newMaxDate.toLocalDateTime())
    }

    fun onMinDatePostPoneDateChange(newMinDate: Long) {
        filterState.value = filterState.value!!.copy(minDatePostPone = newMinDate.toLocalDateTime())
    }

    fun onMaxDatePostPoneDateChange(newMaxDate: Long) {
        filterState.value = filterState.value!!.copy(maxDatePostPone = newMaxDate.toLocalDateTime())
    }

    fun onApplyFilter() {
        //TODO: Implementar
    }

    fun onCleanFilter() {
        filterState.value = FilterPostPoneMatch()
    }

    fun acceptPostPoneMatch(idPostPoneMatch: String) {
        //TODO: Implementar
    }

    fun rejectPostPoneMatch(idPostPoneMatch: String) {
        //TODO: Implementar
    }

    //TODO: Passar o id da equipa na rota
    fun showMoreInfo(
        idOpponent: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(Routes.TeamRoutes.TEAM_PROFILE.route) {
            launchSingleTop = true
        }
    }
}
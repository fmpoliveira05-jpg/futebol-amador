package com.example.amfootball.ui.viewModel

import androidx.compose.runtime.State
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.leadboardDto.LeadboardDto
import com.example.amfootball.navigation.Objects.page.CrudTeamRoutes

class LeadBoardViewModel: ViewModel() {
    private val listTeam: MutableLiveData<List<LeadboardDto>> = MutableLiveData(emptyList<LeadboardDto>())

    private val inicialSizeList = mutableStateOf(value = 10)

    //Getters
    val inicialList: State<List<LeadboardDto>> = derivedStateOf {
        listTeam.value!!.take(inicialSizeList.value)
    }

    val showMoreButton: State<Boolean> = derivedStateOf {
        inicialSizeList.value < listTeam.value!!.size
    }

    //Inicializer
    init {
        //TODO: Depois adaptar para ir buscar ao FireBase
        listTeam.value = LeadboardDto.generateLeadboardExample()
    }

    //Metodo
    fun loadMoreTeams() {
        inicialSizeList.value = inicialSizeList.value.plus(10)
    }

    fun showInfoTeam(
        idTeam: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(route = "${CrudTeamRoutes.PROFILE_TEAM}/${idTeam}") {
            launchSingleTop = true
        }
    }
}
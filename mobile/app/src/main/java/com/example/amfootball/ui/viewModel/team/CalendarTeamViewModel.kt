package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.filters.FilterCalendar
import com.example.amfootball.data.dtos.match.CalendarInfoDto
import com.example.amfootball.data.enums.TypeMatch
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.utils.extensions.toLocalDateTime

class CalendarTeamViewModel: ViewModel() {
    private val filterState: MutableLiveData<FilterCalendar> = MutableLiveData(FilterCalendar())
    val filter: LiveData<FilterCalendar> = filterState

    private val listState: MutableLiveData<List<CalendarInfoDto>> = MutableLiveData(emptyList<CalendarInfoDto>())
    val list: LiveData<List<CalendarInfoDto>> = listState

    init {
        //TODO: Meter para carregar os jogos do calendario da API
        listState.value = CalendarInfoDto.createExampleCalendarList()
    }

    fun onNameChange(newName: String) {
        filterState.value = filterState.value?.copy(opponentName = newName)
    }

    fun onMinDateGameChange(newMinDate: Long) {
        filterState.value = filterState.value?.copy(minGameDate = newMinDate.toLocalDateTime())
    }

    fun onMaxDateGameChange(newMaxDate: Long) {
        filterState.value = filterState.value?.copy(maxGameDate = newMaxDate.toLocalDateTime())
    }

    fun onGameLocalChange(newLocalGame: Boolean?) {
        filterState.value = filterState.value?.copy(gameLocale = newLocalGame)
    }

    fun onTypeMatchChange(typeMatch: TypeMatch?) {
        filterState.value = filterState.value?.copy(typeMatch = typeMatch)
    }

    fun onIsFinishedChange(isFinished: Boolean?) {
        filterState.value = filterState.value?.copy(isFinish = isFinished)
    }

    fun onApplyFilter() {
        //TODO: Meter para chamar Endpoint da API e carregar a lista com os novos dados

        //TODO TEMPORARIAMENTE METER ISTO A DAR LOCALMENTE
    }

    fun onClearFilter() {
        filterState.value = FilterCalendar()
        //TODO: Recarregar a lista da API
    }

    fun onCancelMatch(idMatch: String) {
        //TODO: Chamar endPoint da API para cancelar a partida
    }

    //TODO: DEPOIS ARRANJAR FORMA DE TAMBEM MANDAR O ID
    fun onPostPoneMatch(
        idMatch: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(Routes.TeamRoutes.POST_PONE_MATCH.route) {
            launchSingleTop = true
        }
    }

    fun onStartMatch(idMatch: String) {
        //TODO: Chamar endPoint da API para iniciar a partida (E meter o user em loading ate algum adversario se conectar ao Hub com ele)
        //TODO: Meter aqui verificação a ver se a hora do clique é igual ou superior há da Match
    }

    //TODO: Mandar o Id Match para como parametro
    fun onFinishMatch(
        idMatch: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(Routes.TeamRoutes.FINISH_MATCH.route) {
            launchSingleTop = true
        }
    }
}
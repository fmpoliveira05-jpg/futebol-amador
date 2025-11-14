package com.example.amfootball.ui.viewModel.lists

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.filters.FiltersListTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.navigation.objects.pages.CrudTeamRoutes
import com.example.amfootball.navigation.objects.pages.MatchInviteRoutes
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlin.text.ifEmpty

class ListTeamViewModel: ViewModel() {
    private val listState = mutableStateOf(value= emptyList<ItemTeamInfoDto>())

    private val filterState = MutableStateFlow(FiltersListTeamDto())

    val uiFilterState: StateFlow<FiltersListTeamDto> = filterState.asStateFlow()

    private val listRanks = mutableStateOf(value = emptyList<RankNameDto>())

    val listTeams: State<List<ItemTeamInfoDto>> = listState

    var listRank: State<List<RankNameDto>> = listRanks

    //Setters
    fun onNameChange(name: String) {
        filterState.value = filterState.value.copy(
            name = name.ifEmpty {null}
        )
    }

    fun onCityChange(city: String) {
        filterState.value = filterState.value.copy(
            city = city.ifEmpty {null}
        )
    }

    fun onRankChange(rank: String) {
        filterState.value = filterState.value.copy(
            rank = rank.ifEmpty {null}
        )
    }

    fun onMinPointChange(minPoint: Int?) {
        filterState.value = filterState.value.copy(minPoint = minPoint)
    }

    fun onMaxPointChange(maxPoint: Int?) {
        filterState.value = filterState.value.copy(maxPoint = maxPoint)
    }

    fun onMinAgeChange(minAge: Int?) {
        filterState.value = filterState.value.copy(minAge = minAge)
    }

    fun onMaxAgeChange(maxAge: Int?) {
        filterState.value = filterState.value.copy(maxAge = maxAge)
    }

    fun onMinNumberMembersChange(minNumberMembers: Int?) {
        filterState.value = filterState.value.copy(minNumberMembers = minNumberMembers)
    }

    fun onMaxNumberMembersChange(maxNumberMembers: Int?) {
        filterState.value = filterState.value.copy(maxNumberMembers = maxNumberMembers)
    }

    //Initializer
    init {
        //TODO: Meter para ir buscar as duas listas na API
        listState.value = ItemTeamInfoDto.generateExampleTeams()
        listRanks.value = RankNameDto.generateExampleRanks()
    }

    //Metodos
    fun applyFilters() {
        val currentFilters = filterState.value
        // TODO: Quando tiver a API, é aqui que a vai chamar:

        val list = listState.value

        val filteredList = list.filter { item ->
            val teamName = if (currentFilters.name.isNullOrBlank()) {
                true
            } else {
                item.Name.contains(currentFilters.name, ignoreCase = true)
            }

            val cityName = if (currentFilters.city.isNullOrBlank()) {
                true
            } else {
                item.City.contains(currentFilters.city, ignoreCase = true)
            }

            val rankName = if (currentFilters.rank.isNullOrBlank()) {
                true
            } else {
                item.Rank.contains(currentFilters.rank, ignoreCase = true)
            }

            val minPoint = if (currentFilters.minPoint == null) {
                true
            } else {
                item.Points >= currentFilters.minPoint
            }

            val maxPoint = if (currentFilters.maxPoint == null) {
                true
            } else {
                item.Points <= currentFilters.maxPoint
            }

            val minAge = if (currentFilters.minAge == null) {
                true
            } else {
                item.AverageAge >= currentFilters.minAge
            }

            val maxAge = if (currentFilters.maxAge == null) {
                true
            } else {
                item.AverageAge <= currentFilters.maxAge
            }

            val minNumberMembers = if (currentFilters.minNumberMembers == null) {
                true
            } else {
                item.NumberMembers >= currentFilters.minNumberMembers
            }

            val maxNumberMembers = if (currentFilters.maxNumberMembers == null) {
                true
            } else {
                item.NumberMembers <= currentFilters.maxNumberMembers
            }

            teamName && cityName && rankName && minPoint && maxPoint && minAge && maxAge
                    && minNumberMembers && maxNumberMembers
        }

        listState.value = filteredList
    }

    fun clearFilters() {
        filterState.value = FiltersListTeamDto()
        listState.value = listState.value
    }


    fun sendMatchInvite(idTeam: String,
                        navHostController: NavHostController) {

        //Mandar o id da equipa como parametro
        navHostController.navigate(route = MatchInviteRoutes.SEND_MATCH_INVITE) {
            launchSingleTop = true
        }
    }

    fun sendMemberShipRequest(idTeam: String,
                              navHostController: NavHostController) {
        //TODO: Executar chamada há API para o sendMemberShipRequest
    }

    /**
     * TODO: Falta depois carregar os dados reais da team
     * */
    fun ShowMore(
        idTeam: String,
        navHostController: NavHostController,
    ) {
        navHostController.navigate(route = "${CrudTeamRoutes.PROFILE_TEAM}/${idTeam}") {
            launchSingleTop = true
        }
    }

}
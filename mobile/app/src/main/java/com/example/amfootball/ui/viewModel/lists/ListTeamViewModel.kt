package com.example.amfootball.ui.viewModel.lists

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FiltersListTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.navigation.Objects.page.CrudTeamRoutes
import kotlin.text.ifEmpty

class ListTeamViewModel(): ViewModel() {
    private val listState: MutableLiveData<List<ItemTeamInfoDto>> = MutableLiveData(emptyList<ItemTeamInfoDto>())
    val listTeams: LiveData<List<ItemTeamInfoDto>> = listState
    
    private val filterState: MutableLiveData<FiltersListTeamDto> = MutableLiveData(FiltersListTeamDto())

    val uiFilterState: LiveData<FiltersListTeamDto> = filterState

    private val listRanks: MutableLiveData<List<RankNameDto>> = MutableLiveData(emptyList<RankNameDto>())
    
    var listRank: LiveData<List<RankNameDto>> = listRanks

    //Setters
    fun onNameChange(name: String) {
        filterState.value = filterState.value!!.copy(
            name = name.ifEmpty {null}
        )
    }

    fun onCityChange(city: String) {
        filterState.value = filterState.value!!.copy(
            city = city.ifEmpty {null}
        )
    }

    fun onRankChange(rank: String) {
        filterState.value = filterState.value!!.copy(
            rank = rank.ifEmpty {null}
        )
    }

    fun onMinPointChange(minPoint: Int?) {
        filterState.value = filterState.value!!.copy(minPoint = minPoint)
    }

    fun onMaxPointChange(maxPoint: Int?) {
        filterState.value = filterState.value!!.copy(maxPoint = maxPoint)
    }

    fun onMinAgeChange(minAge: Int?) {
        filterState.value = filterState.value!!.copy(minAge = minAge)
    }

    fun onMaxAgeChange(maxAge: Int?) {
        filterState.value = filterState.value!!.copy(maxAge = maxAge)
    }

    fun onMinNumberMembersChange(minNumberMembers: Int?) {
        filterState.value = filterState.value!!.copy(minNumberMembers = minNumberMembers)
    }

    fun onMaxNumberMembersChange(maxNumberMembers: Int?) {
        filterState.value = filterState.value!!.copy(maxNumberMembers = maxNumberMembers)
    }

    //Initializer
    init {
        //TODO: Meter para ir buscar as duas listas na API
        listState.value = ItemTeamInfoDto.generateExampleTeams()
        listRanks.value = RankNameDto.generateExampleRanks()
    }

    //Metodos
    fun applyFilters() {
        val currentFilters = filterState.value!!
        // TODO: Quando tiver a API, é aqui que a vai chamar:

        val list = listState.value

        val filteredList = list!!.filter { item ->
            val teamName = if (currentFilters.name.isNullOrBlank()) {
                true
            } else {
                item.name.contains(currentFilters.name, ignoreCase = true)
            }

            val cityName = if (currentFilters.city.isNullOrBlank()) {
                true
            } else {
                item.city.contains(currentFilters.city, ignoreCase = true)
            }

            val rankName = if (currentFilters.rank.isNullOrBlank()) {
                true
            } else {
                item.rank.contains(currentFilters.rank, ignoreCase = true)
            }

            val minPoint = if (currentFilters.minPoint == null) {
                true
            } else {
                item.points >= currentFilters.minPoint
            }

            val maxPoint = if (currentFilters.maxPoint == null) {
                true
            } else {
                item.points <= currentFilters.maxPoint
            }

            val minAge = if (currentFilters.minAge == null) {
                true
            } else {
                item.averageAge >= currentFilters.minAge
            }

            val maxAge = if (currentFilters.maxAge == null) {
                true
            } else {
                item.averageAge <= currentFilters.maxAge
            }

            val minNumberMembers = if (currentFilters.minNumberMembers == null) {
                true
            } else {
                item.numberMembers >= currentFilters.minNumberMembers
            }

            val maxNumberMembers = if (currentFilters.maxNumberMembers == null) {
                true
            } else {
                item.numberMembers <= currentFilters.maxNumberMembers
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
        navHostController.navigate(route = Routes.TeamRoutes.SEND_MATCH_INVITE.route) {
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
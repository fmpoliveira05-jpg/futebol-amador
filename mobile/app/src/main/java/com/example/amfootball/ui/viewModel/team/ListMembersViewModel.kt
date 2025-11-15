package com.example.amfootball.ui.viewModel.team

import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.filters.FilterMembersTeam
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow

class ListMembersViewModel {
    private val filterState = MutableStateFlow(FilterMembersTeam())
    val uiFilter = filterState.asStateFlow()

    private val listMemberState = MutableStateFlow(emptyList<MemberTeamDto>())
    val uiList = listMemberState.asStateFlow()
    //private val originList = List<MemberTeamDto>() = emptyList()

    //Init
    init {
        //TODO: Meter para ir buscar na API a lista de Membros da equipa especifica
        var initValue = MemberTeamDto.createExampleList()
        listMemberState.value = initValue
        //originList = initValue
    }

    fun onTypeMemberChange(TypeMember: TypeMember?) {
        filterState.value = filterState.value.copy(typeMember = TypeMember)
    }

    fun onNameChange(newName: String) {
        filterState.value = filterState.value.copy(name = newName)
    }

    fun onMinAgeChange(newAge: Int?) {
        filterState.value = filterState.value.copy(minAge = newAge)
    }

    fun onMaxAgeChange(newMaxAge: Int?) {
        filterState.value = filterState.value.copy(maxAge = newMaxAge)
    }

    fun onPositionChange(newPosition: Position?) {
        filterState.value = filterState.value.copy(position = newPosition)
    }

    fun onClearFilter() {
        filterState.value = FilterMembersTeam()
    }

    fun onApplyFilter() {
        //TODO: Fazer pedido há API de endPoint para filtro
    }

    fun showMoreInfo(
        idUser: String,
        navHostController: NavHostController
    ) {
        //TODO: Levar para a pagina de perfil
    }
}
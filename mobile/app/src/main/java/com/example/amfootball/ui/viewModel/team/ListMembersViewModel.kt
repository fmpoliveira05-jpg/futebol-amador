package com.example.amfootball.ui.viewModel.team

import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.filters.FilterMembersTeam
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import androidx.compose.runtime.State

class ListMembersViewModel: ViewModel() {
    private val filterState = MutableStateFlow(FilterMembersTeam())
    val uiFilter = filterState.asStateFlow()

    private val listMemberState = MutableStateFlow(emptyList<MemberTeamDto>())
    val uiList = listMemberState.asStateFlow()
    private val listTypeMember = mutableStateOf<List<TypeMember?>>(emptyList())
    val uiListTypeMember: State<List<TypeMember?>> = listTypeMember

    private val listPositions = mutableStateOf<List<Position?>>(emptyList())
    val uiListPositions: State<List<Position?>> = listPositions
    //Init
    init {
        //TODO: Meter para ir buscar na API a lista de Membros da equipa especifica
        var initValue = MemberTeamDto.createExampleList()
        listMemberState.value = initValue

        listTypeMember.value = listOf(
            null,
            TypeMember.PLAYER,
            TypeMember.ADMIN_TEAM
        )

        listPositions.value = listOf(
            null,
            Position.FORWARD,
            Position.MIDFIELDER,
            Position.DEFENDER,
            Position.GOALKEEPER
        )
    }

    fun onTypeMemberChange(typeMember: TypeMember?) {
        filterState.value = filterState.value.copy(typeMember = typeMember)
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

    fun onPromoteMember(idPlayer: String) {
        //TODO: Chamar endPoint para promove-lo a admin e depois fazer update na lista
    }

    fun onDemoteMember(idAdmin: String) {
        //TODO: Chamar endPoint para despromover o admin e depois fazer update na lista
    }

    fun onRemovePlayer(idPlayer: String) {
        //TODO: Chamar endPoint para remover o player da Teame e depois fazer update na lista
    }

    fun onShowMoreInfo(
        idUser: String,
        navHostController: NavHostController
    ) {
        //TODO: Levar para a pagina de perfil
    }
}
package com.example.amfootball.ui.viewModel.team

import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMembersFilterError
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.utils.UserConst
import com.example.amfootball.R

class ListMembersViewModel(): ViewModel() {
    private val filterState: MutableLiveData<FilterMembersTeam> = MutableLiveData(FilterMembersTeam())
    val uiFilter: LiveData<FilterMembersTeam> = filterState

    private val errorFilters: MutableLiveData<FilterMembersFilterError> = MutableLiveData(FilterMembersFilterError())

    val uiErrorFilters: LiveData<FilterMembersFilterError> = errorFilters

    private val listMemberState: MutableLiveData<List<MemberTeamDto>> = MutableLiveData(emptyList<MemberTeamDto>())
    val uiList: LiveData<List<MemberTeamDto>> = listMemberState

    private val listTypeMember: MutableLiveData<List<TypeMember?>> = MutableLiveData<List<TypeMember?>>(emptyList())
    val uiListTypeMember: LiveData<List<TypeMember?>> = listTypeMember

    private val listPositions: MutableLiveData<List<Position?>> = MutableLiveData<List<Position?>>(emptyList())
    val uiListPositions: LiveData<List<Position?>> = listPositions
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
        filterState.value = filterState.value!!.copy(typeMember = typeMember)
    }

    fun onNameChange(newName: String) {
        filterState.value = filterState.value!!.copy(name = newName)
    }

    fun onMinAgeChange(newAge: Int?) {
        filterState.value = filterState.value!!.copy(minAge = newAge)
    }

    fun onMaxAgeChange(newMaxAge: Int?) {
        filterState.value = filterState.value!!.copy(maxAge = newMaxAge)
    }

    fun onPositionChange(newPosition: Position?) {
        filterState.value = filterState.value!!.copy(position = newPosition)
    }

    fun onClearFilter() {
        filterState.value = FilterMembersTeam()
    }

    fun onApplyFilter() {
        if(!validateFilter()) {
            return
        }
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

    //TODO: Mandar como parametro o idUser
    fun onShowMoreInfo(
        idUser: String,
        navHostController: NavHostController
    ) {
        navHostController.navigate(Routes.UserRoutes.PROFILE.route) {
            launchSingleTop = true
        }
    }

    private fun validateFilter(): Boolean {
        val name = filterState.value?.name
        val minAge = filterState.value?.minAge
        val maxAge = filterState.value?.maxAge

        var errorName: ErrorMessage? = null
        var errorMinAge: ErrorMessage? = null
        var errorMaxAge: ErrorMessage? = null

        if(name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            errorName = ErrorMessage(
                messageId = R.string.error_max_name_member,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        var minAgeValid = true
        if(minAge != null && minAge < UserConst.MIN_AGE) {
            errorMinAge = ErrorMessage(
                messageId = R.string.error_min_age,
                args = listOf(UserConst.MIN_AGE)
            )
            minAgeValid = false
        }

        var maxAgeValid = true
        if(maxAge != null && maxAge > UserConst.MAX_AGE) {
            errorMaxAge = ErrorMessage(
                messageId = R.string.error_max_age,
                args = listOf(UserConst.MAX_AGE)
            )
            maxAgeValid = false
        }

        if (minAgeValid && maxAgeValid && maxAge != null && minAge != null) {
            if (minAge > maxAge) {
                errorMinAge = ErrorMessage(messageId = R.string.error_min_age_greater_max)
                errorMaxAge = ErrorMessage(messageId = R.string.error_max_age_minor_min)
            }
        }

        errorFilters.value = FilterMembersFilterError(
            nameError = errorName,
            minAgeError = errorMinAge,
            maxAgeError = errorMaxAge
        )

        val isValid = listOf(errorName, errorMinAge, errorMaxAge).all {
            it == null
        }

        return isValid
    }
}
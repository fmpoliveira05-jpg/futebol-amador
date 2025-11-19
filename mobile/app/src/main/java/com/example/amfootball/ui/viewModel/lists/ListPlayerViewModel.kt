package com.example.amfootball.ui.viewModel.lists

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterListPlayerDto
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterPlayersErrors
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.utils.UserConst
import com.example.amfootball.R
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PlayerConst

class ListPlayerViewModel(): ViewModel() {
    private val filterState: MutableLiveData<FilterListPlayerDto> = MutableLiveData(FilterListPlayerDto())
    val uiFilters: LiveData<FilterListPlayerDto> = filterState

    private val filterErrorState: MutableLiveData<FilterPlayersErrors> = MutableLiveData(FilterPlayersErrors())
    val filterError: LiveData<FilterPlayersErrors> = filterErrorState

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
        if(!validateForm()) {
            return
        }
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

    private fun validateForm(): Boolean {
        val name = filterState.value?.name
        val city = filterState.value?.city
        val minAge = filterState.value?.minAge
        val maxAge = filterState.value?.maxAge
        val minSize = filterState.value?.minSize
        val maxSize = filterState.value?.maxSize

        var nameError: ErrorMessage? = null
        var cityError: ErrorMessage? = null
        var minAgeError: ErrorMessage? = null
        var maxAgeError: ErrorMessage? = null
        var minSizeError: ErrorMessage? = null
        var maxSizeError: ErrorMessage? = null

        if(name != null && name.length > UserConst.MAX_NAME_LENGTH) {
            nameError = ErrorMessage(
                messageId = R.string.error_max_name_player,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        if(city != null && city.length > GeneralConst.MAX_CITY_LENGTH) {
            cityError = ErrorMessage(
                messageId = R.string.error_max_city,
                args = listOf(GeneralConst.MAX_CITY_LENGTH)
            )
        }

        var isValidMinAge = true
        if(minAge != null) {
            if (minAge < UserConst.MIN_AGE) {
                minAgeError = ErrorMessage(
                    messageId = R.string.error_min_age,
                    args = listOf(UserConst.MIN_AGE)
                )
            }

            if (minAge > UserConst.MAX_AGE) {
                minAgeError = ErrorMessage(
                    messageId = R.string.error_max_age,
                    args = listOf(UserConst.MAX_AGE)
                )
            }
            isValidMinAge = false
        }

        var isValidMaxAge = true
        if(maxAge != null ) {
            if(maxAge < UserConst.MIN_AGE) {
                maxAgeError = ErrorMessage(
                    messageId = R.string.error_min_age,
                    args = listOf(UserConst.MIN_AGE)
                )
            }

            if (maxAge > UserConst.MAX_AGE) {
                maxAgeError = ErrorMessage(
                    messageId = R.string.error_max_age,
                    args = listOf(UserConst.MAX_AGE)
                )
            }
            isValidMaxAge = false
        }

        if(isValidMinAge && isValidMaxAge && minAge != null && maxAge != null && minAge > maxAge) {
            minAgeError = ErrorMessage(
                messageId = R.string.error_min_age_greater_max,
            )

            maxAgeError = ErrorMessage(
                messageId = R.string.error_min_age_greater_max,
            )
        }

        var isValidMinSize = true
        if(minSize != null) {
            if (minSize < PlayerConst.MIN_HEIGHT) {
                minSizeError = ErrorMessage(
                    messageId = R.string.error_min_size,
                    args = listOf(PlayerConst.MIN_HEIGHT)
                )
            }

            if (minSize > PlayerConst.MAX_HEIGHT) {
                minSizeError = ErrorMessage(
                    messageId = R.string.error_max_size,
                    args = listOf(PlayerConst.MAX_HEIGHT)
                )
            }
            isValidMinSize = false
        }

        var isValidMaxSize = true
        if (maxSize != null) {
            if (maxSize < PlayerConst.MIN_HEIGHT) {
                maxSizeError = ErrorMessage(
                    messageId = R.string.error_min_size,
                    args = listOf(PlayerConst.MIN_HEIGHT)
                )
            }

            if (maxSize > PlayerConst.MAX_HEIGHT) {
                maxSizeError = ErrorMessage(
                    messageId = R.string.error_max_size,
                    args = listOf(PlayerConst.MAX_HEIGHT)
                )
            }
            isValidMaxSize = false
        }

        if (isValidMinSize && isValidMaxSize && minSize != null && maxSize != null && minSize > maxSize) {
            minSizeError = ErrorMessage(messageId = R.string.error_min_size_greater_max)
            maxSizeError = ErrorMessage(messageId = R.string.error_max_size_minor_min)
        }

        filterErrorState.value = FilterPlayersErrors(
            nameError = nameError,
            cityError = cityError,
            minAgeError = minAgeError,
            maxAgeError = maxAgeError,
            minSizeError = minSizeError,
            maxSizeError = maxSizeError
        )

        val isValid = listOf(nameError, cityError, minAgeError, maxAgeError, minSizeError, maxSizeError).all {
            it == null
        }

        return isValid
    }
}
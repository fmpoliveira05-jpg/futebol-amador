package com.example.amfootball.ui.viewModel.team

import androidx.navigation.NavHostController
import com.example.amfootball.data.filters.FilterPostPoneMatch
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.ListPostPoneMatchFiltersError
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.utils.extensions.toLocalDateTime
import com.example.amfootball.R
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import javax.inject.Inject

//TODO: Implementar os metodos todos com as chamadas há API (se necessário)
@HiltViewModel
class ListPostPoneMatchViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver
) : ListsViewModels<PostPoneMatchDto>(networkObserver = networkObserver) {
    private val filterState: MutableStateFlow<FilterPostPoneMatch> = MutableStateFlow<FilterPostPoneMatch>(FilterPostPoneMatch())
    val filter: StateFlow<FilterPostPoneMatch> = filterState

    private val filtersErrorsState: MutableStateFlow<ListPostPoneMatchFiltersError> = MutableStateFlow<ListPostPoneMatchFiltersError>(ListPostPoneMatchFiltersError())
    val filterErros: StateFlow<ListPostPoneMatchFiltersError> = filtersErrorsState

    init {
        //TODO: Carregar a lista da API
        listState.value = PostPoneMatchDto.createExamplePostPoneMatchList()
    }

    fun onOpponentNameChange(newName: String) {
        filterState.value = filterState.value.copy(nameOpponent = newName)
    }

    fun onIsHomeChange(isHome: Boolean?) {
        filterState.value = filterState.value.copy(isHome = isHome)
    }

    fun onMinDateGameChange(newMinDate: Long) {
        filterState.value = filterState.value.copy(minDataGame = newMinDate.toLocalDateTime())
    }

    fun onMaxDateChange(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxDateGame = newMaxDate.toLocalDateTime())
    }

    fun onMinDatePostPoneDateChange(newMinDate: Long) {
        filterState.value = filterState.value.copy(minDatePostPone = newMinDate.toLocalDateTime())
    }

    fun onMaxDatePostPoneDateChange(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxDatePostPone = newMaxDate.toLocalDateTime())
    }

    fun onApplyFilter() {
        if(!validateFilters()) {
            return
        }
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
    fun showMoreInfo(idOpponent: String, navHostController: NavHostController) {
        onlineFunctionality(
            action = {
                navHostController.navigate("${Routes.TeamRoutes.TEAM_PROFILE.route}/${idOpponent}") {
                    launchSingleTop = true
                }
            },
            toastMessage = "Só pode ver mais informações da equipa se estiver online."
        )
    }

    private fun validateFilters(): Boolean {
        val nameOpponent = filterState.value.nameOpponent
        val minDateGame = filterState.value.minDataGame
        val maxDateGame = filterState.value.maxDateGame
        val minDatePostPone = filterState.value.minDatePostPone
        val maxDatePostPone = filterState.value.maxDatePostPone

        var nameOpponentError: ErrorMessage? = null
        var minDateGameError: ErrorMessage? = null
        var maxDateGameError: ErrorMessage? = null
        var minDatePostPoneError: ErrorMessage? = null
        var maxDatePostPoneError: ErrorMessage? = null

        if (nameOpponent != null && nameOpponent.length > TeamConst.MAX_NAME_LENGTH) {
            nameOpponentError = ErrorMessage(
                messageId = R.string.error_max_name_opponent,
                args = listOf(TeamConst.MAX_NAME_LENGTH)
            )
        }

        if (minDateGame != null && maxDateGame != null) {
            if (minDateGame > maxDateGame) {
                minDateGameError = ErrorMessage(
                    messageId = R.string.error_min_date_after,
                    args = listOf(R.string.error_post_pone_Date)
                )
                maxDateGameError = ErrorMessage(
                    messageId = R.string.error_max_date_before,
                    args = listOf(R.string.error_post_pone_Date)
                )
            }
        }

        if (minDatePostPone != null && maxDatePostPone != null) {
            if (minDatePostPone > maxDatePostPone) {
                minDatePostPoneError = ErrorMessage(
                    messageId = R.string.error_min_date_after,
                    args = listOf(R.string.error_post_pone_Date)
                )
                maxDatePostPoneError = ErrorMessage(
                    messageId = R.string.error_max_date_before,
                    args = listOf(R.string.error_post_pone_Date)
                )
            }
        }

        filtersErrorsState.value = ListPostPoneMatchFiltersError(
            nameOpponentError = nameOpponentError,
            minDateGameError = minDateGameError,
            maxDateGameError = maxDateGameError,
            minDatePostPoneError = minDatePostPoneError,
            maxDatePostPoneError = maxDatePostPoneError
        )

        val isValid = listOf(nameOpponentError, minDateGameError, maxDateGameError,
            minDatePostPoneError, maxDatePostPoneError).all {
            it == null
        }

        return isValid
    }
}
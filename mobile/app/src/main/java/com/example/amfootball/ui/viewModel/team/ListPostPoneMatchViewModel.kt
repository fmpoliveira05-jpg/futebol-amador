package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.SavedStateHandle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.filtersError.ListPostPoneMatchFiltersError
import com.example.amfootball.data.filters.FilterPostPoneMatch
import com.example.amfootball.data.manager.CalendarManager
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import com.example.amfootball.core.utils.TeamConst
import com.example.amfootball.core.extensions.toLocalDateTime
import com.example.amfootball.data.remote.dtos.postponeMatch.PostPoneResponse
import com.example.amfootball.data.remote.dtos.postponeMatch.PostponeDto
import com.example.amfootball.data.remote.services.PostPoneMatchService
import com.example.amfootball.domains.enums.StatusPostPone
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

/**
* ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Listagem de Pedidos de Adiamento.
*
* Gere o estado dos filtros (pesquisa por nome, local e, crucialmente, por quatro critérios de data:
* Data Original Mínima/Máxima e Data Proposta Mínima/Máxima) e coordena as ações de Aceitar/Rejeitar.
*
* Herda de [ListsViewModels] para obter funcionalidades de gestão de UI State, Loading, e lista principal.
*
* @property networkObserver Observador de conectividade injetado no [ListsViewModels].
*/
@HiltViewModel
class ListPostPoneMatchViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val postPoneMatchService: PostPoneMatchService,
    private val calendarManager: CalendarManager,
    private val savedStateHandle: SavedStateHandle
) : ListsViewModels<PostponeDto>(networkObserver = networkObserver) {

    private val teamId: String = savedStateHandle.get<String>("teamId") ?: ""

    /**
     * Estado interno mutável contendo os critérios de filtro atuais.
     */
    private val filterState: MutableStateFlow<FilterPostPoneMatch> = MutableStateFlow(FilterPostPoneMatch())

    /**
     * Fluxo público de leitura dos critérios de filtro.
     */
    val filter: StateFlow<FilterPostPoneMatch> = filterState.asStateFlow()

    /**
     * Estado interno mutável contendo os erros de validação de filtros.
     */
    private val filtersErrorsState: MutableStateFlow<ListPostPoneMatchFiltersError> = MutableStateFlow(ListPostPoneMatchFiltersError())

    /**
     * Fluxo público de leitura dos erros de filtro.
     */
    val filterErros: StateFlow<ListPostPoneMatchFiltersError> = filtersErrorsState.asStateFlow()

    init {
        loadListPostPone()
    }

    /**
     * Atualiza o campo de filtro com o nome do adversário.
     */
    fun onOpponentNameChange(newName: String) {
        filterState.value = filterState.value.copy(nameOpponent = newName)
    }

    /**
     * Atualiza o campo de filtro para o local do jogo (Casa, Fora ou Ambos/Null).
     */
    fun onIsHomeChange(isHome: Boolean?) {
        filterState.value = filterState.value.copy(isHome = isHome)
    }

    /**
     * Atualiza a data **mínima** do jogo **Original**.
     */
    fun onMinDateGameChange(newMinDate: Long) {
        filterState.value = filterState.value.copy(minDataGame = newMinDate.toLocalDateTime())
    }

    /**
     * Atualiza a data **máxima** do jogo **Original**.
     */
    fun onMaxDateChange(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxDateGame = newMaxDate.toLocalDateTime())
    }

    /**
     * Atualiza a data **mínima** de **Adiamento Proposta**.
     */
    fun onMinDatePostPoneDateChange(newMinDate: Long) {
        filterState.value = filterState.value.copy(minDatePostPone = newMinDate.toLocalDateTime())
    }

    /**
     * Atualiza a data **máxima** de **Adiamento Proposta**.
     */
    fun onMaxDatePostPoneDateChange(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxDatePostPone = newMaxDate.toLocalDateTime())
    }

    /**
     * Aplica os filtros atualmente definidos.
     *
     * O fluxo de execução:
     * 1. Executa a validação síncrona dos filtros ([validateFilters]).
     * 2. Se for válido: Faz uma chamada assíncrona para o backend para obter a lista filtrada.
     */
    fun onApplyFilter() {
        if (!validateFilters()) {
            return
        }

        if (networkObserver.isOnlineOneShot()) {
            loadListPostPone()
        } else {
            listState.value = filterOffline(
                originalList = originalList,
                filter = filterState.value
            )
        }
    }

    /**
     * Limpa todos os filtros de data e nome, revertendo a lista para o estado original.
     */
    fun onCleanFilter() {
        filterState.value = FilterPostPoneMatch()
    }

    /**
     * Aceita um pedido de adiamento.
     *
     * @param idPostPoneMatch O ID do pedido de adiamento a ser aceite.
     */
    fun acceptPostPoneMatch(idPostPoneMatch: String, idOpponent: String, matchDate: Long) {
        if(!isNetworkAvailable()) {
            updateToast(R.string.toast_offline_accept_postpone)
            return
        }

        launchDataLoad {
            val acceptPostPone = PostPoneResponse(
                idMatch = idPostPoneMatch,
                statusPostPone = StatusPostPone.ACCEPTED.value,
                idTeam = teamId,
                idOpponent = idOpponent
            )

            postPoneMatchService.acceptPostPoneMatch(teamId = teamId, postPone = acceptPostPone)

            removeItemFromState(idMatch = idPostPoneMatch)

            calendarManager.updateMatchDateOnly(matchId = idPostPoneMatch, newStart = matchDate, newEnd = matchDate)
            updateToast(R.string.toast_success_accept_postpone)
        }
    }

    /**
     * Rejeita um pedido de adiamento.
     *
     * @param idPostPoneMatch O ID do pedido de adiamento a ser rejeitado.
     */
    fun rejectPostPoneMatch(idPostPoneMatch: String, idOpponent: String) {
        if(!isNetworkAvailable()) {
            updateToast(R.string.toast_offline_reject_postpone)
            return
        }

         launchDataLoad {

             val rejectPostPone = PostPoneResponse(
                 idMatch = idPostPoneMatch,
                 statusPostPone = StatusPostPone.REJECTED.value,
                 idTeam = teamId,
                 idOpponent = idOpponent
             )

             postPoneMatchService.rejectPostPoneMatch(teamId = teamId, postPone = rejectPostPone)
             updateToast(R.string.toast_success_reject_postpone)
         }
    }

    /**
     * Navega para o ecrã de perfil do adversário.
     *
     * Executa a navegação apenas se o dispositivo estiver online.
     *
     * @param idOpponent O ID da equipa adversária.
     * @param navHostController O controlador de navegação.
     */
    fun showMoreInfo(idOpponent: String, navHostController: NavHostController) {
        onlineFunctionality(
            action = {
                navHostController.navigate("${Routes.TeamRoutes.TEAM_PROFILE.route}/${idOpponent}") {
                    launchSingleTop = true
                }
            },
            toastMessage = R.string.toast_offline_info_team
        )
    }

    private fun loadListPostPone() {
        launchDataLoad {
            val teams = postPoneMatchService.getPostPoneMatch(teamId = teamId, filterState.value)

            listState.value = teams
            if (filterState.value == FilterPostPoneMatch()) {
                originalList = teams
            }
        }
    }

    private fun removeItemFromState(idMatch: String) {
        val currentList = listState.value.toMutableList()
        currentList.removeAll { it.idMatch == idMatch }
        listState.value = currentList

        val currentOriginal = originalList.toMutableList()
        currentOriginal.removeAll { it.idMatch == idMatch }
        originalList = currentOriginal
    }

    private fun filterOffline(
        originalList: List<PostponeDto>,
        filter: FilterPostPoneMatch
    ): List<PostponeDto> {
        return originalList.filter{ item ->
            val name = filter.nameOpponent.isNullOrBlank()
                    || item.opponent.name.contains(filter.nameOpponent, ignoreCase = true)

            //val isHome = filter.isHome == null || item.isHome == filter.isHome

            val minDate = filter.minDataGame == null || item.gameDate.toLocalDate() >= filter.minDataGame.toLocalDate()
            val maxDate = filter.maxDateGame == null || item.gameDate.toLocalDate() <= filter.maxDateGame.toLocalDate()

            val minDatePostPone = filter.minDatePostPone == null || item.gameDate.toLocalDate() >= filter.minDatePostPone.toLocalDate()
            val maxDatePostPone = filter.maxDatePostPone == null || item.gameDate.toLocalDate() <= filter.maxDatePostPone.toLocalDate()

            name && minDate && maxDate && minDatePostPone && maxDatePostPone
        }
    }

    /**
     * Validação síncrona dos critérios de filtro.
     *
     * Verifica o comprimento do nome do adversário e a ordem das datas.
     *
     * **Regras de Data:**
     * - Data Original Mínima não pode ser posterior à Máxima.
     * - Data Proposta Mínima não pode ser posterior à Máxima.
     *
     * @return `true` se todos os campos forem válidos, `false` caso contrário.
     */
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

        val isValid = listOf(
            nameOpponentError, minDateGameError, maxDateGameError,
            minDatePostPoneError, maxDatePostPoneError
        ).all { it == null }

        return isValid
    }
}
package com.example.amfootball.ui.viewModel.matchInvite

import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMatchInviteError
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import com.example.amfootball.utils.UserConst
import com.example.amfootball.utils.extensions.toLocalDateTime
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Listagem de Convites de Jogo.
 *
 * Este componente gere o estado dos filtros (pesquisa por remetente, datas) e coordena as ações
 * de decisão (Aceitar/Rejeitar/Negociar) sobre os convites recebidos.
 *
 * Herda de [ListsViewModels] para obter funcionalidades de gestão de UI State, Loading, e da lista principal.
 *
 * @property networkObserver Observador de conectividade injetado no [ListsViewModels].
 */
@HiltViewModel
class ListMatchInviteViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver
) : ListsViewModels<InfoMatchInviteDto>(networkObserver = networkObserver) {

    /**
     * Estado interno mutável contendo os critérios de filtro atuais.
     */
    private val filtersState: MutableStateFlow<FilterMatchInvite> = MutableStateFlow(FilterMatchInvite())

    /**
     * Fluxo público de leitura dos filtros observados pela UI.
     */
    val uiFilters: StateFlow<FilterMatchInvite> = filtersState

    /**
     * Estado interno mutável contendo os erros de validação de filtros.
     */
    private val filtersErrorState: MutableStateFlow<FilterMatchInviteError> = MutableStateFlow(FilterMatchInviteError())

    /**
     * Fluxo público de leitura dos erros de filtro.
     */
    val filterError: StateFlow<FilterMatchInviteError> = filtersErrorState

    //Initializor
    init {
        //TODO: Carregar dados do Backend
        val initList = InfoMatchInviteDto.generatePreviewList()
        listState.value = initList
        originalList = initList
    }

    /**
     * Atualiza o campo de filtro com o nome do remetente (equipa adversária).
     *
     * @param newSenderName O novo texto para o filtro de nome.
     */
    fun onNameSenderChange(newSenderName: String) {
        filtersState.value = filtersState.value.copy(senderName = newSenderName)
    }

    /**
     * Atualiza o campo de filtro para a data mínima do jogo proposto.
     *
     * @param newMinDate Timestamp (Long) da nova data mínima.
     */
    fun onMinDateChange(newMinDate: Long) {
        filtersState.value = filtersState.value.copy(minDate = newMinDate.toLocalDateTime())
    }

    /**
     * Atualiza o campo de filtro para a data máxima do jogo proposto.
     *
     * @param newMaxDate Timestamp (Long) da nova data máxima.
     */
    fun onMaxDateChange(newMaxDate: Long) {
        filtersState.value = filtersState.value.copy(maxDate = newMaxDate.toLocalDateTime())
    }

    //TODO: Implementar
    /**
     * Aplica os filtros atualmente definidos.
     *
     * O fluxo é o seguinte:
     * 1. Executa a validação síncrona dos filtros ([validateFitler]).
     * 2. Se for válido: Faz uma chamada assíncrona para o backend para obter a lista filtrada.
     *
     */
    fun onApplyFilter() {
        if (!validateFitler()) {
            return
        }
    }

    //TODO: Implementar
    /**
     * Limpa todos os filtros e recarrega a lista original.
     *
     * Reinicia o estado dos filtros para o valor inicial ([FilterMatchInvite]) e atualiza a lista.
     */
    fun onFilterClear() {
        filtersState.value = FilterMatchInvite()
        listState.value = originalList
    }

    /**
     * Aceita um convite de jogo.
     *
     * @param idMatchInvite O ID do convite a ser aceite.
     */
    fun acceptMatchInvite(idMatchInvite: String) {
        //TODO: Remover da lista e fazer pedido há API para aceitar
    }

    /**
     * Inicia o fluxo de negociação (contra-proposta) para um convite de jogo.
     *
     * @param idMatchInvite O ID do convite a ser negociado.
     * @param navHostController Controlador de navegação para transição para o ecrã de negociação.
     */
    //TODO: Passar o idMatch por parametro (Importante para teste de sistema)
    fun negociateMatchInvite(idMatchInvite: String, navHostController: NavHostController) {
        navHostController.navigate(route = Routes.TeamRoutes.NEGOCIATE_MATCH_INVITE.route) {
            launchSingleTop = true
        }
    }

    /**
     * Rejeita um convite de jogo.
     *
     * @param idMatchInvite O ID do convite a ser rejeitado.
     */
    fun rejectMatchInvite(idMatchInvite: String) {
        //TODO: Mandar pedido há API para rejeitar
    }

    //TODO: Corrijir
    /**
     * Navega para o ecrã de detalhes (Perfil da Equipa/Jogador) associado ao convite.
     *
     * @param idMatchInvite O ID do convite (deve ser usado para extrair o ID do remetente).
     * @param navHostController Controlador de navegação.
     */
    fun showMoreDetails(idMatchInvite: String, navHostController: NavHostController) {
        navHostController.navigate(route = "${Routes.TeamRoutes.TEAM_PROFILE.route}/$idMatchInvite") {
            launchSingleTop = true
        }
    }

    /**
     * Validação síncrona dos critérios de filtro.
     *
     * Verifica o comprimento do nome do remetente e a ordem das datas (Min Date vs Max Date).
     *
     * @return `true` se todos os filtros forem válidos, `false` caso contrário.
     */
    private fun validateFitler(): Boolean {
        val nameSender = filtersState.value.senderName
        val minDate = filtersState.value.minDate
        val maxDate = filtersState.value.maxDate

        var nameSenderError: ErrorMessage? = null
        var minDateError: ErrorMessage? = null
        var maxDateError: ErrorMessage? = null

        if (nameSender != null && nameSender.length > UserConst.MAX_NAME_LENGTH) {
            nameSenderError = ErrorMessage(
                messageId = R.string.error_max_name_sender,
                args = listOf(UserConst.MAX_NAME_LENGTH)
            )
        }

        if (minDate != null && maxDate != null && minDate > maxDate) {
            minDateError = ErrorMessage(
                messageId = R.string.error_min_date_after,
                args = listOf(R.string.error_date_sende)
            )

            maxDateError = ErrorMessage(
                messageId = R.string.error_max_date_before,
                args = listOf(R.string.error_date_sende)
            )
        }

        filtersErrorState.value = FilterMatchInviteError(
            senderNameError = nameSenderError,
            minDateError = minDateError,
            maxDateError = maxDateError
        )

        val isValid = listOf(nameSender, minDateError, maxDateError).all { it == null }

        return isValid
    }
}
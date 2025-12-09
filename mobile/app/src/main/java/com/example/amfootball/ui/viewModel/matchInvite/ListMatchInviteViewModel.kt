package com.example.amfootball.ui.viewModel.matchInvite

import androidx.lifecycle.SavedStateHandle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterMatchInviteError
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.MatchInviteService
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.ListsViewModels
import com.example.amfootball.utils.ListsSizesConst
import com.example.amfootball.utils.UserConst
import com.example.amfootball.utils.extensions.toLocalDateTime
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio e gestão de estado do ecrã de Listagem de Convites de Jogo.
 *
 * Este componente atua como o orquestrador entre a camada de dados (API/Cache) e a UI.
 * As suas principais responsabilidades incluem:
 * 1. **Gestão de Dados:** Carregamento inicial, paginação e atualização da lista de convites.
 * 2. **Filtragem:** Aplicação de filtros complexos (Nome, Data) tanto online (via API) como offline (via Cache).
 * 3. **Ações de Decisão:** Processamento das respostas aos convites (Aceitar, Rejeitar, Negociar).
 *
 * Herda de [ListsViewModels] para aproveitar a infraestrutura de gestão de estado de listas (Loading, Error, Empty States).
 *
 * @property matchInviteService Serviço para comunicação com a API de convites.
 * @property networkObserver Observador de conectividade para alternar entre lógica Online/Offline.
 * @property savedStateHandle Manipulador de estado para recuperar argumentos de navegação (ex: teamId).
 */
@HiltViewModel
class ListMatchInviteViewModel @Inject constructor(
    private val matchInviteService: MatchInviteService,
    private val networkObserver: NetworkConnectivityObserver,
    private val savedStateHandle: SavedStateHandle
) : ListsViewModels<MatchInviteDto>(networkObserver = networkObserver) {

    /**
     * ID da equipa atual, recuperado dos argumentos de navegação.
     * Usado em todas as chamadas à API que requerem contexto da equipa.
     */
    private val teamId = savedStateHandle.get<String>("teamId")

    /**
     * Estado interno mutável (Backing Property) contendo os critérios de filtro atuais.
     */
    private val filterState: MutableStateFlow<FilterMatchInvite> = MutableStateFlow(FilterMatchInvite())

    /**
     * Fluxo imutável exposto para a UI que representa os filtros ativos.
     * A UI deve observar este fluxo para preencher os campos de filtro.
     */
    val uiFilters: StateFlow<FilterMatchInvite> = filterState

    /**
     * Estado interno mutável contendo os erros de validação dos campos de filtro.
     */
    private val filtersErrorState: MutableStateFlow<FilterMatchInviteError> = MutableStateFlow(FilterMatchInviteError())

    /**
     * Fluxo imutável exposto para a UI contendo mensagens de erro nos filtros.
     * (Ex: Data mínima maior que data máxima, nome muito longo).
     */
    val filterError: StateFlow<FilterMatchInviteError> = filtersErrorState

    init {
        loadDataList()
    }

    /**
     * Carrega a lista de convites de jogo.
     *
     * Lógica executada:
     * 1. Verifica se o [teamId] é válido.
     * 2. Faz o pedido à API usando os filtros atuais.
     * 3. Atualiza o [listState] (lista visível).
     * 4. Se não existirem filtros ativos, atualiza também a [originalList] (Cache para modo offline/limpeza de filtros).
     */
    private fun loadDataList() {
        launchDataLoad {
            if (teamId != null) {
                val list = matchInviteService.getListMatchInvite(
                    teamId = teamId,
                    filter = filterState.value
                )

                listState.value = list
                if (filterState.value == FilterCalendar()) {
                    originalList = list
                }
            } else {
                updateToast(message = R.string.toast_team_id_null)
            }
        }
    }

    /**
     * Atualiza o estado do filtro com o novo nome do remetente (equipa adversária).
     *
     * @param newSenderName O texto inserido pelo utilizador.
     */
    fun onNameSenderChange(newSenderName: String) {
        filterState.value = filterState.value.copy(senderName = newSenderName)
    }

    /**
     * Atualiza o estado do filtro com a nova data mínima.
     * Converte o timestamp (Long) para [LocalDateTime].
     *
     * @param newMinDate Data em milissegundos selecionada no componente de calendário.
     */
    fun onMinDateChange(newMinDate: Long) {
        filterState.value = filterState.value.copy(minDate = newMinDate.toLocalDateTime())
    }

    /**
     * Atualiza o estado do filtro com a nova data máxima.
     * Converte o timestamp (Long) para [LocalDateTime].
     *
     * @param newMaxDate Data em milissegundos selecionada no componente de calendário.
     */
    fun onMaxDateChange(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxDate = newMaxDate.toLocalDateTime())
    }

    /**
     * Executa a lógica de aplicação dos filtros.
     *
     * Processo:
     * 1. Valida os dados inseridos ([validateFitler]). Se inválido, aborta.
     * 2. Verifica a conectividade:
     * - **Online:** Solicita uma nova lista filtrada à API ([loadDataList]).
     * - **Offline:** Filtra a lista localmente usando a [originalList] ([filterOffline]).
     */
    fun onApplyFilter() {
        if (!validateFitler()) {
            updateToast(message = R.string.toast_invalid_filters)
            return
        }

        if (isNetworkAvailable()) {
            loadDataList()
        } else {
            listState.value = filterOffline(
                originalList = originalList,
                filter = filterState.value
            )
        }
    }

    /**
     * Limpa todos os critérios de filtro e restaura a lista completa.
     *
     * Ações:
     * 1. Reinicia [filterState] e [filtersErrorState] para os valores padrão.
     * 2. Reinicia a paginação ([inicialSizeList]).
     * 3. Dependendo da rede:
     * - **Online:** Recarrega os dados "limpos" da API.
     * - **Offline:** Restaura imediatamente os dados da [originalList].
     */
    fun onFilterClear() {
        filterState.value = FilterMatchInvite()
        filtersErrorState.value = FilterMatchInviteError()
        inicialSizeList.value = ListsSizesConst.INICIAL_SIZE

        if (networkObserver.isOnlineOneShot()) {
            loadDataList()
        } else {
            listState.value = originalList
        }
    }

    //TODO: Falta fazer pedido há API do Calendar para adicionar o jogo no calendario das duas equipas.
    /**
     * Envia o pedido para aceitar um convite de jogo.
     *
     * Em caso de sucesso na API, remove o convite da lista local imediatamente
     * para refletir a mudança na UI.
     *
     * @param idMatchInvite O identificador único do convite a aceitar.
     */
    fun acceptMatchInvite(idMatchInvite: String) {
        launchDataLoad {
            if (teamId == null) {
                stopLoading()
                return@launchDataLoad
            }

            matchInviteService.acceptMatchInvitee(teamId = teamId, matchInviteId = idMatchInvite)
            removeItemFromList(idToRemove = idMatchInvite)
        }
    }

    /**
     * Inicia o fluxo de negociação de um convite.
     *
     * Verifica a conectividade antes de navegar, pois a negociação exige internet.
     * Se online, navega para o ecrã de negociação passando o ID do convite.
     *
     * @param idMatchInvite O identificador do convite a negociar.
     * @param navHostController Controlador para realizar a navegação.
     */
    fun negociateMatchInvite(idMatchInvite: String, navHostController: NavHostController) {
        onlineFunctionality(
            action = {
                navHostController.navigate(route = "${Routes.TeamRoutes.NEGOCIATE_MATCH_INVITE.route}/$teamId/$idMatchInvite") {
                    launchSingleTop = true
                }
            },
            toastMessage = R.string.toast_offline_negotiation
        )
    }

    /**
     * Envia o pedido para rejeitar um convite de jogo.
     *
     * Em caso de sucesso na API, remove o convite da lista local imediatamente.
     *
     * @param idMatchInvite O identificador único do convite a rejeitar.
     */
    fun rejectMatchInvite(idMatchInvite: String) {
        launchDataLoad {
            if (teamId == null) {
                stopLoading()
                return@launchDataLoad
            }

            matchInviteService.rejectMatchInivite(teamId = teamId, matchInviteId = idMatchInvite)
            removeItemFromList(idToRemove = idMatchInvite)
        }
    }

    /**
     * Navega para o perfil detalhado da equipa adversária associada ao convite.
     *
     * @param idMatchInvite O ID do convite (usado para resolver a equipa associada).
     * @param navHostController Controlador para realizar a navegação.
     */
    fun showMoreDetails(idMatchInvite: String, navHostController: NavHostController) {
        navHostController.navigate(route = "${Routes.TeamRoutes.TEAM_PROFILE.route}/$idMatchInvite") {
            launchSingleTop = true
        }
    }

    /**
     * Remove um item especifico das listas de estado locais.
     *
     * É crucial remover tanto de [listState] (o que o user vê agora)
     * quanto de [originalList] (o cache). Se não removermos do cache,
     * o item reapareceria ao limpar os filtros.
     *
     * @param idToRemove O ID do convite a ser removido.
     */
    private fun removeItemFromList(idToRemove: String) {
        listState.update { currentList ->
            currentList.filter { item ->
                item.id != idToRemove
            }
        }

        originalList = originalList.filter { item ->
            item.id != idToRemove
        }
    }

    /**
     * Realiza a filtragem dos dados localmente (Modo Offline).
     *
     * Aplica os critérios de "Nome contém..." e "Intervalo de datas" sobre a lista original.
     *
     * @param originalList A lista completa de dados em cache.
     * @param filter Os critérios de filtro a aplicar.
     * @return Uma nova lista contendo apenas os elementos que correspondem aos critérios.
     */
    private fun filterOffline(originalList: List<MatchInviteDto>, filter: FilterMatchInvite): List<MatchInviteDto> {
        return originalList.filter { item ->
            val name = filter.senderName.isNullOrBlank()
                    || item.opponent.name.contains(filter.senderName, ignoreCase = true)

            val minDate = filter.minDate == null
                    || item.gameDate.toLocalDate() >= filter.minDate.toLocalDate()

            val maxDate = filter.maxDate == null
                    || item.gameDate.toLocalDate() <= filter.maxDate.toLocalDate()

            name && minDate && maxDate
        }
    }

    /**
     * Valida as regras de negócio dos filtros.
     *
     * Regras:
     * 1. O nome do remetente não pode exceder [UserConst.MAX_NAME_LENGTH].
     * 2. Se ambas as datas forem fornecidas, a Data Mínima não pode ser posterior à Data Máxima.
     *
     * Atualiza o [filtersErrorState] com as mensagens apropriadas se houver erros.
     *
     * @return `true` se os filtros forem válidos, `false` caso contrário.
     */
    private fun validateFitler(): Boolean {
        val nameSender = filterState.value.senderName
        val minDate = filterState.value.minDate
        val maxDate = filterState.value.maxDate

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

        val isValid = listOf(nameSenderError, minDateError, maxDateError).all { it == null }

        return isValid
    }
}
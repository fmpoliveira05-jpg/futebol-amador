package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.enums.MatchStatus
import com.example.amfootball.data.enums.TypeMatch
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.errors.filtersError.FilterCalendarError
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.repository.CalendarRepository
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.utils.extensions.toLocalDate
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão do ecrã de Calendário da Equipa.
 *
 * Gere o ciclo de vida dos dados de partidas (matches), incluindo carregamento da API,
 * filtragem local/remota, validação de filtros e execução de ações de gestão de jogos
 * (cancelar, adiar, iniciar, finalizar).
 *
 * Implementa lógica offline-first para filtros quando não há conexão à internet.
 *
 * @property networkObserver Observador para monitorizar o estado da rede em tempo real.
 * @property calendarRepository Repositório para acesso aos dados do calendário.
 * @property savedStateHandle Manipulador para recuperar argumentos de navegação (ex: teamId).
 */
@HiltViewModel
class CalendarTeamViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val calendarRepository: CalendarRepository,
    private val savedStateHandle: SavedStateHandle
) : ViewModel() {

    /** ID da equipa recuperado dos argumentos da navegação. Essencial para carregar os dados. */
    private val teamId = savedStateHandle.get<String>("teamId")

    /** Estado atual dos filtros de pesquisa aplicados pelo utilizador. */
    private val filterState: MutableStateFlow<FilterCalendar> = MutableStateFlow(FilterCalendar())
    val filter: StateFlow<FilterCalendar> = filterState

    /**
     * Lista de jogos visível na UI.
     * Pode ser o resultado direto da API ou uma versão filtrada localmente.
     */
    private val listState: MutableStateFlow<List<InfoMatchCalendar>> = MutableStateFlow(emptyList())
    val list: StateFlow<List<InfoMatchCalendar>> = listState

    /**
     * Cache da lista original carregada da API.
     * Usada para permitir a limpeza de filtros e filtragem offline sem novos pedidos à rede.
     */
    private var originalList: List<InfoMatchCalendar> = emptyList()

    /** Estado dos erros de validação dos inputs de filtro (ex: Data Mínima > Data Máxima). */
    private val listErrors: MutableStateFlow<FilterCalendarError> = MutableStateFlow(FilterCalendarError())
    val uiErrors: StateFlow<FilterCalendarError> = listErrors

    /** Estado global da UI (Loading, Erros de API, Toasts). */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(true))
    val uiState: StateFlow<UiState> = _uiState

    /** Estado de conectividade (True = Online, False = Offline). */
    private val isConnected: MutableStateFlow<Boolean> = MutableStateFlow(networkObserver.isOnlineOneShot())
    val isOnline: StateFlow<Boolean> = isConnected

    //Inicializer
    init {
        observeNetworkChanges()
        loadCalendar()
    }

    // --- SETTERS DE FILTROS ---

    /** Atualiza o filtro de nome do adversário. */
    fun onNameChange(newName: String) {
        filterState.value = filterState.value.copy(opponentName = newName)
    }

    /**
     * Atualiza a data mínima do jogo.
     * Converte o timestamp (Long) recebido do DatePicker para LocalDate.
     */
    fun onMinDateGameChange(newMinDate: Long) {
        filterState.value = filterState.value.copy(minGameDate = newMinDate.toLocalDate())
    }

    /**
     * Atualiza a data máxima do jogo.
     * Converte o timestamp (Long) recebido do DatePicker para LocalDate.
     */
    fun onMaxDateGameChange(newMaxDate: Long) {
        filterState.value = filterState.value.copy(maxGameDate = newMaxDate.toLocalDate())
    }

    /** Atualiza o filtro de local (Casa/Fora). */
    fun onGameLocalChange(newLocalGame: Boolean?) {
        filterState.value = filterState.value.copy(isHome = newLocalGame)
    }

    /** Atualiza o filtro de tipo de jogo (Competitivo/Casual). */
    fun onTypeMatchChange(typeMatch: TypeMatch?) {
        filterState.value = filterState.value.copy(typeMatch = typeMatch)
    }

    /** Atualiza o filtro de estado do jogo (Finalizado/Não Finalizado). */
    fun onIsFinishedChange(isFinish: Boolean?) {
        filterState.value = filterState.value.copy(isFinish = isFinish)
    }

    // --- AÇÕES PRINCIPAIS ---

    /**
     * Aplica os filtros definidos.
     *
     * 1. Valida os filtros ([validateFilters]). Se inválido, aborta.
     * 2. Se Online: Recarrega os dados da API com os novos filtros.
     * 3. Se Offline: Filtra a lista localmente ([filterOffline]) usando a [originalList].
     */
    fun onApplyFilter() {
        if(!validateFilters()) {
            return
        }

        if (networkObserver.isOnlineOneShot()) {
            loadCalendar()
        } else {
            listState.value = filterOffline(
                originalList = originalList,
                filter = filterState.value
            )
        }
    }

    /**
     * Limpa todos os filtros ativos e restaura a lista original.
     * Não realiza chamadas à API, usa a cache local [originalList] para performance instantânea.
     */
    fun onClearFilter() {
        filterState.value = FilterCalendar()
        listErrors.value = FilterCalendarError()
        listState.value = originalList
    }

    //TODO: Faltam validações
    /**
     * Navega para o ecrã de cancelamento de partida.
     *
     * Requer conexão à internet. Se offline, exibe um Toast de erro via [UiState].
     */
    fun onCancelMatch(idMatch: String, navHostController: NavHostController) {
        if(networkObserver.isOnlineOneShot()) {
            navHostController.navigate("${Routes.TeamRoutes.CANCEL_MATCH.route}/{${idMatch}}") {
                launchSingleTop = true
            }
        } else {
            _uiState.update {
                it.copy(toastMessage = "Para cancelar o jogo é necessária conexão à internet.")
            }
        }
    }

    //TODO: Faltam validações
    /**
     * Navega para o ecrã de adiamento de partida.
     * Requer conexão à internet.
     */
    fun onPostPoneMatch(idMatch: String, navHostController: NavHostController) {
        if(networkObserver.isOnlineOneShot()) {
            navHostController.navigate("${Routes.TeamRoutes.POST_PONE_MATCH.route}/${idMatch}") {
                launchSingleTop = true
            }
        } else {
            _uiState.update {
                it.copy(toastMessage = "Para adiar o jogo é necessária conexão à internet.")
            }
        }
    }

    //TODO: Faltam validações
    /**
     * Inicia a partida.
     * Requer conexão à internet.
     */
    fun onStartMatch(idMatch: String) {
        if(networkObserver.isOnlineOneShot()) {
            //TODO: Chamar endPoint da API para iniciar a partida (E meter o user em loading ate algum adversario se conectar ao Hub com ele)
            //TODO: Meter aqui verificação a ver se a hora do clique é igual ou superior há da Match
        } else {
            _uiState.update {
                it.copy(toastMessage = "Para iniciar o jogo é necessária conexão à internet.")
            }
        }
    }

    //TODO: Meter Validações
    /**
     * Navega para o ecrã de finalização de partida (inserção de resultados).
     * Requer conexão à internet.
     */
    fun onFinishMatch(idMatch: String, navHostController: NavHostController) {
        if(networkObserver.isOnlineOneShot()) {
            navHostController.navigate("${Routes.TeamRoutes.FINISH_MATCH.route}/{${idMatch}}") {
                launchSingleTop = true
            }
        } else {
            _uiState.update {
                it.copy(toastMessage = "Para finalizar a partida é necessária conexão à internet.")
            }
        }

    }

    /*
    * Callback chamado pela UI quando um Toast é exibido.
    * Limpa a mensagem do estado para evitar que o Toast apareça repetidamente.
    */
    fun onToastShown() {
        _uiState.update { it.copy(toastMessage = null) }
    }

    /**
     * Carrega a lista de jogos da API.
     *
     * - Verifica conectividade (exibe erro se offline).
     * - Verifica se [teamId] existe.
     * - Atualiza [listState] e [originalList] com os dados recebidos.
     * - Gere o estado de Loading.
     */
    //TODO: Falta fazer pedido há API
    fun loadCalendar() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }

                return@launch
            }

            try {
                if (teamId == null) {
                    _uiState.update {
                        it.copy(isLoading = false, errorMessage = "O id da equipa é obrigatório")
                    }

                    return@launch
                }

                val calendar = calendarRepository.getCalendar(teamId = teamId, filter = filterState.value)

                listState.value = calendar
                originalList = calendar

                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    // --- MÉTODOS PRIVADOS ---
    /**
     * Inicia a observação contínua do estado da conectividade de rede.
     * Atualiza [isConnected] em tempo real.
     */
    private fun observeNetworkChanges() {
        viewModelScope.launch {
            networkObserver.observeConnectivity()
                .collect { isOnline ->
                    isConnected.value = isOnline
                }
        }
    }

    /**
     * Filtra a lista de jogos em memória (offline).
     * Utiliza lógica "AND" (todos os critérios devem ser verdadeiros).
     */
    private fun filterOffline(originalList: List<InfoMatchCalendar>, filter: FilterCalendar): List<InfoMatchCalendar> {
        return originalList.filter { item ->
            val name = filter.opponentName.isNullOrBlank()
                    || item.opponent.name.contains(filter.opponentName, ignoreCase = true)
            val minDate = filter.minGameDate == null || item.gameDate.toLocalDate() >= filter.minGameDate
            val maxDate = filter.maxGameDate == null || item.gameDate.toLocalDate() <= filter.maxGameDate
            val matchStatus = filter.isFinish == null || if(filter.isFinish) {
                item.matchStatus == MatchStatus.DONE
            } else {
                item.matchStatus != MatchStatus.SCHEDULED
            }
            val gameLocal = filter.isHome == null || item.isHome == filter.isHome
            val typeMatch = filter.typeMatch == null || item.typeMatch == filter.typeMatch

            name && minDate && maxDate && matchStatus && gameLocal && typeMatch
        }
    }

    /**
     * Valida os filtros de pesquisa.
     * Verifica limites de caracteres e consistência de datas (Mínima <= Máxima).
     * @return `true` se válido, `false` se houver erros (atualizando [uiErrors]).
     */
    private fun validateFilters(): Boolean {
        val opponentName = filterState.value.opponentName
        val minDateGame = filterState.value.minGameDate
        val maxDateGame = filterState.value.maxGameDate

        var nameOpponentError: ErrorMessage? = null
        var minDateGameError: ErrorMessage? = null
        var maxDateGameError: ErrorMessage? = null

        if (opponentName != null && opponentName.length > TeamConst.MAX_NAME_LENGTH) {
            nameOpponentError = ErrorMessage(
                messageId = R.string.error_max_name_team,
                args = listOf(TeamConst.MAX_NAME_LENGTH)
            )
        }

        if (minDateGame != null && maxDateGame != null) {
            if (minDateGame > maxDateGame) {
                minDateGameError = ErrorMessage(
                    messageId = R.string.error_min_date_after,
                    args = listOf(R.string.error_date_game)
                )

                maxDateGameError = ErrorMessage(
                    messageId = R.string.error_max_date_before,
                    args = listOf(R.string.error_date_game)
                )
            }
        }

        listErrors.value = FilterCalendarError(
            opponentNameError = nameOpponentError,
            minGameDateError =  minDateGameError,
            maxGameDateError = maxDateGameError
        )

        val isValid = listOf(nameOpponentError, minDateGameError, maxDateGameError).all {
            it == null
        }

        return isValid
    }
}
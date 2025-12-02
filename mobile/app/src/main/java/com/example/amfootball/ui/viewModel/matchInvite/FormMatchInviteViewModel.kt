package com.example.amfootball.ui.viewModel.matchInvite

import android.util.Log
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import androidx.navigation.NavHostController
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.errors.formErrors.MatchInviteFormErros
import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import javax.inject.Inject
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.enums.Forms.MatchFormMode
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.CalendarService
import com.example.amfootball.data.services.TeamService
import com.example.amfootball.navigation.objects.Arguments
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.utils.MatchConsts
import com.example.amfootball.utils.Patterns
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

//TODO: Terminar de implementar os metodos que faltam
/**
 * ViewModel responsável pela gestão do formulário de interações de partida (Convites e Gestão).
 *
 * Este ViewModel atua de forma polimórfica dependendo do [MatchFormMode] fornecido via navegação:
 * - **SEND:** Criação de um novo convite de jogo.
 * - **NEGOCIATE:** Negociação de um convite existente (contraproposta).
 * - **CANCEL:** Cancelamento de um jogo agendado.
 * - **POSTPONE:** Pedido de adiamento de um jogo.
 *
 * Gere o estado do formulário, validações de input e comunicação com os repositórios.
 *
 * @property savedStateHandle Manipulador de estado para recuperar argumentos de navegação (IDs e Modo).
 * @property calendarRepository Repositório para operações relacionadas com partidas e calendário.
 * @property teamRepository Repositório para obter dados das equipas (ex: Oponente).
 * @property networkObserver Observador de conectividade para garantir operações online.
 * @property sessionManager Gestor de sessão para obter o ID da equipa do utilizador logado.
 */
@HiltViewModel
class FormMatchInviteViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val calendarRepository: CalendarService,
    private val teamRepository: TeamService,
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager
) : ViewModel() {

    /** ID da partida, recuperado da navegação (pode ser nulo em modo de criação). */
    private val matchId: String? = savedStateHandle.get<String>(Arguments.MATCH_ID)

    /** ID do convite específico, usado em negociações. */
    private val matchInviteId: String? = savedStateHandle.get<String>(Arguments.MATCH_INVITE_ID)

    /** String crua do modo do formulário vinda da navegação. */
    private val modeStr = savedStateHandle.get<String>(Arguments.FORM_MODE)

    /** ID da equipa do utilizador atual. */
    private val idMyTeam = sessionManager.getUserProfile()?.idTeam ?: ""

    /**
     * O modo de operação atual do formulário ([MatchFormMode]).
     * Determinado a partir do argumento de navegação [modeStr].
     * Padrão: [MatchFormMode.SEND] se inválido ou nulo.
     */
    val mode: MatchFormMode = try {
        if (modeStr != null) {
            MatchFormMode.valueOf(modeStr)
        }
        else {
            MatchFormMode.SEND
        }
    } catch (e: Exception) {
        MatchFormMode.SEND
    }

    /** Estado atual dos dados do formulário (Data, Hora, Local, Oponente). */
    private val formState: MutableStateFlow<MatchInviteDto> = MutableStateFlow(MatchInviteDto())
    val uiFormState: StateFlow<MatchInviteDto> = formState

    /** Estado dos erros de validação dos campos do formulário. */
    private val errors: MutableStateFlow<MatchInviteFormErros> = MutableStateFlow(MatchInviteFormErros())
    val uiErrorsForm: StateFlow<MatchInviteFormErros> = errors

    /** Estado global da UI (Loading, Erros de Rede, Sucesso). */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(true))
    val uiState: StateFlow<UiState> = _uiState

    init {
        loadData()
    }

    // --- MÉTODOS DE UI (Setters) ---

    /**
     * Atualiza a data do jogo selecionada no formulário.
     * Converte milissegundos (do DatePicker) para String formatada.
     *
     * @param millis Data selecionada em milissegundos (Epoch).
     */
    fun onGameDateChange(millis: Long) {
        val newDate = convertMillisToDate(millis)
        val current = formState.value

        formState.value = current.copy(
            gameDateString = newDate
        )
    }

    /**
     * Atualiza a hora do jogo selecionada no formulário.
     *
     * @param newTime Nova hora em formato String (ex: "14:30").
     */
    fun onTimeGameChange(newTime: String) {
        val current = formState.value

        formState.value = current.copy(
            gameTimeString = newTime
        )
    }

    /**
     * Define se o jogo é "Em Casa" ou "Fora".
     *
     * @param isHome `true` se for em casa, `false` se for fora.
     */
    fun onLocalGameChange(isHome: Boolean) {
        formState.value = formState.value.copy(isHomeGame = isHome)
    }

    // --- MÉTODOS DE AÇÃO (Submit) ---
    /**
     * Carrega os dados iniciais do formulário com base no [mode].
     *
     * - **CANCEL/POSTPONE:** Carrega os dados da partida existente via API.
     * - **SEND:** Prepara um formulário vazio ou com dados do oponente pré-selecionado.
     */
    fun loadData() {
        when(modeStr) {
            MatchFormMode.NEGOCIATE.name -> {
                //TODO: Fazer o pedido ao endPoint de negociate
            }
            MatchFormMode.SEND.name -> {
                loadDataSend()
            }
            MatchFormMode.CANCEL.name -> {
                loadDataMatch()
            }
            MatchFormMode.POSTPONE.name -> {
                loadDataMatch()
            }
            else -> {
                _uiState.update { it.copy(isLoading = false, errorMessage = "Página invalida") }
            }
        }
    }
    /**
     * Submete o formulário principal (Criar, Negociar ou Adiar).
     *
     * Valida os campos obrigatórios e direciona para a lógica específica baseada no [mode].
     * Após sucesso, navega de volta para o Calendário.
     *
     * @param navHostController Controlador para navegação após sucesso.
     */
    fun onSubmitForm(navHostController: NavHostController) {
        if(!isFormValid()) {
            return
        }

        when(modeStr) {
            MatchFormMode.NEGOCIATE.name -> {
                //TODO: Fazer o pedido ao endPoint de negociate
            }
            MatchFormMode.SEND.name -> {
                //TODO: Mandar pedido há API para ir buscar apenas o nome do Opponente
            }
            MatchFormMode.POSTPONE.name -> {
                //TODO: Fazer o pedido ao endPoint de postponeMatch
            }
            else -> {
                //Lançar exceção
            }
        }

        navHostController.navigate("${Routes.TeamRoutes.CALENDAR.route}/$idMyTeam") {
            popUpTo(0)
        }
    }

    //TODO: Implementar lógica de cancelamento
    /**
     * Executa a ação de cancelamento de uma partida.
     *
     * Valida o motivo do cancelamento antes de proceder.
     *
     * @param navHostController Controlador para navegação.
     * @param cancelReason Texto com o motivo do cancelamento.
     */
    fun onCancelForm(navHostController: NavHostController, cancelReason: String) {
        _uiState.update { it.copy(isLoading = true) }
        if (!isCancelValid(cancelReason = cancelReason)) {
            _uiState.update { it.copy(isLoading = false) }
            return
        }

        viewModelScope.launch {
            try {
                if (!networkObserver.isOnlineOneShot()) {
                    _uiState.update {
                        it.copy(
                            isLoading = false,
                            errorMessage = "Sem internet. Verifique a sua conexão."
                        )
                    }
                    return@launch
                }

                Log.d("FormMatchInviteViewModel", "onCancelForm: $matchId")
                if(matchId == null) {
                    _uiState.update { it.copy(isLoading = false, errorMessage = "Não foi encontrada nenhuma equipa com esse Id") }
                    return@launch
                }

                calendarRepository.cancelMatch(
                    teamId = idMyTeam,
                    matchId = matchId,
                    description = cancelReason
                )

                _uiState.update { it.copy(isLoading = false) }
                val rota = "${Routes.TeamRoutes.CALENDAR.route}/$idMyTeam"

                navHostController.navigate(rota) {
                    popUpTo(Routes.TeamRoutes.HOMEPAGE.route) {
                        inclusive = false
                    }
                    launchSingleTop = true
                }
            } catch (e: Exception) {
                _uiState.update { it.copy(isLoading = false, errorMessage = e.message) }
            }
        }
    }

    // --- MÉTODOS PRIVADOS (Lógica Interna) ---

    //TODO: Implementar e arranjar forma de receber o id do oponnet
    private fun loadDataSend() {
    }

    /**
     * Carrega os detalhes de uma partida existente a partir da API.
     * Utilizado para preencher o formulário em modos de edição (Cancel/Postpone).
     *
     * Converte o resultado da API num DTO de UI utilizando [MatchInviteDto.createFromBackend].
     */
    private fun loadDataMatch() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }

            if (!networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }

                return@launch
            }

            try {
                if(matchId == null) {
                    _uiState.update { it.copy(isLoading = false, errorMessage = "Não foi encontrada nenhuma equipa com esse Id") }
                    return@launch
                }

                val rawMatch = calendarRepository.getMatchTeam(teamId = idMyTeam, matchId = matchId)
                val processedMatch = MatchInviteDto.createFromBackend(rawMatch)

                formState.value = processedMatch
                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                _uiState.update { it.copy(isLoading = false, errorMessage = e.message) }
            }
        }
    }

    //TODO: Talvez meter regras de negocio de tempo aqui
    /**
     * Valida os campos do formulário principal (Data e Hora).
     *
     * @return `true` se todos os campos obrigatórios estiverem preenchidos.
     */
    private fun isFormValid(): Boolean {
        val dateGame = formState.value.gameDateString
        val timeGame = formState.value.gameTimeString

        var errorDateGame: ErrorMessage? = null
        var errorTime: ErrorMessage? = null

        if (dateGame.isNullOrBlank()) {
            errorDateGame = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        }

        if (timeGame.isNullOrBlank()) {
            errorTime = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        }

        errors.value = MatchInviteFormErros(
            dateError = errorDateGame,
            timeError = errorTime
        )

        val isValid = listOf(errorDateGame, errorTime).all {
            it == null
        }

        return isValid
    }

    /**
     * Valida o motivo de cancelamento.
     *
     * Verifica se o motivo não está vazio e se respeita os limites de caracteres
     * definidos em [MatchConsts].
     *
     * @param cancelReason O texto do motivo.
     * @return `true` se o motivo for válido.
     */
    private fun isCancelValid(cancelReason: String?): Boolean {
        var errorReason: ErrorMessage? = null

        if (cancelReason.isNullOrBlank()) {
            errorReason = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        } else {
            val cancelReasonLength = cancelReason.length
            if (cancelReasonLength < MatchConsts.MIN_CANCEL_REASON_LENGTH) {
                errorReason = ErrorMessage(
                    messageId = R.string.error_min_length_cancel_match_reason,
                    args = listOf(MatchConsts.MIN_CANCEL_REASON_LENGTH)
                )
            } else if (cancelReasonLength > MatchConsts.MAX_CANCEL_REASON_LENGTH) {
                errorReason = ErrorMessage(
                    messageId = R.string.error_max_length_cancel_match_reason,
                    args = listOf(MatchConsts.MAX_CANCEL_REASON_LENGTH)
                )
            }
        }

        errors.value = MatchInviteFormErros(
            cancelReasonError = errorReason,
        )

        return errorReason == null
    }

    /**
     * Utilitário para converter milissegundos em String de data (dd/MM/yyyy).
     * Utiliza o fuso horário padrão do sistema.
     */
    private fun convertMillisToDate(millis: Long): String {
        val formatter = DateTimeFormatter.ofPattern(Patterns.DATE)
        return Instant.ofEpochMilli(millis)
            .atZone(ZoneId.systemDefault()) // Usa o fuso horário do sistema.
            .toLocalDate()
            .format(formatter)
    }
}
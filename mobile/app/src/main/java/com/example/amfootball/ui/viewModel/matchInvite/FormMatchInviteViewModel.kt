package com.example.amfootball.ui.viewModel.matchInvite

import android.util.Log
import androidx.lifecycle.SavedStateHandle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.core.extensions.toLocalDate
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.MatchConsts
import com.example.amfootball.data.NetworkConnectivityObserver
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.manager.CalendarManager
import com.example.amfootball.data.remote.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.remote.dtos.matchInivite.SendMatchInviteDto
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.data.remote.services.CalendarService
import com.example.amfootball.data.remote.services.MatchInviteService
import com.example.amfootball.data.remote.services.TeamService
import com.example.amfootball.domains.enums.pages.MatchFormMode
import com.example.amfootball.domains.errors.ErrorMessage
import com.example.amfootball.domains.errors.formErrors.MatchInviteFormErros
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.FormsViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão do formulário de interações de partida (Convites e Gestão).
 *
 * Este ViewModel atua de forma polimórfica dependendo do [MatchFormMode] fornecido via navegação:
 * - **SEND:** Criação de um novo convite de jogo.
 * - **NEGOCIATE:** Negociação de um convite existente (contraproposta).
 * - **CANCEL:** Cancelamento de um jogo agendado (requer motivo).
 * - **POSTPONE:** Pedido de adiamento de um jogo.
 *
 * Gere o estado do formulário, validações de input e comunicação com os repositórios.
 *
 * @property savedStateHandle Manipulador de estado para recuperar argumentos de navegação (IDs e Modo).
 * @property calendarRepository Repositório para operações relacionadas com partidas e calendário (cancelar/adiar).
 * @property teamRepository Repositório para obter dados das equipas (ex: Oponente).
 * @property matchInviteRepository Repositório para operações de convite de jogo.
 * @property networkObserver Observador de conectividade para garantir operações online.
 * @property sessionManager Gestor de sessão para obter o ID da equipa do utilizador logado.
 * @property calendarManager Gestor de calendário local para remover eventos no cancelamento.
 */
@HiltViewModel
class FormMatchInviteViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val calendarRepository: CalendarService,
    private val teamRepository: TeamService,
    private val matchInviteRepository: MatchInviteService,
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager,
    private val calendarManager: CalendarManager
) : FormsViewModel<MatchInviteDto, MatchInviteFormErros>(
    initialData = MatchInviteDto(),
    initialError = MatchInviteFormErros(),
    networkObserver = networkObserver
) {
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
        } else {
            MatchFormMode.SEND
        }
    } catch (e: Exception) {
        MatchFormMode.SEND
    }

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
        val date = millis.toLocalDate().toString()

        formState.value = formState.value.copy(
            gameDateString = date,
            gameDateRaw = date
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
     * Carrega os dados iniciais do formulário com base no [mode] de operação.
     *
     * - **SEND:** Carrega o ID e Nome do oponente a partir dos argumentos de navegação.
     * - **NEGOCIATE:** Carrega os detalhes do convite de jogo existente.
     * - **CANCEL/POSTPONE:** Carrega os detalhes de uma partida já agendada.
     */
    fun loadData() {
        when (modeStr) {
            MatchFormMode.NEGOCIATE.name -> {
                loadDataNegociate()
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
                stopLoading()
                updateToast(message = R.string.toast_invalid_page)
            }
        }
    }

    /**
     * Submete o formulário principal (Criar, Negociar ou Adiar).
     *
     * 1. Valida os campos obrigatórios via [isFormValid].
     * 2. Direciona para a lógica de submissão específica ([sendMatchInvite], [negotiateMatchInvite], [postponeMatch]).
     * 3. Após sucesso, navega de volta para o Calendário.
     *
     * @param navHostController Controlador para navegação após sucesso.
     */
    fun onSubmitForm(navHostController: NavHostController) {
        if (!isFormValid()) {
            Log.d("FormMatchInviteViewModel", "onSubmitForm: FormInvalido")
            return
        }

        when (modeStr) {
            MatchFormMode.NEGOCIATE.name -> {
                negotiateMatchInvite(navHostController)
            }

            MatchFormMode.SEND.name -> {
                sendMatchInvite(navHostController)
                Log.d("FormMatchInviteViewModel", "onSubmitForm: Entrou no send")

            }

            MatchFormMode.POSTPONE.name -> {
                postponeMatch(navHostController)
            }

            else -> {
                updateToast(message = R.string.toast_invalid_operation)
                return
            }
        }

        navHostController.navigate("${Routes.TeamRoutes.CALENDAR.route}/$idMyTeam") {
            popUpTo(0)
        }
    }

    /**
     * Submete um pedido de adiamento de jogo ([MatchFormMode.POSTPONE]).
     */
    private fun postponeMatch(navHostController: NavHostController) {
        submitForm(
            apiCall = {
                val opponentId = formState.value.opponent.id
                val gameDate = formState.value.gameDateRaw
                val matchInvite = getSendMatchInviteDto(opponentId, gameDate)
                matchInvite.idMatch = matchId

                calendarRepository.postPoneMatch(idMyTeam, matchInvite)
            },
            onSuccess = {
                navHostController.navigate("${Routes.TeamRoutes.CALENDAR.route}/$idMyTeam") {
                    popUpTo(Routes.TeamRoutes.HOMEPAGE.route) {
                        inclusive = false
                    }
                    launchSingleTop = true
                }
            }
        )
    }

    /**
     * Submete uma contraproposta de jogo ([MatchFormMode.NEGOCIATE]).
     */
    private fun negotiateMatchInvite(navHostController: NavHostController) {
        submitForm(
            apiCall = {
                val opponentId = formState.value.opponent.id
                val gameDate = formState.value.gameDateRaw
                val matchInvite = getSendMatchInviteDto(opponentId, gameDate)

                matchInviteRepository.negociateMatchInvite(idMyTeam, matchInvite)
            },
            onSuccess = {
                navHostController.navigate("${Routes.TeamRoutes.CALENDAR.route}/$idMyTeam") {
                    popUpTo(Routes.TeamRoutes.HOMEPAGE.route) {
                        inclusive = false
                    }
                    launchSingleTop = true
                }
            }
        )
    }

    /**
     * Envia um novo convite de jogo ([MatchFormMode.SEND]).
     */
    private fun sendMatchInvite(navHostController: NavHostController) {
        submitForm(
            apiCall = {
                val opponentId = formState.value.opponent.id
                val gameDate = formState.value.gameDateRaw
                val matchInvite = getSendMatchInviteDto(opponentId, gameDate)

                matchInviteRepository.sendMatchInvite(idMyTeam, matchInvite)
            },
            onSuccess = {
                navHostController.navigate("${Routes.TeamRoutes.CALENDAR.route}/$idMyTeam") {
                    popUpTo(Routes.TeamRoutes.HOMEPAGE.route) {
                        inclusive = false
                    }
                    launchSingleTop = true
                }
            }
        )
    }

    /**
     * Executa a ação de cancelamento de uma partida.
     *
     * Valida o motivo do cancelamento antes de proceder.
     *
     * @param navHostController Controlador para navegação.
     * @param cancelReason Texto com o motivo do cancelamento.
     */
    fun onCancelForm(navHostController: NavHostController, cancelReason: String) {
        if (!isCancelValid(cancelReason)) {
            return
        }

        submitForm(
            apiCall = {
                if (matchId != null) {
                    calendarRepository.cancelMatch(
                        teamId = idMyTeam,
                        matchId = matchId,
                        description = cancelReason
                    )

                    calendarManager.removeMatch(matchId)
                }

            },
            onSuccess = {
                navHostController.navigate("${Routes.TeamRoutes.CALENDAR.route}/$idMyTeam") {
                    popUpTo(Routes.TeamRoutes.HOMEPAGE.route) {
                        inclusive = false
                    }
                    launchSingleTop = true
                }
            }
        )
    }

    // --- MÉTODOS PRIVADOS (Lógica Interna) ---

    /**
     * Prepara o DTO de formulário para o modo SEND, preenchendo o oponente a partir dos argumentos.
     */
    private fun loadDataSend() {
        val opponentTeamId = savedStateHandle.get<String>(Arguments.TEAM_ID)
        val opponentTeamName = savedStateHandle.get<String>(Arguments.TEAM_NAME)

        if (opponentTeamName != null && opponentTeamId != null) {
            val teamDto = TeamDto(id = opponentTeamId, opponentTeamName)
            formState.value = formState.value.copy(opponent = teamDto)
        }

        stopLoading()
    }

    /**
     * Carrega os dados de um convite de jogo existente para o modo NEGOCIATE.
     */
    private fun loadDataNegociate() {
        launchDataLoad {
            if (matchInviteId != null) {
                val rawMatch =
                    matchInviteRepository.getInviteMatch(
                        teamId = idMyTeam,
                        matchInviteId = matchInviteId
                    )
                val processedMatch = MatchInviteDto.createFromBackend(rawMatch)

                formState.value = processedMatch
            }
        }
    }

    /**
     * Carrega os detalhes de uma partida existente a partir da API.
     * Utilizado para preencher o formulário em modos de edição (Cancel/Postpone).
     */
    private fun loadDataMatch() {
        launchDataLoad {
            if (matchId != null) {
                val rawMatch = calendarRepository.getMatchTeam(teamId = idMyTeam, matchId = matchId)
                val processedMatch = MatchInviteDto.createFromBackend(rawMatch)

                formState.value = processedMatch
            }
        }
    }

    /**
     * Converte o estado atual do formulário em [SendMatchInviteDto] para a API.
     *
     * @param opponentId ID do oponente.
     * @param gameDate Data do jogo no formato `yyyy-MM-dd`.
     * @return O DTO pronto para a API.
     */
    private fun getSendMatchInviteDto(opponentId: String?, gameDate: String?): SendMatchInviteDto {
        val fullDateTime = "${gameDate}T${formState.value.gameTimeString}:00"
        val matchInv = SendMatchInviteDto(
            idSender = idMyTeam,
            idReceiver = opponentId!!,
            gameDate = fullDateTime,
            homePitch = formState.value.isHomeGame
        )

        return matchInv
    }

    override fun validateForm(): Boolean {
        var isValideForm = true

        if (modeStr != MatchFormMode.SEND.name) {
            isValideForm = isFormValid()
        }

        return isValideForm
    }

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

        if (dateGame.isBlank()) {
            errorDateGame = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        }

        if (timeGame.isNullOrBlank()) {
            errorTime = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        }

        formErrors.value = MatchInviteFormErros(
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

        formErrors.value = MatchInviteFormErros(
            cancelReasonError = errorReason,
        )

        return errorReason == null
    }
}
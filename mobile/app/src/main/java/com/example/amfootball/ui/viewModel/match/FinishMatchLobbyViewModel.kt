package com.example.amfootball.ui.viewModel.match

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.data.events.FinishMatchUiState
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.match.ResultMatchDto
import com.example.amfootball.data.remote.sockets.FinishMatchSockerServer
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão do estado e lógica da sala de espera (Lobby)
 * de finalização de partida.
 *
 * Utiliza o SignalR ([FinishMatchSockerServer]) para coordenar a confirmação do resultado
 * reportado com o administrador da equipa adversária.
 *
 * @property socketService O serviço SignalR dedicado ao Hub de Finalização de Partida.
 * @property sessionManager Gerenciador de sessão para obter o ID da equipa do utilizador.
 * @property savedStateHandle Manipulador do estado guardado, usado para ler argumentos de navegação.
 */
@HiltViewModel
class FinishMatchLobbyViewModel @Inject constructor(
    private val socketService: FinishMatchSockerServer,
    private val sessionManager: SessionManager,
    savedStateHandle: SavedStateHandle
): ViewModel() {

    /**
     * ID da partida a ser iniciada. Recuperado dos argumentos de navegação.
     */
    val matchId: String = savedStateHandle.get<String>(Arguments.MATCH_ID) ?: ""

    /**
     * ID da equipa adversária. Recuperado dos argumentos de navegação.
     */
    val opponentId: String = savedStateHandle.get<String>(Arguments.OPPONENT_ID) ?: ""

    /**
     * Número de golos da equipa do utilizador, passados via argumento.
     */
    private val myGoals: Int = savedStateHandle.get<Int>(Arguments.MY_GOALS) ?: 0

    /**
     * Número de golos da equipa adversária, passados via argumento.
     */
    private val opGoals: Int = savedStateHandle.get<Int>(Arguments.OPPONENT_GOALS) ?: 0

    /**
     * ID da equipa do utilizador atual, obtido através do [SessionManager].
     */
    private val teamId = sessionManager.fetchTeamId()

    /**
     * Estado interno mutável da UI. Inicia em [FinishMatchUiState.Loading].
     */
    private val _uiState = MutableStateFlow<FinishMatchUiState>(FinishMatchUiState.Loading)

    /**
     * Fluxo público e imutável do estado da UI, observado pela View (Compose/Fragment).
     */
    val uiState = _uiState.asStateFlow()

    /**
     * Bloco de inicialização.
     * Conecta ao socket e começa a escutar eventos assim que o ViewModel é criado.
     */
    init {
        connectAndJoin()
        observeSocketEvents()
    }

    /**
     * Inicia o processo de conexão e entrada na sala do jogo.
     *
     * 1. Valida se [matchId] e [teamId] são válidos.
     * 2. Atualiza o estado para [FinishMatchUiState.Connecting].
     * 3. Tenta estabelecer a conexão via socket.
     * 4. Se conectado, chama `joinStartMatch` e muda o estado para [FinishMatchUiState.WaitingForOpponent].
     * 5. Caso contrário, define o estado como [FinishMatchUiState.Error].
     */
    private fun connectAndJoin() {
        if (matchId.isBlank() || teamId.isBlank()) {
            _uiState.value = FinishMatchUiState.Error("Dados inválidos para iniciar jogo.")
            return
        }

        viewModelScope.launch {
            _uiState.value = FinishMatchUiState.Connecting
            val connected = socketService.startConnection()

            if (connected) {
                val result = ResultMatchDto(
                    idMatch = matchId,
                    idTeam = teamId,
                    idOpponent = opponentId,
                    numGoalsTeam = myGoals,
                    numGoalsOpponent = opGoals
                )
                socketService.joinFinishMatch(dto = result)
                _uiState.value = FinishMatchUiState.WaitingForConfirmation
            } else {
                _uiState.value = FinishMatchUiState.Error("Não foi possível conectar ao servidor.")
            }
        }
    }

    /**
     * Observa os eventos emitidos pelo socket (ex: confirmação de finalização).
     *
     * Atualiza o estado da UI para [FinishMatchUiState.MatchFinished] quando
     * o servidor confirma que ambos os administradores concordaram com o resultado.
     */
    private fun observeSocketEvents() {
        viewModelScope.launch {
            socketService.finishEvents.collect { finished ->
                if (finished) {
                    _uiState.value = FinishMatchUiState.MatchFinished
                }
            }
        }
    }

    /**
     * Ação do utilizador para editar o resultado (caso perceba que se enganou).
     *
     * Fluxo:
     * 1. Invoca [socketService.leaveFinishMatch] para sair logicamente do Hub no servidor.
     * 2. Atualiza o estado para [FinishMatchUiState.Editing], o que deve instruir a View
     * a navegar de volta para o formulário de inserção de golos.
     */
    fun onEditResult() {
        viewModelScope.launch {
            socketService.leaveFinishMatch()

            _uiState.value = FinishMatchUiState.Editing
        }
    }

    /**
     * Chamado quando o ViewModel é destruído.
     * Garante que a conexão do socket é encerrada fisicamente ([socketService.stopConnection])
     * para evitar fugas de memória ou conexões ativas em segundo plano.
     */
    override fun onCleared() {
        super.onCleared()
        socketService.stopConnection()
    }
}
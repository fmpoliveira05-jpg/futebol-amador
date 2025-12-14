package com.example.amfootball.ui.viewModel.match

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.SignalRMessages
import com.example.amfootball.data.events.StartMatchUiState
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.sockets.StartMatchSocketService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica de negócio do ecrã de "Lobby" de Início de Partida.
 *
 * Este ViewModel gerencia a sincronização em tempo real entre dois utilizadores (administradores de equipa)
 * que estão prestes a iniciar um jogo. Utiliza [StartMatchSocketService] para estabelecer
 * uma conexão via Socket (SignalR).
 *
 * **Fluxo de Estados ([StartMatchUiState]):**
 * 1. **Loading**: Estado inicial ao abrir o ecrã.
 * 2. **Connecting**: Tenta estabelecer a conexão com o hub do SignalR.
 * 3. **WaitingForOpponent**: Conexão estabelecida e utilizador inserido na "sala" do jogo. Aguarda o outro utilizador.
 * 4. **MatchStarted**: Sinal recebido de que ambos estão prontos e o jogo começou.
 * 5. **Error**: Falha na validação de dados ou na conexão.
 *
 * @property socketService Serviço responsável pela comunicação via Socket (SignalR).
 * @property sessionManager Gestor de sessão para obter o ID da equipa atual.
 * @property savedStateHandle Manipulador de estado para recuperar os argumentos de navegação (IDs da partida e oponente).
 */
@HiltViewModel
class StartMatchViewModel @Inject constructor(
    private val socketService: StartMatchSocketService,
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
     * ID da equipa do utilizador atual, obtido através do [SessionManager].
     */
    private val teamId = sessionManager.fetchTeamId()

    /**
     * Estado interno mutável da UI.
     */
    private val _uiState = MutableStateFlow<StartMatchUiState>(StartMatchUiState.Loading)

    /**
     * Fluxo público e imutável do estado da UI, observado pela View (Compose).
     */
    val uiState = _uiState.asStateFlow()

    init {
        connectAndJoin()
        observeSocketEvents()
    }

    /**
     * Inicia o processo de conexão e entrada na sala do jogo.
     *
     * 1. Valida se [matchId] e [teamId] são válidos.
     * 2. Atualiza o estado para [StartMatchUiState.Connecting].
     * 3. Tenta estabelecer a conexão via socket.
     * 4. Se conectado, chama `joinStartMatch` e muda o estado para [StartMatchUiState.WaitingForOpponent].
     * 5. Caso contrário, define o estado como [StartMatchUiState.Error].
     */
    private fun connectAndJoin() {
        if (matchId.isBlank() || teamId.isBlank()) {
            _uiState.value = StartMatchUiState.Error("Dados inválidos para iniciar jogo.")
            return
        }

        viewModelScope.launch {
            _uiState.value = StartMatchUiState.Connecting

            val connected = socketService.startConnection()

            if (connected) {
                socketService.joinStartMatch(matchId, teamId)
                _uiState.value = StartMatchUiState.WaitingForOpponent
            } else {
                _uiState.value = StartMatchUiState.Error("Não foi possível conectar ao servidor.")
            }
        }
    }

    /**
     * Observa os eventos recebidos através do socket.
     *
     * Escuta especificamente mensagens que contenham [SignalRMessages.MATCH_STARTED].
     * Ao receber este sinal, atualiza o estado para [StartMatchUiState.MatchStarted],
     * o que deve desencadear a navegação para o ecrã de jogo na UI.
     */
    private fun observeSocketEvents() {
        viewModelScope.launch {
            socketService.matchEvents.collect { message ->
                if (message.contains(SignalRMessages.MATCH_STARTED, ignoreCase = true)) {
                    _uiState.value = StartMatchUiState.MatchStarted
                }
            }
        }
    }

    /**
     * Ação do utilizador para cancelar a espera.
     *
     * 1. Notifica o servidor para sair da sala ([socketService.leaveStartMatch]).
     * 2. Encerra a conexão do socket.
     * 3. Atualiza o estado para [StartMatchUiState.Cancelled], fazendo a UI voltar atrás.
     */
    fun onCancelWaiting() {
        viewModelScope.launch {
            socketService.leaveStartMatch()
            socketService.stopConnection()
            _uiState.value = StartMatchUiState.Cancelled
        }
    }

    /**
     * Chamado quando o ViewModel é destruído.
     * Garante que a conexão do socket é encerrada para evitar fugas de memória ou conexões fantasma.
     */
    override fun onCleared() {
        super.onCleared()
        socketService.stopConnection()
    }
}
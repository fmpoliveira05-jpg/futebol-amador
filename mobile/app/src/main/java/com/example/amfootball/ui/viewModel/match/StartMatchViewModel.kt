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

@HiltViewModel
class StartMatchViewModel @Inject constructor(
    private val socketService: StartMatchSocketService,
    private val sessionManager: SessionManager,
    savedStateHandle: SavedStateHandle
): ViewModel() {

    val matchId: String = savedStateHandle.get<String>(Arguments.MATCH_ID) ?: ""
    private val teamId = sessionManager.fetchTeamId()

    private val _uiState = MutableStateFlow<StartMatchUiState>(StartMatchUiState.Loading)
    val uiState = _uiState.asStateFlow()

    init {
        connectAndJoin()
        observeSocketEvents()
    }

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

    private fun observeSocketEvents() {
        viewModelScope.launch {
            socketService.matchEvents.collect { message ->
                if (message.contains(SignalRMessages.MATCH_STARTED, ignoreCase = true)) {
                    _uiState.value = StartMatchUiState.MatchStarted
                }
            }
        }
    }

    fun onCancelWaiting() {
        viewModelScope.launch {
            socketService.leaveStartMatch()
            socketService.stopConnection()
            _uiState.value = StartMatchUiState.Cancelled
        }
    }

    override fun onCleared() {
        super.onCleared()
        socketService.stopConnection()
    }
}
package com.example.amfootball.ui.screens.match

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.core.utils.SignalRUrls
import com.example.amfootball.data.events.StartMatchUiState
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.match.StartMatchViewModel

/**
 * Ecrã Principal do Lobby de Início de Partida (Stateful Screen).
 *
 * Responsável por gerir o ciclo de vida do processo de inicialização de um jogo.
 * Observa o estado do [StartMatchViewModel] e reage a eventos críticos, como o início
 * do jogo (navegação para FinishMatch) ou cancelamento.
 *
 * @param navHostController Controlador de navegação para gerir transições entre ecrãs.
 * @param viewModel ViewModel injetado via Hilt que contém a lógica de negócio e o estado do socket.
 */
@Composable
fun StartMatchLobbyScreen(
    navHostController: NavHostController,
    viewModel: StartMatchViewModel = hiltViewModel()
) {
    val state by viewModel.uiState.collectAsStateWithLifecycle()
    LaunchedEffect(state) {
        if (state is StartMatchUiState.MatchStarted) {
            val matchId = viewModel.matchId
            val opponentId = viewModel.opponentId

            navHostController.navigate("${Routes.TeamRoutes.FINISH_MATCH.route}/$matchId/$opponentId") {
                popUpTo("${SignalRUrls.START_MATCH_URL}/$matchId/$opponentId") { inclusive = true }
            }
        } else if (state is StartMatchUiState.Cancelled) {
            navHostController.popBackStack()
        }
    }

    StartMatchLobbyContent(
        state = state,
        onCancelWaiting = viewModel::onCancelWaiting,
        onBack = { navHostController.popBackStack() }
    )
}

/**
 * Orquestrador de conteúdo visual (Stateless Content).
 *
 * Decide qual componente visual exibir (Loading, Waiting, Error) com base no estado atual da UI.
 *
 * @param state O estado atual da interface ([StartMatchUiState]).
 * @param onCancelWaiting Callback acionado quando o utilizador cancela a espera.
 * @param onBack Callback acionado para voltar ao ecrã anterior em caso de erro.
 */
@Composable
private fun StartMatchLobbyContent(
    state: StartMatchUiState,
    onCancelWaiting: () -> Unit,
    onBack: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        when (state) {
            StartMatchUiState.Connecting -> {
                StartMatchLoadingView()
            }
            StartMatchUiState.WaitingForOpponent -> {
                StartMatchWaitingView(onCancel = onCancelWaiting)
            }
            is StartMatchUiState.Error -> {
                StartMatchErrorView(
                    msg = state.msg,
                    onBack = onBack
                )
            }
            else -> {}
        }
    }
}

/**
 * Vista atómica para o estado de carregamento/conexão.
 *
 * Exibe um indicador circular e uma mensagem de "A conectar".
 */
@Composable
private fun StartMatchLoadingView() {
    CircularProgressIndicator()
    Text(stringResource(id = R.string.lobby_connecting))
}

/**
 * Vista atómica para o estado de espera pelo oponente.
 *
 * Exibe informações de espera e fornece um botão destrutivo (vermelho) para cancelar a operação.
 *
 * @param onCancel Callback executado ao clicar no botão de cancelar.
 */
@Composable
private fun StartMatchWaitingView(onCancel: () -> Unit) {
    CircularProgressIndicator()
    Spacer(Modifier.height(16.dp))

    Text(
        text = stringResource(id = R.string.lobby_waiting_title),
        style = MaterialTheme.typography.titleLarge
    )
    Text(text = stringResource(id = R.string.lobby_waiting_body))

    Spacer(Modifier.height(32.dp))

    Button(
        onClick = onCancel,
        colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
    ) {
        Text(stringResource(id = R.string.lobby_btn_cancel))
    }
}

/**
 * Vista atómica para a exibição de erros.
 *
 * Apresenta a mensagem de falha e um botão padrão para retroceder.
 *
 * @param msg A mensagem de erro a ser exibida.
 * @param onBack Callback executado ao clicar no botão de voltar.
 */
@Composable
private fun StartMatchErrorView(
    msg: String,
    onBack: () -> Unit
) {
    Text(
        text = stringResource(id = R.string.lobby_error_prefix, msg),
        color = MaterialTheme.colorScheme.error
    )

    Spacer(Modifier.height(16.dp))

    Button(onClick = onBack) {
        Text(stringResource(id = R.string.btn_back))
    }
}

@Preview(name = "1. Connecting - PT", group = "Lobby States", locale = "pt-rPT", showBackground = true)
@Preview(name = "1. Connecting - EN", group = "Lobby States", locale = "en", showBackground = true)
@Composable
fun PreviewConnectingEN() {
    AMFootballTheme {
        StartMatchLobbyContent(
            state = StartMatchUiState.Connecting,
            onCancelWaiting = {},
            onBack = {}
        )
    }
}

@Preview(name = "2. Waiting - PT", group = "Lobby States", locale = "pt-rPT", showBackground = true)
@Preview(name = "2. Waiting - EN", group = "Lobby States", locale = "en", showBackground = true)
@Composable
fun PreviewWaitingPT() {
    AMFootballTheme {
        StartMatchLobbyContent(
            state = StartMatchUiState.WaitingForOpponent,
            onCancelWaiting = {},
            onBack = {}
        )
    }
}

@Preview(name = "3. Error - PT", group = "Lobby States", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewErrorPT() {
    AMFootballTheme {
        StartMatchLobbyContent(
            state = StartMatchUiState.Error(msg = "Falha de conexão com o servidor do jogo."),
            onCancelWaiting = {},
            onBack = {}
        )
    }
}

@Preview(name = "3. Error - EN", group = "Lobby States", locale = "en", showBackground = true)
@Composable
fun PreviewErrorEN() {
    AMFootballTheme() {
        StartMatchLobbyContent(
            state = StartMatchUiState.Error(msg = "Failed to connect to the game server."),
            onCancelWaiting = {},
            onBack = {}
        )
    }
}
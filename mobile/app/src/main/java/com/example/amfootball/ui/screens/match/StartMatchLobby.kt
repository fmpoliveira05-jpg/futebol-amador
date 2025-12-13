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
import com.example.amfootball.data.events.StartMatchUiState
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.match.StartMatchViewModel

/**
 * Ecrã do Lobby de Início de Partida (Stateful Screen).
 *
 * Responsável por gerir o ciclo de vida do processo de inicialização de um jogo,
 * observando o estado do [StartMatchViewModel] e reagindo a eventos críticos (Iniciado ou Cancelado).
 *
 * @param navHostController Controlador de navegação para transições automáticas.
 * @param viewModel ViewModel injetado via Hilt que gere a sincronização da partida.
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

            navHostController.navigate("${Routes.TeamRoutes.FINISH_MATCH.route}/${matchId}") {
                popUpTo("${Routes.TeamRoutes.START_MATCH.route}/${matchId}") { inclusive = true }
            }
        } else if (state is StartMatchUiState.Cancelled) {
            navHostController.popBackStack()
        }
    }

    StartMatchLobbyContent(
        state = state,
        onCancelWaiting = viewModel::onCancelWaiting,
        navHostController = navHostController
    )
}

/**
 * Conteúdo visual do Lobby de Início de Partida (Stateless Content).
 *
 * Exibe diferentes indicadores e botões com base no estado atual ([StartMatchUiState]).
 *
 * @param state O estado atual da UI.
 * @param onCancelWaiting Callback para o botão de cancelamento (disponível em [StartMatchUiState.WaitingForOpponent]).
 * @param navHostController Controlador de navegação (usado para o botão 'Voltar' em caso de erro).
 */
@Composable
private fun StartMatchLobbyContent(
    state: StartMatchUiState,
    onCancelWaiting: () -> Unit,
    navHostController: NavHostController
) {
    Column(
        modifier = Modifier.fillMaxSize().padding(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        when (state) {
            StartMatchUiState.Connecting -> {
                CircularProgressIndicator()
                Text(stringResource(id = R.string.lobby_connecting))
            }
            StartMatchUiState.WaitingForOpponent -> {
                CircularProgressIndicator()
                Spacer(Modifier.height(16.dp))

                Text(
                    text = stringResource(id = R.string.lobby_waiting_title),
                    style = MaterialTheme.typography.titleLarge
                )
                Text(text = stringResource(id = R.string.lobby_waiting_body))

                Spacer(Modifier.height(32.dp))
                Button(
                    onClick = { onCancelWaiting() },
                    colors = ButtonDefaults.buttonColors(containerColor = MaterialTheme.colorScheme.error)
                ) {
                    Text(stringResource(id = R.string.lobby_btn_cancel))
                }
            }
            is StartMatchUiState.Error -> {
                Text(
                    text = stringResource(id = R.string.lobby_error_prefix,
                        (state as StartMatchUiState.Error).msg
                    ),
                    color = MaterialTheme.colorScheme.error
                )

                Spacer(Modifier.height(16.dp))

                Button(
                    onClick = {
                        navHostController.popBackStack()
                    }
                ) {
                    Text(stringResource(id = R.string.btn_back))
                }
            }
            else -> {}
        }
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
            navHostController = rememberNavController()
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
            navHostController = rememberNavController()
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
            navHostController = rememberNavController()
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
            navHostController = rememberNavController()
        )
    }
}
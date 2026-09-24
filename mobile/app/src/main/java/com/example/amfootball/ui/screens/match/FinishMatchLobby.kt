package com.example.amfootball.ui.screens.match

import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.data.events.FinishMatchUiState
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.match.FinishMatchLobbyViewModel
import com.example.amfootball.R
import com.example.amfootball.ui.theme.AMFootballTheme

/**
 * Ecrã de Lobby de Finalização de Partida (Stateful Screen).
 *
 * Esta tela serve como um estado intermediário após o utilizador reportar um resultado.
 * Aguarda a confirmação (via SignalR) do resultado por parte do adversário ou a finalização
 * pelo servidor.
 *
 * **Responsabilidades:**
 * 1. Coletar o estado da UI ([FinishMatchUiState]) do [FinishMatchLobbyViewModel].
 * 2. Gerir a navegação: se o estado for [FinishMatchUiState.MatchFinished], redireciona
 * para o Calendário.
 * 3. Delega a renderização do estado visual para [FinishMatchLobbyContent].
 *
 * @param navHostController Controlador de navegação para gerir transições.
 * @param viewModel ViewModel injetado via Hilt que gere a lógica do lobby.
 */
@Composable
fun FinishMatchLobbyScreen(
    navHostController: NavHostController,
    viewModel: FinishMatchLobbyViewModel = hiltViewModel()
) {
    val state by viewModel.uiState.collectAsStateWithLifecycle()

    LaunchedEffect(state) {
        if (state is FinishMatchUiState.MatchFinished) {
            navHostController.navigate(Routes.TeamRoutes.CALENDAR.route) {
                popUpTo(Routes.TeamRoutes.FINISH_MATCH.route) { inclusive = true }
            }
        }
    }

    FinishMatchLobbyContent(
        state = state,
        onEditResult = viewModel::onEditResult,
        onBack = { navHostController.popBackStack() }
    )
}

/**
 * Conteúdo visual do Lobby de Finalização de Partida (Stateless Component).
 *
 * Renderiza o ecrã apropriado baseado no [FinishMatchUiState] atual.
 *
 * @param state O estado atual da UI (Conectando, Esperando, Erro).
 * @param onEditResult Callback acionado quando o utilizador clica em editar o resultado.
 * @param onBack Callback acionado para voltar ao ecrã anterior em caso de erro.
 */
@Composable
private fun FinishMatchLobbyContent(
    state: FinishMatchUiState,
    onEditResult: () -> Unit,
    onBack: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        when (state) {
            FinishMatchUiState.Connecting, FinishMatchUiState.Loading -> {
                LoadingView()
            }
            FinishMatchUiState.WaitingForConfirmation -> {
                WaitingView(onEditResult = onEditResult)
            }
            is FinishMatchUiState.Error -> {
                ErrorView(
                    message = state.msg,
                    onBack = onBack
                )
            }
            else -> {}
        }
    }
}

/**
 * Componente que exibe um indicador de progresso e uma mensagem de carregamento/conexão.
 */
@Composable
private fun LoadingView() {
    CircularProgressIndicator()
    Spacer(Modifier.height(16.dp))
    Text(stringResource(id = R.string.lobby_connecting))
}

/**
 * Componente que exibe o estado de espera após o resultado ter sido submetido.
 *
 * Inclui uma opção para o utilizador editar o resultado caso tenha cometido um erro.
 *
 * @param onEditResult Callback para iniciar a edição do resultado.
 */
@Composable
private fun WaitingView(onEditResult: () -> Unit) {
    CircularProgressIndicator()
    Spacer(Modifier.height(24.dp))

    Text(
        text = stringResource(id = R.string.finish_match_submitted_title),
        style = MaterialTheme.typography.headlineSmall,
        color = MaterialTheme.colorScheme.primary,
        textAlign = TextAlign.Center
    )

    Spacer(Modifier.height(8.dp))

    Text(
        text = stringResource(id = R.string.finish_match_waiting_body),
        style = MaterialTheme.typography.bodyLarge,
        textAlign = TextAlign.Center
    )

    Spacer(Modifier.height(48.dp))

    OutlinedButton(
        onClick = onEditResult,
        modifier = Modifier.fillMaxWidth(0.8f)
    ) {
        Icon(
            imageVector = Icons.Default.Edit,
            contentDescription = null,
            modifier = Modifier.size(18.dp)
        )
        Spacer(Modifier.width(8.dp))
        Text(stringResource(id = R.string.btn_edit_result))
    }
}

/**
 * Componente que exibe uma mensagem de erro e um botão para voltar.
 *
 * @param message A mensagem de erro específica.
 * @param onBack Callback para voltar ao ecrã anterior.
 */
@Composable
private fun ErrorView(message: String, onBack: () -> Unit) {
    Text(
        text = stringResource(id = R.string.lobby_error_prefix, message),
        color = MaterialTheme.colorScheme.error,
        textAlign = TextAlign.Center
    )
    Spacer(Modifier.height(16.dp))
    Button(onClick = onBack) {
        Text(stringResource(id = R.string.btn_back))
    }
}

@Preview(name = "1. Esperando - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "1. Waiting - EN", locale = "en", showBackground = true)
@Composable
fun PreviewWaitingState() {
    AMFootballTheme {
        FinishMatchLobbyContent(
            state = FinishMatchUiState.WaitingForConfirmation,
            onEditResult = {},
            onBack = {}
        )
    }
}

@Preview(name = "2. Connectando - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "2. Connecting - EN", locale = "en", showBackground = true)
@Composable
fun PreviewConnectingState() {
    AMFootballTheme {
        FinishMatchLobbyContent(
            state = FinishMatchUiState.Connecting,
            onEditResult = {},
            onBack = {}
        )
    }
}

@Preview(name = "3. Erro - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "3. Error - EN", locale = "en", showBackground = true)
@Composable
fun PreviewErrorState() {
    AMFootballTheme {
        FinishMatchLobbyContent(
            state = FinishMatchUiState.Error("Connection lost"),
            onEditResult = {},
            onBack = {}
        )
    }
}
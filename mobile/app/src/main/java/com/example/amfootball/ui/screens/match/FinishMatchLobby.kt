package com.example.amfootball.ui.screens.match

import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.data.events.FinishMatchUiState
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.match.FinishMatchLobbyViewModel
import com.example.amfootball.ui.viewModel.match.FinishMatchViewModel

@Composable
fun FinishMatchLobbyScreen(
    navHostController: NavHostController,
    viewModel: FinishMatchLobbyViewModel = hiltViewModel()
) {
    val state by viewModel.uiState.collectAsStateWithLifecycle()

    // Se o jogo acabar, sai deste ecrã
    LaunchedEffect(state) {
        if (state is FinishMatchUiState.MatchFinished) {
            navHostController.navigate(Routes.TeamRoutes.CALENDAR.route) {
                popUpTo(Routes.TeamRoutes.FINISH_MATCH.route) { inclusive = true }
            }
        }
    }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        when (state) {
            FinishMatchUiState.Connecting -> {
                CircularProgressIndicator()
                Spacer(Modifier.height(16.dp))
                Text("A conectar ao servidor...")
            }

            FinishMatchUiState.WaitingForConfirmation -> {
                CircularProgressIndicator()
                Spacer(Modifier.height(24.dp))

                Text(
                    text = "Resultado Submetido",
                    style = MaterialTheme.typography.headlineSmall,
                    color = MaterialTheme.colorScheme.primary
                )
                Text(
                    text = "A aguardar confirmação do adversário...",
                    style = MaterialTheme.typography.bodyLarge
                )

                Spacer(Modifier.height(48.dp))

                // BOTÃO DE EDITAR (Design Diferente)
                OutlinedButton(
                    onClick = { viewModel.onEditResult() },
                    modifier = Modifier.fillMaxWidth(0.8f)
                ) {
                    Icon(Icons.Default.Edit, contentDescription = null)
                    Spacer(Modifier.width(8.dp))
                    Text("Editar Resultado")
                }
            }

            is FinishMatchUiState.Error -> {
                Text(
                    text = "Erro: ${(state as FinishMatchUiState.Error).msg}",
                    color = MaterialTheme.colorScheme.error
                )
                Button(onClick = { navHostController.popBackStack() }) {
                    Text("Voltar")
                }
            }
            else -> {}
        }
    }
}
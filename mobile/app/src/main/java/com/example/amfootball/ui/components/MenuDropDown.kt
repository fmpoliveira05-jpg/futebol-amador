package com.example.amfootball.ui.components

import android.util.Log
import androidx.compose.ui.graphics.Color
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.EditCalendar
import androidx.compose.material.icons.filled.Flag
import androidx.compose.material.icons.filled.PlayArrow
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R
import com.example.amfootball.data.enums.MatchStatus
import com.example.amfootball.utils.MatchConsts
import java.time.LocalDateTime
import java.time.temporal.ChronoUnit

/**
 * O menu flutuante ([DropdownMenu]) com todas as ações
 * possíveis para uma partida.
 *
 * Exibe quatro ações principais: Iniciar, Finalizar, Adiar e Cancelar.
 * Após qualquer ação ser selecionada, o menu é automaticamente fechado ([onDismissRequest]).
 *
 * @param expanded Booleano que controla a visibilidade do menu.
 * @param matchStatus Estado atual da partida (iniciado, terminado, etc.).
 * @param onDismissRequest Callback para fechar o menu (acionado ao clicar fora ou selecionar um item).
 * @param onStartMatch Callback a ser executado para iniciar a partida.
 * @param onFinishMatch Callback a ser executado para finalizar a partida.
 * @param onPostPoneMatch Callback a ser executado para adiar a partida.
 * @param onCancelMatch Callback a ser executado para cancelar a partida (usa cor de erro).
 */
@Composable
fun MatchActionsMenu(
    expanded: Boolean,
    matchStatus: MatchStatus,
    gameDate: LocalDateTime,
    onDismissRequest: () -> Unit,
    onStartMatch: () -> Unit,
    onFinishMatch: () -> Unit,
    onPostPoneMatch: () -> Unit,
    onCancelMatch: () -> Unit,
) {
    val now = LocalDateTime.now()
    val hoursUntilGame = ChronoUnit.HOURS.between(now, gameDate)

    Log.d("MATCH_DEBUG", "--- Horas para o Jogo ---")
    Log.d("MATCH_DEBUG", "1. NOW (Local): $now")
    Log.d("MATCH_DEBUG", "2. GAME DATE (Recebida): $gameDate")
    Log.d("MATCH_DEBUG", "3. HORAS RESTANTES: $hoursUntilGame")
    Log.d("MATCH_DEBUG", "4. REQUERIDO: ${MatchConsts.MAX_HOURS_TO_CANCEL}")
    Log.d("MATCH_DEBUG", "5. CONDIÇÃO CANCELAR: ${hoursUntilGame >= MatchConsts.MAX_HOURS_TO_CANCEL}")
    Log.d("MATCH_DEBUG", "----------------------------")
    DropdownMenu(
        expanded = expanded,
        onDismissRequest = onDismissRequest
    ) {
        if (matchStatus == MatchStatus.SCHEDULED || matchStatus == MatchStatus.POST_PONED) {
            if (gameDate >= now) {
                DropdownItem(
                    text = stringResource(id = R.string.button_start_match),
                    onClick = {
                        onStartMatch()
                        onDismissRequest()
                    },
                    leadingIcon = Icons.Default.PlayArrow,
                    color = MaterialTheme.colorScheme.primary
                )
            }

            if (hoursUntilGame >= MatchConsts.MAX_HOURS_TO_POST_PONE) {
                DropdownItem(
                    text = stringResource(id = R.string.button_post_pone_match),
                    onClick = {
                        onPostPoneMatch()
                        onDismissRequest()
                    },
                    leadingIcon = Icons.Default.EditCalendar
                )
            }

            if (hoursUntilGame >= MatchConsts.MAX_HOURS_TO_CANCEL) {
                DropdownItem(
                    text = stringResource(id = R.string.button_cancel_match),
                    onClick = {
                        onCancelMatch()
                        onDismissRequest()
                    },
                    color = MaterialTheme.colorScheme.error,
                    leadingIcon = Icons.Default.Flag,
                )
            }
        } else if(matchStatus == MatchStatus.IN_PROGRESS) {
            DropdownItem(
                text = stringResource(id = R.string.button_finish_match),
                onClick = {
                    onFinishMatch()
                    onDismissRequest()
                },
                color = MaterialTheme.colorScheme.primary,
                leadingIcon = Icons.Default.Flag
            )
        }
    }
}

/**
 * Um componente Composable para um único item dentro de um DropdownMenu.
 *
 * Inclui um ícone à esquerda e permite definir cores customizadas para o texto e o ícone.
 *
 * @param text O texto principal a ser exibido no item (também usado como contentDescription do ícone).
 * @param onClick Callback a ser executado quando o item é clicado.
 * @param color A cor do texto e do ícone (padrão: [Color.Unspecified]).
 * @param leadingIcon O [ImageVector] do ícone exibido à esquerda do texto.
 */
@Composable
fun DropdownItem(
    text: String,
    onClick: () -> Unit,
    color: Color = Color.Unspecified,
    leadingIcon: ImageVector,
) {
    DropdownMenuItem(
        text = {
            Text(
                text = text,
                color = color
            )
        },
        leadingIcon = {
            Icon(
                imageVector = leadingIcon,
                contentDescription = text,
                tint = color
            )
        },
        onClick = onClick
    )
}
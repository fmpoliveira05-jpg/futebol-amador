package com.example.amfootball.ui.components

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

/**
 * O menu flutuante ([DropdownMenu]) com todas as ações
 * possíveis para uma partida.
 *
 * Exibe quatro ações principais: Iniciar, Finalizar, Adiar e Cancelar.
 * Após qualquer ação ser selecionada, o menu é automaticamente fechado ([onDismissRequest]).
 *
 * @param expanded Booleano que controla a visibilidade do menu.
 * @param onDismissRequest Callback para fechar o menu (acionado ao clicar fora ou selecionar um item).
 * @param onStartMatch Callback a ser executado para iniciar a partida.
 * @param onFinishMatch Callback a ser executado para finalizar a partida.
 * @param onPostPoneMatch Callback a ser executado para adiar a partida.
 * @param onCancelMatch Callback a ser executado para cancelar a partida (usa cor de erro).
 */
@Composable
fun MatchActionsMenu(
    expanded: Boolean,
    onDismissRequest: () -> Unit,
    onStartMatch: () -> Unit,
    onFinishMatch: () -> Unit,
    onPostPoneMatch: () -> Unit,
    onCancelMatch: () -> Unit
) {
    DropdownMenu(
        expanded = expanded,
        onDismissRequest = onDismissRequest
    ) {
        DropdownItem(
            text = stringResource(id = R.string.button_start_match),
            onClick = {
                onStartMatch()
                onDismissRequest()
            },
            leadingIcon = Icons.Default.PlayArrow,
            color = MaterialTheme.colorScheme.primary
        )

        DropdownItem(
            text = stringResource(id = R.string.button_finish_match),
            onClick = {
                onFinishMatch()
                onDismissRequest()
            },
            leadingIcon = Icons.Default.Flag
        )

        DropdownItem(
            text = stringResource(id = R.string.button_post_pone_match),
            onClick = {
                onPostPoneMatch()
                onDismissRequest()
            },
            leadingIcon = Icons.Default.EditCalendar
        )

        DropdownItem(
            text = stringResource(id = R.string.button_cancel_match),
            onClick = {
                onCancelMatch()
                onDismissRequest()
            },
            color = MaterialTheme.colorScheme.error,
            leadingIcon = Icons.Default.Flag
        )
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
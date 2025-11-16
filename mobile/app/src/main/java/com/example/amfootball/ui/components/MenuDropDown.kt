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

/**
 * O menu flutuante (DropdownMenu) com todas as ações
 * possíveis para uma partida.
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
            text = "Iniciar Partida",
            onClick = {
                onStartMatch()
                onDismissRequest()
            },
            leadingIcon = Icons.Default.PlayArrow,
            color = MaterialTheme.colorScheme.primary
        )

        DropdownItem(
            text = "Finalizar Partida",
            onClick = {
                onFinishMatch()
                onDismissRequest()
            },
            leadingIcon = Icons.Default.Flag
        )

        DropdownItem(
            text = "Adiar Partida",
            onClick = {
                onPostPoneMatch()
                onDismissRequest()
            },
            leadingIcon = Icons.Default.EditCalendar
        )

        DropdownItem(
            text = "Cancelar Partida",
            onClick = {
                onCancelMatch()
                onDismissRequest()
            },
            color = MaterialTheme.colorScheme.error,
            leadingIcon = Icons.Default.Flag
        )
    }
}

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
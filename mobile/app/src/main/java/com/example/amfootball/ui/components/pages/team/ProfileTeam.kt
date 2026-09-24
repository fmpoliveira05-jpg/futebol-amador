package com.example.amfootball.ui.components.pages.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

/**
 * Diálogo de Alerta para confirmar a eliminação de uma equipa.
 * * Este componente é responsável por exibir a interface de alerta e delegar as ações
 * de confirmação ou cancelamento para o componente pai.
 *
 * @param onDismiss Callback para fechar o diálogo (executado ao clicar fora ou no botão Cancelar).
 * @param onDeleteClick Callback para iniciar o processo de eliminação (executado ao clicar em Confirmar).
 */
@Composable
fun ProfileTeamDialogDelte(
    onDismiss: () -> Unit,
    onDeleteClick: () -> Unit
) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(text = stringResource(R.string.dialog_title_delete_team)) },
        text = { Text(text = stringResource(R.string.dialog_msg_delete_team)) },
        confirmButton = {
            TextButton(
                onClick = {
                    onDeleteClick()
                    onDismiss()
                }
            ) {
                Text(
                    text = stringResource(R.string.dialog_confirm),
                    color = MaterialTheme.colorScheme.error
                )
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text(text = stringResource(R.string.dialog_cancel))
            }
        }
    )
}

/**
 * Agrupador de layout que exibe os botões de Ação de Gestão (Editar e Apagar) lado a lado.
 *
 * @param onEditClick Callback para iniciar a navegação para o ecrã de edição.
 * @param onDeleteClick Callback para abrir o diálogo de confirmação de eliminação.
 */
@Composable
fun RowButtonsProfileTeam(
    onEditClick: () -> Unit,
    onDeleteClick: () -> Unit,
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        EditTeamButton(
            onClick = onEditClick,
            modifier = Modifier.weight(1f)
        )

        DeleteTeamButton(
            onClick = onDeleteClick,
            modifier = Modifier.weight(1f)
        )
    }
}

/**
 * Botão principal para a ação de Editar os dados da equipa.
 *
 * @param onClick Callback executada ao clicar no botão.
 * @param modifier Modificador de layout (usado tipicamente para definir o peso na Row).
 */
@Composable
private fun EditTeamButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    Button(
        onClick = onClick,
        modifier = modifier,
    ) {
        Icon(
            imageVector = Icons.Default.Edit,
            contentDescription = stringResource(R.string.button_description_edit_team),
            modifier = Modifier.padding(end = 8.dp)
        )
        Text(text = stringResource(R.string.button_edit_team))
    }
}

/**
 * Botão destrutivo para Apagar a equipa.
 *
 * Utiliza o esquema de cores [MaterialTheme.colorScheme.error] para sinalizar o perigo da ação.
 *
 * @param onClick Callback executada ao clicar no botão (deve abrir o diálogo de confirmação).
 * @param modifier Modificador de layout.
 */
@Composable
private fun DeleteTeamButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    Button(
        onClick = onClick,
        modifier = modifier,
        colors = ButtonDefaults.buttonColors(
            containerColor = MaterialTheme.colorScheme.error,
            contentColor = MaterialTheme.colorScheme.onError
        )
    ) {
        Icon(
            imageVector = Icons.Default.Delete,
            contentDescription = stringResource(id = R.string.button_delete_team_description),
            modifier = Modifier.padding(end = 8.dp)
        )
        Text(text = stringResource(R.string.button_delete_team))
    }
}
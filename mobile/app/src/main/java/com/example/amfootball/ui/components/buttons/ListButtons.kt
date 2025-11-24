package com.example.amfootball.ui.components.buttons

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Email
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R

/**
 * Um componente [IconButton] usado para confirmar ou aceitar uma ação.
 *
 * Exibe o ícone [Icons.Default.Check] com a cor primária do tema ([MaterialTheme.colorScheme.primary]).
 * A descrição de conteúdo é obtida através de [R.string.accept_button_description].
 *
 * @param accept A função lambda a ser executada quando o botão for clicado.
 */
@Composable
fun AcceptButton(accept: () -> Unit) {
    IconButton(
        onClick = accept
    ) {
        Icon(
            imageVector = Icons.Default.Check,
            contentDescription = stringResource(id = R.string.accept_button_description),
            tint = MaterialTheme.colorScheme.primary
        )
    }
}

/**
 * Um componente [IconButton] usado para rejeitar ou cancelar uma ação.
 *
 * Exibe o ícone [Icons.Default.Close] com a cor de erro do tema ([MaterialTheme.colorScheme.error]).
 * A descrição de conteúdo é obtida através de [R.string.reject_button_description].
 *
 * @param reject A função lambda a ser executada quando o botão for clicado.
 */
@Composable
fun RejectButton(reject: () -> Unit) {
    IconButton(
        onClick = reject
    ) {
        Icon(
            imageVector = Icons.Default.Close,
            contentDescription = stringResource(id = R.string.reject_button_description),
            tint = MaterialTheme.colorScheme.error
        )
    }
}

/**
 * Um componente [IconButton] usado para iniciar uma ação de edição.
 *
 * Exibe o ícone [Icons.Default.Edit] com a cor secundária do tema ([MaterialTheme.colorScheme.secondary]).
 * A descrição de conteúdo é fornecida externamente para garantir contexto.
 *
 * @param edit A função lambda a ser executada quando o botão for clicado.
 * @param contentDescription A string que descreve a ação para acessibilidade (obrigatória
 * para garantir que o leitor de tela entenda o que está sendo editado).
 */
@Composable
fun EditButton(
    edit: () -> Unit,
    contentDescription: String
) {
    IconButton(
        onClick = edit
    ) {
        Icon(
            imageVector = Icons.Default.Edit,
            contentDescription = contentDescription,
            tint = MaterialTheme.colorScheme.secondary
        )
    }
}

/**
 * Um componente [IconButton] usado para enviar um pedido de adesão (membership request).
 *
 * Exibe o ícone [Icons.Default.Email] com a cor primária do tema ([MaterialTheme.colorScheme.primary]).
 * A descrição de conteúdo é obtida através de [R.string.button_send_membership_request_description].
 *
 * @param sendMemberShipRequest A função lambda a ser executada quando o botão for clicado.
 */
@Composable
fun ListSendMemberShipRequestButton(sendMemberShipRequest: () -> Unit) {
    IconButton(onClick = sendMemberShipRequest) {
        Icon(
            imageVector = Icons.Default.Email,
            contentDescription = stringResource(id = R.string.button_send_membership_request_description),
            tint = MaterialTheme.colorScheme.primary
        )
    }
}

/**
 * Um componente Composable [IconButton] usado para indicar que mais detalhes estão disponíveis
 * para um item e navegar para a tela de detalhes.
 *
 * Este botão exibe o ícone de seta para a direita ([Icons.Default.ChevronRight]) e usa
 * a cor de contorno ([MaterialTheme.colorScheme.outline]) para um visual discreto.
 * É tipicamente usado no final de itens em uma lista.
 *
 * @param showMoreDetails A função lambda a ser executada quando o botão for clicado,
 * geralmente contendo a lógica de navegação para a tela de detalhes.
 * @param contentDescription A descrição de conteúdo para acessibilidade (leitor de tela).
 * O padrão é [R.string.list_teams_view_team] (Sugere-se que este recurso forneça
 * uma descrição como "Ver detalhes do time").
 */
@Composable
fun ShowMoreInfoButton(
    showMoreDetails: () -> Unit,
    contentDescription: String = stringResource(id = R.string.list_teams_view_team)
) {
    IconButton(
        onClick = { showMoreDetails() }
    ) {
        Icon(
            imageVector = Icons.Default.ChevronRight,
            contentDescription = contentDescription,
            tint = MaterialTheme.colorScheme.outline
        )
    }
}
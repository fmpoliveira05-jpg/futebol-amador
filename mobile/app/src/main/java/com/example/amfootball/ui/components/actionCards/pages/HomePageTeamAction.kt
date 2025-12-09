package com.example.amfootball.ui.components.actionCards.pages

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R
import com.example.amfootball.ui.components.cards.ActionCard

/**
 * Cartão de ação destrutiva para sair da equipa.
 *
 * @param onClick Callback executada ao clicar.
 */
@Composable
fun ActionCardLeaveTeam(onClick: () -> Unit) {
    ActionCard(
        title = stringResource(id = R.string.action_card_leave_team),
        subtitle = stringResource(id = R.string.action_card_leave_team_description),
        icon = Icons.AutoMirrored.Filled.ExitToApp,
        onClick = onClick,
    )
}
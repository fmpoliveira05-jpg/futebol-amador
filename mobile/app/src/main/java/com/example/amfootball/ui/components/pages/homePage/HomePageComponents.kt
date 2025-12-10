package com.example.amfootball.ui.components.pages.homePage

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.Person
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R
import com.example.amfootball.ui.components.actionCards.ActionCard

/**
 * Cartão específico para a ação de Criar Equipa.
 *
 * @param onNavigateCreateTeam Callback a executar no clique.
 */
@Composable
fun ActionCardCreateTeam(onNavigateCreateTeam: () -> Unit) {
    ActionCard(
        title = stringResource(id = R.string.button_create_team),
        subtitle = stringResource(id = R.string.button_description_create_team),
        icon = Icons.Default.Add,
        onClick = onNavigateCreateTeam
    )
}

/**
 * Cartão específico para a ação de Listar Equipas disponíveis.
 *
 * @param onNavigateToListTeams Callback a executar no clique.
 */
@Composable
fun ActionCardListTeam(
    onNavigateToListTeams: () -> Unit,
    subTitle: String
) {
    ActionCard(
        title = stringResource(id = R.string.button_list_teams),
        subtitle = subTitle,
        icon = Icons.Default.Edit,
        onClick = onNavigateToListTeams
    )
}

/**
 * Cartão específico para a ação de Visualizar Pedidos de Adesão.
 *
 * @param onNavigationToRequests Callback a executar no clique.
 */
@Composable
fun ActionCardListMembershipRequests(onNavigationToRequests: () -> Unit) {
    ActionCard(
        title = stringResource(id = R.string.button_membership_request),
        subtitle = stringResource(id = R.string.button_description_membership_request),
        icon = Icons.Default.Person,
        onClick = onNavigationToRequests
    )
}

@Composable
fun ActionCardMyTeam(onNavigateToTeamHome: () -> Unit) {
    ActionCard(
        title = stringResource(id = R.string.action_card_team),
        subtitle = stringResource(id = R.string.action_card_team_description),
        icon = Icons.Default.Home,
        onClick = onNavigateToTeamHome
    )
}

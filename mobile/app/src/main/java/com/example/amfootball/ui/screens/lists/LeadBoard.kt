package com.example.amfootball.ui.screens.lists

import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.remote.dtos.leadboard.InfoTeamLeadboard
import com.example.amfootball.ui.actions.lists.LeadBoardActions
import com.example.amfootball.ui.actions.lists.ShowMoreItensAction
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.previewsMocks.ItemActionsMock
import com.example.amfootball.ui.previewsMocks.LeadboardMocks
import com.example.amfootball.ui.viewModel.lists.LeadBoardViewModel

/**
 * Ecrã principal da Tabela de Classificação (Leaderboard).
 *
 * Este Composable atua como o ponto de entrada (Stateful), conectando o [LeadBoardViewModel]
 * à interface do utilizador. Responsável por coletar o estado da lista e definir as ações.
 *
 * @param navHostController Controlador de navegação.
 * @param viewModel ViewModel responsável pela lógica de negócio e carregamento de dados.
 */
@Composable
fun LeaderboardScreen(
    navHostController: NavHostController,
    viewModel: LeadBoardViewModel = hiltViewModel()
) {
    val list by viewModel.uiList.collectAsStateWithLifecycle()

    val leadBoardActions = LeadBoardActions(
        onShowMore = viewModel::showInfoTeam,
    )

    val showMoreItensAction = ShowMoreItensAction(
        isValidShowMore = { viewModel.showMoreButtonVisible },
        onLoadMore = { viewModel.loadMoreItems() }
    )

    LeadBoardContent(
        list = list,
        leadBoardActions = leadBoardActions,
        showMoreItensAction = showMoreItensAction,
        navHostController = navHostController
    )
}

/**
 * Conteúdo UI da Tabela de Classificação (Stateless).
 *
 * Utiliza o [ListSurface] para renderizar a lista de equipas ou o estado vazio.
 *
 * @param list Lista de DTOs da classificação a exibir.
 * @param leadBoardActions Ações disponíveis (carregar mais, ver detalhes).
 * @param navHostController Controlador de navegação para transição de ecrãs.
 */
@Composable
private fun LeadBoardContent(
    list: List<InfoTeamLeadboard>,
    leadBoardActions: LeadBoardActions,
    showMoreItensAction: ShowMoreItensAction,
    navHostController: NavHostController
) {
    val isShowMoreVisible by showMoreItensAction.isValidShowMore().collectAsStateWithLifecycle()

    ListSurface(
        list = list,
        listItems = { team ->
            LeaderBoardItems(
                team = team,
                showInfoTeam = {
                    leadBoardActions.onShowMore(
                        team.id,
                        navHostController
                    )
                }
            )
        },
        isValidShowMore = isShowMoreVisible,
        showMoreItems = showMoreItensAction.onLoadMore,
        messageEmptyList = stringResource(id = R.string.leadboard_empty)
    )
}

/**
 * Item individual da lista de classificação.
 *
 * Apresenta:
 * - Posição (#1, #2...).
 * - Nome da equipa.
 * - Rank e Pontos atuais.
 * - Logótipo da equipa.
 * - Botão para ver mais detalhes.
 *
 * @param team Dados da equipa na classificação.
 * @param showInfoTeam Callback executado ao clicar no botão de detalhes.
 */
@Composable
private fun LeaderBoardItems(
    team: InfoTeamLeadboard,
    showInfoTeam: () -> Unit
) {
    ListItem(
        overlineContent = {
            Text(
                text = "#${team.position}",
                style = MaterialTheme.typography.titleSmall,
                fontWeight = FontWeight.Bold
            )
        },
        headlineContent = {
            Text(text = team.name)
        },
        supportingContent = {
            Text(
                text = buildAnnotatedString {
                    pushStyle(SpanStyle(fontWeight = FontWeight.Bold))
                    append("Rank: ${team.nameRank}")
                    pop()

                    append("  ")

                    pushStyle(SpanStyle(color = MaterialTheme.colorScheme.primary))
                    append("(${team.currentPoints} Pts)")
                    pop()
                },
                style = MaterialTheme.typography.bodyMedium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        },
        leadingContent = {
            StringImageList(
                image = team.logoTeam,
                contentDescription = stringResource(
                    id = R.string.logo_team_name,
                    stringResource(R.string.logo_team),
                    team.name
                )
            )
        },
        trailingContent = {
            ShowMoreInfoButton(
                showMoreDetails = { showInfoTeam() }
            )
        },
    )
}

@Preview(
    name = "Leaderboard - Com Dados",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "Leaderboard - With Data",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewLeaderboardContentPopulated() {
    LeadBoardContent(
        list = LeadboardMocks.fakeList,
        leadBoardActions = LeadboardMocks.fakeActions,
        showMoreItensAction = ItemActionsMock.mockShowMoreItensAction,
        navHostController = rememberNavController()
    )
}

@Preview(
    name = "Leaderboard - Vazia",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "Leaderboard - Empty",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewLeaderboardContentEmpty() {
    LeadBoardContent(
        list = emptyList(),
        leadBoardActions = LeadboardMocks.fakeActions,
        showMoreItensAction = ItemActionsMock.mockShowMoreItensActionHidden,
        navHostController = rememberNavController()
    )
}
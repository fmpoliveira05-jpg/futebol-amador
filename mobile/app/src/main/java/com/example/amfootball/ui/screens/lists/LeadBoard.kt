package com.example.amfootball.ui.screens.lists

import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.lists.LeadBoardActions
import com.example.amfootball.data.dtos.leadboard.InfoTeamLeadboard
import com.example.amfootball.data.dtos.leadboard.LeadboardDto
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.StringImageList
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
    viewModel: LeadBoardViewModel = viewModel()
) {
    val list = viewModel.inicialList.value
    val leadBoardActions = LeadBoardActions(
        onShowMore = viewModel::showInfoTeam,
        isValidShowMoreTeams = viewModel::showMoreButton,
        onLoadMoreTeams = viewModel::loadMoreTeams
    )

    LeadBoardContent(
        list = list,
        leadBoardActions = leadBoardActions,
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
    list: List<LeadboardDto>,
    leadBoardActions: LeadBoardActions,
    navHostController: NavHostController
) {

    ListSurface(
        list = list,
        listItems = { team ->
            LeaderBoardItems(
                team = team,
                showInfoTeam = {
                    leadBoardActions.onShowMore(
                        team.team.id,
                        navHostController
                    )
                }
            )
        },
        isValidShowMore = leadBoardActions.isValidShowMoreTeams().value,
        showMoreItems = { leadBoardActions.onLoadMoreTeams() },
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
    team: LeadboardDto,
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
            Text(text = team.team.name)
        },
        supportingContent = {
            Text(
                text = buildAnnotatedString {
                    pushStyle(SpanStyle(fontWeight = FontWeight.Bold))
                    append("Rank: ${team.team.nameRank}")
                    pop()

                    append("  ")

                    pushStyle(SpanStyle(color = MaterialTheme.colorScheme.primary))
                    append("(${team.team.currentPoints} Pts)")
                    pop()
                },
                style = MaterialTheme.typography.bodyMedium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        },
        leadingContent = {
            StringImageList(
                image = team.team.logoTeam,
                contentDescription = stringResource(
                    id = R.string.logo_team_name,
                    stringResource(R.string.logo_team),
                    team.team.name
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
    val fakeList = listOf(
        LeadboardDto(
            position = 1,
            team = InfoTeamLeadboard(
                id = "1",
                name = "Porto Lions",
                currentPoints = 1250,
                nameRank = "Elite",
                logoTeam = null
            )
        ),
        LeadboardDto(
            position = 2,
            team = InfoTeamLeadboard(
                id = "2",
                name = "Lisboa Navigators",
                currentPoints = 980,
                nameRank = "Pro",
                logoTeam = null
            )
        ),
        LeadboardDto(
            position = 3,
            team = InfoTeamLeadboard(
                id = "3",
                name = "Braga Warriors",
                currentPoints = 450,
                nameRank = "Amateur",
                logoTeam = null
            )
        )
    )

    val showMoreState = remember { mutableStateOf(true) }
    val fakeActions = LeadBoardActions(
        onShowMore = { _, _ -> },
        isValidShowMoreTeams = { showMoreState },
        onLoadMoreTeams = {}
    )

    LeadBoardContent(
        list = fakeList,
        leadBoardActions = fakeActions,
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
    val showMoreState = remember { mutableStateOf(true) }
    val fakeActions = LeadBoardActions(
        onShowMore = { _, _ -> },
        isValidShowMoreTeams = { showMoreState },
        onLoadMoreTeams = {}
    )

    LeadBoardContent(
        list = emptyList(),
        leadBoardActions = fakeActions,
        navHostController = rememberNavController()
    )
}
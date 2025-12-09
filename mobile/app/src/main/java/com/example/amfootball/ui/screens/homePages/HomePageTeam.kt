package com.example.amfootball.ui.screens.homePages

import android.content.res.Configuration
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.EmojiEvents
import androidx.compose.material.icons.filled.Groups
import androidx.compose.material.icons.filled.SportsEsports
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.homePageActions.HomePageTeamActions
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.enums.UserRole
import com.example.amfootball.data.mocks.UiStateMock
import com.example.amfootball.data.mocks.homePages.HomePageTeamMock
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.actionCards.CompactActionCard
import com.example.amfootball.ui.components.cards.ActionCard
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.components.notification.ToastHandler
import com.example.amfootball.ui.viewModel.homePages.TeamHomePageViewModel

/**
 * Ecrã Principal da Equipa (Team Dashboard).
 *
 * Este é um componente "Stateful" (com estado) que atua como o ponto de entrada para a gestão de uma equipa específica.
 *
 * Responsabilidades:
 * 1. Coletar o estado do ViewModel.
 * 2. Gerir feedbacks visuais (Toasts).
 * 3. Orquestrar a navegação através de [HomePageTeamActions].
 *
 * @param globalNavController Controlador de navegação global da aplicação.
 * @param viewModel ViewModel injetado via Hilt.
 */
@Composable
fun HomePageTeamScreen(
    globalNavController: NavHostController,
    viewModel: TeamHomePageViewModel = hiltViewModel()
) {
    val role by viewModel.role.collectAsStateWithLifecycle()
    val team by viewModel.team.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    ToastHandler(
        toastMessage = uiState.toastMessage,
        onToastShown = viewModel::onToastShown
    )

    val homePageTeamActions = HomePageTeamActions(
        onNavigateCasualMatch = {
            viewModel.onNavigateCasualMatch(
                onSucess = {
                    globalNavController.navigate(Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE.route) {
                        launchSingleTop = true
                    }
                }
            )
        },
        onNavigateRankedMatch = {
            viewModel.onNavigateRankedMatch(
                onSucess = {
                    globalNavController.navigate(Routes.TeamRoutes.SEARCH_COMPETIVE_MATCH.route) {
                        launchSingleTop = true
                    }
                }
            )
        },
        onNavigateCalendar = {
            viewModel.onNavigateCalendar(
                onSucess = {
                    globalNavController.navigate("${Routes.TeamRoutes.CALENDAR.route}/${team.id}") {
                        launchSingleTop = true
                    }
                }
            )
        },
        onNavigateMembers = {
            viewModel.onNavigateMembers(
                onSucess = {
                    globalNavController.navigate(Routes.TeamRoutes.MEMBERLIST.route) {
                        launchSingleTop = true
                    }
                }
            )
        },
    )

    HomePageTeam(
        team = team,
        uiState = uiState,
        isOnline = isOnline,
        role = role,
        homePageTeamActions = homePageTeamActions
    )
}

/**
 * Conteúdo visual da Home Page da Equipa (Stateless).
 *
 * @param team Objeto com os dados da equipa.
 * @param uiState Estado atual da UI.
 * @param isOnline Booleano indicando conectividade.
 * @param role O papel do utilizador na equipa.
 * @param homePageTeamActions Ações de navegação.
 */
@Composable
fun HomePageTeam(
    team: TeamDto,
    uiState: UiState,
    isOnline: Boolean,
    role: UserRole,
    homePageTeamActions: HomePageTeamActions
) {
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = {},
        content = {
            OfflineBanner(
                isVisible = !isOnline,
                text = stringResource(id = R.string.without_internet)
            )

            TeamHomePageDrawer(
                team = team,
                role = role,
                homePageTeamActions = homePageTeamActions
            )
        }
    )
}

/**
 * Componente que define a estrutura de scroll e layout da página.
 *
 * Organiza o ecrã em:
 * 1. Cabeçalho ([HeaderHomePageTeam]).
 * 2. Conteúdo Principal ([HomePageTeamContent]).
 *
 * @param team Objeto com os dados da equipa para exibir no cabeçalho.
 * @param role Papel do utilizador para controlo de acesso visual.
 * @param onNavigateCasualMatch Ação para navegação de jogo casual.
 * @param onNavigateRankedMatch Ação para navegação de jogo rankeado.
 * @param onNavigateCalendar Ação para navegação do calendário.
 * @param onNavigateMembers Ação para navegação da lista de membros.
 */
@Composable
private fun TeamHomePageDrawer(
    team: TeamDto,
    role: UserRole,
    homePageTeamActions: HomePageTeamActions
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.Start
    ) {
        HeaderHomePageTeam(team = team)

        HomePageTeamContent(
            role = role,
            homePageTeamActions = homePageTeamActions
        )
    }
}

/**
 * Agrupador das secções de conteúdo (Match Center e Gestão).
 *
 * Aplica um espaçamento vertical consistente (24.dp) entre cada secção filha
 * utilizando `verticalArrangement`.
 *
 * @param role Se for [UserRole.ADMIN_TEAM], exibe a secção "Match Center".
 * @param onNavigateCasualMatch Callback para criar partida casual.
 * @param onNavigateRankedMatch Callback para criar partida rankeada.
 * @param onNavigateCalendar Callback para ver calendário.
 * @param onNavigateMembers Callback para ver lista de membros.
 */
@Composable
private fun HomePageTeamContent(
    role: UserRole,
    homePageTeamActions: HomePageTeamActions
) {
    Column(
        modifier = Modifier.fillMaxWidth(),
        verticalArrangement = Arrangement.spacedBy(24.dp)
    ) {

        if (role == UserRole.ADMIN_TEAM) {
            HomePageMatchCenter(
                homePageTeamActions = homePageTeamActions
            )
        }

        HomePageManagerTeam(
            homePageTeamActions = homePageTeamActions
        )
    }
}

/**
 * Cabeçalho da página que exibe a identidade da equipa.
 * Mostra o logótipo (imagem) e o nome da equipa lado a lado.
 *
 * @param team DTO da equipa contendo o nome e URL da imagem.
 */
@Composable
private fun HeaderHomePageTeam(team: TeamDto) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(bottom = 24.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.Start
    ) {
        StringImageList(
            image = team.image,
            contentDescription = stringResource(
                id = R.string.logo_team_name,
                stringResource(R.string.logo_team),
                team.name
            ),
            textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_logo_team))
        )

        Spacer(modifier = Modifier.width(16.dp))

        Text(
            text = team.name,
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.testTag(stringResource(id = R.string.tag_name_team))
        )
    }
}

/**
 * Secção "Match Center": Contém os botões para agendar partidas.
 * Utiliza cartões compactos ([CompactActionCard]) dispostos horizontalmente.
 *
 * @param onNavigateCasualMatch Ação ao clicar no botão "Casual".
 * @param onNavigateRankedMatch Ação ao clicar no botão "Rankeada".
 */
@Composable
private fun HomePageMatchCenter(homePageTeamActions: HomePageTeamActions) {
    Text(
        text = stringResource(id = R.string.schedule_match),
        style = MaterialTheme.typography.titleMedium,
        modifier = Modifier.padding(bottom = 8.dp)
    )

    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        CompactActionCard(
            title = stringResource(id = R.string.action_card_casual),
            icon = Icons.Default.SportsEsports,
            onClick = homePageTeamActions.onNavigateCasualMatch,
            contentDescription = stringResource(id = R.string.action_card_casual_description),
            modifier = Modifier.weight(1f)
        )

        CompactActionCard(
            title = stringResource(id = R.string.action_card_ranked),
            icon = Icons.Default.EmojiEvents,
            onClick = homePageTeamActions.onNavigateRankedMatch,
            contentDescription = stringResource(id = R.string.action_card_ranked_description),
            modifier = Modifier.weight(1f)
        )
    }
}

/**
 * Secção "Gestão de Equipa": Contém botões para Calendário e Membros.
 * Utiliza cartões de ação padrão ([ActionCard]) dispostos verticalmente.
 *
 * @param onNavigateMembers Ação ao clicar no botão "Membros".
 * @param onNavigateCalendar Ação ao clicar no botão "Calendário".
 */
@Composable
private fun HomePageManagerTeam(homePageTeamActions: HomePageTeamActions) {
    Text(
        text = stringResource(id = R.string.manager_team),
        style = MaterialTheme.typography.titleMedium,
        modifier = Modifier.padding(bottom = 8.dp)
    )

    Column(
        verticalArrangement = Arrangement.spacedBy(12.dp),
        modifier = Modifier.fillMaxWidth()
    ) {
        ActionCard(
            title = stringResource(id = R.string.action_card_calendar),
            subtitle = stringResource(id = R.string.action_card_calendar_description),
            icon = Icons.Default.CalendarMonth,
            onClick = homePageTeamActions.onNavigateCalendar,
            textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_action_card_calendar))
        )

        ActionCard(
            title = stringResource(id = R.string.action_card_members),
            subtitle = stringResource(id = R.string.action_card_members_description),
            icon = Icons.Default.Groups,
            onClick = homePageTeamActions.onNavigateMembers,
        )
    }
}

@Preview(
    name = "1. Admin - English",
    group = "Admin View",
    showBackground = true,
    locale = "en"
)
@Preview(
    name = "1. Admin - Português (PT)",
    group = "Admin View",
    showBackground = true,
    locale = "pt-rPT"
)
@Preview(
    name = "1. Admin - Dark Mode",
    group = "Admin View",
    showBackground = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)
@Composable
fun PreviewTeamHomePageAdmin() {
    MaterialTheme {
        HomePageTeam(
            team = HomePageTeamMock.mockTeam,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            role = UserRole.ADMIN_TEAM,
            homePageTeamActions = HomePageTeamActions(
                onNavigateCasualMatch = {},
                onNavigateRankedMatch = {},
                onNavigateMembers = {},
                onNavigateCalendar = {}
            )
        )
    }
}

@Preview(
    name = "2. Member - English",
    group = "Member View",
    showBackground = true,
    locale = "en"
)
@Preview(
    name = "2. Member - Português (PT)",
    group = "Member View",
    showBackground = true,
    locale = "pt-rPT"
)
@Preview(
    name = "2. Member - Dark Mode",
    group = "Member View",
    showBackground = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)
@Composable
fun PreviewTeamHomePageMember() {
    MaterialTheme {
        HomePageTeam(
            team = HomePageTeamMock.mockTeam,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            role = UserRole.MEMBER_TEAM,
            homePageTeamActions = HomePageTeamActions(
                onNavigateCasualMatch = {},
                onNavigateRankedMatch = {},
                onNavigateMembers = {},
                onNavigateCalendar = {}
            )
        )
    }
}
package com.example.amfootball.ui.screens.homePages

import android.content.res.Configuration
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
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
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
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
import com.example.amfootball.data.events.UiState
import com.example.amfootball.data.remote.dtos.homePageTeam.HomePageTeamDto
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.domains.enums.UserRole
import com.example.amfootball.ui.actions.homePageActions.HomePageTeamActions
import com.example.amfootball.ui.previewsMocks.UiStateMock
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.actionCards.ActionCard
import com.example.amfootball.ui.components.actionCards.CompactActionCard
import com.example.amfootball.ui.components.pages.homePage.ActionCardLeaveTeam
import com.example.amfootball.ui.components.diaglos.pages.LeaveTeamAlertDialog
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.components.notification.ToastHandler
import com.example.amfootball.ui.components.pages.homePage.RecentFormSection
import com.example.amfootball.ui.components.pages.homePage.UpcomingMatchesSection
import com.example.amfootball.ui.previewsMocks.HomePageTeamMock
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
                    globalNavController.navigate("${Routes.TeamRoutes.CALENDAR.route}/${team.team.id}") {
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
        onLeaveTeam = {
            viewModel.onLeaveTeam(
                onSucess = {
                    globalNavController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                        launchSingleTop = true
                    }
                }
            )
        }
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
    team: HomePageTeamDto,
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
                teamData = team,
                role = role,
                homePageTeamActions = homePageTeamActions
            )
        }
    )
}

/**
 * Componente que define a estrutura de scroll e layout da página.
 *
 * Organiza o ecrã em três secções principais:
 * 1. Cabeçalho ([HeaderHomePageTeam]): Identidade da equipa.
 * 2. Conteúdo de Gestão ([HomePageTeamContent]): Match Center e ferramentas.
 * 3. Zona de Saída ([ActionCardLeaveTeam]): Opção para abandonar a equipa.
 *
 * @param team Objeto com os dados da equipa para exibir no cabeçalho.
 * @param role Papel do utilizador para controlo de acesso visual.
 * @param homePageTeamActions Ações de navegação a serem propagadas para os componentes filhos.
 */
@Composable
private fun TeamHomePageDrawer(
    teamData: HomePageTeamDto,
    role: UserRole,
    homePageTeamActions: HomePageTeamActions
) {
    var showLeaveDialog by remember { mutableStateOf(false) }

    if (showLeaveDialog) {
        LeaveTeamAlertDialog(
            onLeaveTeam = homePageTeamActions.onLeaveTeam,
            onDismiss = { showLeaveDialog = false }
        )
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.Start
    ) {
        HeaderHomePageTeam(team = teamData.team)

        Spacer(modifier = Modifier.height(16.dp))

        RecentFormSection(history = teamData.vitorySequenceTeam)

        Spacer(modifier = Modifier.height(16.dp))

        UpcomingMatchesSection(matches = teamData.nextsMatch)

        Spacer(modifier = Modifier.height(32.dp))

        HomePageTeamContent(
            role = role,
            homePageTeamActions = homePageTeamActions
        )

        Spacer(modifier = Modifier.height(32.dp))

        ActionCardLeaveTeam(onClick = { showLeaveDialog = true })

        Spacer(modifier = Modifier.height(16.dp))
    }
}

/**
 * Agrupador das secções de conteúdo (Match Center e Gestão).
 *
 * Aplica um espaçamento vertical consistente (24.dp) entre cada secção filha
 * utilizando `verticalArrangement`.
 *
 * @param role Se for [UserRole.ADMIN_TEAM], exibe a secção "Match Center".
 * @param homePageTeamActions Ações de navegação a serem propagadas.
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
 * @param homePageTeamActions Objeto contendo os callbacks de navegação.
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
 * @param homePageTeamActions Objeto contendo os callbacks de navegação.
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

@Preview(name = "1. Admin Full - PT", group = "Admin Full", locale = "pt-rPT", showBackground = true)
@Preview(name = "1. Admin Full - EN", group = "Admin Full", locale = "en", showBackground = true)
@Composable
fun PreviewAdminFullData() {
    MaterialTheme {
        HomePageTeam(
            team = HomePageTeamMock.dataWithContent,
            uiState = UiState(),
            isOnline = true,
            role = UserRole.ADMIN_TEAM,
            homePageTeamActions = HomePageTeamMock.actions
        )
    }
}

@Preview(name = "2. Member Full - PT", group = "Member Full", locale = "pt-rPT", showBackground = true)
@Preview(name = "2. Member Full - EN", group = "Member Full", locale = "en", showBackground = true)
@Composable
fun PreviewMemberFullData() {
    MaterialTheme {
        HomePageTeam(
            team = HomePageTeamMock.dataWithContent,
            uiState = UiState(),
            isOnline = true,
            role = UserRole.MEMBER_TEAM,
            homePageTeamActions = HomePageTeamMock.actions
        )
    }
}

@Preview(name = "3. Admin Empty - PT", group = "Admin Empty", locale = "pt-rPT", showBackground = true)
@Preview(name = "3. Admin Empty - EN", group = "Admin Empty", locale = "en", showBackground = true)
@Composable
fun PreviewEmptyData() {
    MaterialTheme {
        HomePageTeam(
            team = HomePageTeamMock.dataEmpty,
            uiState = UiState(),
            isOnline = true,
            role = UserRole.ADMIN_TEAM,
            homePageTeamActions = HomePageTeamMock.actions
        )
    }
}
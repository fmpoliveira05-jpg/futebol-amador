package com.example.amfootball.ui.screens.homePages

import android.content.res.Configuration
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.homePageActions.HomePageActions
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.enums.UserRole
import com.example.amfootball.data.mocks.UiStateMock
import com.example.amfootball.data.mocks.homePages.HomePageMock
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.actionCards.ActionCardSection
import com.example.amfootball.ui.components.actionCards.pages.ActionCardCreateTeam
import com.example.amfootball.ui.components.actionCards.pages.ActionCardListMembershipRequests
import com.example.amfootball.ui.components.actionCards.pages.ActionCardListTeam
import com.example.amfootball.ui.components.actionCards.pages.ActionCardMyTeam
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.components.notification.ToastHandler
import com.example.amfootball.ui.viewModel.homePages.HomePageViewModel

/**
 * Ecrã Principal (Home Page) da aplicação AmFootball.
 *
 * Este é um componente "Stateful" (com estado) que atua como o ponto de entrada do dashboard.
 * Ele é responsável por:
 * 1. Coletar o estado do ViewModel (User, UiState, Conectividade).
 * 2. Gerir os Toasts (feedbacks) da aplicação.
 * 3. Definir a lógica de navegação e encapsulá-la em [HomePageActions].
 *
 * @param globalNavController Controlador de navegação para alternar entre ecrãs.
 * @param viewModel O ViewModel injetado via Hilt que contém a lógica de negócio e estado.
 */
@Composable
fun HomePageScreen(
    globalNavController: NavHostController,
    viewModel: HomePageViewModel = hiltViewModel()
) {
    val user by viewModel.user.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    val id = user?.loginResponseDto?.localId

    ToastHandler(
        toastMessage = uiState.toastMessage,
        onToastShown = viewModel::onToastShown
    )

    val homePageActions = HomePageActions(
        onNavigateCreateTeam = {
            viewModel.onNavigateCreateTeam(onSuccessNavigation = {
                globalNavController.navigate(Routes.TeamRoutes.CREATE_TEAM.route) {
                    launchSingleTop = true
                }
            })
        },
        onNavigationToRequests = {
            viewModel.onNavigationToRequests(
                idPlayer = id,
                onSuccessNavigation = {
                    globalNavController.navigate("${Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST.route}/${id}") {
                        launchSingleTop = true
                    }
                }
            )
        },
        onNavigateToListTeams = {
            viewModel.onNavigateToListTeams(
                onSuccessNavigation = {
                    globalNavController.navigate(Routes.PlayerRoutes.TEAM_LIST.route) {
                        launchSingleTop = true
                    }
                }
            )
        },
        onNavigateToTeamHome = {
            val teamId = user?.effectiveTeamId
            if (teamId != null) {
                globalNavController.navigate("${Routes.TeamRoutes.HOMEPAGE.route}/$teamId") {
                    launchSingleTop = true
                }
            }
        }
    )

    HomePageContent(
        user = user,
        uiState = uiState,
        isOnline = isOnline,
        homePageActions = homePageActions
    )
}

/**
 * Conteúdo visual da Home Page (Stateless).
 *
 * Responsável por estruturar a UI, exibir estados de carregamento (Loading)
 * e alertas de conectividade (OfflineBanner).
 *
 * @param user O perfil do jogador carregado (pode ser null).
 * @param uiState O estado atual da UI (loading, erros, mensagens).
 * @param isOnline Booleano que indica se há conexão à internet.
 * @param homePageActions Objeto contendo todas as callbacks de navegação.
 */
@Composable
fun HomePageContent(
    user: PlayerProfileDto?,
    uiState: UiState,
    isOnline: Boolean,
    homePageActions: HomePageActions
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

            HomePageDrawer(
                user = user,
                homePageActions = homePageActions
            )
        }
    )
}

/**
 * Componente que desenha a lista vertical de ações e o cabeçalho de boas-vindas.
 *
 * Seleciona quais cartões mostrar com base no papel (Role) do utilizador.
 *
 * @param user Dados do utilizador para exibir o nome e verificar a role.
 * @param homePageActions Ações de navegação disponíveis.
 */
@Composable
private fun HomePageDrawer(
    user: PlayerProfileDto?,
    homePageActions: HomePageActions
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.Start
    ) {
        Text(
            text = stringResource(
                id = R.string.welcome_app,
                user?.name ?: stringResource(R.string.user)
            ), style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.padding(bottom = 24.dp)
        )

        Text(
            text = stringResource(id = R.string.quick_actions),
            style = MaterialTheme.typography.titleMedium,
            modifier = Modifier.padding(bottom = 8.dp)
        )

        if (user != null) {
            when (user.role) {
                UserRole.PLAYER_WITHOUT_TEAM -> {
                    ActionCardPlayerWithoutTeam(
                        homePageActions = homePageActions
                    )
                }

                UserRole.ADMIN_TEAM, UserRole.MEMBER_TEAM -> {
                    ActionCardMemberTeam(homePageActions = homePageActions)
                }

                else -> {
                    ActionCardUnauthorizedUser(onNavigateToListTeams = homePageActions.onNavigateToListTeams)
                }
            }
        } else {
            ActionCardUnauthorizedUser(onNavigateToListTeams = homePageActions.onNavigateToListTeams)
        }
    }
}

/**
 * Secção de cartões para jogadores COM equipa (Admin ou Membro).
 *
 * @param homePageActions Ações de navegação.
 */
@Composable
private fun ActionCardMemberTeam(homePageActions: HomePageActions) {
    ActionCardSection(
        content = {
            ActionCardMyTeam(
                onNavigateToTeamHome = homePageActions.onNavigateToTeamHome
            )

            ActionCardListTeam(
                onNavigateToListTeams = homePageActions.onNavigateToListTeams,
                subTitle = stringResource(id = R.string.button_description_list_teams_match)
            )
        }
    )
}

/**
 * Secção de cartões para utilizadores não autenticados.
 *
 * @param onNavigateToListTeams Ação para navegar para a lista de equipas.
 */
@Composable
private fun ActionCardUnauthorizedUser(onNavigateToListTeams: () -> Unit) {
    ActionCardSection(
        content = {
            ActionCardListTeam(
                onNavigateToListTeams = onNavigateToListTeams,
                subTitle = stringResource(id = R.string.button_description_list_team)
            )
        }
    )
}

/**
 * Secção de cartões para jogadores COM equipa (Admin ou Membro).
 *
 * @param homePageActions Ações de navegação.
 */
@Composable
private fun ActionCardPlayerWithoutTeam(homePageActions: HomePageActions) {
    ActionCardSection(
        content = {
            ActionCardCreateTeam(
                onNavigateCreateTeam = homePageActions.onNavigateCreateTeam
            )
            ActionCardListTeam(
                onNavigateToListTeams = homePageActions.onNavigateToListTeams,
                subTitle = stringResource(id = R.string.button_description_list_teams_membership)
            )
            ActionCardListMembershipRequests(
                onNavigationToRequests = homePageActions.onNavigationToRequests
            )
        }
    )
}

@Preview(name = "1. Unauth - English", group = "Unauthorized", showBackground = true, locale = "en")
@Preview(name = "1. Unauth - PT", group = "Unauthorized", showBackground = true, locale = "pt-rPT")
@Preview(
    name = "1. Unauth - Dark",
    group = "Unauthorized",
    showBackground = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)
@Composable
fun PreviewHomePageUnauthorized() {
    MaterialTheme {
        HomePageContent(
            user = null,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            homePageActions = HomePageActions(
                onNavigateToListTeams = {},
                onNavigateCreateTeam = {},
                onNavigationToRequests = {},
                onNavigateToTeamHome = {}
            )
        )
    }
}

@Preview(name = "2. No Team - English", group = "No Team", showBackground = true, locale = "en")
@Preview(name = "2. No Team - PT", group = "No Team", showBackground = true, locale = "pt-rPT")
@Preview(
    name = "2. No Team - Dark",
    group = "No Team",
    showBackground = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)
@Composable
fun PreviewHomePagePlayerNoTeam() {
    MaterialTheme {
        HomePageContent(
            user = HomePageMock.mockUserNoTeam,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            homePageActions = HomePageActions(
                onNavigateToListTeams = {},
                onNavigateCreateTeam = {},
                onNavigationToRequests = {},
                onNavigateToTeamHome = {}
            )
        )
    }
}

// CENÁRIO 3: Jogador COM Equipa
@Preview(name = "3. With Team - English", group = "With Team", showBackground = true, locale = "en")
@Preview(name = "3. With Team - PT", group = "With Team", showBackground = true, locale = "pt-rPT")
@Preview(
    name = "3. With Team - Dark",
    group = "With Team",
    showBackground = true,
    uiMode = Configuration.UI_MODE_NIGHT_YES
)
@Composable
fun PreviewHomePagePlayerWithTeam() {
    MaterialTheme {
        HomePageContent(
            user = HomePageMock.mockUserWithTeam,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            homePageActions = HomePageActions(
                onNavigateToListTeams = {},
                onNavigateCreateTeam = {},
                onNavigationToRequests = {},
                onNavigateToTeamHome = {}
            )
        )
    }
}
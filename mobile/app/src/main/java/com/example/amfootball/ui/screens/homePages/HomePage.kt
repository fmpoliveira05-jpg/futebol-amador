package com.example.amfootball.ui.screens.homePages

import android.content.res.Configuration
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Person
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
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.mocks.homePages.HomePageMock
import com.example.amfootball.data.mocks.lists.UiStateMock
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.cards.ActionCard
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
 * 3. Definir a lógica de navegação injetando as callbacks necessárias.
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

    HomePageContent(
        user = user,
        uiState = uiState,
        isOnline = isOnline,
        onNavigateCreateTeam = { viewModel.onNavigateCreateTeam(onSuccessNavigation = {
            globalNavController.navigate(Routes.TeamRoutes.CREATE_TEAM.route) {
                launchSingleTop = true
            }
        }) },
        onNavigationToRequests = { viewModel.onNavigationToRequests(
            idPlayer = id,
            onSuccessNavigation ={
                globalNavController.navigate("${Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST.route}/${id}" ){
                    launchSingleTop = true
                }
            }
        ) },
        onNavigateToListTeams = { viewModel.onNavigateToListTeams(
            onSuccessNavigation = {
                globalNavController.navigate(Routes.PlayerRoutes.TEAM_LIST.route){
                    launchSingleTop = true
                }
            }
        ) }
    )
}

//TODO: Meter restrições para visualizar e interajir com os botões
/**
 * Conteúdo visual da Home Page (Stateless).
 *
 * Responsável por estruturar a UI, exibir estados de carregamento (Loading)
 * e alertas de conectividade (OfflineBanner).
 *
 * @param user O perfil do jogador carregado (pode ser null).
 * @param uiState O estado atual da UI (loading, erros, mensagens).
 * @param isOnline Booleano que indica se há conexão à internet.
 * @param onNavigateCreateTeam Ação ao clicar em "Criar Equipa".
 * @param onNavigationToRequests Ação ao clicar em "Pedidos de Adesão".
 * @param onNavigateToListTeams Ação ao clicar em "Lista de Equipas".
 */
@Composable
fun HomePageContent(
    user: PlayerProfileDto?,
    uiState: UiState,
    isOnline: Boolean,
    onNavigateCreateTeam: () -> Unit,
    onNavigationToRequests: () -> Unit,
    onNavigateToListTeams: () -> Unit
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
                onNavigateCreateTeam = onNavigateCreateTeam,
                onNavigationToRequests = onNavigationToRequests,
                onNavigateToListTeams = onNavigateToListTeams
            )
        }
    )
}

/**
 * Componente que desenha a lista vertical de ações e o cabeçalho de boas-vindas.
 *
 * @param user Dados do utilizador para exibir o nome.
 * @param onNavigateCreateTeam Callback para criação de equipa.
 * @param onNavigationToRequests Callback para ver pedidos.
 * @param onNavigateToListTeams Callback para listar equipas.
 */
@Composable
private fun HomePageDrawer(
    user: PlayerProfileDto?,
    onNavigateCreateTeam: () -> Unit,
    onNavigationToRequests: () -> Unit,
    onNavigateToListTeams: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState()),
        horizontalAlignment = Alignment.Start
    ) {
        Text(
            text = stringResource(id = R.string.welcome_app, user?.name ?: stringResource(R.string.user)),            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.padding(bottom = 24.dp)
        )

        Text(
            text = stringResource(id = R.string.quick_actions),
            style = MaterialTheme.typography.titleMedium,
            modifier = Modifier.padding(bottom = 8.dp)
        )

        if(user != null) {
            if (user.team?.id.isNullOrEmpty()) {
                ActionCardPlayerWithoutTeam(
                    onNavigateCreateTeam = onNavigateCreateTeam,
                    onNavigationToRequests = onNavigationToRequests,
                    onNavigateToListTeams = onNavigateToListTeams
                )
            } else  {
                ActionCardMemberTeam(onNavigateToListTeams = onNavigateToListTeams)
            }
        } else {
            ActionCardUnauthorizedUser(onNavigateToListTeams = onNavigateToListTeams)
        }
    }
}

/**
 * Secção de cartões para utilizadores não autenticados.
 * Exibe apenas a opção de listar equipas.
 *
 * @param onNavigateToListTeams Ação para navegar para a lista de equipas.
 */
@Composable
private fun ActionCardUnauthorizedUser(
    onNavigateToListTeams: () -> Unit
) {
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
 * Secção de cartões para jogadores autenticados sem equipa.
 * Exibe opções para criar equipa, listar equipas e ver pedidos.
 *
 * @param onNavigateCreateTeam Ação para criar equipa.
 * @param onNavigationToRequests Ação para ver pedidos.
 * @param onNavigateToListTeams Ação para listar equipas.
 */
@Composable
private fun ActionCardPlayerWithoutTeam(
    onNavigateCreateTeam: () -> Unit,
    onNavigationToRequests: () -> Unit,
    onNavigateToListTeams: () -> Unit
) {
    ActionCardSection(
        content = {
            ActionCardCreateTeam(onNavigateCreateTeam = onNavigateCreateTeam)
            ActionCardListTeam(
                onNavigateToListTeams = onNavigateToListTeams,
                subTitle = stringResource(id = R.string.button_description_list_teams_membership)
            )
            ActionCardListMembershipRequests(onNavigationToRequests = onNavigationToRequests)        }
    )
}

/**
 * Secção de cartões para membros de uma equipa.
 * Exibe apenas a opção de listar outras equipas (pois a gestão da própria equipa é feita noutro ecrã).
 *
 * @param onNavigateToListTeams Ação para listar equipas.
 */
@Composable
private fun ActionCardMemberTeam(
    onNavigateToListTeams: () -> Unit
)  {
    ActionCardSection(
        content = {
            ActionCardListTeam(
                onNavigateToListTeams = onNavigateToListTeams,
                subTitle = stringResource(id = R.string.button_description_list_teams_match)
            )
        }
    )
}

/**
 * Wrapper de layout para organizar os cartões de ação numa coluna com espaçamento padrão.
 *
 * @param content O conteúdo (cartões) a ser exibido dentro da coluna.
 */
@Composable
private fun ActionCardSection(content: @Composable () -> Unit) {
    Column(
        verticalArrangement = Arrangement.spacedBy(12.dp),
        modifier = Modifier.fillMaxWidth()
    ) {
        content()
    }
}

/**
 * Cartão específico para a ação de Criar Equipa.
 *
 * @param onNavigateCreateTeam Callback a executar no clique.
 */
@Composable
private fun ActionCardCreateTeam(onNavigateCreateTeam: () -> Unit) {
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
private fun ActionCardListTeam(
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
private fun ActionCardListMembershipRequests(onNavigationToRequests: () -> Unit) {
    ActionCard(
        title = stringResource(id = R.string.button_membership_request),
        subtitle = stringResource(id = R.string.button_description_membership_request),
        icon = Icons.Default.Person,
        onClick = onNavigationToRequests
    )
}

@Preview(name = "1. Unauth - English", group = "Unauthorized", showBackground = true, locale = "en")
@Preview(name = "1. Unauth - PT", group = "Unauthorized", showBackground = true, locale = "pt-rPT")
@Preview(name = "1. Unauth - Dark", group = "Unauthorized", showBackground = true, uiMode = Configuration.UI_MODE_NIGHT_YES)
@Composable
fun PreviewHomePageUnauthorized() {
    MaterialTheme {
        HomePageContent(
            user = null,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            onNavigateCreateTeam = {},
            onNavigationToRequests = {},
            onNavigateToListTeams = {}
        )
    }
}

@Preview(name = "2. No Team - English", group = "No Team", showBackground = true, locale = "en")
@Preview(name = "2. No Team - PT", group = "No Team", showBackground = true, locale = "pt-rPT")
@Preview(name = "2. No Team - Dark", group = "No Team", showBackground = true, uiMode = Configuration.UI_MODE_NIGHT_YES)
@Composable
fun PreviewHomePagePlayerNoTeam() {
    MaterialTheme {
        HomePageContent(
            user = HomePageMock.mockUserNoTeam,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            onNavigateCreateTeam = {},
            onNavigationToRequests = {},
            onNavigateToListTeams = {}
        )
    }
}

// CENÁRIO 3: Jogador COM Equipa
@Preview(name = "3. With Team - English", group = "With Team", showBackground = true, locale = "en")
@Preview(name = "3. With Team - PT", group = "With Team", showBackground = true, locale = "pt-rPT")
@Preview(name = "3. With Team - Dark", group = "With Team", showBackground = true, uiMode = Configuration.UI_MODE_NIGHT_YES)
@Composable
fun PreviewHomePagePlayerWithTeam() {
    MaterialTheme {
        HomePageContent(
            user = HomePageMock.mockUserWithTeam,
            uiState = UiStateMock.mockUiStateContent,
            isOnline = true,
            onNavigateCreateTeam = {},
            onNavigationToRequests = {},
            onNavigateToListTeams = {}
        )
    }
}
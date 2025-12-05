package com.example.amfootball.ui.components.navBar

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarDefaults
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.example.amfootball.navigation.objects.AppRouteInfo
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.buttons.NavigateButton

/**
 * Barra de navegação inferior principal da aplicação.
 *
 * Implementa o padrão de navegação com 5 destinos, onde o item central ([Routes.BottomNavBarRoutes.PAGE_OPTIONS])
 * atua como um botão de ação flutuante (FAB) integrado, que abre um menu modal (BottomSheet) em vez
 * de navegar diretamente para um ecrã.
 *
 * @param navController O controlador de navegação para alternar entre os ecrãs principais.
 * @param onShowBottomSheet Callback disparado ao clicar no botão central de "Opções".
 * @param currentSelectedRoute A rota atual (string) para destacar o ícone correto.
 * @param onRouteSelected Callback para atualizar o estado da rota selecionada no pai.
 */
@Composable
fun MainBottomNavBar(
    navController: NavHostController,
    onShowBottomSheet: () -> Unit,
    currentSelectedRoute: String,
    onRouteSelected: (String) -> Unit
) {
    NavigationBar(windowInsets = NavigationBarDefaults.windowInsets) {
        Routes.BottomNavBarRoutes.entries.forEach { destination ->
            NavigationBarItem(
                selected = currentSelectedRoute == destination.route,
                onClick = {
                    if (destination == Routes.BottomNavBarRoutes.PAGE_OPTIONS) {
                        onShowBottomSheet()
                    } else {
                        onRouteSelected(destination.route)
                        navController.navigate(destination.route) {
                            launchSingleTop = true
                            restoreState = true
                            popUpTo(navController.graph.startDestinationId) { saveState = true }
                        }
                    }
                },
                icon = {
                    Icon(
                        destination.icon,
                        contentDescription = stringResource(destination.contentDescription)
                    )
                },
                label = {
                    Text(
                        stringResource(destination.labelResId),
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                }
            )
        }
    }
}

/**
 * Conteúdo dinâmico do Modal Bottom Sheet (Menu de Opções).
 *
 * Este componente decide quais os atalhos de navegação a mostrar com base no contexto atual
 * ([currentScreenRoute]). Permite acesso rápido a funcionalidades secundárias que não cabem na Bottom Bar.
 *
 * **Lógica de Exibição:**
 * - Se estiver na **Home Geral**: Mostra atalhos para procurar equipas, jogadores e leaderboard.
 * - Se estiver na **Home de Equipa**: Mostra atalhos de gestão (Calendário, Pedidos, Membros).
 *
 * @param modifier Modificador de layout.
 * @param navController Controlador para navegar ao clicar nos botões do grid.
 * @param currentScreenRoute A rota onde o utilizador estava quando abriu o menu.
 * @param teamId O ID da equipa (opcional), necessário para navegar para rotas que exigem parâmetros (ex: Calendário).
 */
@Composable
fun BottomSheetContent(
    modifier: Modifier = Modifier,
    navController: NavHostController,
    currentScreenRoute: String?,
    teamId: String? = null
) {
    val buttonsToShow = mutableListOf<AppRouteInfo>()

    buttonsToShow.addAll(
        listOf(
            Routes.GeralRoutes.LEADERBOARD,
            )
    )
    when (currentScreenRoute) {
        Routes.BottomNavBarRoutes.HOMEPAGE.route -> {
            buttonsToShow.addAll(
                listOf(
                    Routes.PlayerRoutes.TEAM_LIST,
                    Routes.PlayerRoutes.PLAYER_LIST,
                    Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST,
                )
            )
        }

        Routes.BottomNavBarRoutes.HOMEPAGE_TEAM.route -> {
            buttonsToShow.addAll(
                listOf(
                    Routes.TeamRoutes.HOMEPAGE,
                    Routes.TeamRoutes.CALENDAR,
                    Routes.TeamRoutes.TEAM_PROFILE,

                    Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST,
                    Routes.TeamRoutes.MEMBERLIST,
                    Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM,

                    Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE,
                    Routes.TeamRoutes.LIST_MATCH_INVITES,
                    Routes.TeamRoutes.SEARCH_COMPETIVE_MATCH,

                    Routes.TeamRoutes.LIST_POST_PONE_MATCH
                )
            )
        }

        Routes.BottomNavBarRoutes.CHAT_LIST.route -> {

        }

        Routes.BottomNavBarRoutes.USER_PROFILE.route -> {

        }
    }

    LazyVerticalGrid(
        columns = GridCells.Adaptive(minSize = 90.dp),
        modifier = modifier
            .fillMaxWidth()
            .padding(16.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
        horizontalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        items(buttonsToShow) { routeInfo ->
            NavigateButton(
                icon = routeInfo.icon,
                label = stringResource(id = routeInfo.labelResId),
                onClick = {
                    when (routeInfo) {
                        Routes.TeamRoutes.CALENDAR -> {
                            if (teamId != null) {
                                navController.navigate("${routeInfo.route}/{${teamId}}")
                            } else {
                                println("Erro: Tentativa de abrir calendário sem ID de equipa")
                            }
                        }

                        else -> {
                            navController.navigate(routeInfo.route)
                        }
                    }
                }
            )
        }
    }
}
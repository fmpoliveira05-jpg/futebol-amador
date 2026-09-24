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
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.example.amfootball.domains.enums.UserRole
import com.example.amfootball.ui.components.buttons.NavigateButton
import com.example.amfootball.ui.navigation.objects.AppRouteInfo
import com.example.amfootball.ui.navigation.objects.Routes

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
    onRouteSelected: (String) -> Unit,
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
 * Conteúdo dinâmico do Modal Bottom Sheet (Menu de Opções Secundárias).
 *
 * Exibe um grid de botões de navegação, cujo conteúdo é determinado dinamicamente
 * com base na rota atual ([currentScreenRoute]) e no papel do utilizador ([role]).
 * Isto permite mostrar atalhos de gestão de equipa apenas quando o utilizador está
 * na área da equipa e tem permissões de administrador.
 *
 * @param modifier Modificador de layout.
 * @param navController Controlador para navegar ao clicar nos botões do grid.
 * @param currentScreenRoute A rota onde o utilizador estava quando abriu o menu.
 * @param teamId O ID da equipa (opcional), usado para construir rotas que exigem este parâmetro (ex: Calendário).
 * @param role O papel do utilizador ([UserRole]) que determina a visibilidade de certas opções de gestão.
 */
@Composable
fun BottomSheetContent(
    modifier: Modifier = Modifier,
    navController: NavHostController,
    currentScreenRoute: String?,
    teamId: String? = null,
    role: UserRole
) {
    val buttonsToShow = remember(currentScreenRoute, role, teamId) {
        val list = mutableListOf<AppRouteInfo>()

        when (currentScreenRoute) {
            Routes.BottomNavBarRoutes.HOMEPAGE.route -> {
                list.add(Routes.GeralRoutes.HOMEPAGE)
                list.add(Routes.PlayerRoutes.TEAM_LIST)
                list.add(Routes.PlayerRoutes.PLAYER_LIST)
                list.add(Routes.GeralRoutes.LEADERBOARD)

                if (role == UserRole.PLAYER_WITHOUT_TEAM) {
                    list.add(Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST)
                    list.add(Routes.PlayerRoutes.TEAM_LIST_MEMBERSHIP_REQUEST)
                }
            }

            Routes.BottomNavBarRoutes.HOMEPAGE_TEAM.route -> {
                if (role == UserRole.ADMIN_TEAM || role == UserRole.MEMBER_TEAM) {
                    list.add(Routes.TeamRoutes.HOMEPAGE)
                    list.add(Routes.TeamRoutes.CALENDAR)
                    list.add(Routes.TeamRoutes.TEAM_PROFILE)
                    list.add(Routes.TeamRoutes.MEMBERLIST)
                }

                if (role == UserRole.ADMIN_TEAM) {
                    list.add(Routes.TeamRoutes.LIST_MATCH_INVITES)
                    list.add(Routes.TeamRoutes.LIST_POST_PONE_MATCH)
                    list.add(Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST)

                    list.add(Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM)
                    list.add(Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE)
                    list.add(Routes.TeamRoutes.SEARCH_COMPETIVE_MATCH)

                }
            }
        }
        list
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
                onClick = { navController.navigate(routeInfo.route) }
            )
        }
    }
}
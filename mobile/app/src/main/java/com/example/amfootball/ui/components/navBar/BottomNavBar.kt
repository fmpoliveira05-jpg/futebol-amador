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
                icon = { Icon(destination.icon, contentDescription = stringResource(destination.contentDescription)) },
                label = { Text(stringResource(destination.labelResId), maxLines = 1, overflow = TextOverflow.Ellipsis) }
            )
        }
    }
}

@Composable
fun BottomSheetContent(
    modifier: Modifier = Modifier,
    navController: NavHostController,
    currentScreenRoute: String?,
    teamId: String? = null
){
    val buttonsToShow = mutableListOf<AppRouteInfo>()

    when (currentScreenRoute) {
        Routes.BottomNavBarRoutes.HOMEPAGE.route -> {
            buttonsToShow.addAll(
                listOf(
                    Routes.GeralRoutes.HOMEPAGE,
                    Routes.PlayerRoutes.TEAM_LIST,
                    Routes.PlayerRoutes.PLAYER_LIST,
                    Routes.GeralRoutes.LEADERBOARD,
                    Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST,
                )
            )
        }
        Routes.BottomNavBarRoutes.TEAM_LIST.route -> {
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
        /*

        * */
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


@Composable
private fun HomePageContentBottonSheet(modifier: Modifier,navController: NavHostController) {
    //NavigateButton()
}
@Composable
private fun TeamListContentBottomSheet(modifier: Modifier,navController: NavHostController){
    /*
    NavigateButton(
        icon = Routes.TeamRoutes.TEAM_LIST.icon,
        label = stringResource(Routes.TeamRoutes.TEAM_LIST.contentDescription),
        onClick = {
            navController.navigate(Routes.TeamRoutes.TEAM_LIST.route)
        }
    )
    * */
    NavigateButton(
        icon = Routes.TeamRoutes.HOMEPAGE.icon,
        label = stringResource(id = Routes.TeamRoutes.HOMEPAGE.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.HOMEPAGE.route)
        }
    )
    NavigateButton(
        icon = Routes.TeamRoutes.CALENDAR.icon,
        label = stringResource(id = Routes.TeamRoutes.CALENDAR.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.CALENDAR.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.icon,
        label = stringResource(id = Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.CALENDAR.icon,
        label = stringResource(id = Routes.TeamRoutes.CALENDAR.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.CALENDAR.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.icon,
        label = stringResource(id = Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE.icon,
        label = stringResource(id = Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.LIST_MATCH_INVITES.icon,
        label = stringResource(id = Routes.TeamRoutes.LIST_MATCH_INVITES.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.LIST_MATCH_INVITES.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.SEND_MATCH_INVITE.icon,
        label = stringResource(id = Routes.TeamRoutes.SEND_MATCH_INVITE.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.SEND_MATCH_INVITE.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.MEMBERLIST.icon,
        label = stringResource(id = Routes.TeamRoutes.MEMBERLIST.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.MEMBERLIST.route)
        }
    )

    NavigateButton(
        icon = Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM.icon,
        label = stringResource(id = Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM.labelResId),
        onClick = {
            navController.navigate(Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM.route)
        }
    )
}



@Composable
private fun ChatContentBottomSheet(modifier: Modifier,navController: NavHostController){

}

@Composable
fun PlayerProfileContentBottomSheet(modifier: Modifier,navController: NavHostController){

}

@Composable
fun CommonContentBottomSheet(modifier: Modifier,navController: NavHostController){
    NavigateButton(modifier ,Routes.PlayerRoutes.PLAYER_LIST.icon, stringResource(Routes.PlayerRoutes.PLAYER_LIST.contentDescription), onClick = {navController.navigate(Routes.PlayerRoutes.PLAYER_LIST.route)})

}
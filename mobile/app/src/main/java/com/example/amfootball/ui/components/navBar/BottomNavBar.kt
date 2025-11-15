package com.example.amfootball.ui.components.NavBar

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarDefaults
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.currentBackStackEntryAsState
import com.example.amfootball.navigation.Objects.Routes

@Composable
fun MainBottomNavBar(
    navController: NavHostController,
    onShowBottomSheet: () -> Unit
) {
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    NavigationBar(windowInsets = NavigationBarDefaults.windowInsets) {
        Routes.BottomNavBarRoutes.entries.forEach { destination ->
            NavigationBarItem(
                selected = currentRoute == destination.route,
                onClick = {
                    if (destination == Routes.BottomNavBarRoutes.PAGE_OPTIONS) {
                        onShowBottomSheet()
                    } else {
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
    modifier: Modifier,
    navController: NavHostController,
    currentScreenRoute: String?
){
    Row(
        modifier = modifier
            .fillMaxWidth()
            .padding(16.dp),
        horizontalArrangement = Arrangement.SpaceEvenly,
        verticalAlignment = Alignment.CenterVertically
    ) {
        CommonContentBottomSheet(navController)

        when (currentScreenRoute) {
            Routes.BottomNavBarRoutes.HOMEPAGE.route -> {
                HomePageContentBottonSheet(navController)
            }

            Routes.BottomNavBarRoutes.TEAM_LIST.route -> {
                TeamListContentBottomSheet(navController)
            }

            Routes.BottomNavBarRoutes.CHAT_LIST.route -> {
                ChatContentBottomSheet(navController)
            }

            Routes.BottomNavBarRoutes.USER_PROFILE.route -> {
                PlayerProfileContentBottomSheet(navController)
            }
        }
    }
}

@Composable
private fun HomePageContentBottonSheet(navController: NavHostController) {
    //NavigateButton()
}
@Composable
private fun TeamListContentBottomSheet(navController: NavHostController){
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
private fun NavigateButton(icon: ImageVector, label: String, onClick: () -> Unit) {
    TODO("Not yet implemented")
}

@Composable
private fun ChatContentBottomSheet(navController: NavHostController){

}

@Composable
fun PlayerProfileContentBottomSheet(navController: NavHostController){

}

@Composable
fun CommonContentBottomSheet(navController: NavHostController){
    NavigateButton(Routes.PlayerRoutes.PLAYER_LIST.icon, stringResource(Routes.PlayerRoutes.PLAYER_LIST.contentDescription), onClick = {navController.navigate(Routes.PlayerRoutes.PLAYER_LIST.route)})

}
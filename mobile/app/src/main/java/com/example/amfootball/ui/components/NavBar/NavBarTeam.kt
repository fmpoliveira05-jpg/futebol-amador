package com.example.amfootball.ui.components.NavBar

import androidx.compose.foundation.layout.RowScope
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.List
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.NavigationItem
import com.example.amfootball.navigation.Objects.NavBar.RouteNavBarHomePage
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.ui.screens.Lists.ListPlayersScreen
import com.example.amfootball.ui.screens.Lists.ListTeamScreen
import com.example.amfootball.ui.screens.MatchInvite.ListMatchInviteScreen
import com.example.amfootball.ui.screens.MembershipRequest.ListMemberShipRequest
import com.example.amfootball.ui.screens.Team.CalendarScreen
import com.example.amfootball.ui.screens.Team.HomePageTeamScreen
import com.example.amfootball.ui.screens.Team.ListMembersScreen
import com.example.amfootball.ui.theme.AMFootballTheme
import kotlin.collections.arrayListOf

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NavigatonDrawerTeam(globalNavController: NavHostController,
                        isLoggedIn: Boolean,
                        onLogout: () -> Unit){
    val drawerItemList = prepareNavigationDrawerItems()
    val internalNavController = rememberNavController()

    NavigatonDrawer(
        itens = drawerItemList,
        titleNavBar = "NavBar Team",
        scaffoldContent = { innerNav ->
            ScaffoldContentTeamNavBar(
                navController = innerNav,
                globalNavController = globalNavController
            )
        },
        internalNavController = internalNavController,
        globalNavController = globalNavController,
        topBarActions = {
            HomePageTeamTopBarActions(
                isLoggedIn = isLoggedIn,
                onLogout = onLogout,
            )
        }
    )
}
//Metodo que define todas as rotas da NavBar

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ScaffoldContentTeamNavBar(navController: NavHostController,
                              globalNavController: NavHostController
) {
    NavHost(navController = navController, startDestination = RoutesNavBarTeam.HOME_PAGE_TEAM) {
        composable(RoutesNavBarTeam.HOME_PAGE_TEAM) {
            HomePageTeamScreen(globalNavController = globalNavController)
        }
        composable(RoutesNavBarTeam.CALENDAR) {
            CalendarScreen(globalNavController = globalNavController)
        }
        composable(RoutesNavBarTeam.LIST_MEMBERS) {
            ListMembersScreen()
        }
        composable(RoutesNavBarTeam.LIST_MEMBERSHIP_REQUEST) {
            ListMemberShipRequest()
        }
        composable(RoutesNavBarTeam.SEARCH_PLAYERS_WITHOU_TEAM){
            ListPlayersScreen()
        }
        composable(RoutesNavBarTeam.SEARCH_TEAMS_TO_MATCH_INVITE) {
            ListTeamScreen(navHostController = globalNavController)
        }
        composable(RoutesNavBarTeam.LIST_MATCHINVITE) {
            ListMatchInviteScreen()
        }
    }
}

//Definição dos itens da navBar
@Composable
private fun prepareNavigationDrawerItems(): List<NavigationItem> {
    val drawerItemsList = arrayListOf<NavigationItem>()

    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.item_home),
        description = stringResource(id = R.string.item_home_description),
        icon = Icons.Filled.Home,
        route = RouteNavBarHomePage.HOME_PAGE,
        isGlobalRoute = true))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.navbar_home_page_team),
        description = stringResource(id = R.string.description_navbar_homepage_team),
        icon = Icons.Filled.Home,
        route = RoutesNavBarTeam.HOME_PAGE_TEAM,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.navbar_calendar),
        description = stringResource(id = R.string.description_navbar_calendar),
        icon = Icons.Filled.DateRange,
        route = RoutesNavBarTeam.CALENDAR,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id= R.string.navbar_members_list),
        description = stringResource(id = R.string.description_navbar_members_list),
        icon = Icons.Filled.List,
        route = RoutesNavBarTeam.LIST_MEMBERS,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.navbar_players_without_team_list),
        description = stringResource(id = R.string.description_navbar_players_without_team_list),
        icon = Icons.Filled.Search,
        route = RoutesNavBarTeam.SEARCH_PLAYERS_WITHOU_TEAM,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.navbar_memberships_list),
        description = stringResource(id = R.string.description_navbar_memberships_list),
        icon = Icons.Filled.List,
        route = RoutesNavBarTeam.LIST_MEMBERSHIP_REQUEST,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.navbar_teams_to_matchinvite_list),
        description = stringResource(id = R.string.description_navbar_teams_to_matchinvite_list),
        icon = Icons.Filled.List,
        route = RoutesNavBarTeam.SEARCH_TEAMS_TO_MATCH_INVITE,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.navbar_matchinvite_list),
        description = stringResource(id = R.string.description_navbar_matchinvite_list),
        icon = Icons.Filled.List,
        route = RoutesNavBarTeam.LIST_MATCHINVITE,
        isGlobalRoute = false))
    return drawerItemsList
}

@Composable
private fun RowScope.HomePageTeamTopBarActions(
    isLoggedIn: Boolean,
    onLogout: () -> Unit
) {
    if (isLoggedIn) {
        IconButton(onClick = onLogout) {
            Icon(
                imageVector = Icons.AutoMirrored.Filled.Logout,
                contentDescription = "Logout"
            )
        }
    }
}

@Preview(name = "Team Navigation - EN", locale = "en", showBackground = true)
@Preview(name = "Team Navigation - PT", locale = "pt", showBackground = true)
@Composable
fun NavigatonDrawerTeamPreviewLogged() {
    AMFootballTheme {
        NavigatonDrawerTeam(
            globalNavController = rememberNavController(),
            isLoggedIn = true,
            onLogout = {}
        )
    }
}

@Preview(name = "Team Navigation - EN", locale = "en", showBackground = true)
@Preview(name = "Team Navigation - PT", locale = "pt", showBackground = true)
@Composable
fun NavigatonDrawerTeamPreviewLogout() {
    AMFootballTheme {
        NavigatonDrawerTeam(
            globalNavController = rememberNavController(),
            isLoggedIn = false,
            onLogout = {}
        )
    }
}
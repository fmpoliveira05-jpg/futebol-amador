package com.example.amfootball.ui.components.NavBar

import androidx.compose.foundation.layout.RowScope
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material.icons.filled.Home
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.NavigationItem
import com.example.amfootball.navigation.Objects.NavBar.RouteNavBarHomePage
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.Team.CalendarScreen

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
            ScaffoldContentTeamNavBar(navController = innerNav)
        },
        internalNavController = internalNavController, // Passa o interno
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
fun ScaffoldContentTeamNavBar(navController: NavHostController) {
    NavHost(navController = navController, startDestination = RouteNavBarHomePage.HOME_PAGE) {
        composable(RoutesNavBarTeam.CALENDAR) {
            CalendarScreen()
        }
        composable(RouteNavBarHomePage.HOME_PAGE) {
            HomePageScreen()
        }
    }
}

//Definição dos itens da navBar
@Composable
private fun prepareNavigationDrawerItems(): List<NavigationItem> {
    val drawerItemsList = arrayListOf<NavigationItem>()

    drawerItemsList.add(NavigationItem(label = stringResource(R.string.navbar_home_page_team),
        description = stringResource(R.string.description_NavBar_HomePageTeam),
        icon = Icons.Filled.Home,
        route = RoutesNavBarTeam.HOME_PAGE_TEAM,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(R.string.navbar_Calendar),
        description = stringResource(R.string.descritpion_NavBar_Calendar),
        icon = Icons.Filled.DateRange,
        route = RoutesNavBarTeam.CALENDAR,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = "Home",
        description = "Página inicial",
        icon = Icons.Filled.Home,
        route = RouteNavBarHomePage.HOME_PAGE,
        isGlobalRoute = true))
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
package com.example.amfootball.ui.components.NavBar

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.navigation.AppGraphRoute
import com.example.amfootball.navigation.RouteNavBarHomePage
import com.example.amfootball.navigation.RoutesNavBarTeam

@Composable
fun MainNavigation() {
    val globalNavController = rememberNavController()
    NavHost(
        navController = globalNavController,
        startDestination = RouteNavBarHomePage.HOME_PAGE
    ) {
        //Chama a NavBar da home page
        composable(RouteNavBarHomePage.HOME_PAGE) {
            NavigatonDrawerNavBarHomePage(globalNavController = globalNavController)
        }

        //Chama a NavBar da paguna da team
        composable(RoutesNavBarTeam.HOME_PAGE_TEAM) {
            NavigatonDrawerTeam(globalNavController = globalNavController)
        }
    }
}
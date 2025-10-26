package com.example.amfootball.ui.components.NavBar

import androidx.compose.runtime.Composable
import androidx.navigation.NavHost
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.navigation.AppGraphRoute

@Composable
fun DrawPrincipalNavBar() {
    val appNavController = rememberNavController()

    NavHost(
        navController = appNavController,
        startDestination = AppGraphRoute.HOME_NAV
    ) {
        composable(AppGraphRoute.HOME_NAV) {
            NavigatonDrawerNavBarHomePage(appNavController = appNavController)
        }
        composable(AppGraphRoute.TEAM_NAV) {
            NavigatonDrawerTeam(appNavController = appNavController)
        }
    }
}
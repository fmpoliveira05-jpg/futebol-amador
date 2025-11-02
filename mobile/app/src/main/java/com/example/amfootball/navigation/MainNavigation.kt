package com.example.amfootball.navigation

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.navigation.Objects.NavBar.RouteNavBarHomePage
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.ui.screens.user.LoginScreen
import com.example.amfootball.ui.screens.user.SignInScreen
import androidx.navigation.NavGraphBuilder
import com.example.amfootball.navigation.Objects.AutRoutes
import com.example.amfootball.navigation.Objects.UserPages
import com.example.amfootball.ui.components.NavBar.NavigatonDrawerNavBarHomePage
import com.example.amfootball.ui.components.NavBar.NavigatonDrawerTeam
import com.example.amfootball.ui.screens.user.ProfileScreen

@Composable
fun MainNavigation() {
    val globalNavController = rememberNavController()

    //Simulação Login
    // Na tua app real, isto viria de um ViewModel (ex: authViewModel.isLoggedIn)
    var isLoggedIn by remember { mutableStateOf(false) }

    val onLogoutClick: () -> Unit = {
        isLoggedIn = false
        globalNavController.navigate(RouteNavBarHomePage.HOME_PAGE) {
            popUpTo(0) // Limpa a stack de navegação
        }
    }

    NavHost(
        navController = globalNavController,
        startDestination = RouteNavBarHomePage.HOME_PAGE
    ) {
        NavBars(
            globalNavController = globalNavController,
            isLoggedIn = isLoggedIn,
            onLogoutClick = onLogoutClick
        )

        Pages(
            globalNavController = globalNavController
        )
    }
}

private fun NavGraphBuilder.NavBars(
    globalNavController: NavHostController,
    isLoggedIn: Boolean,
    onLogoutClick: () -> Unit
) {
    composable(RouteNavBarHomePage.HOME_PAGE) {
        NavigatonDrawerNavBarHomePage(
            globalNavController = globalNavController,
            isLoggedIn = isLoggedIn,
            onLogout = onLogoutClick
        )
    }

    composable(RoutesNavBarTeam.HOME_PAGE_TEAM) {
        NavigatonDrawerTeam(
            globalNavController = globalNavController,
            isLoggedIn = isLoggedIn,
            onLogout = onLogoutClick
        )
    }
}

private fun NavGraphBuilder.Pages(globalNavController: NavHostController) {
    //Provavelmente para futuro vai ser importante o NavHostController
    composable(UserPages.PERFIL_USER) {
        ProfileScreen(globalNavController)
    }

    AutPages(globalNavController = globalNavController)
}

/**
 * Paginas de autentificação
 * */
private fun NavGraphBuilder.AutPages(globalNavController: NavHostController) {
    //Depois meter para os dois verificações para só user não autenticados
    // poderem aceder a esta página
    composable(AutRoutes.LOGIN) {
        LoginScreen(globalNavController)
    }

    composable(AutRoutes.SIGN_IN) {
        SignInScreen()
    }
}
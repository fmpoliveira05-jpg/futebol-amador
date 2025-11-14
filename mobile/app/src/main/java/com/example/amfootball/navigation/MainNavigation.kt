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
import com.example.amfootball.navigation.objects.navBar.RouteNavBarHomePage
import com.example.amfootball.navigation.objects.navBar.RoutesNavBarTeam
import com.example.amfootball.ui.screens.user.LoginScreen
import com.example.amfootball.ui.screens.user.SignUpScreen
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavType
import androidx.navigation.navArgument
import com.example.amfootball.navigation.objects.pages.AutRoutes
import com.example.amfootball.navigation.objects.pages.CrudTeamRoutes
import com.example.amfootball.navigation.objects.pages.MatchInviteRoutes
import com.example.amfootball.navigation.objects.RotasUser
import com.example.amfootball.ui.components.navBar.NavigatonDrawerNavBarHomePage
import com.example.amfootball.ui.components.navBar.NavigatonDrawerTeam
import com.example.amfootball.ui.screens.team.FormTeamScreen
import com.example.amfootball.ui.screens.team.ProfileTeamScreen
import com.example.amfootball.ui.screens.user.ProfileScreen
import com.example.amfootball.ui.screens.matchInvite.FormMatchInviteScreen

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

//Função que declara todas as rotas da app
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

/**
 * Função que declara todas as páginas da app
 * */
private fun NavGraphBuilder.Pages(globalNavController: NavHostController) {
    AutPages(globalNavController = globalNavController)

    UserPages(globalNavController = globalNavController)

    CrudTeamPages(globalNavController = globalNavController)

    MatchInivitePages(globalNavController = globalNavController)
}

/**
 * Paginas de autentificação
 * */
private fun NavGraphBuilder.AutPages(globalNavController: NavHostController) {
    //Depois meter para os dois verificações para só user não autenticados
    composable(AutRoutes.LOGIN) {
        LoginScreen(globalNavController)
    }

    composable(AutRoutes.SIGN_IN) {
        SignUpScreen(globalNavController)
    }
}

/**
 * Paginas do Utilizador
 * */
private fun NavGraphBuilder.UserPages(globalNavController: NavHostController) {
    composable(RotasUser.USER_PROFILE) {
        ProfileScreen(globalNavController)
    }
}

/**
 * Páginas do CRUD da Equipa
 * */
private fun NavGraphBuilder.CrudTeamPages(globalNavController: NavHostController){
    composable(route = CrudTeamRoutes.CREATE_TEAM) {
        FormTeamScreen(navHostController = globalNavController)
    }

    composable(
        route = CrudTeamRoutes.PROFILE_TEAM_URL,
        arguments = listOf(
            navArgument(CrudTeamRoutes.ARG_TEAM_ID) {
                type = NavType.StringType
            }
        )
    ) { navBackStackEntry -> //Tem os argumentos

        val idTeam = navBackStackEntry.arguments?.getString(CrudTeamRoutes.ARG_TEAM_ID)

        if (idTeam != null) {
            ProfileTeamScreen(
                navHostController = globalNavController,
                idTeam = idTeam
            )
        } else {
            //Posso depois aqui fazer algo personalizado
            print("Error")
        }
    }
}

/**
 * Páginas de MatchInvite
 * */
private fun NavGraphBuilder.MatchInivitePages(globalNavController: NavHostController){
    composable(route = MatchInviteRoutes.SEND_MATCH_INVITE) {
        FormMatchInviteScreen(navHostController = globalNavController)
    }
}
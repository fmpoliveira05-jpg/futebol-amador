package com.example.amfootball.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.ui.screens.User.LoginScreen
import com.example.amfootball.ui.screens.User.SignUpScreen
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavType
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.navArgument
import com.example.amfootball.navigation.Objects.Pages.CrudTeamRoutes
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.ui.components.AppModalBottomSheet
import com.example.amfootball.ui.components.NavBar.BottomSheetContent
import com.example.amfootball.ui.components.NavBar.MainBottomNavBar
import com.example.amfootball.ui.components.NavBar.NavigatonDrawerNavBarHomePage
import com.example.amfootball.ui.components.NavBar.NavigatonDrawerTeam
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.MatchInvite.SendMatchInviteScreen
import com.example.amfootball.ui.screens.Team.CrudTeam.CreateTeamScreen
import com.example.amfootball.ui.screens.Team.HomePageTeamScreen
import com.example.amfootball.ui.screens.Team.ProfileTeamScreen
import com.example.amfootball.ui.screens.User.ProfileScreen


@Composable
fun MainNavigation() {
    val globalNavController = rememberNavController()

    var isLoggedIn by remember { mutableStateOf(false) }

    var showBottomSheet by remember { mutableStateOf(false) }

    val onLogoutClick: () -> Unit = {
        isLoggedIn = false
        globalNavController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
            popUpTo(0) // Limpa toda a stack
        }
    }

    Scaffold(
        topBar = {
            MainTopAppBar(
                navController = globalNavController,
                isLoggedIn = isLoggedIn
            )
        },
        bottomBar = {
            MainBottomNavBar(
                navController = globalNavController,
                onShowBottomSheet = { showBottomSheet = true }
            )
        }
    ) { innerPadding ->
        NavHost(
            navController = globalNavController,
            startDestination = Routes.GeralRoutes.HOMEPAGE.route,
            modifier = Modifier.padding(innerPadding) // Aplica o padding do Scaffold!
        ) {
            composable(Routes.GeralRoutes.HOMEPAGE.route) {
                HomePageScreen(globalNavController = globalNavController)
            }
            composable(Routes.TeamRoutes.HOMEPAGE.route) {
                HomePageTeamScreen(globalNavController = globalNavController)
            }

            Pages(globalNavController = globalNavController)
        }

        // O BottomSheet fica aqui, fora do NavHost, controlado pelo estado local
        if (showBottomSheet) {
            val navBackStackEntry by globalNavController.currentBackStackEntryAsState()
            val currentRoute = navBackStackEntry?.destination?.route

            AppModalBottomSheet(onDismiss = { showBottomSheet = false }) {
                BottomSheetContent(Modifier, globalNavController, currentRoute)
            }
        }
    }
}

/*
private fun NavGraphBuilder.NavBars(
    globalNavController: NavHostController,
    isLoggedIn: Boolean,
    onLogoutClick: () -> Unit
) {
    composable(Routes.GeralRoutes.HOMEPAGE.route) {
        NavigatonDrawerNavBarHomePage(
            globalNavController = globalNavController,
            isLoggedIn = isLoggedIn,
            onLogout = onLogoutClick
        )
    }

    composable(Routes.TeamRoutes.HOMEPAGE.route) {
        NavigatonDrawerTeam(
            globalNavController = globalNavController,
            isLoggedIn = isLoggedIn,
            onLogout = onLogoutClick
        )
    }
}

 */

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
    composable(Routes.UserRoutes.LOGIN.route) {
        LoginScreen(globalNavController)
    }

    composable(Routes.UserRoutes.SIGNUP.route) {
        SignUpScreen(globalNavController)
    }
}

/**
 * Paginas do Utilizador
 * */
private fun NavGraphBuilder.UserPages(globalNavController: NavHostController) {
    composable(Routes.UserRoutes.PROFILE.route) {
        ProfileScreen(globalNavController)
    }
}

/**
 * Páginas do CRUD da Equipa
 * */
private fun NavGraphBuilder.CrudTeamPages(globalNavController: NavHostController){
    composable(route = CrudTeamRoutes.CREATE_TEAM) {
        CreateTeamScreen(navHostController = globalNavController)
    }

    composable(
        route = CrudTeamRoutes.PROFILE_TEAM_URL,
        arguments = listOf(
            navArgument(CrudTeamRoutes.ARG_TEAM_ID) {
                type = NavType.StringType
            }
        )
    ) { navBackStackEntry ->

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
    composable(route = Routes.TeamRoutes.SEND_MATCH_INVITE.route) {
        SendMatchInviteScreen(navHostController = globalNavController)
    }
}
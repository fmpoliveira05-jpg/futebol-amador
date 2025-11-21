package com.example.amfootball.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.ui.screens.user.LoginScreen
import com.example.amfootball.ui.screens.user.SignUpScreen
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavType
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.navArgument
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.navigation.objects.page.CrudTeamRoutes
import com.example.amfootball.ui.components.AppModalBottomSheet
import com.example.amfootball.ui.components.NavBar.BottomSheetContent
import com.example.amfootball.ui.components.NavBar.MainBottomNavBar
import com.example.amfootball.ui.components.NavBar.MainTopAppBar
import com.example.amfootball.ui.screens.Chat.ChatListScreen
import com.example.amfootball.ui.screens.Chat.GroupChatFootballPreview
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.LeaderboardScreen
import com.example.amfootball.ui.screens.lists.ListMemberShipRequest
import com.example.amfootball.ui.screens.lists.ListPlayersScreen
import com.example.amfootball.ui.screens.lists.ListTeamScreen
import com.example.amfootball.ui.screens.match.CancelMatchScreen
import com.example.amfootball.ui.screens.match.FinishMatchScreen
import com.example.amfootball.ui.screens.match.MatchMakerScreen
import com.example.amfootball.ui.screens.matchInvite.FormMatchInviteScreen
import com.example.amfootball.ui.screens.matchInvite.ListMatchInviteScreen
import com.example.amfootball.ui.screens.settings.PreferenceScreen
import com.example.amfootball.ui.screens.settings.SettingsScreen
import com.example.amfootball.ui.screens.team.CalendarScreen
import com.example.amfootball.ui.screens.team.FormTeamScreen
import com.example.amfootball.ui.screens.team.HomePageTeamScreen
import com.example.amfootball.ui.screens.team.ListMembersScreen
import com.example.amfootball.ui.screens.team.ListPostPoneMatchScreen
import com.example.amfootball.ui.screens.team.ProfileTeamScreen
import com.example.amfootball.ui.screens.user.ProfileScreen
import com.example.amfootball.ui.viewModel.lists.ListPlayerViewModel
import com.example.amfootball.ui.viewModel.team.ProfileTeamViewModel
import com.example.amfootball.ui.viewModel.user.ProfilePlayerViewModel
import com.example.amfootball.utils.extensions.composableProtected


@Composable
fun MainNavigation() {
    val globalNavController = rememberNavController()

    var isLoggedIn by remember { mutableStateOf(false) }
    val sessionManager by remember { mutableStateOf(SessionManager(context = globalNavController.context)) }
    var showBottomSheet by remember { mutableStateOf(false) }
    var selectedBottomNavRoute by remember { mutableStateOf(Routes.BottomNavBarRoutes.HOMEPAGE.route) }
    val currentUserId by remember { mutableStateOf(sessionManager.getUserProfile()) }

    if (currentUserId != null){
        isLoggedIn = true
    }

    Scaffold(
        topBar = {
            MainTopAppBar(
                navController = globalNavController,
                isLoggedIn = isLoggedIn,
            )
        },
        bottomBar = {
            MainBottomNavBar(
                navController = globalNavController,
                onShowBottomSheet = { showBottomSheet = true },
                currentSelectedRoute = selectedBottomNavRoute,
                onRouteSelected = { newRoute -> selectedBottomNavRoute = newRoute }
            )
        }
    ) { innerPadding ->
        NavHost(
            navController = globalNavController,
            startDestination = Routes.GeralRoutes.HOMEPAGE.route,
            modifier = Modifier.padding(innerPadding) // Aplica o padding do Scaffold!
        ) {
            homePages(globalNavController = globalNavController)

            pages(
                globalNavController = globalNavController,
                sessionManager = sessionManager
            )
        }

        // O BottomSheet fica aqui, fora do NavHost, controlado pelo estado local
        if (showBottomSheet) {
            val navBackStackEntry by globalNavController.currentBackStackEntryAsState()
            val currentRoute = navBackStackEntry?.destination?.route

            AppModalBottomSheet(onDismiss = { showBottomSheet = false }) {
                BottomSheetContent(
                    Modifier,
                    globalNavController,
                    selectedBottomNavRoute)
            }
        }
    }
}

private fun NavGraphBuilder.homePages(globalNavController: NavHostController) {
    composable(Routes.GeralRoutes.HOMEPAGE.route) {
        HomePageScreen(globalNavController = globalNavController)
    }

    composable(Routes.TeamRoutes.HOMEPAGE.route){
        HomePageTeamScreen(globalNavController)
    }
}

/**
 * Função que declara todas as páginas da app
 * */
private fun NavGraphBuilder.pages(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    autPages(globalNavController = globalNavController)

    userPages(
        globalNavController = globalNavController,
        sessionManager = sessionManager
    )

    teamPages(globalNavController = globalNavController)

    chatPages(globalNavController = globalNavController)

    systemPages(globalNavController = globalNavController)
}

/**
 * Paginas de autentificação
 * */
private fun NavGraphBuilder.autPages(globalNavController: NavHostController) {
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
private fun NavGraphBuilder.userPages(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    profilePlayer(
        globalNavController = globalNavController,
        sessionManager = sessionManager
    )

    composable(Routes.PlayerRoutes.TEAM_LIST.route){
        ListTeamScreen(navHostController = globalNavController)
    }

    composable(Routes.PlayerRoutes.PLAYER_LIST.route) {
        val viewModel = hiltViewModel<ListPlayerViewModel>()

        ListPlayersScreen(navHostController = globalNavController, viewModel = viewModel)
    }

    composable(Routes.GeralRoutes.LEADERBOARD.route) {
        LeaderboardScreen(navHostController = globalNavController)
    }
    composable(Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST.route) {
        ListMemberShipRequest(navHostController = globalNavController)
    }
}

private fun NavGraphBuilder.profilePlayer(
    globalNavController: NavHostController,
    sessionManager: SessionManager) {
    composable(route = Routes.UserRoutes.PROFILE.route) {
        val viewModel = hiltViewModel<ProfilePlayerViewModel>()

        ProfileScreen(viewModel = viewModel)
    }

    composableProtected(
        route = "${Routes.UserRoutes.PROFILE.route}/{playerId}",
        arguments = listOf(
            navArgument("playerId") { type = NavType.StringType }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            val viewModel = hiltViewModel<ProfilePlayerViewModel>()

            ProfileScreen(viewModel = viewModel)
        }
    )
}

/**
 * Paginas da Time
 * */
private fun NavGraphBuilder.teamPages(globalNavController: NavHostController) {
    crudTeamPages(globalNavController = globalNavController)

    teamMatch(globalNavController = globalNavController)

    composable(Routes.TeamRoutes.MEMBERLIST.route) {
        ListMembersScreen(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.route) {
        ListMemberShipRequest(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM.route) {
        ListPlayersScreen(navHostController = globalNavController)
    }
}

private fun NavGraphBuilder.teamMatch(globalNavController: NavHostController) {
    composable(Routes.TeamRoutes.CALENDAR.route) {
        CalendarScreen(globalNavController)
    }

    managementMatch(globalNavController = globalNavController)

    casualMatches(globalNavController = globalNavController)

    competitiveMatches(globalNavController = globalNavController)

    composable(Routes.TeamRoutes.LIST_POST_PONE_MATCH.route) {
        ListPostPoneMatchScreen(navHostController = globalNavController)
    }
}

private fun NavGraphBuilder.managementMatch(globalNavController: NavHostController) {
    composable(Routes.TeamRoutes.POST_PONE_MATCH.route) {
        FormMatchInviteScreen(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.FINISH_MATCH.route) {
        FinishMatchScreen(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.CANCEL_MATCH.route) {
        CancelMatchScreen(navHostController = globalNavController)
    }
}


private fun NavGraphBuilder.casualMatches(globalNavController: NavHostController) {
    composable(Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE.route) {
        ListTeamScreen(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.LIST_MATCH_INVITES.route) {
        ListMatchInviteScreen(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.SEND_MATCH_INVITE.route) {
        FormMatchInviteScreen(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.NEGOCIATE_MATCH_INVITE.route) {
        FormMatchInviteScreen(navHostController = globalNavController)
    }
}

private fun NavGraphBuilder.competitiveMatches(globalNavController: NavHostController) {
    composable(Routes.TeamRoutes.SEARCH_COMPETIVE_MATCH.route) {
        MatchMakerScreen(navHostController = globalNavController)
    }
}

/**
 * Páginas do CRUD da Equipa
 * */
private fun NavGraphBuilder.crudTeamPages(globalNavController: NavHostController){
    composable(route = Routes.TeamRoutes.CREATE_TEAM.route) {
        FormTeamScreen(navHostController = globalNavController)
    }

    composable(route = CrudTeamRoutes.UPDATE_TEAM) {
        FormTeamScreen(navHostController = globalNavController)
    }

    profileTeam()
}

private fun NavGraphBuilder.profileTeam() {
    composable(route = Routes.TeamRoutes.TEAM_PROFILE.route) {
        val viewModel = hiltViewModel<ProfileTeamViewModel>()

        ProfileTeamScreen(viewModel = viewModel)
    }

    composable("${Routes.TeamRoutes.TEAM_PROFILE.route}/{teamId}",
        arguments = listOf(
            navArgument("teamId") { type = NavType.StringType }
        )
    ) {
        val viewModel = hiltViewModel<ProfileTeamViewModel>()

        ProfileTeamScreen(viewModel = viewModel)
    }
}

private fun NavGraphBuilder.chatPages(globalNavController: NavHostController) {
    composable(Routes.PlayerRoutes.CHAT_LIST.route){
        ChatListScreen(navController = globalNavController)
    }
    composable(Routes.PlayerRoutes.SINGLE_CHAT.route){
        GroupChatFootballPreview()
    }
}

private fun NavGraphBuilder.systemPages(globalNavController: NavHostController) {
    composable(Routes.GeralRoutes.SETTINGS.route){
        SettingsScreen(navController = globalNavController)
    }

    composable(Routes.GeralRoutes.PREFERENCE.route){
        PreferenceScreen()
    }
}

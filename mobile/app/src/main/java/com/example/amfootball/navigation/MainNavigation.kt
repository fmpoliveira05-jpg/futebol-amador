package com.example.amfootball.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
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
import com.example.amfootball.data.enums.Forms.MatchFormMode
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.navigation.objects.Arguments
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.AppModalBottomSheet
import com.example.amfootball.ui.components.navBar.BottomSheetContent
import com.example.amfootball.ui.components.navBar.MainBottomNavBar
import com.example.amfootball.ui.components.navBar.MainTopAppBar
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.LeaderboardScreen
import com.example.amfootball.ui.screens.Chat.ChatListScreen
import com.example.amfootball.ui.screens.Chat.ChatScreen
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
import com.example.amfootball.ui.viewModel.AuthViewModel
import com.example.amfootball.ui.viewModel.lists.ListPlayerViewModel
import com.example.amfootball.ui.viewModel.team.ProfileTeamViewModel
import com.example.amfootball.ui.viewModel.chat.ChatViewModel
import com.example.amfootball.ui.viewModel.lists.ListTeamViewModel
import com.example.amfootball.ui.viewModel.matchInvite.FormMatchInviteViewModel
import com.example.amfootball.ui.viewModel.team.CalendarTeamViewModel
import com.example.amfootball.ui.viewModel.team.ListMembersViewModel
import com.example.amfootball.ui.viewModel.team.TeamFormViewModel
import com.example.amfootball.ui.viewModel.user.ProfilePlayerViewModel
import com.example.amfootball.utils.extensions.composableProtected

@Composable
fun MainNavigation() {
    val globalNavController = rememberNavController()

    val authViewModel: AuthViewModel = hiltViewModel<AuthViewModel>()
    val isLoggedIn by authViewModel.isUserLoggedIn.collectAsState()

    val sessionManager by remember { mutableStateOf(SessionManager(context = globalNavController.context)) }
    var showBottomSheet by remember { mutableStateOf(false) }
    var selectedBottomNavRoute by remember { mutableStateOf(Routes.BottomNavBarRoutes.HOMEPAGE.route) }
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
                sessionManager = sessionManager,
                authViewModel = authViewModel
            )
        }

        // O BottomSheet fica aqui, fora do NavHost, controlado pelo estado local
        if (showBottomSheet) {
            val currentUser = sessionManager.getUserProfile()
            val currentTeamId = currentUser?.idTeam ?: ""

            AppModalBottomSheet(onDismiss = { showBottomSheet = false }) {
                BottomSheetContent(
                    Modifier,
                    globalNavController,
                    selectedBottomNavRoute,
                    teamId = currentTeamId)
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
    sessionManager: SessionManager,
    authViewModel: AuthViewModel
) {
    autPages(
        globalNavController = globalNavController,
        authViewModel = authViewModel
    )

    userPages(
        globalNavController = globalNavController,
        sessionManager = sessionManager
    )

    teamPages(globalNavController = globalNavController, sessionManager = sessionManager)

    chatPages(globalNavController = globalNavController)

    systemPages(globalNavController = globalNavController)
}

/**
 * Paginas de autentificação
 * */
private fun NavGraphBuilder.autPages(
    globalNavController: NavHostController,
    authViewModel: AuthViewModel
) {
    composable(Routes.UserRoutes.LOGIN.route) {
        LoginScreen(
            navHostController = globalNavController,
            authViewModel = authViewModel
        )
    }

    composable(Routes.UserRoutes.SIGNUP.route) {
        SignUpScreen(
            navHostController = globalNavController,
            authViewModel = authViewModel
        )
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
        navHostController = globalNavController,
        sessionManager = sessionManager
    )

    composable(Routes.PlayerRoutes.TEAM_LIST.route){
        val viewModel = hiltViewModel<ListTeamViewModel>()

        ListTeamScreen(navHostController = globalNavController, viewModel = viewModel)
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
    sessionManager: SessionManager,
    navHostController: NavHostController
) {
    composableProtected(
        route = Routes.UserRoutes.PROFILE.route,
        sessionManager = sessionManager,
        navController = navHostController,
        content = {
            val viewModel = hiltViewModel<ProfilePlayerViewModel>()

            ProfileScreen(viewModel = viewModel)
        }
    )

    composable(route = "${Routes.UserRoutes.PROFILE.route}/{${Arguments.PLAYER_ID}}",
        arguments = listOf(
            navArgument(Arguments.PLAYER_ID) { type = NavType.StringType }
        )
    ) {
        val viewModel = hiltViewModel<ProfilePlayerViewModel>()

        ProfileScreen(viewModel = viewModel)
    }
}

/**
 * Paginas da Time
 * */
private fun NavGraphBuilder.teamPages(globalNavController: NavHostController, sessionManager: SessionManager) {
    crudTeamPages(globalNavController = globalNavController)

    teamMatch(sessionManager = sessionManager, globalNavController = globalNavController)

    composable(
        route = "${Routes.TeamRoutes.MEMBERLIST.route}/{${Arguments.TEAM_ID}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
        )
    ) {
        val viewModel = hiltViewModel<ListMembersViewModel>()

        ListMembersScreen(navHostController = globalNavController, viewModel = viewModel)
    }

    composable(Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.route) {
        ListMemberShipRequest(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM.route) {
        ListPlayersScreen(navHostController = globalNavController)
    }
}

private fun NavGraphBuilder.teamMatch(globalNavController: NavHostController, sessionManager: SessionManager) {
    composableProtected(
        navController = globalNavController,
        route = "${Routes.TeamRoutes.CALENDAR.route}/{${Arguments.TEAM_ID}}",
        sessionManager = sessionManager,
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
        ),
        content = {
            val viewModel = hiltViewModel<CalendarTeamViewModel>()

            CalendarScreen(navHostController = globalNavController, viewModel = viewModel)
        }
    )

    managementMatch(globalNavController = globalNavController, sessionManager = sessionManager)

    casualMatches(globalNavController = globalNavController, sessionManager = sessionManager)

    competitiveMatches(globalNavController = globalNavController)

    composable(Routes.TeamRoutes.LIST_POST_PONE_MATCH.route) {
        ListPostPoneMatchScreen(navHostController = globalNavController)
    }
}

private fun NavGraphBuilder.managementMatch(globalNavController: NavHostController, sessionManager: SessionManager) {
    composableProtected(
        route = "${Routes.TeamRoutes.POST_PONE_MATCH.route}/${Arguments.MATCH_ID}",
        arguments = listOf(
            navArgument(Arguments.MATCH_ID) { type = NavType.StringType },
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.POSTPONE.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            val viewModel = hiltViewModel<FormMatchInviteViewModel>()

            FormMatchInviteScreen(navHostController = globalNavController, viewModel = viewModel)
        }
    )

    composable(Routes.TeamRoutes.FINISH_MATCH.route) {
        FinishMatchScreen(navHostController = globalNavController)
    }

    composableProtected(
        route = "${Routes.TeamRoutes.CANCEL_MATCH.route}/{${Arguments.MATCH_ID}}",
        arguments = listOf(
            navArgument(Arguments.MATCH_ID) { type = NavType.StringType },
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.CANCEL.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            val viewModel = hiltViewModel<FormMatchInviteViewModel>()

            FormMatchInviteScreen(navHostController = globalNavController, viewModel = viewModel)
            //CancelMatchScreen(navHostController = globalNavController)
        }
    )
}


private fun NavGraphBuilder.casualMatches(globalNavController: NavHostController, sessionManager: SessionManager) {
    composable(Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE.route) {
        ListTeamScreen(navHostController = globalNavController)
    }

    composable(Routes.TeamRoutes.LIST_MATCH_INVITES.route) {
        ListMatchInviteScreen(navHostController = globalNavController)
    }

    composableProtected(
        route = Routes.TeamRoutes.SEND_MATCH_INVITE.route,
        arguments = listOf(
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.SEND.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            val viewModel = hiltViewModel<FormMatchInviteViewModel>()

            FormMatchInviteScreen(navHostController = globalNavController, viewModel = viewModel)
        }
    )

    composableProtected(
        route = "${Routes.TeamRoutes.NEGOCIATE_MATCH_INVITE.route}/${Arguments.TEAM_ID}/CancelMatch/${Arguments.MATCH_INVITE_ID}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType },
            navArgument(Arguments.MATCH_INVITE_ID) { type = NavType.StringType },
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.NEGOCIATE.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            val viewModel = hiltViewModel<FormMatchInviteViewModel>()

            FormMatchInviteScreen(navHostController = globalNavController, viewModel = viewModel)
        }
    )

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
        val viewModel = hiltViewModel<TeamFormViewModel>()

        FormTeamScreen(navHostController = globalNavController, viewModel = viewModel)
    }

    //TODO: Meter para ser security Route e validar o tipo de user e também mandar o id na rota e trocar para hiltViewModel
    composable("${Routes.TeamRoutes.UPDATE_TEAM.route}/{${Arguments.TEAM_ID}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
        )
    ) {
        val viewModel = hiltViewModel<TeamFormViewModel>()

        FormTeamScreen(navHostController = globalNavController, viewModel = viewModel)
    }

    profileTeam()
}

private fun NavGraphBuilder.profileTeam() {
    composable(route = Routes.TeamRoutes.TEAM_PROFILE.route) {
        val viewModel = hiltViewModel<ProfileTeamViewModel>()

        ProfileTeamScreen(viewModel = viewModel)
    }

    composable("${Routes.TeamRoutes.TEAM_PROFILE.route}/{${Arguments.TEAM_ID}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
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

    composable(Routes.PlayerRoutes.SINGLE_CHAT.route,
        arguments = listOf(
            navArgument(Routes.chatRoomId) { type = NavType.StringType }
        )) {
        val viewModel = hiltViewModel<ChatViewModel>()

        ChatScreen(chatViewModel = viewModel)
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

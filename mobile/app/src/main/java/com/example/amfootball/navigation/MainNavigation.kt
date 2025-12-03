package com.example.amfootball.navigation

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.example.amfootball.data.enums.Forms.MatchFormMode
import com.example.amfootball.data.enums.settings.AppTheme
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.navigation.objects.Arguments
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.AppModalBottomSheet
import com.example.amfootball.ui.components.navBar.BottomSheetContent
import com.example.amfootball.ui.components.navBar.MainBottomNavBar
import com.example.amfootball.ui.components.navBar.MainTopAppBar
import com.example.amfootball.ui.screens.Chat.ChatListScreen
import com.example.amfootball.ui.screens.Chat.ChatScreen
import com.example.amfootball.ui.screens.homePages.HomePageScreen
import com.example.amfootball.ui.screens.homePages.HomePageTeamScreen
import com.example.amfootball.ui.screens.lists.LeaderboardScreen
import com.example.amfootball.ui.screens.lists.ListMemberShipRequest
import com.example.amfootball.ui.screens.lists.ListPlayersScreen
import com.example.amfootball.ui.screens.lists.ListTeamScreen
import com.example.amfootball.ui.screens.match.FinishMatchScreen
import com.example.amfootball.ui.screens.match.MatchMakerScreen
import com.example.amfootball.ui.screens.matchInvite.FormMatchInviteScreen
import com.example.amfootball.ui.screens.matchInvite.ListMatchInviteScreen
import com.example.amfootball.ui.screens.settings.SettingsScreen
import com.example.amfootball.ui.screens.team.CalendarScreen
import com.example.amfootball.ui.screens.team.FormTeamScreen
import com.example.amfootball.ui.screens.team.ListMembersScreen
import com.example.amfootball.ui.screens.team.ListPostPoneMatchScreen
import com.example.amfootball.ui.screens.team.ProfileTeamScreen
import com.example.amfootball.ui.screens.user.LoginScreen
import com.example.amfootball.ui.screens.user.ProfileScreen
import com.example.amfootball.ui.screens.user.SignUpScreen
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.SettingsViewModel
import com.example.amfootball.ui.viewModel.auth.AuthViewModel
import com.example.amfootball.utils.extensions.composableNotProtectedRoute
import com.example.amfootball.utils.extensions.composableProtected
import com.example.amfootball.utils.extensions.composableProtectedAdminTeam
import com.example.amfootball.utils.extensions.composableProtectedMemberTeam
import com.example.amfootball.utils.extensions.composableProtectedPlayerWithouTeam

//TODO: Colocar autorização nas rotas, até agora só temos autentificação
@Composable
fun MainNavigation() {
    val globalNavController = rememberNavController()
    val authViewModel: AuthViewModel = hiltViewModel<AuthViewModel>()
    val context = LocalContext.current
    val sessionManager by remember { mutableStateOf(SessionManager(context = context)) }
    val settingsViewModel: SettingsViewModel = hiltViewModel()

    var showBottomSheet by remember { mutableStateOf(false) }
    var selectedBottomNavRoute by remember { mutableStateOf(Routes.BottomNavBarRoutes.HOMEPAGE.route) }

    AMFootballTheme(
        darkTheme = isDarkMode(settingsViewModel.theme.collectAsState().value),
        dynamicColor = false
    ) {
        Scaffold(
            topBar = {
                MainTopAppBar(
                    navController = globalNavController,
                    isLoggedIn = authViewModel.isUserLoggedIn.collectAsState().value,
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
                modifier = Modifier.padding(innerPadding)
            ) {
                homePages(
                    globalNavController = globalNavController,
                    sessionManager = sessionManager
                )

                pages(
                    globalNavController = globalNavController,
                    sessionManager = sessionManager,
                    authViewModel = authViewModel
                )

                composable(Routes.GeralRoutes.SETTINGS.route) {
                    SettingsScreen(
                        navController = globalNavController,
                        settingsViewModel = settingsViewModel
                    )
                }
            }

            if (showBottomSheet) {
                val currentUser = sessionManager.getUserProfile()
                val currentTeamId = currentUser?.effectiveTeamId

                AppModalBottomSheet(onDismiss = { showBottomSheet = false }) {
                    BottomSheetContent(
                        Modifier,
                        globalNavController,
                        selectedBottomNavRoute,
                        teamId = currentTeamId
                    )
                }
            }
        }
    }
}

private fun NavGraphBuilder.homePages(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composable(Routes.GeralRoutes.HOMEPAGE.route) {
        HomePageScreen(
            globalNavController = globalNavController,
        )
    }

    composableProtectedMemberTeam(
        route = Routes.TeamRoutes.HOMEPAGE.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            HomePageTeamScreen(globalNavController)
        }
    )
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
        authViewModel = authViewModel,
        sessionManager = sessionManager
    )

    userPages(
        globalNavController = globalNavController,
        sessionManager = sessionManager
    )

    teamPages(globalNavController = globalNavController, sessionManager = sessionManager)

    chatPages(globalNavController = globalNavController, sessionManager = sessionManager)
}

/**
 * Paginas de autentificação
 * */
private fun NavGraphBuilder.autPages(
    globalNavController: NavHostController,
    sessionManager: SessionManager,
    authViewModel: AuthViewModel
) {
    composableNotProtectedRoute(
        route = "${Routes.UserRoutes.LOGIN.route}?redirect={redirect}",
        navController = globalNavController,
        sessionManager = sessionManager,
        arguments = listOf(
            navArgument("redirect") {
                defaultValue = null
                nullable = true
                type = NavType.StringType
            }
        ),
        content = {
            LoginScreen(
                navHostController = globalNavController,
                authViewModel = authViewModel
            )
        }
    )

    composableNotProtectedRoute(
        route = Routes.UserRoutes.SIGNUP.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            SignUpScreen(
                navHostController = globalNavController,
                authViewModel = authViewModel
            )
        }
    )
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

    composable(Routes.PlayerRoutes.TEAM_LIST.route) {
        ListTeamScreen(navHostController = globalNavController)
    }

    composable(Routes.PlayerRoutes.PLAYER_LIST.route) {
        ListPlayersScreen(navHostController = globalNavController)
    }

    composable(Routes.GeralRoutes.LEADERBOARD.route) {
        LeaderboardScreen(navHostController = globalNavController)
    }

    composableProtectedPlayerWithouTeam(
        route = Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListMemberShipRequest(navHostController = globalNavController)
        }
    )
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
            ProfileScreen()
        }
    )

    composable(
        route = "${Routes.UserRoutes.PROFILE.route}/{${Arguments.PLAYER_ID}}",
        arguments = listOf(
            navArgument(Arguments.PLAYER_ID) { type = NavType.StringType }
        )
    ) {
        ProfileScreen()
    }
}

/**
 * Paginas da Time
 * */
private fun NavGraphBuilder.teamPages(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    crudTeamPages(globalNavController = globalNavController, sessionManager = sessionManager)

    teamMatch(sessionManager = sessionManager, globalNavController = globalNavController)

    composableProtectedMemberTeam(
        route = Routes.TeamRoutes.MEMBERLIST.route,
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            ListMembersScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListMemberShipRequest(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListPlayersScreen(navHostController = globalNavController)
        }
    )
}

private fun NavGraphBuilder.teamMatch(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedMemberTeam(
        navController = globalNavController,
        route = "${Routes.TeamRoutes.CALENDAR.route}/{${Arguments.TEAM_ID}}",
        sessionManager = sessionManager,
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
        ),
        content = {
            CalendarScreen(navHostController = globalNavController)
        }
    )

    managementMatch(globalNavController = globalNavController, sessionManager = sessionManager)

    casualMatches(globalNavController = globalNavController, sessionManager = sessionManager)

    competitiveMatches(globalNavController = globalNavController, sessionManager = sessionManager)

    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.LIST_POST_PONE_MATCH.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListPostPoneMatchScreen(navHostController = globalNavController)
        }
    )
}

private fun NavGraphBuilder.managementMatch(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedAdminTeam(
        route = "${Routes.TeamRoutes.POST_PONE_MATCH.route}/{${Arguments.MATCH_ID}}",
        arguments = listOf(
            navArgument(Arguments.MATCH_ID) { type = NavType.StringType },
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.POSTPONE.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            FormMatchInviteScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.FINISH_MATCH.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            FinishMatchScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = "${Routes.TeamRoutes.CANCEL_MATCH.route}/{${Arguments.MATCH_ID}}",
        arguments = listOf(
            navArgument(Arguments.MATCH_ID) { type = NavType.StringType },
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.CANCEL.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            FormMatchInviteScreen(navHostController = globalNavController)
        }
    )
}


private fun NavGraphBuilder.casualMatches(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.SEARCH_TEAMS_TO_MATCH_INVITE.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListTeamScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.LIST_MATCH_INVITES.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListMatchInviteScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.SEND_MATCH_INVITE.route,
        arguments = listOf(
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.SEND.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            FormMatchInviteScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = "${Routes.TeamRoutes.NEGOCIATE_MATCH_INVITE.route}/{${Arguments.TEAM_ID}}/{${Arguments.MATCH_INVITE_ID}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType },
            navArgument(Arguments.MATCH_INVITE_ID) { type = NavType.StringType },
            navArgument(Arguments.FORM_MODE) { defaultValue = MatchFormMode.NEGOCIATE.name }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            FormMatchInviteScreen(navHostController = globalNavController)
        }
    )
}

private fun NavGraphBuilder.competitiveMatches(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.SEARCH_COMPETIVE_MATCH.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            MatchMakerScreen(navHostController = globalNavController)
        }
    )
}

/**
 * Páginas do CRUD da Equipa
 * */
private fun NavGraphBuilder.crudTeamPages(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedPlayerWithouTeam(
        route = Routes.TeamRoutes.CREATE_TEAM.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            FormTeamScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = "${Routes.TeamRoutes.UPDATE_TEAM.route}/{${Arguments.TEAM_ID}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
        ),
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            FormTeamScreen(navHostController = globalNavController)
        }
    )

    profileTeam(globalNavController = globalNavController, sessionManager = sessionManager)
}

private fun NavGraphBuilder.profileTeam(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedMemberTeam(
        route = Routes.TeamRoutes.TEAM_PROFILE.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ProfileTeamScreen()
        }
    )

    composable(
        "${Routes.TeamRoutes.TEAM_PROFILE.route}/{${Arguments.TEAM_ID}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
        )
    ) {
        ProfileTeamScreen()
    }
}

private fun NavGraphBuilder.chatPages(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedAdminTeam(
        route = Routes.PlayerRoutes.CHAT_LIST.route,
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ChatListScreen(navController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = Routes.PlayerRoutes.SINGLE_CHAT.route,
        arguments = listOf(
            navArgument(Routes.chatRoomId) { type = NavType.StringType }
        ),
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ChatScreen()
        }
    )
}

@Composable
private fun isDarkMode(currentAppTheme: String): Boolean {
    return when (currentAppTheme) {
        AppTheme.LIGHT_MODE.name -> false
        AppTheme.DARK_MODE.name -> true
        else -> isSystemInDarkTheme()
    }
}
package com.example.amfootball.ui.navigation

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.navArgument
import com.example.amfootball.core.extensions.composableNotProtectedRoute
import com.example.amfootball.core.extensions.composableProtected
import com.example.amfootball.core.extensions.composableProtectedAdminTeam
import com.example.amfootball.core.extensions.composableProtectedMemberTeam
import com.example.amfootball.core.extensions.composableProtectedPlayerWithouTeam
import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.SignalRUrls
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.domains.enums.UserRole
import com.example.amfootball.domains.enums.pages.ListMembershipRequestMode
import com.example.amfootball.domains.enums.pages.ListPlayerMode
import com.example.amfootball.domains.enums.pages.ListTeamMode
import com.example.amfootball.domains.enums.pages.MatchFormMode
import com.example.amfootball.domains.enums.settings.AppTheme
import com.example.amfootball.ui.components.AppModalBottomSheet
import com.example.amfootball.ui.components.navBar.BottomSheetContent
import com.example.amfootball.ui.components.navBar.MainBottomNavBar
import com.example.amfootball.ui.components.navBar.MainTopAppBar
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.screens.chat.ChatListScreen
import com.example.amfootball.ui.screens.chat.ChatScreen
import com.example.amfootball.ui.screens.homePages.HomePageScreen
import com.example.amfootball.ui.screens.homePages.HomePageTeamScreen
import com.example.amfootball.ui.screens.lists.LeaderboardScreen
import com.example.amfootball.ui.screens.lists.ListMemberShipRequest
import com.example.amfootball.ui.screens.lists.ListPlayersScreen
import com.example.amfootball.ui.screens.lists.ListTeamScreen
import com.example.amfootball.ui.screens.match.FinishMatchLobbyScreen
import com.example.amfootball.ui.screens.match.FinishMatchScreen
import com.example.amfootball.ui.screens.match.MatchMakerScreen
import com.example.amfootball.ui.screens.match.StartMatchLobbyScreen
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

@Composable
fun MainNavigation(
    globalNavController: NavHostController,
    startDestination: String
) {
    val authViewModel: AuthViewModel = hiltViewModel<AuthViewModel>()
    val settingsViewModel: SettingsViewModel = hiltViewModel()

    val context = LocalContext.current
    val sessionManager by remember { mutableStateOf(SessionManager(context = context)) }

    var showBottomSheet by rememberSaveable { mutableStateOf(false) }
    var selectedBottomNavRoute by rememberSaveable { mutableStateOf(Routes.BottomNavBarRoutes.HOMEPAGE.route) }

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
                startDestination = startDestination,
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
                val role = sessionManager.getUserProfile()?.role ?: UserRole.UNAUTHORIZED


                AppModalBottomSheet(onDismiss = { showBottomSheet = false }) {
                    BottomSheetContent(
                        Modifier,
                        globalNavController,
                        selectedBottomNavRoute,
                        teamId = currentTeamId,
                        role = role
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

    composable(
        route = Routes.PlayerRoutes.TEAM_LIST.route,
        arguments = listOf(
            navArgument(Arguments.LIST_TEAM_MODE) { defaultValue = ListTeamMode.LIST_TEAM.name }
        ),
    ) {
        ListTeamScreen(navHostController = globalNavController)
    }

    composableProtectedPlayerWithouTeam(
        route = Routes.PlayerRoutes.TEAM_LIST_MEMBERSHIP_REQUEST.route,
        arguments = listOf(
            navArgument(Arguments.LIST_TEAM_MODE) {
                defaultValue = ListTeamMode.LIST_TEAM_MEMBERSHIP_REQUEST.name
            }
        ),
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListTeamScreen(navHostController = globalNavController)
        }
    )

    composable(
        route = Routes.PlayerRoutes.PLAYER_LIST.route,
        arguments = listOf(
            navArgument(Arguments.LIST_PLAYER_MODE) {
                defaultValue = ListPlayerMode.PLAYER_LIST.name
            }
        )
    ) {
        ListPlayersScreen(navHostController = globalNavController)
    }

    composable(Routes.GeralRoutes.LEADERBOARD.route) {
        LeaderboardScreen(navHostController = globalNavController)
    }

    composableProtectedPlayerWithouTeam(
        route = Routes.PlayerRoutes.LIST_MEMBERSHIP_REQUEST.route,
        arguments = listOf(
            navArgument(Arguments.LIST_MEMBERSHIP_REQUEST_MODE) {
                defaultValue = ListMembershipRequestMode.MEMBERSHIP_PLAYER.name
            }
        ),
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

    composable(
        Routes.UserRoutes.EDIT_PROFILE.route) {
        SignUpScreen(navHostController=navHostController,profileEditMode = true)
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
        arguments = listOf(
            navArgument(Arguments.LIST_MEMBERSHIP_REQUEST_MODE) {
                defaultValue = ListMembershipRequestMode.MEMBERSHIP_TEAM.name
            }
        ),
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListMemberShipRequest(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = Routes.TeamRoutes.SEARCH_PLAYERS_WITH_OUT_TEAM.route,
        arguments = listOf(
            navArgument(Arguments.LIST_PLAYER_MODE) {
                defaultValue = ListPlayerMode.PLAYER_WITHOU_TEAM.name
            }
        ),
        navController = globalNavController,
        sessionManager = sessionManager,
        content = {
            ListPlayersScreen(navHostController = globalNavController)
        }
    )

    hubPages(globalNavController = globalNavController, sessionManager = sessionManager)
}

private fun NavGraphBuilder.hubPages(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedAdminTeam(
        route = "${SignalRUrls.START_MATCH_URL}/{${Arguments.MATCH_ID}}/{${Arguments.OPPONENT_ID}}",
        arguments = listOf(
            navArgument(Arguments.MATCH_ID) { type = NavType.StringType },
            navArgument(Arguments.OPPONENT_ID) { type = NavType.StringType }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            StartMatchLobbyScreen(navHostController = globalNavController)
        }
    )

    composableProtectedAdminTeam(
        route = "${SignalRUrls.FINISH_MATCH_URL}/{${Arguments.MATCH_ID}}/{${Arguments.OPPONENT_ID}}/{${Arguments.OPPONENT_GOALS}}/{${Arguments.MY_GOALS}}",
        arguments = listOf(
            navArgument(Arguments.MATCH_ID) { type = NavType.StringType },
            navArgument(Arguments.OPPONENT_ID) { type = NavType.StringType },
            navArgument(Arguments.OPPONENT_GOALS) { type = NavType.IntType },
            navArgument(Arguments.MY_GOALS) { type = NavType.IntType }
        ),
        sessionManager = sessionManager,
        navController = globalNavController,
        content = {
            FinishMatchLobbyScreen(navHostController = globalNavController)
        }
    )
}
private fun NavGraphBuilder.teamMatch(
    globalNavController: NavHostController,
    sessionManager: SessionManager
) {
    composableProtectedMemberTeam(
        navController = globalNavController,
        route = Routes.TeamRoutes.CALENDAR.route,
        sessionManager = sessionManager,
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
        route = "${Routes.TeamRoutes.FINISH_MATCH.route}/{${Arguments.MATCH_ID}}/{${Arguments.OPPONENT_ID}}",
        arguments = listOf(
            navArgument(Arguments.MATCH_ID) { type = NavType.StringType },
            navArgument(Arguments.OPPONENT_ID) { type = NavType.StringType },
        ),
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
        arguments = listOf(
            navArgument(Arguments.LIST_TEAM_MODE) {
                defaultValue = ListTeamMode.LIST_TEAM_MATCH_INVITE.name
            }
        ),
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
        route = "${Routes.TeamRoutes.SEND_MATCH_INVITE.route}/{${Arguments.TEAM_ID}}/{${Arguments.TEAM_NAME}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType },
            navArgument(Arguments.TEAM_NAME) { type = NavType.StringType },
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
            ProfileTeamScreen(navHostController = globalNavController)
        }
    )

    composable(
        "${Routes.TeamRoutes.TEAM_PROFILE.route}/{${Arguments.TEAM_ID}}",
        arguments = listOf(
            navArgument(Arguments.TEAM_ID) { type = NavType.StringType }
        )
    ) {
        ProfileTeamScreen(navHostController = globalNavController)
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
package com.example.amfootball.ui.components.navBar

import androidx.compose.foundation.layout.Row
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.List
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.NavigationItem
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.LeaderboardScreen
import com.example.amfootball.ui.screens.settings.SettingsScreen
import com.example.amfootball.ui.screens.settings.PreferenceScreen
import androidx.compose.material.icons.automirrored.filled.Login
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material3.Text
import androidx.compose.runtime.getValue
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.filled.PersonAdd
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.OutlinedButton
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.compose.currentBackStackEntryAsState
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.R
import com.example.amfootball.ui.screens.Lists.ListTeamScreen
import com.example.amfootball.ui.screens.lists.ListMemberShipRequest
import com.example.amfootball.ui.theme.AMFootballTheme
import okhttp3.Route

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NavigatonDrawerNavBarHomePage(globalNavController: NavHostController,
                                  isLoggedIn: Boolean,
                                  onLogout: () -> Unit,
){
    val drawerItemList = prepareNavigationDrawerItems()
    val internalNavController = rememberNavController()

    //Serve para meter os novos botões
    val navBackStackEntry by internalNavController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    NavigatonDrawer(
        itens = drawerItemList,
        titleNavBar = "NavBar HomePage",
        scaffoldContent = { innerNav ->
            ScaffoldContentNavBarHomePage(
                navController = innerNav,
                globalNavController = globalNavController
            )
        },
        internalNavController = internalNavController,
        globalNavController = globalNavController,
        topBarActions = {
            HomePageTopBarActions(
                isLoggedIn = isLoggedIn,
                currentRoute = currentRoute,
                onLogout = onLogout,
                globalNavController = globalNavController
            )
        }
    )
}

//Metodo que define todas as rotas da NavBar

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ScaffoldContentNavBarHomePage(navController: NavHostController,
                                  globalNavController: NavHostController) {
    NavHost(navController = navController, startDestination = Routes.GeralRoutes.HOMEPAGE.route) {
        composable(Routes.GeralRoutes.HOMEPAGE.route) {
            HomePageScreen(globalNavController = globalNavController)
        }
        composable(Routes.PlayerRoutes.TEAM_LIST.route) {
            ListTeamScreen(navHostController = globalNavController)
        }
        composable(Routes.GeralRoutes.LEADERBOARD.route) {
            LeaderboardScreen(navHostController = globalNavController)
        }
        composable(Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.route) {
            ListMemberShipRequest(navHostController = globalNavController)
        }
        composable(route = Routes.GeralRoutes.SETTINGS.route) {
            SettingsScreen()
        }
        composable(route = Routes.GeralRoutes.PREFERENCE.route) {
            PreferenceScreen()
        }
    }
}

//Se clicar na pagina da team buga e mostra outra navBar por debaixo da atual
//Definição dos itens da navBar
@Composable
private fun prepareNavigationDrawerItems(): List<NavigationItem> {
    val drawerItemsList = arrayListOf<NavigationItem>()

    drawerItemsList.add(NavigationItem(label = stringResource(R.string.item_home),
        description = stringResource(id = R.string.item_home_description),
        icon = Icons.Filled.Home,
        route = Routes.GeralRoutes.HOMEPAGE.route,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.item_team),
        description = stringResource(id = R.string.item_team_description),
        icon = Icons.Filled.Home,
        route = Routes.TeamRoutes.HOMEPAGE.route,
        isGlobalRoute =  true))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.navbar_memberships_list),
        description = stringResource(id = R.string.description_navbar_memberships_list),
        icon = Icons.Filled.List,
        route = Routes.TeamRoutes.LIST_MEMBERSHIP_REQUEST.route,
        isGlobalRoute = false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.item_list_team),
        description = stringResource(id = R.string.item_list_team_description),
        icon = Icons.Filled.List,
        route = Routes.PlayerRoutes.TEAM_LIST.route,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.item_list_players),
        description = stringResource(id = R.string.item_list_players_description),
        icon = Icons.Filled.List,
        route = Routes.PlayerRoutes.PLAYER_LIST.route,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.item_leadboard),
        description = stringResource(id = R.string.item_leadboard_description),
        icon = Icons.Filled.List, //Depois sacar
        route = Routes.GeralRoutes.LEADERBOARD.route,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.item_settings),
        description = stringResource(id = R.string.item_settings_description),
        icon = Routes.GeralRoutes.SETTINGS.icon,
        route = Routes.GeralRoutes.SETTINGS.route,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = stringResource(id = R.string.item_preference),
        description = stringResource(id = R.string.item_preference_description),
        icon = Routes.GeralRoutes.PREFERENCE.icon, //Depois trocar
        route = Routes.GeralRoutes.PREFERENCE.route,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = "Perfil",
        description = stringResource(Routes.UserRoutes.PROFILE.contentDescription),
        icon = Routes.UserRoutes.PROFILE.icon, //Depois trocar
        route = Routes.UserRoutes.PROFILE.route,
        isGlobalRoute = true))
    return drawerItemsList
}

/**
 * Define as ações da TopAppBar para a secção HomePage,
 * mostrando "Logout" se estiver logado, ou "Login"/"Registar"
 * se não estiver logado e estiver na rota principal.
 */
@Composable
private fun RowScope.HomePageTopBarActions(
    isLoggedIn: Boolean,
    currentRoute: String?,
    onLogout: () -> Unit,
    globalNavController: NavHostController
) {
    if (isLoggedIn) {
        //TODO: Aparecer botão de criar equipa se ele não for admin nem jogador de equipa
        IconButton(onClick = onLogout) {
            Icon(
                imageVector = Icons.AutoMirrored.Filled.Logout,
                contentDescription = "Logout"
            )
        }
    } else {
        if (currentRoute == Routes.GeralRoutes.HOMEPAGE.route) {
            Row {
                LoginButton(onClick = {
                    globalNavController.navigate(Routes.UserRoutes.LOGIN.route)
                },
                    modifier = Modifier.padding(end = 4.dp)
                )

                RegisterButton(onClick = {
                    globalNavController.navigate(Routes.UserRoutes.SIGNUP.route)
                })
            }

        }
    }
}

@Composable
private fun LoginButton(onClick: () -> Unit, modifier: Modifier = Modifier) {
    Button(
        onClick = onClick,
        modifier = modifier
    ) {
        Icon(
            imageVector = Icons.AutoMirrored.Filled.Login,
            contentDescription = null,
            modifier = Modifier.size(ButtonDefaults.IconSize)
        )
        Spacer(Modifier.size(ButtonDefaults.IconSpacing))
        Text("Login")
    }
}

@Composable
private fun RegisterButton(onClick: () -> Unit, modifier: Modifier = Modifier) {
    OutlinedButton(
        onClick = onClick,
        modifier = modifier
    ) {
        Icon(
            imageVector = Icons.Default.PersonAdd,
            contentDescription = null,
            modifier = Modifier.size(ButtonDefaults.IconSize)
        )
        Spacer(Modifier.size(ButtonDefaults.IconSpacing))
        Text("Registar")
    }
}

@Preview(name = "Team Navigation - EN", locale = "en", showBackground = true)
@Preview(name = "Team Navigation - PT", locale = "pt", showBackground = true)
@Composable
fun NavigatonDrawerNavBarPreviewLogged() {
    AMFootballTheme {
        NavigatonDrawerNavBarHomePage(
            globalNavController = rememberNavController(),
            isLoggedIn = true,
            onLogout = {}
        )
    }
}

@Preview(name = "Team Navigation - EN", locale = "en", showBackground = true)
@Preview(name = "Team Navigation - PT", locale = "pt", showBackground = true)
@Composable
fun NavigatonDrawerNavBarPreviewLogout() {
    AMFootballTheme {
        NavigatonDrawerNavBarHomePage(
            globalNavController = rememberNavController(),
            isLoggedIn = false,
            onLogout = {}
        )
    }
}
package com.example.amfootball.ui.components.NavBar

import androidx.compose.foundation.layout.Row
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.List
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.NavigationItem
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.navigation.Objects.NavBar.RouteNavBarHomePage
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.LeaderboardScreen
import com.example.amfootball.ui.screens.Lists.ListPlayersScreen
import com.example.amfootball.ui.screens.Lists.ListTeamScreen
import com.example.amfootball.ui.screens.SettingsScreen
import com.example.amfootball.ui.screens.PreferenceScreen
import androidx.compose.material.icons.automirrored.filled.Login
import androidx.compose.material.icons.automirrored.filled.Logout
import androidx.compose.material3.Text
import androidx.compose.runtime.getValue
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.filled.PersonAdd
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.OutlinedButton
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.navigation.compose.currentBackStackEntryAsState
import com.example.amfootball.navigation.Objects.AutRoutes
import com.example.amfootball.navigation.Objects.GeralRoutes

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
            ScaffoldContentNavBarHomePage(navController = innerNav)
        },
        internalNavController = internalNavController, // Passa o interno
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
fun ScaffoldContentNavBarHomePage(navController: NavHostController) {
    NavHost(navController = navController, startDestination = RouteNavBarHomePage.HOME_PAGE) {
        composable(RouteNavBarHomePage.HOME_PAGE) {
            HomePageScreen()
        }
        composable(RouteNavBarHomePage.EQUIPAS) {
            ListTeamScreen()
        }
        composable(RouteNavBarHomePage.PLAYERS) {
            ListPlayersScreen()
        }
        composable(RouteNavBarHomePage.LEADBOARD) {
            LeaderboardScreen()
        }
        composable(GeralRoutes.SETTINGS) {
            SettingsScreen()
        }
        composable(GeralRoutes.PREFERENCE) {
            PreferenceScreen()
        }
    }
}

//Se clicar na pagina da team buga e mostra outra navBar por debaixo da atual
//Definição dos itens da navBar
@Composable
private fun prepareNavigationDrawerItems(): List<NavigationItem> {
    val drawerItemsList = arrayListOf<NavigationItem>()

    drawerItemsList.add(NavigationItem(label = "Home",
        description = "Página inicial",
        icon = Icons.Filled.Home,
        route = RouteNavBarHomePage.HOME_PAGE,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = "Home My Team",
        description = "Página da sua equipa",
        icon = Icons.Filled.Home,
        route = RoutesNavBarTeam.HOME_PAGE_TEAM,
        isGlobalRoute =  true))
    drawerItemsList.add(NavigationItem(label = "Equipas",
        description = "Lista com as equipas da app",
        icon = Icons.Filled.List,
        route = RouteNavBarHomePage.EQUIPAS,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = "Jogadores",
        description = "Lista com todos os jogadores do app",
        icon = Icons.Filled.List,
        route = RouteNavBarHomePage.PLAYERS,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = "LeadBoard",
        description = "Tabela das melhores equipas do app",
        icon = Icons.Filled.List, //Depois sacar
        route = RouteNavBarHomePage.LEADBOARD,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = "Configurações",
        description = "Configrações do app",
        icon = Icons.Filled.Settings,
        route = GeralRoutes.SETTINGS,
        isGlobalRoute =  false))
    drawerItemsList.add(NavigationItem(label = "Preferencias",
        description = "Preferencias do site",
        icon = Icons.Filled.Person, //Depois trocar
        route = GeralRoutes.PREFERENCE,
        isGlobalRoute =  false))
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
        IconButton(onClick = onLogout) {
            Icon(
                imageVector = Icons.AutoMirrored.Filled.Logout,
                contentDescription = "Logout"
            )
        }
    } else {
        if (currentRoute == RouteNavBarHomePage.HOME_PAGE) {
            Row {
                LoginButton(onClick = {
                    globalNavController.navigate(AutRoutes.LOGIN)
                },
                    modifier = Modifier.padding(end = 4.dp)
                )

                RegisterButton(onClick = {
                    globalNavController.navigate(AutRoutes.SIGN_IN)
                })
            }

        }
    }
}

@Composable
private fun LoginButton(onClick: () -> Unit, modifier: Modifier = Modifier) {
    Button(
        onClick = onClick,
        // REMOVE .fillMaxWidth() DAQUI
        modifier = modifier // Aplica apenas o modifier passado
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
        // REMOVE .fillMaxWidth() DAQUI
        modifier = modifier // Aplica apenas o modifier passado
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
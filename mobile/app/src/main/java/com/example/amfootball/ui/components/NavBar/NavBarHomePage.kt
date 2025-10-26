package com.example.amfootball.ui.components.NavBar

import android.preference.PreferenceScreen
import android.provider.Settings
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.List
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.NavigationItem
import com.example.amfootball.navigation.GeralRoutes
import com.example.amfootball.navigation.RoutesNavBarTeam
import com.example.amfootball.navigation.RouteNavBarHomePage
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.LeaderboardScreen
import com.example.amfootball.ui.screens.Lists.ListPlayersScreen
import com.example.amfootball.ui.screens.Lists.ListTeamScreen
import com.example.amfootball.ui.screens.SettingsScreen
import com.example.amfootball.ui.screens.PreferenceScreen

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NavigatonDrawerNavBarHomePage(globalNavController: NavHostController){
    val drawerItemList = prepareNavigationDrawerItems()
    val internalNavController = rememberNavController()

    NavigatonDrawer(
        itens = drawerItemList,
        titleNavBar = "NavBar HomePage",
        scaffoldContent = { innerNav ->
            ScaffoldContentNavBarHomePage(navController = innerNav)
        },
        internalNavController = internalNavController, // Passa o interno
        globalNavController = globalNavController
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
        /*
        composable(RoutesNavBarTeam.HOME_PAGE_TEAM) {
            NavigatonDrawerTeam()
        }
        * */
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
        isGlobalRoute =  true))
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
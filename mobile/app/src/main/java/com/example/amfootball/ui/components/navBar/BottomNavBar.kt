package com.example.amfootball.ui.components.NavBar

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarDefaults
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.ui.components.AppModalBottomSheet
import com.example.amfootball.ui.components.Buttons.NavigateButton
import com.example.amfootball.ui.screens.HomePageScreen
import com.example.amfootball.ui.screens.settings.SettingsScreen

@Composable
fun MainBottomNavBar(
    navController: NavHostController,
    onShowBottomSheet: () -> Unit // Callback para mostrar o Bottom Sheet
) {
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    NavigationBar(windowInsets = NavigationBarDefaults.windowInsets) {
        Routes.BottomNavBarRoutes.entries.forEach { destination ->
            NavigationBarItem(
                selected = currentRoute == destination.route,
                onClick = {
                    if (destination == Routes.BottomNavBarRoutes.PAGE_OPTIONS) {
                        onShowBottomSheet()
                    } else {
                        navController.navigate(destination.route) {
                            launchSingleTop = true
                            restoreState = true
                            popUpTo(navController.graph.startDestinationId) { saveState = true }
                        }
                    }
                },
                icon = { Icon(destination.icon, contentDescription = stringResource(destination.contentDescription)) },
                label = { Text(stringResource(destination.labelResId), maxLines = 1, overflow = TextOverflow.Ellipsis) }
            )
        }
    }
}

@Composable
fun BottomSheetContent(
    modifier: Modifier,
    navController: NavHostController,
    currentScreenRoute: String?
){
    Row(
        modifier = modifier
            .fillMaxWidth()
            .padding(16.dp),
        horizontalArrangement = Arrangement.SpaceEvenly,
        verticalAlignment = Alignment.CenterVertically
    ) {
        CommonContentBottomSheet(navController)

        when (currentScreenRoute) {
            Routes.BottomNavBarRoutes.HOMEPAGE.route -> {
                HomePageContentBottonSheet(navController)
            }

            Routes.BottomNavBarRoutes.TEAM_LIST.route -> {
                TeamListContentBottomSheet(navController)
            }

            Routes.BottomNavBarRoutes.CHAT.route -> {
                ChatContentBottomSheet(navController)
            }

            Routes.BottomNavBarRoutes.USER_PROFILE.route -> {
                PlayerProfileContentBottomSheet(navController)
            }
        }
    }
}

@Composable
fun HomePageContentBottonSheet(navController: NavHostController) {
    //NavigateButton()
}
@Composable
fun TeamListContentBottomSheet(navController: NavHostController){
    //NavigateButton(Routes.TeamRoutes.TEAM_LIST.icon, stringResource(Routes.TeamRoutes.TEAM_LIST.contentDescription), onClick =)
}

@Composable
fun ChatContentBottomSheet(navController: NavHostController){

}

@Composable
fun PlayerProfileContentBottomSheet(navController: NavHostController){

}

@Composable
fun CommonContentBottomSheet(navController: NavHostController){
    NavigateButton(Routes.PlayerRoutes.PLAYER_LIST.icon, stringResource(Routes.PlayerRoutes.PLAYER_LIST.contentDescription), onClick = {navController.navigate(Routes.PlayerRoutes.PLAYER_LIST.route)})

}
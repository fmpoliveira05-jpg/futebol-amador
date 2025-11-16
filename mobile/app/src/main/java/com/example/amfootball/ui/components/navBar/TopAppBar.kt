package com.example.amfootball.ui.components.NavBar

import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.res.stringResource
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.currentBackStackEntryAsState
import com.example.amfootball.R
import com.example.amfootball.navigation.Objects.Routes
import kotlin.collections.find
import com.example.amfootball.navigation.Objects.AppRouteInfo
import com.example.amfootball.ui.components.buttons.BackButton
import com.example.amfootball.ui.viewModel.AuthViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainTopAppBar(
    navController: NavHostController,
    isLoggedIn: Boolean
) {
    // Tenta obter o título da rota atual. Se não encontrar, não mostra título.
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route
    val routeTitleResId = getAllRoutes().find { it.route == currentRoute }?.labelResId
    val haveBackButton = getAllRoutes().find { it.route == currentRoute }?.haveBackButton
    val authViewModel: AuthViewModel = viewModel()

    TopAppBar(
        title = {
            Row(verticalAlignment = Alignment.CenterVertically) {

                if (routeTitleResId != null) {
                    Text(text = stringResource(id = routeTitleResId))
                }
            }
        },
        navigationIcon = {
            if (haveBackButton == true) {
                BackButton(navController = navController)
            }
        },
        actions = {
            IconButton(onClick = { navController.navigate(Routes.GeralRoutes.SETTINGS.route) }) {
                Icon(
                    imageVector = Routes.GeralRoutes.SETTINGS.icon,
                    contentDescription = stringResource(id = R.string.item_settings_description)
                )
            }

            if (!isLoggedIn) {
                IconButton(onClick = { navController.navigate(Routes.UserRoutes.LOGIN.route) }) {
                    Icon(
                        imageVector = Routes.UserRoutes.LOGIN.icon,
                        contentDescription = stringResource(Routes.UserRoutes.LOGIN.labelResId)
                    )
                }
                IconButton(onClick = { navController.navigate(Routes.UserRoutes.SIGNUP.route) }) {
                    Icon(
                        imageVector = Routes.UserRoutes.SIGNUP.icon,
                        contentDescription = stringResource(Routes.UserRoutes.SIGNUP.labelResId)
                    )
                }
            }
            else {
                IconButton(onClick = { authViewModel.logoutUser() }) {
                    Icon(
                        imageVector = Routes.UserRoutes.LOGOUT.icon,
                        contentDescription = stringResource(Routes.UserRoutes.LOGOUT.labelResId)
                    )
                }

            }
        }
    )
}

// Função auxiliar para obter uma lista de todas as rotas para encontrar o título
private fun getAllRoutes(): List<AppRouteInfo> {
    return Routes.GeralRoutes.entries +
            Routes.TeamRoutes.entries +
            Routes.UserRoutes.entries +
            Routes.PlayerRoutes.entries +
            Routes.BottomNavBarRoutes.entries
}

// Adicione esta interface no seu ficheiro Routes.kt para unificar as suas rotas
// Ficheiro: navigation/Objects/Routes.kt
// interface RouteInfo {
//     val route: String
//     val labelResId: Int
// }
// E faça cada enum implementar isto: enum class GeralRoutes(...) : RouteInfo { ... }

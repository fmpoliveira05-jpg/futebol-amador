package com.example.amfootball.navigation

import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Login
import androidx.compose.material.icons.filled.PersonAdd
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.res.stringResource
import androidx.navigation.NavHostController
import androidx.navigation.compose.currentBackStackEntryAsState
import com.example.amfootball.R
import com.example.amfootball.navigation.Objects.Routes

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

    TopAppBar(
        title = {
            if (routeTitleResId != null) {
                Text(text = stringResource(id = routeTitleResId))
            }
        },
        actions = {
            // Ação de Definições/Preferências, sempre visível
            IconButton(onClick = { navController.navigate(Routes.GeralRoutes.SETTINGS.route) }) {
                Icon(
                    imageVector = Icons.Default.Settings,
                    contentDescription = stringResource(id = R.string.item_settings_description)
                )
            }

            // Ações de Autenticação
            if (!isLoggedIn) {
                IconButton(onClick = { navController.navigate(Routes.UserRoutes.LOGIN.route) }) {
                    Icon(
                        imageVector = Icons.AutoMirrored.Filled.Login,
                        contentDescription = "Login"
                    )
                }
                IconButton(onClick = { navController.navigate(Routes.UserRoutes.SIGNUP.route) }) {
                    Icon(
                        imageVector = Icons.Default.PersonAdd,
                        contentDescription = "Sign Up"
                    )
                }
            }
        }
    )
}

// Função auxiliar para obter uma lista de todas as rotas para encontrar o título
private fun getAllRoutes(): List<Routes.RouteInfo> {
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
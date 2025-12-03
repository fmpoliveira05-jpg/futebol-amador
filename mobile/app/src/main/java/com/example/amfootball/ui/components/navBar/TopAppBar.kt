package com.example.amfootball.ui.components.navBar

import androidx.compose.foundation.layout.Row
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.res.stringResource
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.currentBackStackEntryAsState
import com.example.amfootball.R
import com.example.amfootball.navigation.objects.AppRouteInfo
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.buttons.BackButton
import com.example.amfootball.ui.viewModel.auth.AuthViewModel

/**
 * Barra de navegação superior principal da aplicação (Top App Bar).
 *
 * Este componente é altamente dinâmico e "Context-Aware", ajustando o seu conteúdo com base na rota atual.
 *
 * **Funcionalidades:**
 * 1. **Título Dinâmico:** Identifica a rota atual e exibe o título correspondente definido no [Routes].
 * 2. **Navegação:** Exibe automaticamente o botão de "Voltar" ([BackButton]) se a rota atual tiver `haveBackButton = true`.
 * 3. **Gestão de Sessão:** Altera os ícones de ação à direita com base no estado [isLoggedIn]:
 * - **Logado:** Mostra ícone de Logout e Settings.
 * - **Visitante:** Mostra ícones de Login, Signup e Settings.
 *
 * @param navController O controlador de navegação para gerir o back stack e navegação entre ecrãs.
 * @param isLoggedIn Estado booleano que indica se existe uma sessão ativa. Controla a visibilidade das ações de Auth.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainTopAppBar(
    navController: NavHostController,
    isLoggedIn: Boolean,
) {
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route
    val foundRoute = getAllRoutes().find {
        currentRoute?.startsWith(it.route) == true
    }
    val haveBackButton = foundRoute?.haveBackButton
    val routeTitleResId = foundRoute?.labelResId
    val authViewModel = hiltViewModel<AuthViewModel>()

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
            } else {
                IconButton(onClick = {
                    authViewModel.logoutUser()
                    navController.navigate(currentRoute!!)
                }) {
                    Icon(
                        imageVector = Routes.UserRoutes.LOGOUT.icon,
                        contentDescription = stringResource(Routes.UserRoutes.LOGOUT.labelResId)
                    )
                }

            }
        }
    )
}

/**
 * Agrega todas as rotas definidas nos vários Enums de navegação numa única lista.
 *
 * Utilizado para pesquisar metadados da rota atual (título, ícone, back button) de forma centralizada.
 *
 * @return Uma lista plana contendo todas as instâncias de [AppRouteInfo] da aplicação.
 */
private fun getAllRoutes(): List<AppRouteInfo> {
    return Routes.GeralRoutes.entries +
            Routes.TeamRoutes.entries +
            Routes.UserRoutes.entries +
            Routes.PlayerRoutes.entries +
            Routes.BottomNavBarRoutes.entries
}
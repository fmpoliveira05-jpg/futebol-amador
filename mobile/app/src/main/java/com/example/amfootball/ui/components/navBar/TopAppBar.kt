package com.example.amfootball.ui.components.navBar

import androidx.compose.foundation.layout.Row
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.res.stringResource
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.currentBackStackEntryAsState
import com.example.amfootball.R
import com.example.amfootball.navigation.objects.Routes
import kotlin.collections.find
import com.example.amfootball.navigation.objects.AppRouteInfo
import com.example.amfootball.ui.components.buttons.BackButton
import com.example.amfootball.ui.viewModel.auth.AuthViewModel

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
            }
            else {
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

private fun getAllRoutes(): List<AppRouteInfo> {
    return Routes.GeralRoutes.entries +
            Routes.TeamRoutes.entries +
            Routes.UserRoutes.entries +
            Routes.PlayerRoutes.entries +
            Routes.BottomNavBarRoutes.entries
}
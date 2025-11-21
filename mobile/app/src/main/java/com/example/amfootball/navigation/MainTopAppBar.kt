package com.example.amfootball.navigation

/*
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainTopAppBar(
    navController: NavHostController,
    isLoggedIn: Boolean,
    sessionManager: SessionManager
) {
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route
    val routeInfo = getAllRoutes().find { it.route == currentRoute }
    val routeTitleResId = routeInfo?.labelResId
    val haveBackButton = routeInfo?.haveBackButton

    TopAppBar(
        title = {
            if (routeTitleResId != null) {
                Text(text = stringResource(id = routeTitleResId))
            }
        },
        navigationIcon = {
            // A seta só aparece se a rota a definir
            if (haveBackButton == true) {
                BackButton(navController = navController)
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

private fun getAllRoutes(): List<AppRouteInfo> {
    return Routes.GeralRoutes.entries +
            Routes.TeamRoutes.entries +
            Routes.UserRoutes.entries +
            Routes.PlayerRoutes.entries +
            Routes.BottomNavBarRoutes.entries
}

 */
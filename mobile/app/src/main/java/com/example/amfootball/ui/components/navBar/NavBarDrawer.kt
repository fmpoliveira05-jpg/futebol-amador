package com.example.amfootball.ui.components.navBar

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material3.DrawerState
import androidx.compose.material3.DrawerValue
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.ModalDrawerSheet
import androidx.compose.material3.ModalNavigationDrawer
import androidx.compose.material3.NavigationDrawerItem
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.rememberDrawerState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.example.amfootball.data.NavigationItem
import kotlinx.coroutines.launch
import com.example.amfootball.R

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NavigationDrawer(itens: List<NavigationItem>,
                    titleNavBar: String,
                    scaffoldContent: @Composable (NavHostController) -> Unit,
                    internalNavController: NavHostController,
                    globalNavController: NavHostController,
                    topBarActions: @Composable RowScope.() -> Unit = {} //Diz há topBar que pode receber ações
) {
    val drawerState = rememberDrawerState(initialValue = DrawerValue.Closed)
    val drawerItemList = itens
    var selectedItem by remember { mutableStateOf(drawerItemList[0]) }

    ModalNavigationDrawer(
        drawerState = drawerState,
        drawerContent = {
            ModalDrawerSheet {
                Spacer(Modifier.height(12.dp))
                drawerItemList.forEach { item ->
                    DrawerItem(
                        item = item,
                        selectedItem = selectedItem,
                        updatedSelected = { selectedItem = it },
                        internalNavController = internalNavController, // Passa o interno
                        globalNavController = globalNavController,   // Passa o global
                        drawerState = drawerState,
                    )
                }
            }
        },
        content = {
            Scaffold(
                drawerState = drawerState,
                titleNavBar = titleNavBar,
                internalNavController = internalNavController,
                scaffoldContent = scaffoldContent,
                topBarActions = topBarActions
            )
        }
    )
}
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DrawerItem(item: NavigationItem,
               selectedItem: NavigationItem,
               updatedSelected: (i: NavigationItem) -> Unit,
               internalNavController: NavHostController,
               globalNavController: NavHostController,
               drawerState: DrawerState
) {
    val coroutineScope = rememberCoroutineScope()

    NavigationDrawerItem(
        icon = { Icon(imageVector = item.icon, contentDescription = item.description) },
        label = { Text(text = item.label) },
        selected = (item == selectedItem),
        onClick = {
            coroutineScope.launch {
                if (item.isGlobalRoute) {
                    globalNavController.navigate(item.route)
                } else {
                    internalNavController.navigate(item.route)
                }
                drawerState.close()
            }
            updatedSelected(item)
        }
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun Scaffold(drawerState: DrawerState,
             titleNavBar: String,
             internalNavController: NavHostController,
             scaffoldContent: @Composable (NavHostController) -> Unit,
             topBarActions: @Composable RowScope.() -> Unit = {}
) {
    val coroutineScope = rememberCoroutineScope()

    Scaffold(
        topBar = {
            TopBar(
                onNavIconClick = {
                    coroutineScope.launch {
                        drawerState.open()
                    }
                },
                titleNavBar = titleNavBar,
                actions = topBarActions
            )
        },
        content = { padding ->
            Column(modifier = Modifier.padding(padding)) {
                scaffoldContent(internalNavController)
            }
        }
    )
}

//Metodo que ao clicar no icon da navBar mostra a mesma
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TopBar(onNavIconClick: () -> Unit,
           titleNavBar: String,
           actions: @Composable RowScope.() -> Unit = {}
) {
    TopAppBar(
        title = { Text(text = titleNavBar) },
        navigationIcon = {
            IconButton(
                onClick = {
                    onNavIconClick()
                }
            ) {
                Icon(
                    imageVector = Icons.Default.Menu,
                    contentDescription = stringResource(R.string.content_description_navbar)
                )
            }
        },
        actions = actions
    )
}

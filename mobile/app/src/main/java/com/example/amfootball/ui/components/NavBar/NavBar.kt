package com.example.amfootball.ui.components.NavBar

import androidx.compose.foundation.layout.Column
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
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.NavigationItem
import kotlinx.coroutines.launch
import com.example.amfootball.R

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun NavigatonDrawer(itens: List<NavigationItem>,
                    titleNavBar: String,
                    drawerState: DrawerState,
                    scaffoldContent: @Composable (NavHostController) -> Unit
){
    val drawerState = rememberDrawerState(initialValue = DrawerValue.Closed)
    val navController = rememberNavController()
    val drawerItemList = itens
    var selectedItem by remember { mutableStateOf(drawerItemList[0]) }

    ModalNavigationDrawer(
        drawerState = drawerState,
        drawerContent = {
            //Faz o desenho de cada item da navBar
            ModalDrawerSheet {
                Spacer(Modifier.height(12.dp))
                drawerItemList.forEach { item ->
                    DrawerItem(item,selectedItem,{selectedItem=item},navController,drawerState)
                }
            }
        },
        //Parte que desenha a navBar em si
        content = { Scaffold(drawerState = drawerState, titleNavBar = titleNavBar,navController = navController, scaffoldContent = scaffoldContent)}
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DrawerItem(item: NavigationItem,
               selectedItem: NavigationItem,
               updatedSelected: (i: NavigationItem) -> Unit,
               navController: NavHostController,
               drawerState: DrawerState
) {
    val coroutineScope = rememberCoroutineScope()

    //Resposavel por desenhar os itens da navBar
    NavigationDrawerItem(
        icon = { Icon(imageVector = item.icon, contentDescription = item.description) },
        label = { Text(text = item.label) },
        selected = (item == selectedItem), //Definir item selecionado
        onClick = {
            coroutineScope.launch {
                navController.navigate(item.route)
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
             navController: NavHostController,
             scaffoldContent: @Composable (NavHostController) -> Unit
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
                titleNavBar = titleNavBar
            )
        },
        content = { padding ->
            Column(modifier = Modifier.padding(padding)) {
                scaffoldContent(navController)
            }
        }
    )
}

//Metodo que ao clicar no icon da navBar mostra a mesma
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TopBar(onNavIconClick: () -> Unit, titleNavBar: String) {
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
                    contentDescription = stringResource(R.string.content_Description_NavBar)
                )
            }
        }
    )
}

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
import com.example.amfootball.R
import com.example.amfootball.data.NavigationItem
import kotlinx.coroutines.launch

/**
 * Componente de alto nível que implementa o padrão de navegação lateral (Navigation Drawer).
 *
 * Este componente estrutura a aplicação envolvendo o conteúdo principal num [ModalNavigationDrawer]
 * e fornece a infraestrutura para navegar entre diferentes ecrãs.
 *
 * **Arquitetura de Navegação:**
 * Suporta dois controladores de navegação distintos para gerir hierarquias diferentes:
 * 1. **Internal:** Para navegação dentro do fluxo principal (Dashboard, Equipas, Calendário).
 * 2. **Global:** Para navegação de raiz (Logout, Definições, Login), permitindo sair do contexto do Drawer.
 *
 * @param itens A lista de [NavigationItem] que popula o menu lateral.
 * @param titleNavBar O título a ser exibido na [TopBar].
 * @param scaffoldContent O conteúdo principal do ecrã (Slot API). Recebe o [internalNavController] como parâmetro.
 * @param internalNavController O controlador de navegação para as vistas internas do Drawer.
 * @param globalNavController O controlador de navegação para rotas globais da aplicação.
 * @param topBarActions Ações opcionais (ícones/botões) a exibir no lado direito da barra superior.
 */
//Nota: Não utilizada (atualmente)
@Composable
fun NavigationDrawer(
    itens: List<NavigationItem>,
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
                        internalNavController = internalNavController,
                        globalNavController = globalNavController,
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

/**
 * Representa um item individual clicável dentro do menu lateral.
 *
 * Gere a lógica de decisão de roteamento: verifica se o item é uma rota global ou interna
 * e dispara a navegação no controlador correto, fechando o menu de seguida.
 *
 * @param item O objeto de dados [NavigationItem] contendo ícone, texto e rota.
 * @param selectedItem O item atualmente ativo (para realce visual).
 * @param updatedSelected Callback para atualizar o estado do item selecionado.
 * @param internalNavController Controlador para navegação interna.
 * @param globalNavController Controlador para navegação global (se [item.isGlobalRoute] for true).
 * @param drawerState Estado do Drawer para permitir o fecho automático após o clique.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DrawerItem(
    item: NavigationItem,
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

/**
 * Wrapper personalizado sobre o [Scaffold] do Material3.
 *
 * O objetivo deste wrapper é padronizar a estrutura visual de todos os ecrãs que vivem
 * dentro do NavigationDrawer, garantindo que todos possuem:
 * 1. Uma [TopBar] configurada com o botão de menu (Hamburger).
 * 2. O comportamento correto de abrir o Drawer ao clicar no menu.
 * 3. O padding correto para o conteúdo.
 *
 * @param drawerState Estado do Drawer, necessário para abrir o menu lateral via TopBar.
 * @param titleNavBar O título a exibir na barra superior.
 * @param internalNavController Passado para o conteúdo para permitir navegação interna.
 * @param scaffoldContent O conteúdo do ecrã a ser renderizado.
 * @param topBarActions Ações contextuais opcionais para a TopBar.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun Scaffold(
    drawerState: DrawerState,
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

/**
 * Barra superior da aplicação (AppBar).
 *
 * Exibe o título do ecrã atual e fornece o botão de navegação "Menu" (Hamburger icon)
 * para abrir o Navigation Drawer.
 *
 * @param onNavIconClick Callback disparado ao clicar no ícone do menu.
 * @param titleNavBar O texto do título.
 * @param actions Composable opcional para ícones extra à direita (ex: Filtros, Pesquisa).
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TopBar(
    onNavIconClick: () -> Unit,
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

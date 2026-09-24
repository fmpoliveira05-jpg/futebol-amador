package com.example.amfootball.data

import androidx.compose.ui.graphics.vector.ImageVector

/**
 * Modelo de dados que representa um item individual de navegação na interface da aplicação.
 *
 * Esta classe é utilizada para configurar os menus de navegação (como a Barra Inferior ou Menu Lateral),
 * associando um destino (rota) a elementos visuais (ícone e texto).
 *
 * @property label O título visível do item (ex: "Home", "Perfil").
 * É o texto que aparece por baixo do ícone na barra de navegação.
 *
 * @property description A descrição textual para fins de acessibilidade (Content Description).
 * Essencial para leitores de ecrã (como o TalkBack) descreverem o botão a utilizadores com deficiência visual.
 *
 * @property icon O ícone vetorial ([ImageVector]) que representa visualmente o destino.
 * Geralmente proveniente da biblioteca `Icons.Filled` ou `Icons.Outlined` do Material Design.
 *
 * @property route A string única que identifica o destino no grafo de navegação (NavGraph).
 * É este valor que é passado ao `navController.navigate()` quando o item é clicado.
 *
 * @property isGlobalRoute Indica se este item pertence à navegação global/principal ou se é uma rota auxiliar.
 * - `false` (Padrão): O item faz parte da navegação principal (ex: aparece na BottomBar).
 * - `true`: O item é uma rota global (ex: Ecrã de Login, Definições) que pode não aparecer nos menus de navegação padrão.
 */
data class NavigationItem(
    val label: String,
    val description: String,
    val icon: ImageVector,
    val route: String,
    val isGlobalRoute: Boolean = false
)
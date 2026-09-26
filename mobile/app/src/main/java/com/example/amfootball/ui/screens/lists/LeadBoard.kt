package com.example.amfootball.ui.screens.lists

import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import com.example.amfootball.competicao.ClassificacaoScreen

/**
 * Classificação da liga (antes: ranking por pontos acumulados). A tabela, o seletor de liga e a
 * cópia offline estão em [ClassificacaoScreen].
 */
@Composable
fun LeaderboardScreen(navHostController: NavHostController) {
    ClassificacaoScreen(navHostController = navHostController)
}

package com.example.amfootball.ui.screens.match

import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController

@Composable
fun CancelMatchScreen(navHostController: NavHostController) {
    //TODO: Mostar dados do match e meter uma description para ser introduzido o motivo do cancelamneto

    Text(text = "Cancelar Partida")
}

@Preview(
    name = "Cancelar Partida - PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "Cancel Match - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewCancelMatchScreen() {
    CancelMatchScreen(rememberNavController())
}
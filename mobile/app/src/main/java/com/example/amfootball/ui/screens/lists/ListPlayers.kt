package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController

@Composable
fun ListPlayersScreen(
    navHostController: NavHostController
) {
    Column() {
        Text(text = "Lista de jogadores")
    }
}


@Preview(
    name = "Lista de Jogadores - PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "List Players - En",
    locale = "en",
    showBackground = true
)
@Composable
fun ListPlayersPreview() {
    ListPlayersScreen(rememberNavController())
}
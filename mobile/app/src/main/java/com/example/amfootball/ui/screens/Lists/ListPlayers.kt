package com.example.amfootball.ui.screens.Lists

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview

@Composable
fun ListPlayersScreen() {
    Column() {
        Text(text = "Lista de jogadores")
    }
}


@Preview(showBackground = true)
@Composable
fun ListPlayersPreview() {
    ListPlayersScreen()
}
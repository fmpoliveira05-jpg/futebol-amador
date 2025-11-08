package com.example.amfootball.ui.screens.MatchInvite

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview

@Composable
fun ListMatchInviteScreen() {
    Column() {
        Text(text = "lista de partidas da equipa")
    }
}


@Preview(showBackground = true)
@Composable
fun PreviewListMatchInvite() {
    ListMatchInviteScreen()
}
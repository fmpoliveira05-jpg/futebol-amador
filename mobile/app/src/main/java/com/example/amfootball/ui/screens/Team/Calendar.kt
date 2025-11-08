package com.example.amfootball.ui.screens.Team

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import com.example.amfootball.navigation.Objects.Pages.MatchInviteRoutes

@Composable
fun CalendarScreen(globalNavController: NavHostController) {
    Button(
        onClick = {
            globalNavController.navigate(route = MatchInviteRoutes.SEND_MATCH_INVITE)
        }
    ) {
        Text(text = "Enviar Convite de Partida")
    }
}
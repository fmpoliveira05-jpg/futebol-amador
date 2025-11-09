package com.example.amfootball.ui.screens.Team

import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import androidx.navigation.NavHostController
import com.example.amfootball.navigation.Objects.Pages.MatchInviteRoutes
import com.example.amfootball.R

@Composable
fun CalendarScreen(globalNavController: NavHostController) {
    Button(
        onClick = {
            globalNavController.navigate(route = MatchInviteRoutes.SEND_MATCH_INVITE)
        }
    ) {
        Text(text = stringResource(id= R.string.button_send_match_invite))
    }
}
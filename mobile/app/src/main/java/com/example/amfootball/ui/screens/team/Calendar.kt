package com.example.amfootball.ui.screens.team

import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.res.stringResource
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.navigation.Objects.Routes

@Composable
fun CalendarScreen(globalNavController: NavHostController) {
    Button(
        onClick = {
            globalNavController.navigate(route = Routes.TeamRoutes.SEND_MATCH_INVITE.route)
        }
    ) {
        Text(text = stringResource(id= R.string.button_send_match_invite))
    }
}
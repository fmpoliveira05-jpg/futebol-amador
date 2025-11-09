package com.example.amfootball.ui.screens.MatchInvite

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.ui.components.BackTopBar
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.compose.ui.tooling.preview.Preview
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.ui.components.InputFields.CreateTextFieldOutline
import com.example.amfootball.ui.components.InputFields.DatePickerDocked
import com.example.amfootball.ui.components.InputFields.FieldTimePicker
import com.example.amfootball.ui.components.InputFields.Switcher
import com.example.amfootball.ui.components.InputFields.convertMillisToDate

@Composable
fun SendMatchInviteScreen(navHostController: NavHostController) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = stringResource(id = R.string.title_page_send_match_Invite)
        )},
        content = { paddingValues ->
            ContentSendMatchInviteScreen(
                modifier = Modifier
                    .padding(paddingValues)
                    .padding(16.dp),
                navHostController = navHostController
            )
        }
    )
}

@Composable
private fun ContentSendMatchInviteScreen(modifier: Modifier = Modifier,
                                         navHostController: NavHostController) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        FieldsSendMatchInvite(navHostController = navHostController)
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun FieldsSendMatchInvite(navHostController: NavHostController) {
    //Depois aqui ou outro lado tenho de criar a variavel para transformar estes dois num DateTime
    var gameDate by remember { mutableStateOf("") }
    var gameTime by remember { mutableStateOf("") }
    var isHomeGame by remember { mutableStateOf(true) }

    val isFormValid = remember(gameDate, gameTime) {
        val isDateValid = gameDate.isNotEmpty()
        val isTimeValid = gameTime.isNotEmpty()
        val isTimeLongEnough = gameTime.length == 5

        isDateValid && isTimeValid && isTimeLongEnough
    }
    CreateTextFieldOutline(
        label = "Opponente",
        value = "Equipa xxxx",
        isReadOnly = true,
    )

    DatePickerDocked(
        value = gameDate,
        onDateSelected = { millis ->
            gameDate = convertMillisToDate(millis)
        },
        label = stringResource(id = R.string.game_date),
        contentDescription = stringResource(id = R.string.description_game_date),
    )

    FieldTimePicker(
        value = gameTime,
        onValueChange = { newTime ->
            gameTime = newTime
        },
        label = stringResource(id = R.string.game_hours),
        contentDescription = stringResource(id = R.string.description_game_date)
    )

    Switcher(
        value = isHomeGame,
        onCheckedChange = { isChecked ->
            isHomeGame = isChecked
        },
        text = stringResource(id = R.string.playing_home),
        textChecked = stringResource(id = R.string.checked_playing_home),
        textUnChecked = stringResource(id = R.string.unchecked_playing_home),
    )

    Button(
        onClick = {
            navHostController.navigate(RoutesNavBarTeam.HOME_PAGE_TEAM) {
                popUpTo(0)
                println("Convite enviado!")
            }
        },
        enabled = isFormValid,
        modifier = Modifier
            .fillMaxWidth()
            .height(56.dp)
    ) {
        Icon(Icons.Default.Send, contentDescription = "Enviar Convite")
        Spacer(Modifier.width(8.dp))
        Text(text = stringResource(R.string.button_send_match_invite))
    }
}

@Preview(name = "Send Match Invite - EN", locale = "en", showBackground = true)
@Preview(name = "Enviar Pedido de partida - PT", locale = "pt", showBackground = true)
@Composable
fun SignUpScreenContentPreview() {
    SendMatchInviteScreen(navHostController = rememberNavController())
}
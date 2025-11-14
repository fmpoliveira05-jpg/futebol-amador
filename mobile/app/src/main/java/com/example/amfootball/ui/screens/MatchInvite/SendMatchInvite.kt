package com.example.amfootball.ui.screens.MatchInvite

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Scaffold
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
import com.example.amfootball.navigation.Objects.Routes
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.inputFields.DatePickerDockedLimitedDate
import com.example.amfootball.ui.components.inputFields.FieldTimePicker
import com.example.amfootball.ui.components.inputFields.Switcher
import com.example.amfootball.ui.components.inputFields.convertMillisToDate

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

    //Estado de controlo de clicar no botão

    TextFieldOutline(
        label = "Opponente",
        value = "Equipa xxxx",
        isReadOnly = true,
    )

    DatePickerDockedLimitedDate(
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

    val isFormValid = remember(gameDate, gameTime) {
        IsFormValid(gameDate, gameTime)
    }

    SubmitFormButton(
        onClick = {
            if (isFormValid) {
                navHostController.navigate(Routes.TeamRoutes.HOMEPAGE.route) {
                    popUpTo(0)
                    println("Convite enviado!")
                }
            }
        },
        imageButton = Icons.Default.Send,
        text = stringResource(R.string.button_send_match_invite),
        contentDescription = stringResource(id = R.string.button_description_send_match_invite)
    )
}

private fun IsFormValid(gameDate: String, gameTime: String): Boolean {
    val isDateValid = gameDate.isNotEmpty()
    val isTimeValid = gameTime.isNotEmpty()
    val isTimeLongEnough = gameTime.length == 5

    return isDateValid && isTimeValid && isTimeLongEnough
}


@Preview(name = "Send Match Invite - EN", locale = "en", showBackground = true)
@Preview(name = "Enviar Pedido de partida - PT", locale = "pt", showBackground = true)
@Composable
fun SignUpScreenContentPreview() {
    SendMatchInviteScreen(navHostController = rememberNavController())
}
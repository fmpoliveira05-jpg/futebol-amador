package com.example.amfootball.ui.screens.Match

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.ui.components.BackTopBar
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.R
import com.example.amfootball.ui.components.InputFields.TextFieldOutline

//De alguma for vai ser enviado como parametro deste metodo o adversario e a equipa atual
//Depois quando tiver isso adaptar este metodo
@Composable
fun FinishMatchScreen(navHostController: NavHostController) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = "Finish Match"
            )
        },
        content = { paddingValues ->
            FormFinishMatch(modifier = Modifier
                .padding(paddingValues)
                .padding(16.dp)
            )
        }
    )
}

@Composable
private fun FormFinishMatch(modifier: Modifier = Modifier) {
    Column(modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        TextFieldForm()
    }
}

@Composable
private fun TextFieldForm() {
    var numGoalsTeam by remember { mutableStateOf("") }
    var numGoalsAdversary by remember { mutableStateOf("") }

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_team),
        value = numGoalsTeam,
        onValueChange = { newText ->
            if (newText.isEmpty()) {
                numGoalsTeam = newText
            }
            else if (newText.all { it.isDigit() }) {
                val number = newText.toIntOrNull()
                if (number != null && number > 0 && number < 100) {
                    numGoalsTeam = newText
                }
            }
        },
        isRequired = true,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_opponent_team),
        value = numGoalsAdversary,
        onValueChange = { newText ->
            if (newText.isEmpty()) {
                numGoalsAdversary = newText
            }
            else if (newText.all { it.isDigit() }) {
                val number = newText.toIntOrNull()
                if (number != null && number > 0 && number < 100) {
                    numGoalsAdversary = newText
                }
            }
        },
        isRequired = true,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )
}

@Preview(name= "FinishMatch EN", locale = "en", showBackground = true)
@Preview(name= "FinishMatch PT", locale = "pt", showBackground = true)
@Composable
fun PreviewFinishMatch(){
    AMFootballTheme{
        FinishMatchScreen(navHostController = rememberNavController())
    }
}
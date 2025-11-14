package com.example.amfootball.ui.screens.match

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.ui.components.BackTopBar
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.R
import com.example.amfootball.ui.components.Buttons.SubmitFormButton
import com.example.amfootball.ui.components.InputFields.TextFieldOutline
import com.example.amfootball.ui.viewModel.match.FinishMatchViewModel

//TODO: Mandar também o idMatch e depois com base disso fazer um pedido há API para carregar as teams da match
@Composable
fun FinishMatchScreen(
    navHostController: NavHostController,
    viewModel: FinishMatchViewModel = viewModel()
) {
    val numGoalsTeam = viewModel.numGoalsTeam.value
    val numGoalsOpponent = viewModel.numGoalsOpponent.value

    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = stringResource(id = R.string.title_finish_Match)
            )
        },
        content = { paddingValues ->
            FormFinishMatch(
                numGoalsTeam = numGoalsTeam,
                numGoalsOpponent = numGoalsOpponent,
                onNumGoalsTeamChange = { viewModel.onNumGoalsTeamChange(numGoalsTeam) },
                onNumGoalsOpponentChange = { viewModel.onNumGoalsOponnetChange(numGoalsOpponent) },
                onSubmitForm = { viewModel.onSubmitForm() },
                modifier = Modifier
                .padding(paddingValues)
                .padding(16.dp)
            )
        }
    )
}

@Composable
private fun FormFinishMatch(
    numGoalsTeam: Int,
    numGoalsOpponent: Int,
    onNumGoalsTeamChange: () -> Unit,
    onNumGoalsOpponentChange: () -> Unit,
    onSubmitForm: () -> Unit,
    modifier: Modifier = Modifier) {
    Column(modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        TextFieldForm(
            numGoalsTeam = numGoalsTeam,
            numGoalsOpponent = numGoalsOpponent,
            onNumGoalsTeamChange = onNumGoalsTeamChange,
            onNumGoalsOpponentChange = onNumGoalsOpponentChange,
            onSubmitForm = onSubmitForm
        )
    }
}

@Composable
private fun TextFieldForm(
    numGoalsTeam: Int,
    numGoalsOpponent: Int,
    onNumGoalsTeamChange: () -> Unit,
    onNumGoalsOpponentChange: () -> Unit,
    onSubmitForm: () -> Unit
) {

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_team),
        value = numGoalsTeam.toString(),
        onValueChange = { onNumGoalsTeamChange() },
        isRequired = true,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_opponent_team),
        value = numGoalsOpponent.toString(),
        onValueChange = { onNumGoalsOpponentChange() },
        isRequired = true,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    SubmitFormButton(
        onClick = { onSubmitForm() }
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
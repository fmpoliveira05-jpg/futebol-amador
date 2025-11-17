package com.example.amfootball.ui.screens.match

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.R
import com.example.amfootball.data.actions.forms.FormFinishMatchActions
import com.example.amfootball.data.dtos.match.ResultMatchDto
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.viewModel.match.FinishMatchViewModel
import com.example.amfootball.utils.GeneralConst

//TODO: Mandar também o idMatch e depois com base disso fazer um pedido há API para carregar as teams da match
@Composable
fun FinishMatchScreen(
    navHostController: NavHostController,
    viewModel: FinishMatchViewModel = viewModel()
) {
    val result by viewModel.resutl.observeAsState(initial = null)
    val formActions = FormFinishMatchActions(
        onNumGoalsTeamChange = viewModel::onNumGoalsTeamChange,
        onNumGoalsOpponentChange = viewModel::onNumGoalsOponnetChange,
        onSubmitForm = viewModel::onSubmitForm
    )

    FormFinishMatch(
        result = result,
        formActions = formActions,
        navHostController = navHostController,
        modifier = Modifier
        .padding(16.dp)
    )
}

@Composable
private fun FormFinishMatch(
    result: ResultMatchDto?,
    formActions: FormFinishMatchActions,
    navHostController: NavHostController,
    modifier: Modifier = Modifier) {
    Column(modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        TextFieldForm(
            result = result,
            formActions = formActions,
            navHostController = navHostController
        )
    }
}

@Composable
private fun TextFieldForm(
    result: ResultMatchDto?,
    formActions: FormFinishMatchActions,
    navHostController: NavHostController
) {

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_team),
        value = result?.numGoals?.toString() ?: "",
        minLenght = GeneralConst.MIN_GOALS,
        maxLenght = GeneralConst.MAX_GOALS,
        onValueChange = { formActions.onNumGoalsTeamChange(it.toIntOrNull() ?: 0) },
        isRequired = true,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_opponent_team),
        value = result?.numGoalsOpponent?.toString() ?: "",
        minLenght = GeneralConst.MIN_GOALS,
        maxLenght = GeneralConst.MAX_GOALS,
        onValueChange = { formActions.onNumGoalsOpponentChange(it.toIntOrNull() ?: 0) },
        isRequired = true,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    SubmitFormButton(
        onClick = { formActions.onSubmitForm(navHostController) }
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
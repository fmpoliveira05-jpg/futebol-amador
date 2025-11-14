package com.example.amfootball.ui.screens.matchInvite

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.ui.components.BackTopBar
import androidx.compose.runtime.getValue
import androidx.compose.ui.tooling.preview.Preview
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.actions.forms.FormMatchInviteActions
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.errors.MatchInviteFormErros
import com.example.amfootball.ui.components.Buttons.SubmitFormButton
import com.example.amfootball.ui.components.InputFields.TextFieldOutline
import com.example.amfootball.ui.components.InputFields.DatePickerDockedLimitedDate
import com.example.amfootball.ui.components.InputFields.FieldTimePicker
import com.example.amfootball.ui.components.InputFields.Switcher
import com.example.amfootball.ui.viewModel.MatchInvite.FormMatchInviteViewModel
import com.example.amfootball.utils.TeamConst

@Composable
fun FormMatchInviteScreen(
    viewModel: FormMatchInviteViewModel = viewModel(),
    navHostController: NavHostController
) {
    val fields by viewModel.uiFormState.collectAsState()
    val errors by viewModel.uiErrorsForm.collectAsState()

    val actions = FormMatchInviteActions(
        onGameDateChange = viewModel::onGameDateChange,
        onTimeGameChange = viewModel::onTimeGameChange,
        onLocalGameChange = viewModel::onLocalGameChange,
        onSubmitForm = viewModel::onSubmitForm
    )

    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = stringResource(id = R.string.title_page_send_match_Invite)
            )},
        content = { paddingValues ->
            ContentSendMatchInviteScreen(
                navHostController = navHostController,
                fields = fields,
                actions = actions,
                errors = errors,
                modifier = Modifier
                    .padding(paddingValues)
                    .padding(16.dp),
            )
        }
    )
}

@Composable
private fun ContentSendMatchInviteScreen(
    navHostController: NavHostController,
    fields: MatchInviteDto,
    actions: FormMatchInviteActions,
    errors: MatchInviteFormErros,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        FieldsSendMatchInvite(
            fields = fields,
            errors = errors,
            actions = actions,
            navHostController = navHostController
        )
    }
}

//TODO: Meter para os datePicker e TimePicker receberem texto de erro
@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun FieldsSendMatchInvite(
    fields: MatchInviteDto,
    errors: MatchInviteFormErros,
    actions: FormMatchInviteActions,
    navHostController: NavHostController) {

    TextFieldOutline(
        label = "Opponente",
        value = fields.nameOpponent,
        isReadOnly = true,
    )


    DatePickerDockedLimitedDate(
        value = fields.gameDate ?: "",
        onDateSelected = actions.onGameDateChange,
        label = stringResource(id = R.string.game_date),
        contentDescription = stringResource(id = R.string.description_game_date),
        isError = errors.dateError != null,
        isSingleLine = false,
        errorMessage = errors.dateError?.let { stringResource(id = it) }
    )

    FieldTimePicker(
        value = fields.gameTime ?: "",
        onValueChange = actions.onTimeGameChange,
        label = stringResource(id = R.string.game_hours),
        contentDescription = stringResource(id = R.string.description_game_date) ,
        isError = errors.dateError != null,
        errorMessage = errors.timeError?.let { stringResource(id = it) },
    )

    Switcher(
        value = fields.isHomeGame,
        onCheckedChange = actions.onLocalGameChange,
        text = stringResource(id = R.string.playing_home),
        textChecked = stringResource(id = R.string.checked_playing_home),
        textUnChecked = stringResource(id = R.string.unchecked_playing_home),
    )

    SubmitFormButton(
        onClick = { actions.onSubmitForm(navHostController) },
        imageButton = Icons.Default.Send,
        text = stringResource(R.string.button_send_match_invite),
        contentDescription = stringResource(id = R.string.button_description_send_match_invite)
    )
}



@Preview(name = "Send Match Invite - EN", locale = "en", showBackground = true)
@Preview(name = "Enviar Pedido de partida - PT", locale = "pt", showBackground = true)
@Composable
fun SendMatchInviteScreenPreview() {
    FormMatchInviteScreen(navHostController = rememberNavController())
}

@Preview(name = "Send Match Invite - EN", locale = "en", showBackground = true)
@Preview(name = "Enviar Pedido de partida - PT", locale = "pt", showBackground = true)
@Composable
fun NegociateMatchInviteScreenPreview() {
    FormMatchInviteScreen(navHostController = rememberNavController())
}
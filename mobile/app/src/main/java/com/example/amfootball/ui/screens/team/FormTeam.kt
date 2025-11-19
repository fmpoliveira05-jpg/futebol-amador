package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.imePadding
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.GroupAdd
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.actions.forms.FormTeamActions
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.errors.formErrors.TeamFormErros
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.inputFields.ImagePicker
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.team.TeamFormViewModel
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst

@Composable
fun FormTeamScreen(
    navHostController: NavHostController,
    viewModel: TeamFormViewModel = viewModel()
) {
    val uiForm by viewModel.uiFormState.observeAsState(initial = FormTeamDto())
    val uiErrors by viewModel.uiErrors.observeAsState(initial = TeamFormErros())

    val fieldTeamAction = FormTeamActions(
        onNameChange = viewModel::onNameChange,
        onDescriptionChange = viewModel::onDescriptionChange,
        onImageChange = viewModel::onImageChange,
        onNamePitchChange = viewModel::onNamePitchChange,
        onAddressPitchChange = viewModel::onAddressPitchChange,
    )

    ContentCreateTeam(
        filedsTeam = uiForm,
        fieldTeamAction = fieldTeamAction,
        fieldsErrors = uiErrors,
        onSubmitClick = { viewModel.onSubmit(navHostController = navHostController) },
        modifier = Modifier.padding(16.dp),
    )
}

@Composable
private fun ContentCreateTeam(modifier: Modifier = Modifier,
                              filedsTeam: FormTeamDto,
                              fieldTeamAction: FormTeamActions,
                              fieldsErrors: TeamFormErros,
                              onSubmitClick: () -> Unit
) {
    val scrollState = rememberScrollState()
    Column(
        modifier = modifier
            .verticalScroll(scrollState)
            .imePadding(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        FieldsCreateTeam(
            filedTeam = filedsTeam,
            fieldTeamAction = fieldTeamAction,
            fieldsErrors = fieldsErrors,
            onSubmitClick = onSubmitClick
        )
    }
}

//Falta criar campo para afixar imagem
@Composable
private fun FieldsCreateTeam(
    filedTeam: FormTeamDto,
    fieldTeamAction: FormTeamActions,
    fieldsErrors: TeamFormErros,
    onSubmitClick: () -> Unit
) {
    ImagePicker(
        imageSelectedUri = filedTeam.image,
        onImageSelected = { fieldTeamAction.onImageChange(it) },
        modifier = Modifier.padding(bottom = 24.dp)
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_team),
        value = filedTeam.name,
        onValueChange = { fieldTeamAction.onNameChange(it) },
        isRequired = true,
        maxLenght = TeamConst.MAX_NAME_LENGTH,
        isError = fieldsErrors.nameError != null,
        errorMessage = fieldsErrors.nameError?.let {
            stringResource(id = it.messageId,
                *it.args.toTypedArray())
        }
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_description_team),
        value = filedTeam.description ?: "",
        onValueChange = { fieldTeamAction.onDescriptionChange(it) },
        isSingleLine = false,
        isRequired = false,
        maxLenght = TeamConst.MAX_DESCRIPTION_LENGTH,
        isError = fieldsErrors.descriptionError != null,
        errorMessage = fieldsErrors.descriptionError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray())
        }
    )

    Text(text = stringResource(id = R.string.label_fields_pitch))
    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_pitch_team),
        value = filedTeam.pitch.name,
        onValueChange = { fieldTeamAction.onNamePitchChange(it) },
        isSingleLine = true,
        isRequired = true,
        maxLenght = PitchConst.MAX_NAME_LENGTH,
        isError = fieldsErrors.pitchNameError != null,
        errorMessage = fieldsErrors.pitchNameError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray())
        }
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_address_pitch_team),
        value = filedTeam.pitch.address,
        onValueChange = { fieldTeamAction.onAddressPitchChange(it) },
        isSingleLine = true,
        isReadOnly = false,
        isRequired = true,
        maxLenght = GeneralConst.MAX_ADDRESS_LENGTH,
        isError = fieldsErrors.pitchAddressError != null,
        errorMessage = fieldsErrors.pitchAddressError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray())
        }
    )

    SubmitFormButton(
        onClick = { onSubmitClick() },
        imageButton = Icons.Default.GroupAdd,
        text = stringResource(id = R.string.button_create_team),
        contentDescription = stringResource(id = R.string.button_description_create_team)
    )
}


@Preview(name = "Create Team - En",
    locale = "en",
    showBackground = true)
@Preview(name = "Criar Equipa - PT",
    locale = "pt-rPT",
    showBackground = true)
@Composable
fun PreviewFormTeamCreate() {
    AMFootballTheme {
        ContentCreateTeam(
            filedsTeam = FormTeamDto(),
            fieldTeamAction = FormTeamActions(
                onNameChange = {},
                onDescriptionChange = {},
                onImageChange = {},
                onNamePitchChange = {},
                onAddressPitchChange = {},
            ),
            fieldsErrors = TeamFormErros(), // Sem erros
            onSubmitClick = {}, // Botão não faz nada no preview
            modifier = Modifier.padding(16.dp)
        )
    }}

@Preview(name = "Edit Team - En",
    locale = "en",
    showBackground = true)
@Preview(name = "Editar Equipa - PT",
    locale = "pt-rPT",
    showBackground = true)
@Composable
fun PreviewFormTeamEdit() {
    AMFootballTheme {
        ContentCreateTeam(
            filedsTeam = FormTeamDto.generateEditExampleTeam(),
            fieldTeamAction = FormTeamActions(
                onNameChange = {},
                onDescriptionChange = {},
                onImageChange = {},
                onNamePitchChange = {},
                onAddressPitchChange = {},
            ),
            fieldsErrors = TeamFormErros(),
            onSubmitClick = {},
            modifier = Modifier.padding(16.dp)
        )
    }
}
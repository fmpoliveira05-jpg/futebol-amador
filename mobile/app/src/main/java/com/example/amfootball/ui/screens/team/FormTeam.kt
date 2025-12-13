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
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.PitchConst
import com.example.amfootball.core.utils.TeamConst
import com.example.amfootball.data.events.UiState
import com.example.amfootball.data.remote.dtos.team.FormTeamDto
import com.example.amfootball.domains.errors.formErrors.TeamFormErros
import com.example.amfootball.ui.actions.forms.FormTeamActions
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.EditFormButton
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.ImagePickerString
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.notification.ToastHandler
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.previewsMocks.EditTeamMocks
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.team.TeamFormViewModel

/**
 * Ecrã principal para Criação e Edição de Equipas.
 *
 * Este Composable atua como contentor de estado (Stateful):
 * - Coleta o estado do formulário, erros e rede do [TeamFormViewModel].
 * - Define as ações de interação com os campos (callbacks).
 * - Gere o estado de Loading e Erros de submissão.
 *
 * @param navHostController Controlador de navegação.
 * @param viewModel O ViewModel que gere a lógica do formulário.
 */
@Composable
fun FormTeamScreen(
    navHostController: NavHostController,
    viewModel: TeamFormViewModel = hiltViewModel()
) {
    val uiForm by viewModel.uiFormState.collectAsStateWithLifecycle()
    val uiErrors by viewModel.uiFormErrors.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isEditMode = viewModel.isEditMode

    val fieldTeamAction = FormTeamActions(
        onNameChange = viewModel::onNameChange,
        onDescriptionChange = viewModel::onDescriptionChange,
        onImageChange = viewModel::onImageChange,
        onNamePitchChange = viewModel::onNamePitchChange,
        onAddressPitchChange = viewModel::onAddressPitchChange,
    )

    ToastHandler(
        toastMessage = uiState.toastMessage,
        onToastShown = viewModel::onToastShown
    )

    ContentCreateTeam(
        filedsTeam = uiForm,
        fieldTeamAction = fieldTeamAction,
        fieldsErrors = uiErrors,
        uiState = uiState,
        onSubmitClick = {
            viewModel.onSubmit(
                onSucess = {
                    navHostController.navigate(route = Routes.TeamRoutes.HOMEPAGE.route) {
                        popUpTo(Routes.TeamRoutes.HOMEPAGE.route) { inclusive = true }
                    }
                }
            )
        },
        onRetry = { viewModel.retry() },
        isEditMode = isEditMode,
        modifier = Modifier.padding(16.dp),
    )
}

/**
 * Conteúdo visual do formulário (Stateless).
 *
 * Responsável por organizar o layout, exibir o loading/erro global e o banner offline.
 * Envolve os campos do formulário num scroll vertical.
 *
 * @param filedsTeam Estado atual dos dados do formulário.
 * @param fieldTeamAction Ações para atualizar os campos.
 * @param fieldsErrors Erros de validação específicos de cada campo.
 * @param uiState Estado global da UI (Loading/Erro de rede).
 * @param onRetry Callback para tentar novamente em caso de erro global.
 * @param onSubmitClick Callback para submeter o formulário.
 */
@Composable
private fun ContentCreateTeam(
    filedsTeam: FormTeamDto,
    fieldTeamAction: FormTeamActions,
    fieldsErrors: TeamFormErros,
    uiState: UiState,
    onRetry: () -> Unit,
    onSubmitClick: () -> Unit,
    isEditMode: Boolean,
    modifier: Modifier = Modifier
) {
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = onRetry,
        content = {
            Column(
                modifier = modifier
                    .verticalScroll(rememberScrollState())
                    .imePadding(),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.Center
            ) {
                FieldsCreateTeam(
                    filedTeam = filedsTeam,
                    fieldTeamAction = fieldTeamAction,
                    fieldsErrors = fieldsErrors,
                    onSubmitClick = onSubmitClick,
                    isEditMode = isEditMode
                )
            }
        }
    )
}

/**
 * Composable interno que contém apenas os campos de input e o botão de submissão.
 */
@Composable
private fun FieldsCreateTeam(
    filedTeam: FormTeamDto,
    fieldTeamAction: FormTeamActions,
    fieldsErrors: TeamFormErros,
    onSubmitClick: () -> Unit,
    isEditMode: Boolean
) {
    ImagePickerString(
        imageSelectedUrl = filedTeam.image,
        onImageSelected = { fieldTeamAction.onImageChange(it) },
        modifier = Modifier.padding(bottom = 24.dp),
        contentDescription = stringResource(id = R.string.logo_team),
        contentDescriptionWithoutImage = stringResource(id = R.string.logo_team_add)
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_name_team),
        value = filedTeam.name,
        onValueChange = { fieldTeamAction.onNameChange(it) },
        isRequired = true,
        maxLenght = TeamConst.MAX_NAME_LENGTH,
        isError = fieldsErrors.nameError != null,
        errorMessage = fieldsErrors.nameError?.let {
            stringResource(
                id = it.messageId,
                *it.args.toTypedArray()
            )
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

    if (isEditMode) {
        EditFormButton(
            onClick = { onSubmitClick() },
            text = stringResource(id = R.string.button_edit_team),
            contentDescription = stringResource(id = R.string.button_description_edit_team)
        )
    } else {
        SubmitFormButton(
            onClick = { onSubmitClick() },
            imageButton = Icons.Default.GroupAdd,
            text = stringResource(id = R.string.button_create_team),
            contentDescription = stringResource(id = R.string.button_description_create_team)
        )
    }
}

@Preview(name = "Create Team - En", locale = "en", showBackground = true)
@Preview(name = "Criar Equipa - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewFormTeamCreate() {
    AMFootballTheme {
        ContentCreateTeam(
            filedsTeam = FormTeamDto(),
            fieldTeamAction = EditTeamMocks.mockActions,
            fieldsErrors = TeamFormErros(),
            uiState = UiState(isLoading = false),
            onSubmitClick = {},
            onRetry = {},
            isEditMode = false,
            modifier = Modifier.padding(16.dp)
        )
    }
}

@Preview(name = "Edit Team - En", locale = "en", showBackground = true)
@Preview(name = "Editar Equipa - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewFormTeamEdit() {
    AMFootballTheme {
        ContentCreateTeam(
            filedsTeam = EditTeamMocks.mockEditTeam,
            fieldTeamAction = EditTeamMocks.mockActions,
            fieldsErrors = TeamFormErros(),
            uiState = UiState(isLoading = false),
            onSubmitClick = {},
            onRetry = {},
            isEditMode = true,
            modifier = Modifier.padding(16.dp)
        )
    }
}
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
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.forms.FormTeamActions
import com.example.amfootball.data.dtos.support.PitchInfo
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.errors.formErrors.TeamFormErros
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.inputFields.ImagePicker
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.components.notification.showOfflineToast
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.team.TeamFormViewModel
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.PitchConst
import com.example.amfootball.utils.TeamConst

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
    val context = LocalContext.current
    val uiForm by viewModel.uiFormState.collectAsStateWithLifecycle()
    val uiErrors by viewModel.uiFormErrors.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

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
        uiState = uiState,
        isOnline = isOnline,
        onSubmitClick = {
            if (isOnline) {
                viewModel.onSubmit(navHostController = navHostController)
            } else {
                showOfflineToast(context = context)
            }
        },
        onRetry = {
            if(viewModel.isEditMode) {
                viewModel.loadDataTeam()
            }
        },
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
 * @param isOnline Estado da conectividade para exibir o banner.
 * @param onRetry Callback para tentar novamente em caso de erro global.
 * @param onSubmitClick Callback para submeter o formulário.
 */
@Composable
private fun ContentCreateTeam(
    filedsTeam: FormTeamDto,
    fieldTeamAction: FormTeamActions,
    fieldsErrors: TeamFormErros,
    uiState: UiState,
    isOnline: Boolean,
    onRetry: () -> Unit,
    onSubmitClick: () -> Unit,
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
                OfflineBanner(isVisible = !isOnline)

                FieldsCreateTeam(
                    filedTeam = filedsTeam,
                    fieldTeamAction = fieldTeamAction,
                    fieldsErrors = fieldsErrors,
                    onSubmitClick = onSubmitClick
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

//Mocks de Dados
private val mockEditTeam = FormTeamDto(
    name = "Vitória SC",
    description = "Os Conquistadores. A maior equipa do Minho.",
    pitch = PitchInfo(
        name = "Estádio D. Afonso Henriques",
        address = "Praça 26 de Maio, Guimarães"
    )
)

private val mockActions = FormTeamActions(
    {}, {}, {}, {}, {}
)

@Preview(name = "Create Team - En", locale = "en", showBackground = true)
@Preview(name = "Criar Equipa - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewFormTeamCreate() {
    AMFootballTheme {
        ContentCreateTeam(
            filedsTeam = FormTeamDto(),
            fieldTeamAction = mockActions,
            fieldsErrors = TeamFormErros(),
            uiState = UiState(isLoading = false), // Mock State
            isOnline = true, // Mock Online
            onSubmitClick = {},
            onRetry = {},
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
            filedsTeam = mockEditTeam,
            fieldTeamAction = mockActions,
            fieldsErrors = TeamFormErros(),
            uiState = UiState(isLoading = false),
            isOnline = true,
            onSubmitClick = {},
            onRetry = {},
            modifier = Modifier.padding(16.dp)
        )
    }
}
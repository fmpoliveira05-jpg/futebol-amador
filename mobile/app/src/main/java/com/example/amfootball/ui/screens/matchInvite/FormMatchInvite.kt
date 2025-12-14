package com.example.amfootball.ui.screens.matchInvite

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Send
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.core.utils.MatchConsts
import com.example.amfootball.data.events.UiState
import com.example.amfootball.data.remote.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.domains.enums.pages.MatchFormMode
import com.example.amfootball.domains.errors.formErrors.MatchInviteFormErros
import com.example.amfootball.ui.actions.forms.FormMatchInviteActions
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.SubmitCancelButton
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.DatePickerDockedFutureLimitedDate
import com.example.amfootball.ui.components.inputFields.FieldTimePicker
import com.example.amfootball.ui.components.inputFields.Switcher
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.previewsMocks.FormMatchInviteMock
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.matchInvite.FormMatchInviteViewModel
import java.text.SimpleDateFormat
import java.util.Locale
import java.util.TimeZone

/**
 * Ecrã principal (Stateful Screen) para gestão de convites de partidas.
 *
 * Este ecrã lida com quatro modos de operação definidos por [MatchFormMode]: **SEND**, **NEGOTIATE**, **POSTPONE**, e **CANCEL**.
 *
 * **Responsabilidades:**
 * 1. Conectar-se ao [FormMatchInviteViewModel] e observar os estados ([fields], [errors], [uiState]).
 * 2. Construir e definir as ações ([FormMatchInviteActions]) que ligam a UI à lógica do ViewModel.
 * 3. Delegar a renderização do formulário para o componente Stateless [ContentSendMatchInviteScreen].
 *
 * @param viewModel O ViewModel injetado via Hilt que gerencia a lógica de negócio.
 * @param navHostController Controlador de navegação para redirecionamentos após submissão.
 */
@Composable
fun FormMatchInviteScreen(
    viewModel: FormMatchInviteViewModel = hiltViewModel(),
    navHostController: NavHostController
) {
    val fields by viewModel.uiFormState.collectAsStateWithLifecycle()
    val errors by viewModel.uiFormErrors.collectAsStateWithLifecycle()

    val actions = FormMatchInviteActions(
        onGameDateChange = viewModel::onGameDateChange,
        onTimeGameChange = viewModel::onTimeGameChange,
        onLocalGameChange = viewModel::onLocalGameChange,
        onSubmitForm = viewModel::onSubmitForm,
        onCancelForm = viewModel::onCancelForm
    )

    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val mode = viewModel.mode

    ContentSendMatchInviteScreen(
        navHostController = navHostController,
        fields = fields,
        actions = actions,
        errors = errors,
        mode = mode,
        uiState = uiState,
        retry = viewModel::loadData,
        modifier = Modifier.padding(16.dp),
    )
}

/**
 * Componente de layout (Stateless Content) que estrutura o ecrã.
 *
 * Centraliza o conteúdo e utiliza o [LoadingPage] para gerir os estados de carregamento e erro,
 * garantindo que o formulário só é visível quando os dados estão prontos.
 *
 * @param navHostController Controlador de navegação.
 * @param fields Estado atual dos campos do formulário (DTO).
 * @param actions Interface contendo as callbacks para interação do utilizador.
 * @param errors Estado atual dos erros de validação.
 * @param mode O modo atual do formulário (Send, Negotiate, Postpone, Cancel).
 * @param uiState Estado da UI (loading, erro).
 * @param retry Callback para tentar recarregar os dados em caso de erro.
 * @param modifier Modificadores de layout.
 */
@Composable
private fun ContentSendMatchInviteScreen(
    navHostController: NavHostController,
    fields: MatchInviteDto,
    actions: FormMatchInviteActions,
    errors: MatchInviteFormErros,
    mode: MatchFormMode,
    uiState: UiState,
    retry: () -> Unit,
    modifier: Modifier = Modifier
) {
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = retry,
        content = {
            Column(
                modifier = modifier,
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.Center
            ) {
                FieldsSendMatchInvite(
                    fields = fields,
                    errors = errors,
                    actions = actions,
                    mode = mode,
                    navHostController = navHostController
                )
            }
        }
    )
}

/**
 * Componente que contém os campos de input e a lógica de apresentação condicional do formulário.
 *
 * Gere a visibilidade, a editabilidade e os botões com base no [mode] de operação:
 * - **SEND/NEGOTIATE/POSTPONE**: Permitem edição de data/hora.
 * - **CANCEL**: Bloqueia edição e exige um campo de "motivo de cancelamento".
 *
 * @param fields Dados do formulário.
 * @param errors Erros de validação.
 * @param actions Callbacks de ação.
 * @param mode Modo de operação do formulário.
 * @param navHostController Controlador para navegação no clique do botão.
 */
@Composable
private fun FieldsSendMatchInvite(
    fields: MatchInviteDto,
    errors: MatchInviteFormErros,
    actions: FormMatchInviteActions,
    mode: MatchFormMode,
    navHostController: NavHostController
) {
    var cancelReason by rememberSaveable { mutableStateOf("") }

    TextFieldOutline(
        label = stringResource(id = R.string.filter_opponent),
        value = fields.opponent?.name,
        isReadOnly = true,
    )

    DatePickerDockedFutureLimitedDate(
        value = fields.gameDateString ?: "",
        onDateSelected = actions.onGameDateChange,
        label = stringResource(id = R.string.game_date),
        contentDescription = stringResource(id = R.string.description_game_date),
        isError = errors.dateError != null,
        isSingleLine = false,
        enabled = mode != MatchFormMode.CANCEL,
        errorMessage = errors.dateError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray())
        }
    )

    FieldTimePicker(
        value = fields.gameTimeString ?: "",
        onValueChange = actions.onTimeGameChange,
        label = stringResource(id = R.string.game_hours),
        contentDescription = stringResource(id = R.string.description_game_date),
        isError = errors.dateError != null,
        enabled = mode != MatchFormMode.CANCEL,
        errorMessage = errors.timeError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray())
        },
    )

    Switcher(
        value = fields.isHomeGame,
        onCheckedChange = actions.onLocalGameChange,
        text = stringResource(id = R.string.playing_home),
        textChecked = stringResource(id = R.string.checked_playing_home),
        textUnChecked = stringResource(id = R.string.unchecked_playing_home),
        enabled = mode != MatchFormMode.CANCEL && mode != MatchFormMode.POSTPONE,
    )

    if (mode == MatchFormMode.CANCEL) {
        TextFieldOutline(
            label = stringResource(id = R.string.label_field_description_team),
            value = cancelReason,
            onValueChange = { newValue ->
                cancelReason = newValue
            },
            isSingleLine = false,
            isRequired = true,
            minLenght = MatchConsts.MIN_CANCEL_REASON_LENGTH,
            maxLenght = MatchConsts.MAX_CANCEL_REASON_LENGTH,
            isError = errors.cancelReasonError != null,
            errorMessage = errors.cancelReasonError?.let {
                stringResource(id = it.messageId, *it.args.toTypedArray())
            },
            textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_description_cancel))
        )

    }

    if (mode != MatchFormMode.CANCEL) {
        SubmitFormButton(
            onClick = { actions.onSubmitForm(navHostController) },
            imageButton = Icons.AutoMirrored.Filled.Send,
            text = stringResource(R.string.button_send_match_invite),
            contentDescription = stringResource(id = R.string.button_description_send_match_invite)
        )
    } else {
        SubmitCancelButton(
            text = stringResource(id = R.string.button_cancel_match),
            contentDescription = stringResource(id = R.string.button_cancel_match_description),
            onClick = {
                actions.onCancelForm(
                    navHostController,
                    cancelReason
                )
            },
            textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_button_cancel_match))
        )
    }
}

// =============================================================================
// PREVIEWS
// =============================================================================
/**
 * Preview do modo [MatchFormMode.SEND].
 * Apresenta o formulário vazio para iniciar um convite.
 */
@Preview(name = "1. Send (EN)", group = "Forms", locale = "en", showBackground = true)
@Preview(name = "1. Send (PT)", group = "Forms", locale = "pt", showBackground = true)
@Composable
fun PreviewMatchInviteSend() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = FormMatchInviteMock.emptyFields,
            actions = FormMatchInviteMock.dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.SEND,
            uiState = UiState(isLoading = false, errorMessage = null),
            retry = {}
        )
    }
}

/**
 * Preview do modo [MatchFormMode.NEGOCIATE].
 * Simula dados pré-preenchidos onde o utilizador está a responder a um convite (ex: jogo fora).
 */
@Preview(name = "2. Negotiate (EN)", group = "Forms", locale = "en", showBackground = true)
@Preview(name = "2. Negotiate (PT)", group = "Forms", locale = "pt", showBackground = true)
@Composable
fun PreviewMatchInviteNegotiate() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = FormMatchInviteMock.filledFields.copy(isHomeGame = false),
            actions = FormMatchInviteMock.dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.NEGOCIATE,
            uiState = UiState(isLoading = false, errorMessage = null),
            retry = {}
        )
    }
}

/**
 * Preview do modo [MatchFormMode.POSTPONE].
 * Permite mudar data e hora, mas bloqueia a alteração do local (Casa/Fora).
 */
@Preview(name = "3. Postpone (EN)", group = "Forms", locale = "en", showBackground = true)
@Preview(name = "3. Postpone (PT)", group = "Forms", locale = "pt", showBackground = true)
@Composable
fun PreviewMatchInvitePostpone() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = FormMatchInviteMock.filledFields,
            actions = FormMatchInviteMock.dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.POSTPONE,
            uiState = UiState(isLoading = false, errorMessage = null),
            retry = {}
        )
    }
}

/**
 * Preview do modo [MatchFormMode.CANCEL].
 * Bloqueia a edição de data/hora/local, exibe o campo de motivo e o botão de cancelamento (vermelho).
 */
@Preview(name = "4. Cancel (EN)", group = "Forms", locale = "en", showBackground = true)
@Preview(name = "4. Cancel (PT)", group = "Forms", locale = "pt", showBackground = true)
@Composable
fun PreviewMatchInviteCancel() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = FormMatchInviteMock.filledFields,
            actions = FormMatchInviteMock.dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.CANCEL,
            uiState = UiState(isLoading = false, errorMessage = null),
            retry = {}
        )
    }
}

/**
 * Preview do estado de Loading.
 * Útil para visualizar o spinner a rodar.
 */
@Preview(name = "5. Loading State", group = "States", showBackground = true)
@Composable
fun PreviewMatchInviteLoading() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = FormMatchInviteMock.emptyFields,
            actions = FormMatchInviteMock.dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.SEND,
            uiState = UiState(isLoading = true, errorMessage = null),
            retry = {}
        )
    }
}

/**
 * Preview do estado de Erro.
 * Simula uma falha de rede para mostrar a mensagem de erro e o botão de retry.
 */
@Preview(name = "6. Error State", group = "States", showBackground = true)
@Composable
fun PreviewMatchInviteError() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = FormMatchInviteMock.emptyFields,
            actions = FormMatchInviteMock.dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.SEND,
            uiState = UiState(isLoading = false, errorMessage = "Falha ao conectar ao servidor."),
            retry = {}
        )
    }
}
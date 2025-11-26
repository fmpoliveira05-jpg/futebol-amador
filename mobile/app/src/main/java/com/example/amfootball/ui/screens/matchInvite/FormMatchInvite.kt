package com.example.amfootball.ui.screens.matchInvite

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Send
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.example.amfootball.R
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.tooling.preview.Preview
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.actions.forms.FormMatchInviteActions
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.enums.Forms.MatchFormMode
import com.example.amfootball.data.errors.formErrors.MatchInviteFormErros
import com.example.amfootball.ui.components.buttons.SubmitCancelButton
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.DatePickerDockedFutureLimitedDate
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.components.inputFields.FieldTimePicker
import com.example.amfootball.ui.components.inputFields.Switcher
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.matchInvite.FormMatchInviteViewModel
import com.example.amfootball.utils.MatchConsts

/**
 * Ecrã principal (Stateful) para gestão de convites de partidas.
 *
 * Este ecrã lida com quatro modos de operação definidos por [MatchFormMode]:
 * 1. **SEND**: Criar e enviar um novo convite.
 * 2. **NEGOTIATE**: Contra-propor uma data/hora ou local.
 * 3. **POSTPONE**: Adiar um jogo confirmado.
 * 4. **CANCEL**: Cancelar um jogo confirmado (requer motivo).
 *
 * Ele conecta-se ao [FormMatchInviteViewModel] para observar o estado do formulário e despachar eventos.
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
    val errors by viewModel.uiErrorsForm.collectAsStateWithLifecycle()

    val actions = FormMatchInviteActions(
        onGameDateChange = viewModel::onGameDateChange,
        onTimeGameChange = viewModel::onTimeGameChange,
        onLocalGameChange = viewModel::onLocalGameChange,
        onSubmitForm = viewModel::onSubmitForm,
        onCancelForm = viewModel::onCancelForm
    )

    val mode = viewModel.mode

    ContentSendMatchInviteScreen(
        navHostController = navHostController,
        fields = fields,
        actions = actions,
        errors = errors,
        mode = mode,
        modifier = Modifier.padding(16.dp),
    )
}

/**
 * Componente de layout (Stateless) que estrutura o ecrã.
 *
 * Responsável por centralizar o conteúdo vertical e horizontalmente e aplicar o padding base.
 *
 * @param navHostController Controlador de navegação.
 * @param fields Estado atual dos campos do formulário (DTO).
 * @param actions Interface contendo as callbacks para interação do utilizador.
 * @param errors Estado atual dos erros de validação.
 * @param mode O modo atual do formulário (Send, Negotiate, Postpone, Cancel).
 * @param modifier Modificadores de layout.
 */
@Composable
private fun ContentSendMatchInviteScreen(
    navHostController: NavHostController,
    fields: MatchInviteDto,
    actions: FormMatchInviteActions,
    errors: MatchInviteFormErros,
    mode: MatchFormMode,
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
            mode = mode,
            navHostController = navHostController
        )
    }
}

/**
 * Componente que contém os campos de input e a lógica de apresentação do formulário.
 *
 * Gere a visibilidade e o estado (enabled/disabled) dos campos com base no [mode]:
 * - **Date/Time Pickers**: Editáveis em todos os modos, exceto [MatchFormMode.CANCEL].
 * - **Switcher (Home/Away)**: Editável apenas em SEND e NEGOTIATE. Bloqueado em POSTPONE e CANCEL.
 * - **Cancel Reason**: Visível apenas no modo [MatchFormMode.CANCEL].
 * - **Botões**: Alterna entre [SubmitFormButton] (padrão) e [SubmitCancelButton] (erro/vermelho) dependendo do modo.
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
        value = fields.nameOpponent,
        isReadOnly = true,
    )

    DatePickerDockedFutureLimitedDate(
        value = fields.gameDate ?: "",
        onDateSelected = actions.onGameDateChange,
        label = stringResource(id = R.string.game_date),
        contentDescription = stringResource(id = R.string.description_game_date),
        isError = errors.dateError != null,
        isSingleLine = false,
        enabled = mode != MatchFormMode.CANCEL,
        errorMessage = errors.dateError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray()) }
    )

    FieldTimePicker(
        value = fields.gameTime ?: "",
        onValueChange = actions.onTimeGameChange,
        label = stringResource(id = R.string.game_hours),
        contentDescription = stringResource(id = R.string.description_game_date) ,
        isError = errors.dateError != null,
        enabled = mode != MatchFormMode.CANCEL,
        errorMessage = errors.timeError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray()) },
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
            }
        )

    }

    if(mode != MatchFormMode.CANCEL) {
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
            onClick = { actions.onCancelForm(navHostController, cancelReason) }
        )
    }
}

// =============================================================================
// MOCKS PARA PREVIEWS
// =============================================================================
val emptyFields = MatchInviteDto(
    nameOpponent = "",
    gameDate = null,
    gameTime = null,
    isHomeGame = true
)

val filledFields = MatchInviteDto(
    nameOpponent = "Lisboa Navigators",
    gameDate = "2024-12-25",
    gameTime = "15:30",
    isHomeGame = true // Jogo em casa
)

val dummyActions = FormMatchInviteActions(
    onGameDateChange = {},
    onTimeGameChange = {},
    onLocalGameChange = {},
    onSubmitForm = { _ -> },
    onCancelForm = { _, _ -> }
)

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
fun PreviewMatchInvite_Send() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = emptyFields, // <--- Sem dados
            actions = dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.SEND
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
fun PreviewMatchInvite_Negotiate() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = filledFields.copy(isHomeGame = false), // Simulando proposta do adversário (Fora)
            actions = dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.NEGOCIATE // Verifica se no teu Enum é NEGOTIATE ou NEGOCIATE
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
fun PreviewMatchInvite_Postpone() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = filledFields, // Com dados antigos
            actions = dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.POSTPONE
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
fun PreviewMatchInvite_Cancel() {
    AMFootballTheme {
        ContentSendMatchInviteScreen(
            navHostController = rememberNavController(),
            fields = filledFields, // Com dados do jogo a cancelar
            actions = dummyActions,
            errors = MatchInviteFormErros(),
            mode = MatchFormMode.CANCEL
        )
    }
}
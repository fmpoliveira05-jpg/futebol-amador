package com.example.amfootball.ui.screens.match

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.SignalRUrls
import com.example.amfootball.data.remote.dtos.match.ResultMatchDto
import com.example.amfootball.domains.errors.formErrors.FinishMatchFormErrors
import com.example.amfootball.ui.actions.forms.FormFinishMatchActions
import com.example.amfootball.ui.components.buttons.SubmitFormButton
import com.example.amfootball.ui.components.inputFields.TextFieldOutline
import com.example.amfootball.ui.previewsMocks.FinishMatchMocks
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.match.FinishMatchViewModel

/**
 * Ecrã de finalização de partida (Reportar Resultado) - (Stateful Screen).
 *
 * Este ecrã permite ao utilizador (geralmente admin da equipa) introduzir o resultado final
 * de um jogo realizado (número de golos marcados e sofridos).
 *
 * **Responsabilidades:**
 * 1. Coleta o estado reativo do [FinishMatchViewModel] (dados do formulário e erros).
 * 2. Define as ações em [FormFinishMatchActions], encapsulando a lógica de navegação no callback de sucesso do `onSubmitForm`.
 * 3. Delega a renderização visual para [FormFinishMatch].
 *
 * @param navHostController Controlador de navegação para redirecionamento após submissão bem-sucedida.
 * @param viewModel ViewModel injetado (ou criado) que gere o estado do formulário e a submissão.
 */
@Composable
fun FinishMatchScreen(
    navHostController: NavHostController,
    viewModel: FinishMatchViewModel = hiltViewModel()
) {
    val result by viewModel.uiFormState.collectAsStateWithLifecycle()
    val formErrors by viewModel.uiFormErrors.collectAsStateWithLifecycle()
    val formActions = FormFinishMatchActions(
        onNumGoalsTeamChange = viewModel::onNumGoalsTeamChange,
        onNumGoalsOpponentChange = viewModel::onNumGoalsOponnetChange,
        onSubmitForm = {
            viewModel.onSubmitForm(
                onSucess = {
                    val matchId = result.idMatch
                    val opponentId = result.idOpponent
                    val myGoals = result.numGoalsTeam
                    val opponentGoals = result.numGoalsOpponent

                    navHostController.navigate(route = "${SignalRUrls.FINISH_MATCH_URL}/$matchId/$opponentId/$opponentGoals/$myGoals") {
                        launchSingleTop = true
                    }
                }
            )
        }
    )

    FormFinishMatch(
        result = result,
        formActions = formActions,
        formErrors = formErrors,
        modifier = Modifier
            .padding(16.dp)
    )
}

/**
 * Estrutura visual do formulário de resultado (Stateless Content).
 *
 * Componente de layout que centraliza os campos de input no ecrã e delega
 * a renderização específica para [TextFieldForm].
 *
 * @param result O estado atual dos dados do resultado (golos equipa vs adversário).
 * @param formActions As ações disponíveis para interagir com o formulário.
 * @param formErrors Os erros de validação atuais para cada campo.
 * @param modifier Modificador de layout.
 */
@Composable
private fun FormFinishMatch(
    result: ResultMatchDto?,
    formActions: FormFinishMatchActions,
    formErrors: FinishMatchFormErrors,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        TextFieldForm(
            result = result,
            formActions = formActions,
            formErrors = formErrors,
        )
    }
}

/**
 * Campos de texto e botão de submissão.
 *
 * Renderiza dois inputs numéricos (Golos da Equipa e Golos do Adversário) e valida a entrada
 * em tempo real, convertendo o texto para Inteiro.
 *
 * @param result Dados atuais do formulário.
 * @param formActions Callbacks para alteração de valores e submissão.
 * @param formErrors Erros de validação a exibir nos campos.
 */
@Composable
private fun TextFieldForm(
    result: ResultMatchDto?,
    formActions: FormFinishMatchActions,
    formErrors: FinishMatchFormErrors,
) {
    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_team),
        value = result?.numGoalsTeam?.toString() ?: "",
        minLenght = GeneralConst.MIN_GOALS,
        maxLenght = GeneralConst.MAX_GOALS,
        onValueChange = { formActions.onNumGoalsTeamChange(it.toIntOrNull() ?: 0) },
        isRequired = true,
        isError = formErrors.numGoalTeamError != null,
        errorMessage = formErrors.numGoalTeamError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray())
        },
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    TextFieldOutline(
        label = stringResource(id = R.string.label_field_num_Goals_opponent_team),
        value = result?.numGoalsOpponent?.toString() ?: "",
        minLenght = GeneralConst.MIN_GOALS,
        maxLenght = GeneralConst.MAX_GOALS,
        onValueChange = { formActions.onNumGoalsOpponentChange(it.toIntOrNull() ?: 0) },
        isRequired = true,
        isError = formErrors.numGoalOpponentError != null,
        errorMessage = formErrors.numGoalOpponentError?.let {
            stringResource(id = it.messageId, *it.args.toTypedArray())
        },
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )

    SubmitFormButton(
        onClick = { formActions.onSubmitForm() }
    )
}

@Preview(
    name = "FinishMatch EN",
    locale = "en",
    showBackground = true
)
@Preview(
    name = "Finalização de Partida - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Composable
fun PreviewFinishMatch() {
    AMFootballTheme {
        FormFinishMatch(
            result = FinishMatchMocks.mockResult,
            formActions = FinishMatchMocks.mockActions,
            formErrors = FinishMatchMocks.mockErrors,
            modifier = Modifier.padding(16.dp)
        )
    }
}
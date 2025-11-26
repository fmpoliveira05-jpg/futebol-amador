package com.example.amfootball.ui.components.lists

import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import com.example.amfootball.R
import com.example.amfootball.data.enums.MatchStatus
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMatch
import com.example.amfootball.ui.components.inputFields.DatePickerDocked
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.utils.UserConst

/**
 * Seletor de filtro para a localização do jogo (Casa, Fora ou Ambos).
 *
 * Utiliza o [LabelSelectBox] com opções [Boolean] (true/false) e a opção 'null' para 'Ambos'.
 * A lógica de [itemToString] converte os valores booleanos para as strings de recurso correspondentes.
 *
 * @param selectedValue O valor atualmente selecionado: true (Casa), false (Fora), ou null (Ambos).
 * @param onSelectItem Callback executado ao selecionar uma opção (recebe Boolean?).
 * @param modifier Modificador para estilizar o componente.
 */
@Composable
fun FilterIsHomeMatch(
    selectedValue: Boolean?,
    onSelectItem: (Boolean?) -> Unit,
    modifier: Modifier = Modifier
) {
    val locationOptions: List<Boolean?> = listOf(null, true, false)
    LabelSelectBox(
        label = stringResource(id = R.string.filter_local_game),
        list = locationOptions,
        selectedValue = selectedValue,
        onSelectItem = onSelectItem,
        itemToString = { isHomeValue ->
            when (isHomeValue) {
                true -> {
                    stringResource(id = R.string.filter_home)
                }
                false -> {
                    stringResource(id = R.string.filter_away)
                }
                null -> {
                    stringResource(id = R.string.filter_both)
                }
            }
        },
        modifier = modifier
    )
}

/**
 * Seletor de filtro para o tipo de jogo (Competitivo, Casual ou Indiferente).
 *
 * Utiliza o [LabelSelectBox] com opções baseadas no Enum [TypeMatch] e a opção 'null' para Indiferente.
 * A lógica de [itemToString] utiliza o [stringId] do Enum para obter o nome correto do tipo.
 *
 * @param selectedValue O valor atualmente selecionado: [TypeMatch.COMPETITIVE], [TypeMatch.CASUAL], ou null (Ambos).
 * @param onSelectItem Callback executado ao selecionar uma opção (recebe TypeMatch?).
 * @param modifier Modificador para estilizar o componente.
 */
@Composable
fun FilterIsCompetiveMatch(
    selectedValue: TypeMatch?,
    onSelectItem: (TypeMatch?) -> Unit,
    modifier: Modifier = Modifier
) {

    val competitiveOptions: List<TypeMatch?> = listOf(
        null,
        TypeMatch.COMPETITIVE,
        TypeMatch.CASUAL,
    )
    LabelSelectBox(
        label = stringResource(id = R.string.filter_type_match),
        list = competitiveOptions,
        selectedValue = selectedValue,
        onSelectItem = onSelectItem,
        itemToString = { isCompetitiveValue ->
            when (isCompetitiveValue) {
                TypeMatch.COMPETITIVE -> {
                    stringResource(id = TypeMatch.COMPETITIVE.stringId)
                }
                TypeMatch.CASUAL -> {
                    stringResource(id = TypeMatch.CASUAL.stringId)
                }
                null -> {
                    stringResource(id = R.string.filter_both)
                }
            }
        },
        modifier = modifier
    )
}

/**
 * Seletor de filtro para o estado da partida (Finalizado vs Agendado).
 *
 * Utiliza o [LabelSelectBox] com opções [Boolean] (true/false) e a opção 'null' para Indiferente.
 *
 * @param selectedValue O valor selecionado: true (Finalizado), false (Agendado), null (Indiferente).
 * @param onSelectItem Callback executado ao selecionar uma opção (recebe Boolean?).
 * @param modifier Modificador para estilizar o componente.
 */
@Composable
fun FilterIsFinishMatch(
    selectedValue: Boolean?,
    onSelectItem: (Boolean?) -> Unit,
    modifier: Modifier = Modifier
) {
    // A lista tem de ser de Boolean?, pois é o que o teu filtro espera
    val finishMatchOptions: List<Boolean?> = listOf(null, true, false)

    LabelSelectBox(
        label = stringResource(id = R.string.filter_finish_match),
        list = finishMatchOptions,
        selectedValue = selectedValue,
        onSelectItem = onSelectItem,
        itemToString = { isFinished ->
            when (isFinished) {
                true -> stringResource(id = R.string.match_status_done) // "Finalizado" (ou usa R.string.filter_yes)
                false -> stringResource(id = R.string.match_status_scheduled) // "Agendado" (ou usa R.string.filter_no)
                null -> stringResource(id = R.string.filter_indifferente)
            }
        },
        modifier = modifier
    )
}
/**
 * Campo de input para filtrar equipas pelo nome.
 *
 * É um wrapper que configura o [LabelTextField] com o rótulo e o limite máximo de nome de equipa.
 *
 * @param nameTeam O valor atual do filtro de nome de equipa.
 * @param onNameTeamChange Callback para atualizar o valor do filtro.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterNameTeamTextField(
    nameTeam: String?,
    onNameTeamChange: (String) -> Unit,
    isError: Boolean,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    LabelTextField(
        label = stringResource(id = R.string.filter_name_team),
        value = nameTeam,
        onValueChange = onNameTeamChange,
        maxLenght = TeamConst.MAX_NAME_LENGTH,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Campo de input para filtrar jogadores pelo nome.
 *
 * Configura o [LabelTextField] com o rótulo e o limite máximo de nome de utilizador.
 *
 * @param playerName O valor atual do filtro de nome de jogador.
 * @param onPlayerNameChange Callback para atualizar o valor do filtro.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterNamePlayerTextField(
    playerName: String?,
    onPlayerNameChange: (String) -> Unit,
    isError: Boolean = false,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    LabelTextField(
        label = stringResource(id = R.string.filter_name),
        value = playerName,
        maxLenght = UserConst.MAX_NAME_LENGTH,
        onValueChange = onPlayerNameChange,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Campo de input para filtrar por cidade.
 *
 * Configura o [LabelTextField] com o rótulo e o limite máximo de comprimento para cidades.
 *
 * @param city O valor atual do filtro de cidade.
 * @param onCityChange Callback para atualizar o valor do filtro.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterCityTextField(
    city: String?,
    onCityChange: (String) -> Unit,
    isError: Boolean = false,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    LabelTextField(
        label = stringResource(id = R.string.filter_city),
        value = city,
        maxLenght = GeneralConst.MAX_CITY_LENGTH,
        onValueChange = onCityChange,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Campo de input para definir a idade mínima no filtro.
 *
 * Configura o [LabelTextField] com [KeyboardType.Number] e validação de idade entre [UserConst.MIN_AGE] e [UserConst.MAX_AGE].
 *
 * @param minAge O valor atual do filtro de idade mínima (em String, a ser validado).
 * @param onMinAgeChange Callback para atualizar o valor do filtro.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterMinAgeTextField(
    minAge: String?,
    onMinAgeChange: (String) -> Unit,
    isError: Boolean = false,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    LabelTextField(
        label = stringResource(id = R.string.filter_min_age),
        value = minAge,
        minLenght = UserConst.MIN_AGE,
        maxLenght = UserConst.MAX_AGE,
        onValueChange = onMinAgeChange,
        isError = isError,
        errorMessage = errorMessage,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
        modifier = modifier
    )
}

/**
 * Campo de input para definir a idade máxima no filtro.
 *
 * Configura o [LabelTextField] com [KeyboardType.Number] e validação de idade entre [UserConst.MIN_AGE] e [UserConst.MAX_AGE].
 *
 * @param maxAge O valor atual do filtro de idade máxima (em String, a ser validado).
 * @param onMaxAgeChange Callback para atualizar o valor do filtro.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterMaxAgeTextField(
    maxAge: String?,
    onMaxAgeChange: (String) -> Unit,
    isError: Boolean = false,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    LabelTextField(
        label = stringResource(id = R.string.filter_max_age),
        value = maxAge,
        minLenght = UserConst.MIN_AGE,
        maxLenght = UserConst.MAX_AGE,
        onValueChange = onMaxAgeChange,
        isError = isError,
        errorMessage = errorMessage,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
        modifier = modifier
    )
}

/**
 * Campo de seleção (Dropdown) para filtrar por posição de jogador.
 *
 * Utiliza [LabelSelectBox] com uma lógica de conversão customizada para exibir
 * o nome da posição ou "Todos" (se o valor for null).
 *
 * @param listPosition A lista de opções de posição disponíveis (incluindo null para "Todas").
 * @param selectPosition A posição atualmente selecionada.
 * @param onSelectPosition Callback para atualizar a posição selecionada.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterListPosition(
    listPosition: List<Position?>,
    selectPosition: Position?,
    onSelectPosition: (Position?) -> Unit,
    modifier: Modifier = Modifier
) {
    LabelSelectBox(
        label = stringResource(id = R.string.filter_position),
        list = listPosition,
        selectedValue = selectPosition,
        onSelectItem = onSelectPosition,
        itemToString = { position ->
            when (position) {
                null -> stringResource(id = R.string.filter_selectbox_all)
                else -> stringResource(id = position.stringId)
            }
        },
        modifier = modifier
    )
}

/**
 * Seletor de data para definir a data mínima (de criação/registo) no filtro.
 *
 * Utiliza o [DatePickerDocked] base.
 *
 * @param value A string atual da data mínima selecionada.
 * @param onDateSelected Callback com o timestamp (Long) da nova data mínima.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterMinDatePicker(
    value: String,
    onDateSelected: (newMinDate: Long) -> Unit,
    isError: Boolean,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    DatePickerDocked(
        label = stringResource(id = R.string.filter_min_date),
        contentDescription = stringResource(id = R.string.description_filter_min_date),
        value = value,
        onDateSelected = onDateSelected,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Seletor de data para definir a data máxima (de criação/registo) no filtro.
 *
 * Utiliza o [DatePickerDocked] base.
 *
 * @param value A string atual da data máxima selecionada.
 * @param onDateSelected Callback com o timestamp (Long) da nova data máxima.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterMaxDatePicker(
    value: String,
    onDateSelected: (newMinDate: Long) -> Unit,
    isError: Boolean,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    DatePickerDocked(
        label = stringResource(id = R.string.filter_max_date),
        contentDescription = stringResource(id = R.string.description_filter_min_date),
        value = value,
        onDateSelected = onDateSelected,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Seletor de data para definir a data mínima de um jogo.
 *
 * Utiliza o [DatePickerDocked] base.
 *
 * @param minDateGame A string atual da data mínima do jogo.
 * @param onDateSelected Callback com o timestamp (Long) da nova data mínima.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterMinDateGamePicker(
    minDateGame: String,
    onDateSelected: (newMinDate: Long) -> Unit,
    isError: Boolean,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    DatePickerDocked(
        label = stringResource(id = R.string.filter_min_date_game),
        contentDescription = stringResource(id = R.string.description_filter_min_date),
        value = minDateGame,
        onDateSelected = onDateSelected,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Seletor de data para definir a data máxima de um jogo.
 *
 * Utiliza o [DatePickerDocked] base.
 *
 * @param maxDateGame A string atual da data máxima do jogo.
 * @param onDateSelected Callback com o timestamp (Long) da nova data máxima.
 * @param isError Indica se o campo está em estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida.
 * @param modifier Modificador para estilizar o campo.
 */
@Composable
fun FilterMaxDateGamePicker(
    maxDateGame: String,
    onDateSelected: (newMinDate: Long) -> Unit,
    isError: Boolean,
    errorMessage: String?,
    modifier: Modifier = Modifier
) {
    DatePickerDocked(
        label = stringResource(id = R.string.filter_max_date_game),
        contentDescription = stringResource(id = R.string.description_filter_max_date),
        value = maxDateGame,
        onDateSelected = onDateSelected,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}
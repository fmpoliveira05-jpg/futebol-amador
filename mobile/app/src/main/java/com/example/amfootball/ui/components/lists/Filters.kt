package com.example.amfootball.ui.components.lists

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R
import com.example.amfootball.data.enums.TypeMatch
import com.example.amfootball.ui.components.inputFields.DatePickerDocked
import com.example.amfootball.ui.components.inputFields.LabelSelectBox

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

@Composable
fun FilterIsFinishMatch(
    selectedValue: Boolean?,
    onSelectItem: (Boolean?) -> Unit,
    modifier: Modifier = Modifier
) {
    val finishMatchOptions: List<Boolean?> = listOf(null, true, false)
    LabelSelectBox(
        label = stringResource(id = R.string.filter_finish_match),
        list = finishMatchOptions,
        selectedValue = selectedValue,
        onSelectItem = onSelectItem,
        itemToString = { isFinishMatchValue ->
            when (isFinishMatchValue) {
                true -> {
                    stringResource(id = R.string.filter_yes)
                }
                false -> {
                    stringResource(id = R.string.filter_no)
                }
                null -> {
                    stringResource(id = R.string.filter_indifferente)
                }
            }
        },
        modifier = modifier
    )
}


@Composable
fun FilterMinDateGame(
    value: String,
    onDateSelected: (Long) -> Unit,
    modifier: Modifier = Modifier
) {
    DatePickerDocked(
        label = stringResource(id = R.string.filter_min_date_game),
        contentDescription = stringResource(id = R.string.description_filter_min_date),
        value = value,
        onDateSelected = onDateSelected,
        modifier = modifier
    )
}

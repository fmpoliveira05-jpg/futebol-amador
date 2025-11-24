package com.example.amfootball.ui.screens.team

import android.net.Uri
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.MoreVert
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterCalendarActions
import com.example.amfootball.data.actions.itemsList.ItemsCalendarActions
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.dtos.match.CalendarInfoDto
import com.example.amfootball.data.dtos.support.TeamStatisticsDto
import com.example.amfootball.data.enums.MatchResult
import com.example.amfootball.data.errors.filtersError.FilterCalendarError
import com.example.amfootball.ui.components.MatchActionsMenu
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterIsCompetiveMatch
import com.example.amfootball.ui.components.lists.FilterIsFinishMatch
import com.example.amfootball.ui.components.lists.FilterIsHomeMatch
import com.example.amfootball.ui.components.lists.FilterMaxDateGamePicker
import com.example.amfootball.ui.components.lists.FilterMinDateGamePicker
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.viewModel.team.CalendarTeamViewModel
import com.example.amfootball.utils.Patterns
import com.example.amfootball.utils.TeamConst
import java.time.format.DateTimeFormatter

@Composable
fun CalendarScreen(
    navHostController: NavHostController,
    viewModel: CalendarTeamViewModel = viewModel()
) {
    val filters by viewModel.filter.observeAsState(initial = FilterCalendar())
    val list by viewModel.list.observeAsState(initial = emptyList())
    val filterError by viewModel.uiErrors.observeAsState(initial = FilterCalendarError())
    val filterActons = FilterCalendarActions(
        onNameChange = viewModel::onNameChange,
        onMinDateGameChange = viewModel::onMinDateGameChange,
        onMaxDateGameChange = viewModel::onMaxDateGameChange,
        onGameLocalChange = viewModel::onGameLocalChange,
        onTypeMatchChange = viewModel::onTypeMatchChange,
        onIsFinishedChange = viewModel::onIsFinishedChange,
        onButtonFilterActions = ButtonFilterActions(
            onFilterApply = viewModel::onApplyFilter,
            onFilterClean = viewModel::onClearFilter
        )
    )

    val itensListAction = ItemsCalendarActions(
        onCancelMatch = viewModel::onCancelMatch,
        onPostPoneMatch = viewModel::onPostPoneMatch,
        onStartMatch = viewModel::onStartMatch,
        onFinishMatch = viewModel::onFinishMatch
    )

    var isExpanded by remember { mutableStateOf(false) }

    ListSurface(
        list = list,
        filterSection = {
            FilterSection(
                isExpanded = isExpanded,
                onToggleExpand = { isExpanded = !isExpanded },
                content = { paddingModifier ->
                    FiltersCalendarContent(
                        filters = filters,
                        filterActions = filterActons,
                        filterError = filterError,
                        modifier = paddingModifier,
                    )
                }
            )
        },
        listItems = { match ->
            ListMatchCalendarItem(
                match = match,
                itensListAction = itensListAction,
                navHostController = navHostController,
                modifier = Modifier.fillMaxWidth()
            )
        },
    )
}

@Composable
private fun FiltersCalendarContent(
    filters: FilterCalendar,
    filterActions: FilterCalendarActions,
    filterError: FilterCalendarError,
    modifier: Modifier = Modifier
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_name_team),
                    value = filters.opponentName,
                    maxLenght = TeamConst.MAX_NAME_LENGTH,
                    onValueChange = { filterActions.onNameChange(it) },
                    modifier = Modifier.weight(1f),
                    isError = filterError.opponentNameError != null,
                    errorMessage = filterError.opponentNameError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    }
                )

                FilterIsHomeMatch(
                    selectedValue = filters.gameLocale,
                    onSelectItem = { filterActions.onGameLocalChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterMinDateGamePicker(
                    minDateGame = filters.minGameDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDateGameChange(it) },
                    isError = filterError.minGameDateError != null,
                    errorMessage = filterError.minGameDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMaxDateGamePicker(
                    maxDateGame = filters.maxGameDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDateGameChange(it)},
                    isError = filterError.minGameDateError != null,
                    errorMessage = filterError.maxGameDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterIsCompetiveMatch(
                    selectedValue = filters.typeMatch,
                    onSelectItem = { filterActions.onTypeMatchChange(it) },
                    modifier = Modifier.weight(1f)
                )

                FilterIsFinishMatch(
                    selectedValue = filters.isFinish,
                    onSelectItem = { filterActions.onIsFinishedChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        LineClearFilterButtons(
            buttonsActions = filterActions.onButtonFilterActions,
            modifier = Modifier.weight(1f)
        )
    }
}

@Composable
private fun ListMatchCalendarItem(
    match: CalendarInfoDto,
    itensListAction: ItemsCalendarActions,
    navHostController: NavHostController,
    modifier: Modifier = Modifier
) {
    ElevatedCard(modifier = modifier) {
        Column(
            modifier = Modifier.padding(
                vertical = 12.dp,
                horizontal = 16.dp)
        ) {
            TitleMatchCalendar(
                match = match,
                itensListAction = itensListAction,
                navHostController = navHostController
            )

            Spacer(Modifier.height(8.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                ColumnTeams(
                    match = match,
                    modifier = Modifier.weight(2f)
                )

                ColumnDataMatch(
                    match = match,
                    modifier = Modifier.weight(1f)
                )
            }
        }
    }
}

@Composable
private fun TitleMatchCalendar(
    match: CalendarInfoDto,
    itensListAction: ItemsCalendarActions,
    navHostController: NavHostController
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = stringResource(match.typeMatch.stringId),
            style = MaterialTheme.typography.labelMedium,
            color = MaterialTheme.colorScheme.primary,
            modifier = Modifier.weight(1f)
        )

        OptionsMatch(
            match = match,
            itensListAction = itensListAction,
            navHostController = navHostController
        )
    }
}

@Composable
private fun OptionsMatch(
    match: CalendarInfoDto,
    itensListAction: ItemsCalendarActions,
    navHostController: NavHostController
) {
    var isMenuExpanded by remember { mutableStateOf(false) }
    Box {
        IconButton(onClick = { isMenuExpanded = !isMenuExpanded }) {
            Icon(
                imageVector = Icons.Default.MoreVert,
                contentDescription = "Opções"
            )
        }

        MatchActionsMenu(
            expanded = isMenuExpanded,
            onDismissRequest = { isMenuExpanded = false },
            onStartMatch = {
                itensListAction.onStartMatch(match.idMatch)
            },
            onFinishMatch = {
                itensListAction.onFinishMatch(
                    match.idMatch,
                    navHostController
                )
            },
            onPostPoneMatch = {
                itensListAction.onPostPoneMatch(
                    match.idMatch,
                    navHostController
                )
            },
            onCancelMatch = {
                itensListAction.onCancelMatch(
                    match.idMatch,
                    navHostController
                )
            }
        )
    }
}

@Composable
private fun ColumnTeams(
    match: CalendarInfoDto,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.spacedBy(4.dp)
    ) {
        TeamInfoRow(
            team = match.team,
            image = match.team.infoTeam.image
        )

        TeamInfoRow(
            team = match.opponent,
            image = match.opponent.infoTeam.image
        )
    }
}

@Composable
private fun ColumnDataMatch(
    match: CalendarInfoDto,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.End
    ) {
        Text(
            //TODO: Mter um sWITCH PARA DE ACORDO COM O RESULTADO DIA, ETC. Definir o texto
            text = "Fim",
            style = MaterialTheme.typography.labelMedium
        )

        Text(
            text = match.matchDate.toString(),
            style = MaterialTheme.typography.bodyMedium)
    }
}
@Composable
private fun TeamInfoRow(
    team: TeamStatisticsDto,
    image: Uri? = Uri.EMPTY
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp)
    ) {
        ImageList(
            image = image,
        )

        Text(
            text = team.infoTeam.name,
            style = MaterialTheme.typography.bodyLarge,
            maxLines = 1,
            overflow = TextOverflow.Ellipsis,
            modifier = Modifier.weight(1f)
        )

        if (team.matchResult != MatchResult.UNDEFINED) {
            Text(
                text = team.numGoals.toString(),
                style = MaterialTheme.typography.titleMedium
            )
        }
    }
}

@Preview(
    name = "Calendario da Equipa - PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "Team Calendar - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun CalendarScreenPreview() {
    CalendarScreen(navHostController = rememberNavController())
}
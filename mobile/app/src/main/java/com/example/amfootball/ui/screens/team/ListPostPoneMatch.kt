package com.example.amfootball.ui.screens.team

import android.net.Uri
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPostPoneMatchActions
import com.example.amfootball.data.dtos.filters.FilterPostPoneMatch
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.DatePickerDocked
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.DateRow
import com.example.amfootball.ui.components.lists.FilterIsHomeMatch
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PitchAddressRow
import com.example.amfootball.ui.viewModel.team.ListPostPoneMatchViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

@Composable
fun ListPostPoneMatchScreen(
    navHostController: NavHostController,
    viewModel: ListPostPoneMatchViewModel = viewModel()
) {
    val filters by viewModel.filter.observeAsState(initial = FilterPostPoneMatch())
    val list by viewModel.list.observeAsState(initial = emptyList())

    val filterActions = FilterListPostPoneMatchActions(
        onOpponentNameChange = viewModel::onOpponentNameChange,
        onIsHomeChange = viewModel::onIsHomeChange,
        onMinDateGameChange = viewModel::onMinDateGameChange,
        onMaxDateGameChange = viewModel::onMaxDateChange,
        onMinDatePostPoneChange = viewModel::onMinDatePostPoneDateChange,
        onMaxDatePostPoneChange = viewModel::onMaxDatePostPoneDateChange,
        buttonFilterActions = ButtonFilterActions(
            onFilterApply = viewModel::onApplyFilter,
            onFilterClean = viewModel::onCleanFilter
        )
    )

    var filtersExpanded by remember { mutableStateOf(false) }
    ListSurface(
        list = list,
        filterSection = {
            FilterSection(
                isExpanded = filtersExpanded,
                onToggleExpand = { filtersExpanded = !filtersExpanded },
                content = { paddingModifier ->
                    FilterListPostPoneMatchContent(
                        filters = filters,
                        filterActions = filterActions,
                        modifier = paddingModifier
                    )
                }
            )
        },
        listItems = { postPoneMatch ->
            ItemListPosPoneMatch(
                postPoneMatch = postPoneMatch,
                acceptPostPoneMatch = { viewModel.acceptPostPoneMatch(
                    idPostPoneMatch = postPoneMatch.id,
                )},
                rejectPostPoneMatch = { viewModel.rejectPostPoneMatch(
                    idPostPoneMatch = postPoneMatch.id,
                )},
                showMore = { viewModel.showMoreInfo(
                    idPostPoneMatch = postPoneMatch.id,
                    navHostController = navHostController)
                }
            )
        }
    )
}

@Composable
private fun FilterListPostPoneMatchContent(
    filters: FilterPostPoneMatch,
    filterActions: FilterListPostPoneMatchActions,
    modifier: Modifier
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_name),
                    value = filters.nameOpponent,
                    onValueChange = { filterActions.onOpponentNameChange(it) },
                    modifier = Modifier.weight(1f)
                )

                FilterIsHomeMatch(
                    selectedValue = filters.isHome,
                    onSelectItem = { filterActions.onIsHomeChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                DatePickerDocked(
                    label = stringResource(id = R.string.filter_min_date_game),
                    contentDescription = stringResource(id = R.string.description_filter_min_date),
                    value = filters.minDataGame?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDateGameChange(it) },
                    modifier = Modifier.weight(1f)
                )

                DatePickerDocked(
                    label = stringResource(id = R.string.filter_max_date_game),
                    contentDescription = stringResource(id = R.string.description_filter_max_date),
                    value = filters.maxDateGame?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDateGameChange(it)},
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                DatePickerDocked(
                    label = stringResource(id = R.string.filter_min_date_post_pone),
                    contentDescription = stringResource(id = R.string.description_filter_min_date),
                    value = filters.minDatePostPone?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDatePostPoneChange(it) },
                    modifier = Modifier.weight(1f)
                )

                DatePickerDocked(
                    label = stringResource(id = R.string.filter_max_date_post_pone),
                    contentDescription = stringResource(id = R.string.description_filter_max_date),
                    value = filters.maxDatePostPone?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDatePostPoneChange(it)},
                    modifier = Modifier.weight(1f)
                )
            }
        )

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            buttonsActions = filterActions.buttonFilterActions,
            modifier = Modifier.weight(1f)
        )
    }
}

@Composable
private fun ItemListPosPoneMatch(
    postPoneMatch: PostPoneMatchDto,
    acceptPostPoneMatch: () -> Unit,
    rejectPostPoneMatch: () -> Unit,
    showMore: () -> Unit
) {
    GenericListItem(
        item = postPoneMatch,
        title = { it.opponent.name },
        overline = {},
        supporting = {
            Column {
                DateRow(date = "Game: ${it.gameDate.format(DateTimeFormatter.ofPattern(Patterns.DATE_TIME))}")
                DateRow(date = "Postpone: ${it.postPoneDate.format(DateTimeFormatter.ofPattern(Patterns.DATE_TIME))}")
                PitchAddressRow(ptichAdrress = it.pitchMatch)
            }
        },
        leading = {
            ImageList(
                image = postPoneMatch.opponent.image ?: Uri.EMPTY
            )
        },
        trailing = {
            Row {
                AcceptButton(accept = acceptPostPoneMatch)
                RejectButton(reject = rejectPostPoneMatch)
                ShowMoreInfoButton(
                    showMoreDetails = showMore,
                    contentDescription = stringResource(id = R.string.list_teams_view_team)
                )
            }
        }
    )
}

@Preview(
    name = "Lista de adiamentos - PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "List PostPone Match- EN",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewListPostPoneMatch() {
    ListPostPoneMatchScreen(
        navHostController = rememberNavController()
    )
}

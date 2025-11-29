package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPostPoneMatchActions
import com.example.amfootball.data.actions.itemsList.ItemsListPostPoneMatchActions
import com.example.amfootball.data.filters.FilterPostPoneMatch
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.errors.filtersError.ListPostPoneMatchFiltersError
import com.example.amfootball.data.mocks.Lists.ListPostPoneMatchMocks
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.DatePickerDocked
import com.example.amfootball.ui.components.lists.DateRow
import com.example.amfootball.ui.components.lists.FilterIsHomeMatch
import com.example.amfootball.ui.components.lists.FilterMaxDateGamePicker
import com.example.amfootball.ui.components.lists.FilterMinDateGamePicker
import com.example.amfootball.ui.components.lists.FilterNameTeamTextField
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PitchAddressRow
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.viewModel.team.ListPostPoneMatchViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

@Composable
fun ListPostPoneMatchScreen(
    navHostController: NavHostController,
    viewModel: ListPostPoneMatchViewModel = hiltViewModel()
) {
    val filters by viewModel.filter.collectAsStateWithLifecycle()
    val filtersError by viewModel.filterErros.collectAsStateWithLifecycle()
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

    val list by viewModel.uiList.collectAsStateWithLifecycle()
    val itemsListActions = ItemsListPostPoneMatchActions(
        acceptPostPoneMatch = viewModel::acceptPostPoneMatch,
        rejectPostPoneMatch = viewModel::rejectPostPoneMatch,
        showMoreInfo = viewModel::showMoreInfo
    )

    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    ListPostPoneMatchContent(
        uiState = uiState,
        isOnline = isOnline,
        filters = filters,
        filtersError = filtersError,
        filterActions = filterActions,
        list = list,
        itemsListActions = itemsListActions,
        navHostController = navHostController
    )
}

@Composable
private fun ListPostPoneMatchContent(
    uiState: UiState,
    isOnline: Boolean,
    filters: FilterPostPoneMatch,
    filtersError: ListPostPoneMatchFiltersError,
    filterActions: FilterListPostPoneMatchActions,
    list: List<PostPoneMatchDto>,
    itemsListActions: ItemsListPostPoneMatchActions,
    navHostController: NavHostController
) {
    var filtersExpanded by remember { mutableStateOf(false) }

    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = {},
        content = {
            OfflineBanner(isVisible = !isOnline)

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
                                filtersError = filtersError,
                                modifier = paddingModifier
                            )
                        }
                    )
                },
                listItems = { postPoneMatch ->
                    ItemListPosPoneMatch(
                        postPoneMatch = postPoneMatch,
                        itemsListActions = itemsListActions,
                        navHostController = navHostController
                    )
                },
                messageEmptyList = stringResource(id = R.string.list_post_pone_match_empty)
            )
        }
    )
}

@Composable
private fun FilterListPostPoneMatchContent(
    filters: FilterPostPoneMatch,
    filtersError: ListPostPoneMatchFiltersError,
    filterActions: FilterListPostPoneMatchActions,
    modifier: Modifier
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                FilterNameTeamTextField(
                    nameTeam = filters.nameOpponent,
                    onNameTeamChange = { filterActions.onOpponentNameChange(it) },
                    isError = filtersError.nameOpponentError != null,
                    errorMessage = filtersError.nameOpponentError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
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
                FilterMinDateGamePicker(
                    minDateGame = filters.minDataGame?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDateGameChange(it) },
                    isError = filtersError.minDateGameError != null,
                    errorMessage = filtersError.minDateGameError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMaxDateGamePicker(
                    maxDateGame = filters.maxDateGame?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDateGameChange(it)},
                    isError = filtersError.maxDateGameError != null,
                    errorMessage = filtersError.maxDateGameError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
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
                    isError = filtersError.minDatePostPoneError != null,
                    errorMessage = filtersError.minDatePostPoneError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                DatePickerDocked(
                    label = stringResource(id = R.string.filter_max_date_post_pone),
                    contentDescription = stringResource(id = R.string.description_filter_max_date),
                    value = filters.maxDatePostPone?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDatePostPoneChange(it)},
                    errorMessage = filtersError.maxDatePostPoneError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
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
    itemsListActions: ItemsListPostPoneMatchActions,
    navHostController: NavHostController
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
            StringImageList(
                image = postPoneMatch.opponent.image
            )
        },
        trailing = {
            Row {
                AcceptButton(accept = { itemsListActions.acceptPostPoneMatch(postPoneMatch.id)})
                RejectButton(reject = { itemsListActions.rejectPostPoneMatch(postPoneMatch.id)})
                ShowMoreInfoButton(
                    showMoreDetails = { itemsListActions.showMoreInfo(postPoneMatch.opponent.id, navHostController)},
                    contentDescription = stringResource(id = R.string.list_teams_view_team)
                )
            }
        }
    )
}

@Preview(name = "1. Lista Normal - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "1. List Normal - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListPostPoneMatch_Normal() {
    ListPostPoneMatchContent(
        uiState = UiState(isLoading = false),
        isOnline = true,
        filters = FilterPostPoneMatch(),
        filtersError = ListPostPoneMatchFiltersError(),
        filterActions = ListPostPoneMatchMocks.mockFilterActions,
        list = ListPostPoneMatchMocks.mockPostPoneMatches,
        itemsListActions = ListPostPoneMatchMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "2. Lista Vazia - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "2. List Empty - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListPostPoneMatch_Empty() {
    ListPostPoneMatchContent(
        uiState = UiState(isLoading = false),
        isOnline = true,
        filters = FilterPostPoneMatch(),
        filtersError = ListPostPoneMatchFiltersError(),
        filterActions = ListPostPoneMatchMocks.mockFilterActions,
        list = emptyList(),
        itemsListActions = ListPostPoneMatchMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "3. Loading - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "3. Loading - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListPostPoneMatch_Loading() {
    ListPostPoneMatchContent(
        uiState = UiState(isLoading = true),
        isOnline = true,
        filters = FilterPostPoneMatch(),
        filtersError = ListPostPoneMatchFiltersError(),
        filterActions = ListPostPoneMatchMocks.mockFilterActions,
        list = emptyList(),
        itemsListActions = ListPostPoneMatchMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "4. Erro - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "4. Error - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListPostPoneMatch_Error() {
    ListPostPoneMatchContent(
        uiState = UiState(isLoading = false, errorMessage = "Falha ao carregar adiamentos."),
        isOnline = true,
        filters = FilterPostPoneMatch(),
        filtersError = ListPostPoneMatchFiltersError(),
        filterActions = ListPostPoneMatchMocks.mockFilterActions,
        list = emptyList(),
        itemsListActions = ListPostPoneMatchMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "5. Offline Banner - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "5. Offline Banner - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListPostPoneMatch_Offline() {
    ListPostPoneMatchContent(
        uiState = UiState(isLoading = false),
        isOnline = false,
        filters = FilterPostPoneMatch(),
        filtersError = ListPostPoneMatchFiltersError(),
        filterActions = ListPostPoneMatchMocks.mockFilterActions,
        list = ListPostPoneMatchMocks.mockPostPoneMatches,
        itemsListActions = ListPostPoneMatchMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

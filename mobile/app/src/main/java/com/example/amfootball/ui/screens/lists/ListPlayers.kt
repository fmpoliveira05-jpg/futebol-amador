package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPlayersActions
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.errors.filtersError.FilterPlayersErrors
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.ListSendMemberShipRequestButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.AddressRow
import com.example.amfootball.ui.components.lists.AgeRow
import com.example.amfootball.ui.components.lists.FilterCityTextField
import com.example.amfootball.ui.components.lists.FilterListPosition
import com.example.amfootball.ui.components.lists.FilterMaxAgeTextField
import com.example.amfootball.ui.components.lists.FilterMinAgeTextField
import com.example.amfootball.ui.components.lists.FilterNamePlayerTextField
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PositionRow
import com.example.amfootball.ui.components.lists.SizeRow
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.lists.ListPlayerViewModel
import com.example.amfootball.utils.PlayerConst

@Composable
fun ListPlayersScreen(
    navHostController: NavHostController,
    viewModel: ListPlayerViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val filters by viewModel.uiFilters.collectAsStateWithLifecycle()
    val filtersError by viewModel.filterError.collectAsStateWithLifecycle()
    val list by viewModel.uiList.collectAsStateWithLifecycle()
    val listPosition by viewModel.uiListPositions.collectAsStateWithLifecycle()
    val showMorePlayersVisible by viewModel.showMoreButtonVisible.collectAsStateWithLifecycle()

    val filterActions = FilterListPlayersActions(
        onNameChange = viewModel::onNameChange,
        onCityChange = viewModel::onCityChange,
        onMinAgeChange = viewModel::onMinAgeChange,
        onMaxAgeChange = viewModel::onMaxAgeChange,
        onPositionChange = viewModel::onPositionChange,
        onMinSizeChange = viewModel::onMinSizeChange,
        onMaxSizeChange = viewModel::onMaxSizeChange,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::filterApply,
            onFilterClean = viewModel::cleanFilters
        )
    )

    ListPlayersContent(
        isLoading = uiState.isLoading,
        errorMessage = uiState.errorMessage,
        list = list,
        filters = filters,
        filtersError = filtersError,
        listPosition = listPosition,
        onRetry = { viewModel.retry() },
        filterActions = filterActions,
        onSendMembership = { id -> viewModel.sendMembershipRequest(id) },
        onShowMore = { id ->
            viewModel.showMore(idPlayer = id, navHostController = navHostController)
        },
        isValidShowMore = showMorePlayersVisible,
        showMoreItems = { viewModel.loadMorePlayers() }
    )
}
@Composable
fun ListPlayersContent(
    isLoading: Boolean,
    errorMessage: String?,
    list: List<InfoPlayerDto>,
    filters: FilterListPlayer,
    filtersError: FilterPlayersErrors,
    listPosition: List<Position?>,
    onRetry: () -> Unit,
    isValidShowMore: Boolean,
    showMoreItems: () -> Unit,
    filterActions: FilterListPlayersActions,
    onSendMembership: (String) -> Unit,
    onShowMore: (String) -> Unit
) {
    var filtersExpanded by remember { mutableStateOf(false) }

    LoadingPage(
        isLoading = isLoading,
        errorMsg = errorMessage,
        retry = onRetry,
        content = {
            ListSurface(
                list = list,
                filterSection = {
                    FilterSection(
                        isExpanded = filtersExpanded,
                        onToggleExpand = { filtersExpanded = !filtersExpanded },
                        content = { paddingModifier ->
                            // Nota: A função FilterListPlayerContent precisa ser pública ou estar neste ficheiro
                            FilterListPlayerContent(
                                filters = filters,
                                filtersError = filtersError,
                                filterActions = filterActions,
                                listPosition = listPosition,
                                modifier = paddingModifier
                            )
                        }
                    )
                },
                listItems = { player ->
                    ItemListPlayer(
                        player = player,
                        sendMemberShipRequest = { onSendMembership(player.id) },
                        showMore = { onShowMore(player.id) }
                    )
                },
                isValidShowMore = isValidShowMore,
                showMoreItems = showMoreItems,
                messageEmptyList = stringResource(id = R.string.list_player_empty)
            )
        }
    )
}

@Composable
private fun FilterListPlayerContent(
    filters: FilterListPlayer,
    filtersError: FilterPlayersErrors,
    filterActions: FilterListPlayersActions,
    listPosition: List<Position?>,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        FilterRow(
            content = {
                FilterNamePlayerTextField(
                    playerName = filters.name,
                    onPlayerNameChange = { filterActions.onNameChange(it) },
                    isError = filtersError.nameError != null,
                    errorMessage = filtersError.nameError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterCityTextField(
                    city = filters.city,
                    onCityChange = { filterActions.onCityChange(it) },
                    isError = filtersError.cityError != null,
                    errorMessage = filtersError.cityError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterListPosition(
                    listPosition = listPosition,
                    selectPosition = filters.position,
                    onSelectPosition = { filterActions.onPositionChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterMaxAgeTextField(
                    maxAge = filters.maxAge?.toString(),
                    onMaxAgeChange = { filterActions.onMaxAgeChange(it.toIntOrNull()) },
                    isError = filtersError.maxAgeError != null,
                    errorMessage = filtersError.maxAgeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMinAgeTextField(
                    minAge = filters.minAge?.toString(),
                    onMinAgeChange = { filterActions.onMinAgeChange(it.toIntOrNull()) },
                    isError = filtersError.minAgeError != null,
                    errorMessage = filtersError.minAgeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())},
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_min_size),
                    value = filters.minSize?.toString(),
                    onValueChange = { filterActions.onMinSizeChange(it.toIntOrNull()) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    minLenght = PlayerConst.MIN_HEIGHT,
                    maxLenght = PlayerConst.MAX_HEIGHT,
                    isError = filtersError.minSizeError != null,
                    errorMessage = filtersError.minSizeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                LabelTextField(
                    label = stringResource(id = R.string.filter_max_size),
                    value = filters.maxSize?.toString(),
                    onValueChange = { filterActions.onMaxSizeChange(it.toIntOrNull()) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    minLenght = PlayerConst.MIN_HEIGHT,
                    maxLenght = PlayerConst.MAX_HEIGHT,
                    isError = filtersError.maxSizeError != null,
                    errorMessage = filtersError.maxSizeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            buttonsActions = filterActions.buttonActions,
            modifier = Modifier.weight(1f)
        )
    }
}

@Composable
private fun ItemListPlayer(
    player: InfoPlayerDto,
    sendMemberShipRequest: () -> Unit,
    showMore: () -> Unit,
) {
    GenericListItem(
        item = player,
        title = { it.name },
        leading = {
            StringImageList(
                image = player.image,
            )
        },
        supporting = {
            Column {
                AgeRow(age = player.age)
                AddressRow(address = player.address)
                PositionRow(position = player.position)
                SizeRow(size = player.heigth)
            }
        },
        trailing = {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.End
            ) {
                if (!player.haveTeam) {
                    ListSendMemberShipRequestButton(sendMemberShipRequest = sendMemberShipRequest)
                }

                ShowMoreInfoButton(
                    showMoreDetails = showMore,
                    contentDescription = stringResource(id = R.string.list_teams_view_team)
                )
            }
        }
    )
}

@Preview(
    name = "Lista de Jogadores - PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "List Players - En",
    locale = "en",
    showBackground = true
)
@Composable
fun ListPlayersPreview() {
    val dummyList = listOf(
        InfoPlayerDto(
            id = "1",
            name = "Lionel Messi",
            age = 36,
            address = "Miami, USA",
            heigth = 170,
            position = Position.FORWARD,
            haveTeam = true,
            image = ""
        ),
        InfoPlayerDto(
            id = "2",
            name = "Bernardo Silva",
            age = 29,
            address = "Manchester, UK",
            heigth = 173,
            position = Position.MIDFIELDER,
            haveTeam = false,
            image = ""
        )
    )

    val dummyActions = FilterListPlayersActions(
        onNameChange = {},
        onCityChange = {},
        onMinAgeChange = {},
        onMaxAgeChange = {},
        onPositionChange = {},
        onMinSizeChange = {},
        onMaxSizeChange = {},
        buttonActions = ButtonFilterActions(
            onFilterApply = {},
            onFilterClean = {}
        )
    )

    AMFootballTheme {
        ListPlayersContent(
            isLoading = false,
            errorMessage = null,
            list = dummyList,
            filters = FilterListPlayer(),
            filtersError = FilterPlayersErrors(),
            listPosition = Position.values().toList(),
            onRetry = {},
            filterActions = dummyActions,
            onSendMembership = {},
            onShowMore = {},
            isValidShowMore = true,
            showMoreItems = {}
        )
    }
}

@Preview(
    name = "Lista Vazia - PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "Empty List - En",
    locale = "en",
    showBackground = true
)
@Composable
fun ListPlayersEmptyPreview() {
    val dummyActions = FilterListPlayersActions(
        onNameChange = {},
        onCityChange = {},
        onMinAgeChange = {},
        onMaxAgeChange = {},
        onPositionChange = {},
        onMinSizeChange = {},
        onMaxSizeChange = {},
        buttonActions = ButtonFilterActions(
            onFilterApply = {},
            onFilterClean = {}
        )
    )

    AMFootballTheme {
        ListPlayersContent(
            isLoading = false,
            errorMessage = null,
            list = emptyList(),
            filters = FilterListPlayer(),
            filtersError = FilterPlayersErrors(),
            listPosition = Position.values().toList(),
            onRetry = {},
            filterActions = dummyActions,
            onSendMembership = {},
            onShowMore = {},
            isValidShowMore = false,
            showMoreItems = {}
        )
    }
}
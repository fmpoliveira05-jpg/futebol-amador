package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPlayersActions
import com.example.amfootball.data.dtos.filters.FilterListPlayerDto
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.ListSendMemberShipRequestButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.AddressRow
import com.example.amfootball.ui.components.lists.AgeRow
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PositionRow
import com.example.amfootball.ui.components.lists.SizeRow
import com.example.amfootball.ui.viewModel.lists.ListPlayerViewModel

@Composable
fun ListPlayersScreen(
    navHostController: NavHostController,
    viewModel: ListPlayerViewModel = viewModel()
) {
    val filters by viewModel.uiFilters.observeAsState(initial = FilterListPlayerDto())
    val list by viewModel.uiList.observeAsState(initial = emptyList())
    val listPosition by viewModel.uiListPositions.observeAsState(initial = emptyList())

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

    var filtersExpanded by remember { mutableStateOf(false) }

    ListSurface(
        list = list,
        filterSection = {
            FilterSection(
                isExpanded = filtersExpanded,
                onToggleExpand = { filtersExpanded = !filtersExpanded },
                content = { paddingModifier ->
                    FilterListPlayerContent(
                        filters = filters,
                        filterActions = filterActions,
                        listPosition = listPosition,
                        modifier = paddingModifier
                    )
                }

            )
        },
        listItems = {  player ->
            ItemListPlayer(
                player = player,
                sendMemberShipRequest = { viewModel.sendMembershipRequest(player.id) },
                showMore = { viewModel.showMore(
                    idPlayer = player.id,
                    navHostController = navHostController)
                }
            )
        }
    )
}

@Composable
private fun FilterListPlayerContent(
    filters: FilterListPlayerDto,
    filterActions: FilterListPlayersActions,
    listPosition: List<Position?>,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_name),
                    value = filters.name,
                    onValueChange = { filterActions.onNameChange(it) },
                    modifier = Modifier.weight(1f)
                )
                Spacer(Modifier.width(8.dp))

                LabelTextField(
                    label = stringResource(id = R.string.filter_city),
                    value = filters.city,
                    onValueChange = { filterActions.onCityChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                LabelSelectBox(
                    label = stringResource(id = R.string.filter_position),
                    list = listPosition,
                    selectedValue = filters.position,
                    onSelectItem = { filterActions.onPositionChange(it) },
                    itemToString = { typeMember ->
                        if (typeMember == null) {
                            stringResource(id = R.string.filter_selectbox_all)
                        } else {
                            stringResource(id = typeMember.stringId)
                        }
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_min_age),
                    value = filters.minAge?.toString(),
                    onValueChange = { filterActions.onMinAgeChange(it.toIntOrNull()) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.weight(1f)
                )

                LabelTextField(
                    label = stringResource(id = R.string.filter_max_age),
                    value = filters.maxAge?.toString(),
                    onValueChange = { filterActions.onMaxAgeChange(it.toIntOrNull()) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_min_size),
                    value = filters.minSize?.toString(),
                    onValueChange = { filterActions.onMinSizeChange },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.weight(1f)
                )

                LabelTextField(
                    label = stringResource(id = R.string.filter_max_size),
                    value = filters.maxSize?.toString(),
                    onValueChange = { filterActions.onMaxSizeChange },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            onApplyFiltersClick = filterActions.buttonActions.onFilterApply,
            onClearFilters = filterActions.buttonActions.onFilterClean,
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
            ImageList(
                image = player.image,
            )
        },
        supporting = {
            Column {
                AgeRow(age = player.age)
                AddressRow(address = player.address)
                PositionRow(position = player.position)
                SizeRow(size = player.size)
            }
        },
        trailing = {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.End
            ) {
                ListSendMemberShipRequestButton(sendMemberShipRequest = sendMemberShipRequest)

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
    ListPlayersScreen(rememberNavController())
}
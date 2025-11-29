package com.example.amfootball.ui.screens.matchInvite

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
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
import com.example.amfootball.data.actions.filters.FilterMatchInviteActions
import com.example.amfootball.data.actions.itemsList.ItemListMatchIniviteActions
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.errors.filtersError.FilterMatchInviteError
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.EditButton
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.DateRow
import com.example.amfootball.ui.components.lists.FilterMaxDatePicker
import com.example.amfootball.ui.components.lists.FilterMinDatePicker
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PitchAddressRow
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.viewModel.matchInvite.ListMatchInviteViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

@Composable
fun ListMatchInviteScreen(
    navHostController: NavHostController,
    viewModel: ListMatchInviteViewModel = hiltViewModel()
) {
    val filters by viewModel.uiFilters.collectAsStateWithLifecycle()
    val filterError by viewModel.filterError.collectAsStateWithLifecycle()

    val filterActions = FilterMatchInviteActions(
        onSenderNameChange = viewModel::onNameSenderChange,
        onMinDateSelected = viewModel::onMinDateChange,
        onMaxDateSelected = viewModel::onMaxDateChange,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::onApplyFilter,
            onFilterClean = viewModel::onFilterClear
        )
    )

    val list by viewModel.uiList.collectAsStateWithLifecycle()
    val itemsListActions = ItemListMatchIniviteActions(
        acceptMatchInvite = viewModel::acceptMatchInvite,
        rejectMatchInvite = viewModel::rejectMatchInvite,
        negociateMatchInvite = viewModel::negociateMatchInvite,
        showMoreDetails = viewModel::showMoreDetails
    )

    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    ListMatchInviteContent(
        uiState = uiState,
        isOnline = isOnline,
        filters = filters,
        filterActions = filterActions,
        filterError = filterError,
        list = list,
        itemsListActions = itemsListActions,
        navHostController = navHostController
    )
}

@Composable
private fun ListMatchInviteContent(
    uiState: UiState,
    isOnline: Boolean,
    filters: FilterMatchInvite,
    filterActions: FilterMatchInviteActions,
    filterError: FilterMatchInviteError,
    list: List<InfoMatchInviteDto>,
    itemsListActions: ItemListMatchIniviteActions,
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
                        content = {
                            FilterListMatchInvite(
                                filters = filters,
                                filterActions = filterActions,
                                filterError = filterError,
                                modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
                            )
                        }
                    )
                },
                listItems = { invite ->
                    ItemListMatchInivite(
                        matchInvite = invite,
                        itemsListActions = itemsListActions,
                        navHostController = navHostController
                    )
                },
                messageEmptyList = stringResource(R.string.list_match_invite_empty)
            )
        }
    )

}

@Composable
private fun FilterListMatchInvite(
    filters: FilterMatchInvite,
    filterError: FilterMatchInviteError,
    filterActions: FilterMatchInviteActions,
    modifier: Modifier = Modifier
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_sender_name),
                    value = filters.senderName ?: "",
                    onValueChange = { filterActions.onSenderNameChange(it) },
                    isError = filterError.senderNameError != null,
                    errorMessage = filterError.senderNameError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterMinDatePicker(
                    value = filters.minDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDateSelected(it) },
                    isError = filterError.minDateError != null,
                    errorMessage = filterError.minDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMaxDatePicker(
                    value = filters.maxDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDateSelected(it)},
                    isError = filterError.maxDateError != null,
                    errorMessage = filterError.maxDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
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
private fun ItemListMatchInivite(
    matchInvite: InfoMatchInviteDto,
    itemsListActions: ItemListMatchIniviteActions,
    navHostController: NavHostController
) {
    GenericListItem(
        item = matchInvite,
        title = { it.opponent.name },
        leading = {
            StringImageList(
                image = matchInvite.opponent.image,
            )
        },
        supporting = {
            Column {
                PitchAddressRow(ptichAdrress = matchInvite.pitchGame)

                DateRow(date = matchInvite.gameDate.format(
                    DateTimeFormatter.ofPattern(
                        Patterns.DATE,
                    )
                ))
            }
        },
        trailing = {
            ShowMoreInfoButton(
                showMoreDetails = { itemsListActions.showMoreDetails(matchInvite.id, navHostController) },
                contentDescription = stringResource(id = R.string.list_teams_view_team)
            )
        },
        underneathItems = {
            AcceptButton(accept = {itemsListActions.acceptMatchInvite(matchInvite.id) })
            EditButton(
                edit = { itemsListActions.negociateMatchInvite(matchInvite.id, navHostController) },
                contentDescription = stringResource(id = R.string.negotiate_button_description)
            )
            RejectButton(reject = { itemsListActions.rejectMatchInvite(matchInvite.id)})
        }
    )
}

@Preview(
    name = "Lista de convite de partida - PT",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "List Match Invite - EN",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewListMatchInvite() {
    ListMatchInviteScreen(
        navHostController = rememberNavController(),
    )
}
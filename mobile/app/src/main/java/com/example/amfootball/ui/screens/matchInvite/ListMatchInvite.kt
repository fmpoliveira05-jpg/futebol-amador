package com.example.amfootball.ui.screens.matchInvite

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
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
import com.example.amfootball.data.actions.filters.FilterMatchInviteActions
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.errors.filtersError.FilterMatchInviteError
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.EditButton
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.FilterMaxDatePicker
import com.example.amfootball.ui.components.inputFields.FilterMinDatePicker
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.DateRow
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PitchAddressRow
import com.example.amfootball.ui.viewModel.matchInvite.ListMatchInviteViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

@Composable
fun ListMatchInviteScreen(
    navHostController: NavHostController,
    viewModel: ListMatchInviteViewModel = viewModel()
) {
    val list by viewModel.uiList.observeAsState(initial = emptyList())
    val filters by viewModel.uiFilters.observeAsState(initial = FilterMatchInvite())
    val filterError by viewModel.filterError.observeAsState(initial = FilterMatchInviteError())

    val filterActions = FilterMatchInviteActions(
        onSenderNameChange = viewModel::onNameSenderChange,
        onMinDateSelected = viewModel::onMinDateChange,
        onMaxDateSelected = viewModel::onMaxDateChange,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::onApplyFilter,
            onFilterClean = viewModel::onFilterClear
        )
    )
    var filtersExpanded by remember { mutableStateOf(false) }

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
                acceptMatchInvite = {
                    viewModel.acceptMatchInvite(
                        idMatchInvite = invite.id
                    )
                },
                rejectMatchInvite = {
                    viewModel.rejectMatchInvite(
                        idMatchInvite = invite.id
                    )
                },
                negociateMatchInvite = {
                    viewModel.negociateMatchInvite(
                        idMatchInvite = invite.id,
                        navHostController = navHostController
                    )
                },
                showMore = {
                    viewModel.showMoreDetails(
                        idMatchInvite = invite.id,
                        navHostController = navHostController
                    )
                }
            )
        },
        messageEmptyList = stringResource(R.string.list_match_invite_empty)
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
    acceptMatchInvite: () -> Unit,
    negociateMatchInvite: () -> Unit,
    rejectMatchInvite: () -> Unit,
    showMore: () -> Unit
) {
    GenericListItem(
        item = matchInvite,
        title = { it.nameOpponent },
        leading = {
            ImageList(
                image = matchInvite.logoOpponent,
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
                showMoreDetails = showMore,
                contentDescription = stringResource(id = R.string.list_teams_view_team)
            )
        },
        underneathItems = {
            AcceptButton(accept = acceptMatchInvite)
            EditButton(edit = negociateMatchInvite, contentDescription = stringResource(id = R.string.negotiate_button_description))
            RejectButton(reject = rejectMatchInvite)
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
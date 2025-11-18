package com.example.amfootball.ui.screens.matchInvite

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
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
import com.example.amfootball.data.dtos.filters.FilterMatchInvite
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.DatePickerDocked
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
    val filters by viewModel.uiFilters.observeAsState(initial = FilterMatchInvite())
    val list by viewModel.uiList.observeAsState(initial = emptyList<InfoMatchInviteDto>())
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
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                DatePickerDocked(
                    label = stringResource(id = R.string.filter_min_date),
                    contentDescription = stringResource(id = R.string.description_filter_min_date),
                    value = filters.minDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDateSelected(it) },
                    modifier = Modifier.weight(1f)
                )

                DatePickerDocked(
                    label = stringResource(id = R.string.filter_max_date),
                    contentDescription = stringResource(id = R.string.description_filter_max_date),
                    value = filters.maxDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDateSelected(it)},
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
            NegociateMatchIniviteButton(negociateMatchInvite = negociateMatchInvite)
            RejectButton(reject = rejectMatchInvite)
        }
    )
}

@Composable
fun NegociateMatchIniviteButton(
    negociateMatchInvite: () -> Unit
) {
    IconButton(
        onClick = negociateMatchInvite
    ) {
        Icon(
            imageVector = Icons.Default.Edit,
            contentDescription = stringResource(id = R.string.negociate_button_description),
            tint = MaterialTheme.colorScheme.secondary
        )
    }
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
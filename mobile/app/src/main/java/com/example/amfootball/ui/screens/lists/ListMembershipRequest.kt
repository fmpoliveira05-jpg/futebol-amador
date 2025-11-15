package com.example.amfootball.ui.screens.lists

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
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
import com.example.amfootball.data.actions.filters.FilterMemberShipRequestActions
import com.example.amfootball.data.dtos.filters.FilterMemberShipRequest
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.inputFields.DatePickerDocked
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterHeader
import com.example.amfootball.ui.components.lists.GenericPlayerListItem
import com.example.amfootball.ui.components.lists.InfoRow
import com.example.amfootball.ui.components.lists.ItemAcceptRejectAndShowMore
import com.example.amfootball.ui.components.lists.PlayerImageList
import com.example.amfootball.ui.viewModel.memberShipRequest.ListMemberShipRequestViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

//TODO: Falta adaptar isto para quando for admin mostrar uns memberShipRequest e se for player outros
//TODO: Está tudo feito para funcionar sem a API, mas depois meter
@Composable
fun ListMemberShipRequest(
    navHostController: NavHostController,
    viewModel: ListMemberShipRequestViewModel = viewModel(),
){
    val filters by viewModel.uiFilterState.collectAsState()
    val list by viewModel.uiListState.collectAsState()

    val filterActions = FilterMemberShipRequestActions(
        onSenderNameChange = viewModel::onSenderNameChanged,
        onMinDateSelected = viewModel::onMinDateSelected,
        onMaxDateSelected = viewModel::onMaxDateSelected,
        onApplyFiltersClick = viewModel::applyFilters,
        onClearFilters = viewModel::clearFilters
    )

    var filtersExpanded by remember { mutableStateOf(false) }

    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            item {
                FilterSection(
                    isExpanded = filtersExpanded,
                    onToggleExpand = { filtersExpanded = !filtersExpanded },
                    filters = filters,
                    filterActions = filterActions
                )
                Spacer(Modifier.height(16.dp))
            }

            items(list) { request ->
                ListMemberShipRequestContent(
                    membershipRequest = request,
                    acceptMemberShipRequest = { viewModel.AcceptMemberShipRequest(
                        idReceiver = request.receiver.id,
                        idRequest = request.id,
                        isPlayerSender = request.isPlayerSender,
                        navHostController = navHostController,
                    ) },
                    rejectMemberShipRequest = { viewModel.RejectMemberShipRequest(
                        idReceiver = request.receiver.id,
                        idRequest = request.id,
                        isPlayerSender = request.isPlayerSender,
                    ) },
                    showMore = { viewModel.ShowMore(
                        isPlayerSender = request.isPlayerSender,
                        IdSender = request.sender.id,
                        navHostController = navHostController,
                    ) }
                )

                Spacer(Modifier.height(12.dp))
            }
        }
    }
}

@Composable
private fun FilterSection(
    isExpanded: Boolean,
    onToggleExpand: () -> Unit,
    filters: FilterMemberShipRequest,
    filterActions: FilterMemberShipRequestActions,
    modifier: Modifier = Modifier,
) {
    ElevatedCard(modifier = modifier.fillMaxWidth()) {
        Column {
            //Cabeçalho
            FilterHeader(
                isExpanded = isExpanded,
                onToggleExpand = onToggleExpand
            )

            //Filtros todos
            AnimatedVisibility(visible = isExpanded) {
                FilterListMemberShipRequestContent(
                    filters = filters,
                    filterActions = filterActions,
                    modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
                )
            }
        }
    }
}

@Composable
private fun FilterListMemberShipRequestContent(
    filters: FilterMemberShipRequest,
    filterActions: FilterMemberShipRequestActions,
    modifier: Modifier = Modifier,
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        Row(modifier = Modifier.fillMaxWidth()) {
            LabelTextField(
                label = stringResource(id = R.string.filter_sender_name),
                value = filters.senderName ?: "",
                onValueChange = { filterActions.onSenderNameChange(it) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
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

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            onApplyFiltersClick = filterActions.onApplyFiltersClick,
            onClearFilters = filterActions.onClearFilters,
            modifier = Modifier.weight(1f)
        )
    }
}

@Composable
private fun ListMemberShipRequestContent(
    membershipRequest: MembershipRequestInfoDto,
    acceptMemberShipRequest: () -> Unit = {},
    rejectMemberShipRequest: () -> Unit = {},
    showMore: () -> Unit = {},
) {
    GenericPlayerListItem(
        item = membershipRequest,
        title = { it.sender.name },
        leading = {
            PlayerImageList(
                image = membershipRequest.sender.image,
            )
        },
        supporting = {
            Column {
                InfoRow(
                    icon = Icons.Default.CalendarMonth,
                    text = membershipRequest.dateSend.format(
                        DateTimeFormatter.ofPattern(
                            Patterns.DATE,
                        )
                    ),
                    modifier = Modifier.fillMaxWidth()
                )
            }
        },
        trailing = {
            ItemAcceptRejectAndShowMore(
                accept = acceptMemberShipRequest,
                reject = rejectMemberShipRequest,
                showMore = showMore
            )
        }
    )
}

@Preview(
    name = "Lista de pedidos de adesão Jogador - PT",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "List MemberShip Request player - EN",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewListMemberShipRequestPlayerScreen() {
    ListMemberShipRequest(rememberNavController())
}


@Preview(
    name = "Lista de pedidos de adesão Jogador - PT",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "List MemberShip Request player - EN",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewListMemberShipRequestTeamScreen() {
    ListMemberShipRequest(rememberNavController())
}
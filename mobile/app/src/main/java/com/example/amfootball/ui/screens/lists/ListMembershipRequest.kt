package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMemberShipRequestActions
import com.example.amfootball.data.filters.FilterMemberShipRequest
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.errors.filtersError.FilterMemberShipRequestError
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterMaxDatePicker
import com.example.amfootball.ui.components.lists.FilterMinDatePicker
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.InfoRow
import com.example.amfootball.ui.components.lists.ItemAcceptRejectAndShowMore
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.viewModel.memberShipRequest.ListMemberShipRequestViewModel
import com.example.amfootball.utils.Patterns
import com.example.amfootball.utils.UserConst
import java.time.format.DateTimeFormatter

//TODO: Falta adaptar isto para quando for admin mostrar uns memberShipRequest e se for player outros
@Composable
fun ListMemberShipRequest(
    navHostController: NavHostController,
    viewModel: ListMemberShipRequestViewModel = viewModel(),
){
    val filters by viewModel.uiFilterState.collectAsStateWithLifecycle()
    val filterError by viewModel.uiFilterErrorState.collectAsStateWithLifecycle()
    val list by viewModel.uiListState.collectAsStateWithLifecycle()
    val filterActions = FilterMemberShipRequestActions(
        onSenderNameChange = viewModel::onSenderNameChanged,
        onMinDateSelected = viewModel::onMinDateSelected,
        onMaxDateSelected = viewModel::onMaxDateSelected,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::applyFilters,
            onFilterClean = viewModel::clearFilters
        ),
    )

    var filtersExpanded by remember { mutableStateOf(false) }

    ListSurface(
        list = list,
        filterSection = {
            FilterSection(
                isExpanded = filtersExpanded,
                onToggleExpand = { filtersExpanded = !filtersExpanded },
                content = {
                    FilterListMemberShipRequestContent(
                        filters = filters,
                        filterError = filterError,
                        filterActions = filterActions,
                        modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp)
                    )
                }
            )
        },
        listItems = { request ->
            ListMemberShipRequestContent(
                membershipRequest = request,
                acceptMemberShipRequest = { viewModel.acceptMemberShipRequest(
                    idReceiver = request.receiver.id,
                    idRequest = request.id,
                    isPlayerSender = request.isPlayerSender,
                    navHostController = navHostController,
                ) },
                rejectMemberShipRequest = { viewModel.rejectMemberShipRequest(
                    idReceiver = request.receiver.id,
                    idRequest = request.id,
                    isPlayerSender = request.isPlayerSender,
                ) },
                showMore = { viewModel.showMore(
                    isPlayerSender = request.isPlayerSender,
                    idSender = request.sender.id,
                    navHostController = navHostController,
                ) }
            )
        },
        messageEmptyList = stringResource(id = R.string.list_membership_request_empty)
    )
}

@Composable
private fun FilterListMemberShipRequestContent(
    filters: FilterMemberShipRequest,
    filterError: FilterMemberShipRequestError,
    filterActions: FilterMemberShipRequestActions,
    modifier: Modifier = Modifier,
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_sender_name),
                    value = filters.senderName ?: "",
                    maxLenght = UserConst.MAX_NAME_LENGTH,
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
private fun ListMemberShipRequestContent(
    membershipRequest: MembershipRequestInfoDto,
    acceptMemberShipRequest: () -> Unit = {},
    rejectMemberShipRequest: () -> Unit = {},
    showMore: () -> Unit = {},
) {
    GenericListItem(
        item = membershipRequest,
        title = { it.sender.name },
        leading = {
            ImageList(
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
package com.example.amfootball.ui.screens.MembershipRequest

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalConfiguration
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.core.os.ConfigurationCompat
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.MembershipRequest.MembershipRequestInfoDto
import com.example.amfootball.ui.components.Buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.InputFields.DatePickerDocked
import com.example.amfootball.ui.components.InputFields.LabelTextField
import com.example.amfootball.ui.components.Lists.FilterHeader
import com.example.amfootball.ui.components.Lists.InfoRow
import com.example.amfootball.ui.viewModel.MemberShipRequest.ListMemberShipRequestViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

//TODO: Falta adaptar isto para quando for admin mostrar uns memberShipRequest e se for player outros
//TODO: Está tudo feito para funcionar sem a API, mas depois meter
@Composable
fun ListMemberShipRequest(
    navHostController: NavHostController,
    viewModel: ListMemberShipRequestViewModel = viewModel(),
){
    val senderName = viewModel.senderName.value
    val minDateString = viewModel.minDateDisplayString.value
    val maxDateString = viewModel.maxDateDisplayString.value
    val listMemberShipRequest = viewModel.listMemberShipRequest.value
    var filtersExpanded by remember { mutableStateOf(false) }

    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            item {
                FilterSection(
                    isExpanded = filtersExpanded,
                    onToggleExpand = { filtersExpanded = !filtersExpanded },
                    senderNameValue = senderName,
                    minDateValue = minDateString,
                    maxDateValue = maxDateString,
                    onSenderNameChange = { viewModel.onSenderNameChanged(it) },
                    onMinDateSelected = { viewModel.onMinDateSelected(it) },
                    onMaxDateSelected = { viewModel.onMaxDateSelected(it) },
                    onApplyFiltersClick = { viewModel.applyFilters() },
                    onClearFilters = { viewModel.clearFilters() }
                )
                Spacer(Modifier.height(16.dp))
            }

            /**
             * TODO: Depois meter para receber o senderId e o boolean
             * */
            items(listMemberShipRequest) { request ->
                ListMemberShipRequestContent(
                    membershipRequest = request,
                    acceptMemberShipRequest = { viewModel.AcceptMemberShipRequest(
                        idReceiver = request.IdSender,
                        idRequest = request.Id,
                        isPlayerSender = request.IsPlayerSender,
                        navHostController = navHostController,
                    ) },
                    rejectMemberShipRequest = { viewModel.RejectMemberShipRequest(
                        idReceiver = request.IdSender,
                        idRequest = request.Id,
                        isPlayerSender = request.IsPlayerSender,
                    ) },
                    showMore = { viewModel.ShowMore(
                        isPlayerSender = request.IsPlayerSender,
                        IdSender = request.IdSender,
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
    senderNameValue: String,
    minDateValue: String,
    maxDateValue: String,
    onSenderNameChange: (String) -> Unit,
    onMinDateSelected: (Long) -> Unit,
    onMaxDateSelected: (Long) -> Unit,
    onApplyFiltersClick: () -> Unit,
    onClearFilters: () -> Unit,
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
                    senderNameValue = senderNameValue,
                    minDateValue = minDateValue,
                    maxDateValue = maxDateValue,
                    onSenderNameChange = onSenderNameChange,
                    onMinDateSelected = onMinDateSelected,
                    onMaxDateSelected = onMaxDateSelected,
                    onApplyFiltersClick = onApplyFiltersClick,
                    onClearFilters = onClearFilters,
                    modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
                )
            }

        }
    }
}

@Composable
private fun FilterListMemberShipRequestContent(
    senderNameValue: String,
    minDateValue: String,
    maxDateValue: String,
    onSenderNameChange: (String) -> Unit,
    onMinDateSelected: (Long) -> Unit,
    onMaxDateSelected: (Long) -> Unit,
    onApplyFiltersClick: () -> Unit,
    onClearFilters: () -> Unit,
    modifier: Modifier = Modifier,
) {
    Column(modifier = modifier) {
        Row(modifier = Modifier.fillMaxWidth()) {
            LabelTextField(
                label = stringResource(id = R.string.filter_sender_name),
                value = senderNameValue,
                onValueChange = onSenderNameChange,
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
                value = minDateValue,
                onDateSelected = onMinDateSelected,
                modifier = Modifier.weight(1f)
            )

            DatePickerDocked(
                label = stringResource(id = R.string.filter_max_date),
                contentDescription = stringResource(id = R.string.description_filter_max_date),
                value = maxDateValue,
                onDateSelected = onMaxDateSelected,
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            onApplyFiltersClick = onApplyFiltersClick,
            onClearFilters = onClearFilters,
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
    ListItem(
        headlineContent = { //Conteudo Principal
            Text(
                text= membershipRequest.Sender,
                style = MaterialTheme.typography.titleLarge,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )
        },
        supportingContent = { //Aparece em baixo (Descrição e cidade)
            Column {
                InfoRow(
                    icon = Icons.Default.CalendarMonth,
                    text = membershipRequest.DateSend.format(
                         DateTimeFormatter.ofPattern(
                            Patterns.Date,
                         )
                    ),
                    modifier = Modifier.fillMaxWidth()
                )
            }
        },
        leadingContent = { // imagem (Depois trocar para a imagem da team ou player)
            Icon(
                imageVector = Icons.Default.Star,
                contentDescription = "Logo Team",
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(40.dp)
            )
        },
        trailingContent = {
            TrailingContent(
                acceptMemberShipRequest = acceptMemberShipRequest,
                rejectMemberShipRequest = rejectMemberShipRequest,
                showMore = showMore
            )
        },
    )
}

/**
 * Todo que fica no lado direito da linha
 * */
@Composable
private fun TrailingContent(
    acceptMemberShipRequest: () -> Unit = {},
    rejectMemberShipRequest: () -> Unit = {},
    showMore: () -> Unit = {},
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        IconButton(
            onClick = { acceptMemberShipRequest() }
        ) {
            Icon(
                imageVector = Icons.Default.Check,
                contentDescription = "Aceitar",
                tint = MaterialTheme.colorScheme.primary
            )
        }

        IconButton(
            onClick = { rejectMemberShipRequest() }
        ) {
            Icon(
                imageVector = Icons.Default.Close,
                contentDescription = "Rejeitar",
                tint = MaterialTheme.colorScheme.error
            )
        }

        IconButton(
            onClick = { showMore() }
        ) {
            Icon(
                imageVector = Icons.Default.ChevronRight,
                contentDescription = stringResource(id = R.string.list_teams_view_team),
                tint = MaterialTheme.colorScheme.outline
            )
        }
    }
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
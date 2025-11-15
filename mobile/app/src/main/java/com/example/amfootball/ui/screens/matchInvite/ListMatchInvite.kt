package com.example.amfootball.ui.screens.matchInvite

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
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Stadium
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.FilterMatchInviteActions
import com.example.amfootball.data.dtos.filters.FilterMatchInvite
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.DatePickerDocked
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterHeader
import com.example.amfootball.ui.components.lists.InfoRow
import com.example.amfootball.ui.components.lists.ItemAcceptRejectAndShowMore
import com.example.amfootball.ui.components.lists.PlayerImageList
import com.example.amfootball.ui.viewModel.matchInvite.ListMatchInviteViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

@Composable
fun ListMatchInviteScreen(
    navHostController: NavHostController,
    viewModel: ListMatchInviteViewModel = viewModel()
) {
    val filters by viewModel.uiFilters.collectAsState()
    val list by viewModel.uiList.collectAsState()
    val filterActions = FilterMatchInviteActions(
        onSenderNameChange = viewModel::onNameSenderChange,
        onMinDateSelected = viewModel::onMinDateChange,
        onMaxDateSelected = viewModel::onMaxDateChange,
        onApplyFiltersClick = viewModel::onApplyFilter,
        onClearFilters = viewModel::onFilterClear
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

            items(list) { invite ->
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

                Spacer(Modifier.height(12.dp))
            }
        }
    }
}

@Composable
private fun FilterSection(
    isExpanded: Boolean,
    onToggleExpand: () -> Unit,
    filters: FilterMatchInvite,
    filterActions: FilterMatchInviteActions,
    modifier: Modifier = Modifier
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
                FilterListMatchInvite(
                    filters = filters,
                    filterActions = filterActions,
                    modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
                )
            }
        }
    }
}


@Composable
private fun FilterListMatchInvite(
    filters: FilterMatchInvite,
    filterActions: FilterMatchInviteActions,
    modifier: Modifier = Modifier
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
private fun ItemListMatchInivite(
    matchInvite: InfoMatchInviteDto,
    acceptMatchInvite: () -> Unit,
    negociateMatchInvite: () -> Unit,
    rejectMatchInvite: () -> Unit,
    showMore: () -> Unit
) {
    ElevatedCard(modifier = Modifier.fillMaxWidth()) {
        Column {
            ListItem(
                headlineContent = { //Conteudo Principal
                    Text(
                        text = matchInvite.nameOpponent,
                        style = MaterialTheme.typography.titleLarge,
                        maxLines = 2,
                        overflow = TextOverflow.Ellipsis
                    )
                },
                leadingContent = {
                    PlayerImageList(
                        image = matchInvite.logoOpponent,
                    )
                },
                supportingContent = { //Aparece em baixo (Descrição e cidade)
                    Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        InfoRow(
                            icon = Icons.Default.Stadium,
                            text = matchInvite.pitchGame,
                            modifier = Modifier.fillMaxWidth()
                        )

                        InfoRow(
                            icon = Icons.Default.CalendarMonth,
                            text = matchInvite.gameDate.format(
                                DateTimeFormatter.ofPattern(
                                    Patterns.DATE,
                                )
                            ),
                            modifier = Modifier.fillMaxWidth()
                        )
                    }
                },
                trailingContent = {
                    ShowMoreInfoButton(
                        showMoreDetails = showMore,
                        contentDescription = stringResource(id = R.string.list_teams_view_team)
                    )
                }
            )

            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.Center,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 8.dp, vertical = 4.dp)
            ) {
                AcceptButton(accept = acceptMatchInvite)
                NegociateMatchIniviteButton(negociateMatchInvite = negociateMatchInvite)
                RejectButton(reject = rejectMatchInvite)
            }
        }
    }
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
    ListMatchInviteScreen(rememberNavController())
}
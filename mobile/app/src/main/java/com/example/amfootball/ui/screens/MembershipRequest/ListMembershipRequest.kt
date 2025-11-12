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
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Send
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
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
import com.example.amfootball.data.dtos.MembershipRequest.MembershipRequestInfoDto
import com.example.amfootball.navigation.Objects.NavBar.RoutesNavBarTeam
import com.example.amfootball.navigation.Objects.Pages.CrudTeamRoutes
import com.example.amfootball.ui.components.Buttons.FilterApplyButton
import com.example.amfootball.ui.components.InputFields.DatePickerDocked
import com.example.amfootball.ui.components.InputFields.LabelTextField
import com.example.amfootball.ui.components.Lists.FilterHeader
import com.example.amfootball.ui.components.Lists.InfoRow
import com.example.amfootball.ui.viewModel.MemberShipRequest.ListMemberShipRequestViewModel

//TODO: Implementar filtros, lista e botões de cada lista sem chamadas à API (com ViewModels)
//TODO: Meter os botões aceitar e rejeitar com pedidos à API
//TODO: Meter para ir buscar os pedidos de adesão na lista com pedidos há API, considerando sempre o tipo de jogador
//TODO: Depois vais ser necessário passar como parametro o id do player/team (depende de como tiver na API, rever depois a rota)
@Composable
fun ListMemberShipRequest(
    navHostController: NavHostController,
    viewModel: ListMemberShipRequestViewModel = viewModel(),
){
    val senderName = viewModel.senderName.value
    val minDateString = viewModel.minDateDisplayString.value
    val maxDateString = viewModel.maxDateDisplayString.value

    var filtersExpanded by remember { mutableStateOf(false) }

    //Meter aqui se for team o all vai ter o valor da memberShip da Team e se for um player sem team vai aparecer todos os seus pedidos
    val allMemberShipRequest = remember { MembershipRequestInfoDto.generateMemberShipRequestTeam() } //Se for players vai buscar o do player

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
                    onApplyFiltersClick = { viewModel.applyFilters() }
                )
                Spacer(Modifier.height(16.dp))
            }

            items(allMemberShipRequest) { request ->
                ListMemberShipRequestContent(
                    request = request,
                    navHostController = navHostController)
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

        FilterApplyButton(
            onClick = onApplyFiltersClick,
            modifier = Modifier.fillMaxWidth()
        )
    }
}

@Composable
private fun ListMemberShipRequestContent(
                     membershipRequest: MembershipRequestInfoDto,
                     navHostController: NavHostController
) {
    /**
     * Vamos ter 2
     * 1º Admin Team que ve os memberShipRequest da Team
     * 2º Player sem team que vê os memberShip dele
     *
     * Depois este valor vai ser com base no valor da variavel user global e
     * vai ser mandado como parametro ou logo no if
     * */
    val isAdmin by remember { mutableStateOf(false) }


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
                    text = membershipRequest.Date,
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
        trailingContent = { // Tudo que aparece há direita (Botão Ver Detalher + Send MatchInvite no caso da team ou no caso do player send MemberShipRequest)
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                modifier = Modifier.padding(start = 8.dp)
            ) {
                if(isAdmin) {
                    TextButton(
                        onClick = {
                            navHostController.navigate(route = RoutesNavBarTeam.HOME_PAGE_TEAM) {
                                launchSingleTop = true
                            }
                        },
                        contentPadding = PaddingValues(horizontal = 8.dp, vertical = 4.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Default.Send,
                            contentDescription = "Send Membership Request",
                            modifier = Modifier.size(18.dp)
                        )
                        Spacer(modifier = Modifier.width(6.dp))
                        Text(
                            text = "Send Membership",
                            style = MaterialTheme.typography.bodyMedium,
                            maxLines = 1
                        )
                    }
                } else {
                    TextButton(
                        //chamar do modelView o metodo aceitar
                        onClick = {
                            navHostController.navigate(route = RoutesNavBarTeam.HOME_PAGE_TEAM) {
                                launchSingleTop = true
                            }
                        },
                        contentPadding = PaddingValues(horizontal = 8.dp, vertical = 4.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Default.Send,
                            contentDescription = "Send Match Invite",
                            modifier = Modifier.size(18.dp)
                        )
                        Spacer(modifier = Modifier.width(6.dp))
                        Text(
                            text = "Match Invite",
                            style = MaterialTheme.typography.bodyMedium,
                            maxLines = 1
                        )
                    }
                }


                IconButton(
                    onClick = {
                        //TODO: Chama o showMore do ModelView
                    }
                ) {
                    Icon(
                        imageVector = Icons.Default.ChevronRight,
                        contentDescription = stringResource(id = R.string.list_teams_view_team),
                        tint = MaterialTheme.colorScheme.outline
                    )
                }
            }
        },
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
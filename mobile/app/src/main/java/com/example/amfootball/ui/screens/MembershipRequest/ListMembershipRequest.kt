package com.example.amfootball.ui.screens.MembershipRequest

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.dtos.Filters.FilterMemberShipRequest
import com.example.amfootball.data.dtos.MembershipRequest.MembershipRequestInfoDto
import com.example.amfootball.ui.components.Buttons.FilterApplyButton
import com.example.amfootball.ui.components.InputFields.DatePickerDocked
import com.example.amfootball.ui.components.InputFields.LabelTextField
import com.example.amfootball.ui.components.Lists.FilterHeader
import com.example.amfootball.ui.screens.Lists.ListTeamScreen

@Composable
fun ListMemberShipRequest(){
    var filters by remember { mutableStateOf(FilterMemberShipRequest()) }
    var filtersExpanded by remember { mutableStateOf(false) }

    //Meter aqui se for team o all vai ter o valor da memberShip da Team e se for um player sem team vai aparecer todos os seus pedidos
    val allMemberShipRequest = remember { MembershipRequestInfoDto.generateMemberShipRequestTeam() } //Se for players vai buscar o do player
    val filteredList = remember(allMemberShipRequest, filters) {
        filterTeamList(allMemberShipRequest, filters)
    }

    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            item {
                FilterSection(
                    isExpanded = filtersExpanded,
                    onToggleExpand = { filtersExpanded = !filtersExpanded },
                    filters = filters,
                    onFiltersChange = { newFilters ->
                        filters = newFilters
                    },
                )
                Spacer(Modifier.height(16.dp))
            }

            /*
            * items(filteredList) { team ->
                //ListTeam(team = team, navHostController = navHostController)
                Spacer(Modifier.height(12.dp))
            }
            * */
        }
    }
}

@Composable
private fun FilterSection(
    isExpanded: Boolean,
    onToggleExpand: () -> Unit,
    filters: FilterMemberShipRequest,
    onFiltersChange: (FilterMemberShipRequest) -> Unit,
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
                    onFiltersChange = onFiltersChange,
                    modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
                )
            }

        }
    }
}

@Composable
private fun FilterListMemberShipRequestContent(
    filters: FilterMemberShipRequest,
    onFiltersChange: (FilterMemberShipRequest) -> Unit,
    modifier: Modifier = Modifier,
) {
    Column(modifier = modifier) {
        Row(modifier = Modifier.fillMaxWidth()) {
            LabelTextField(
                label = stringResource(id = R.string.filter_sender_name),
                value = filters.senderName,
                onValueChange = { onFiltersChange(filters.copy(senderName = it)) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            /*
            * DatePickerDocked(
                label = stringResource(id = R.string.filter_min_date),
                value = filters.minDate,
                onDateSelected = { onFiltersChange(filters.copy(minDate = it)) },
                contentDescription = stringResource(id = R.string.description_filter_min_date),
                modifier = Modifier.weight(1f)
            )
            LabelTextField(
                label = stringResource(id = R.string.filter_city),
                value = filters.minDate,
                onValueChange = { onFiltersChange(filters.copy(city = it)) },
                modifier = Modifier.weight(1f)
            )
            * */

            Spacer(Modifier.width(8.dp))


            Spacer(Modifier.width(8.dp))
            Spacer(Modifier.weight(1f))
        }

        Spacer(Modifier.height(16.dp))

        FilterApplyButton(
            onClick = {
                // TODO: Fazer o pedido ao endpoint com os 'filters'
            },
            modifier = Modifier.fillMaxWidth() // Para ocupar a largura toda
        )
    }
}

private fun filterTeamList(
    membershipRequest: List<MembershipRequestInfoDto>,
    filters: FilterMemberShipRequest
): List<MembershipRequestInfoDto> {
    return membershipRequest.filter { request ->
        val senderNameFilterPassed = filters.senderName.isNullOrEmpty() ||
                request.Sender.contains(filters.senderName, ignoreCase = true)

        val minDate = (filters.minDate == null) || (request.DateSend >= filters.minDate)
        val maxDate = (filters.maxDate == null) || (request.DateSend <= filters.maxDate)

        senderNameFilterPassed && minDate && maxDate
    }
}

@Preview(
    name = "Lista de Equipa - PT",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "List Team Screen - EN",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewListTeamScreen() {
    ListTeamScreen(rememberNavController())
}
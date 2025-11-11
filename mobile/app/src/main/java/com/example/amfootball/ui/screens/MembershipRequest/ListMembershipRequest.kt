package com.example.amfootball.ui.screens.MembershipRequest

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.dtos.Filters.FiltersListTeamDto
import com.example.amfootball.data.dtos.ItemTeamInfoDto
import com.example.amfootball.data.dtos.Rank.RankNameDto
import com.example.amfootball.ui.components.Lists.FilterHeader
import com.example.amfootball.ui.screens.Lists.ListTeamScreen

@Composable
fun ListMemberShipRequest(){
    var filters by remember { mutableStateOf(FiltersListTeamDto()) }
    var filtersExpanded by remember { mutableStateOf(false) }
    val listRanks = RankNameDto.generateExampleRanks()

    val allTeams = remember { ItemTeamInfoDto.generateExampleTeams() }
    val filteredList = remember(allTeams, filters) {
        filterTeamList(allTeams, filters)
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
                    listRanks = listRanks
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
    filters: FiltersListTeamDto,
    onFiltersChange: (FiltersListTeamDto) -> Unit,
    listRanks: List<RankNameDto>,
    modifier: Modifier = Modifier,
) {
    ElevatedCard(modifier = modifier.fillMaxWidth()) {
        Column {
            //Cabeçalho
            FilterHeader(
                isExpanded = isExpanded,
                onToggleExpand = onToggleExpand
            )

            /*
            //Filtros todos
            AnimatedVisibility(visible = isExpanded) {
                FiltersListTeamContent(
                    filters = filters,
                    onFiltersChange = onFiltersChange,
                    listRanks = listRanks,
                    modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
                )
            }
            */
        }
    }
}

private fun filterTeamList(
    teams: List<ItemTeamInfoDto>,
    filters: FiltersListTeamDto
): List<ItemTeamInfoDto> {
    val minPoints = filters.minPoint
    val maxPoints = filters.maxPoint
    val minAge = filters.minAge
    val maxAge = filters.maxAge
    val minNumberMembers = filters.minNumberMembers
    val maxNumberMembers = filters.maxNumberMembers

    return teams.filter { team ->
        val nameFilterPassed = filters.name.isNullOrEmpty() ||
                team.Name.contains(filters.name, ignoreCase = true)

        val rankFilterPassed = filters.rank.isNullOrEmpty() ||
                team.Name.contains(filters.rank, ignoreCase = true)

        val cityFilterPassed = filters.city.isNullOrEmpty() ||
                team.City.contains(filters.city, ignoreCase = true)

        val minPointFilterPassed = (minPoints == null) || (team.Points >= minPoints)
        val maxPointFilterPassed = (maxPoints == null) || (team.Points <= maxPoints)

        val minAgeFilterPassed = (minAge == null) || (team.AverageAge >= minAge)
        val maxAgeFilterPassed = (maxAge == null) || (team.AverageAge <= maxAge)

        val minNumberMembersFilterPassed = (minNumberMembers == null) || (team.NumberMembers >= minNumberMembers)
        val maxNumberMembersFilterPassed = (maxNumberMembers == null) || (team.NumberMembers <= maxNumberMembers)

        // Devolve o resultado do teste para esta equipa
        nameFilterPassed && cityFilterPassed && minPointFilterPassed && maxPointFilterPassed &&
                minAgeFilterPassed && maxAgeFilterPassed &&
                minNumberMembersFilterPassed && maxNumberMembersFilterPassed
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
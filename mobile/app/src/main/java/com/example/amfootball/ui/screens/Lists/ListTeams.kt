package com.example.amfootball.ui.screens.Lists

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.ExperimentalAnimationApi
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
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ChevronRight
import androidx.compose.material.icons.filled.Groups
import androidx.compose.material.icons.filled.LocationOn
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
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.dtos.Filters.FiltersListTeamDto
import com.example.amfootball.data.dtos.ItemTeamInfoDto
import com.example.amfootball.navigation.Objects.Pages.CrudTeamRoutes
import com.example.amfootball.ui.components.InputFields.LabelTextField
import com.example.amfootball.ui.components.Lists.FilterHeader
import com.example.amfootball.ui.components.Lists.InfoRow
import com.example.amfootball.R

/**
 * TODO: Meter para quando consultar a team aparecer no profile os dados reais da Team
 * TODO: Meter os botões de send MatchInvite caso seja uma Team a aceder há lista
 * TODO: Meter os botões de send MemberShipRequest caso seja um player sem equipa a consultar a lista
 * */
/**
 * Ver como faço para daqui reutilizar a lista, os filtros ver depois
 * */
@Composable
fun ListTeamScreen(navHostController: NavHostController){
    var filters by remember { mutableStateOf(FiltersListTeamDto()) }
    var filtersExpanded by remember { mutableStateOf(false) }

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
                    }
                )
                Spacer(Modifier.height(16.dp))
            }

            items(filteredList) { team ->
                ListTeam(team = team, navHostController = navHostController)
                Spacer(Modifier.height(12.dp))
            }
        }
    }
}

/**
 * Componente que representa a seção dos filtros (Falta meter o botão de filtrar)
 * */
@OptIn(ExperimentalAnimationApi::class)
@Composable
private fun FilterSection(
    isExpanded: Boolean,
    onToggleExpand: () -> Unit,
    filters: FiltersListTeamDto,
    onFiltersChange: (FiltersListTeamDto) -> Unit,
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
                FiltersListTeamContent(
                    filters = filters,
                    onFiltersChange = onFiltersChange,
                    modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp)
                )
            }
        }
    }
}

/**
 * Filtros da secção de Filtros
 * */
@Composable
private fun FiltersListTeamContent(
    filters: FiltersListTeamDto,
    onFiltersChange: (FiltersListTeamDto) -> Unit,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        Row(modifier = Modifier.fillMaxWidth()) {
            LabelTextField(
                label = stringResource(id = R.string.filter_name_team),
                value = filters.name,
                onValueChange = { onFiltersChange(filters.copy(name = it)) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            LabelTextField(
                label = stringResource(id = R.string.filter_city),
                value = filters.city,
                onValueChange = { onFiltersChange(filters.copy(city = it)) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            // TODO: Adicionar Rank SelectBox

            Spacer(Modifier.weight(1f))
            Spacer(Modifier.width(8.dp))
            NumberFilterField(
                label = stringResource(id = R.string.filter_min_points_Team),
                value = filters.minPoint,
                onValueChange = { onFiltersChange(filters.copy(minPoint = it)) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            NumberFilterField(
                label = stringResource(id = R.string.filter_max_points_Team),
                value = filters.maxPoint,
                onValueChange = { onFiltersChange(filters.copy(maxPoint = it)) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            NumberFilterField(
                label = stringResource(id = R.string.filter_min_average_age_team),
                value = filters.minAge,
                onValueChange = { onFiltersChange(filters.copy(minAge = it)) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            NumberFilterField(
                label = stringResource(id = R.string.filter_max_average_age_team),
                value = filters.maxAge,
                onValueChange = { onFiltersChange(filters.copy(maxAge = it)) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            NumberFilterField(
                label = stringResource(id = R.string.filter_min_members_team),
                value = filters.minNumberMembers,
                onValueChange = { onFiltersChange(filters.copy(minNumberMembers = it)) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            NumberFilterField(
                label = stringResource(id = R.string.filter_max_members_team),
                value = filters.maxNumberMembers,
                onValueChange = { onFiltersChange(filters.copy(maxNumberMembers = it)) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            Spacer(Modifier.weight(1f))
        }
    }
}

/**
 * TextField de filtros de numeros
 * */
@Composable
private fun NumberFilterField(
    label: String,
    value: Int?,
    onValueChange: (Int?) -> Unit,
    modifier: Modifier = Modifier
) {
    var textValue by remember(value) {
        mutableStateOf(value?.toString() ?: "")
    }

    LabelTextField(
        label = label,
        value = textValue,
        onValueChange = { newValue ->
            textValue = newValue
            val newIntValue = newValue.toIntOrNull()
            if (newIntValue != null) {
                onValueChange(newIntValue)
            } else if (newValue.isEmpty()) {
                onValueChange(null)
            }
        },
        modifier = modifier,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )
}

@Composable
private fun ListTeam(team: ItemTeamInfoDto, navHostController: NavHostController) {
    ListItem(
        headlineContent = { //Conteudo Principal
            Text(team.Name, style = MaterialTheme.typography.titleLarge)
        },
        overlineContent = { //Aparece em cima do titulo
            Row {
                Text(
                    text = "Rank: ${team.Rank}",
                    style = MaterialTheme.typography.bodyMedium,
                    fontWeight = FontWeight.Bold
                )
                Spacer(Modifier.width(8.dp))
                Text(
                    text = "(${team.Points} Pts)",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.primary
                )
            }
        },
        supportingContent = { //Aparece em baixo (Descrição e cidade)
            Column {
                Spacer(Modifier.height(8.dp))
                InfoRow(
                    icon = Icons.Default.LocationOn,
                    text = team.City
                )

                Spacer(Modifier.height(4.dp))
                InfoRow(
                    icon = Icons.Default.Groups,
                    text = "${team.NumberMembers} ${stringResource(id = R.string.list_teams_members)}"
                )
            }
        },
        leadingContent = { // imagem
            Icon(
                imageVector = Icons.Default.Star,
                contentDescription = "Logo Team",
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(40.dp)
            )
        },
        trailingContent = {  // Tudo que aparece há direita (Botão Ver Detalher + Send MatchInvite no caso da team ou no caso do player send MemberShipRequest)
            IconButton(
                //Depois preciso de adaptar para também mandar os dados da equipa
                onClick = {
                    navHostController.navigate(CrudTeamRoutes.PROFILE_TEAM) {
                        launchSingleTop = true
                    }
                }
            ) {
                Icon(
                    imageVector  = Icons.Default.ChevronRight,
                    contentDescription = stringResource(id = R.string.list_teams_view_team),
                    tint = MaterialTheme.colorScheme.outline
                )
            }
        },
    )
}

/**
 * Criterios de Filtragem
 * */
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
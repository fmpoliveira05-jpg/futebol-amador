package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.R
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.navigation.objects.page.MembershipRequestRoutes
import com.example.amfootball.ui.components.buttons.FilterApplyButton
import com.example.amfootball.ui.components.buttons.ListSendMemberShipRequestButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.lists.AddressRow
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.NumMembersTeamRow
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.TeamConst
import kotlin.String

/**
 * TODO: Quando carregar da API os dados ter em atenção e quando for um tipo de players carregar as teams de uma forma e outro tipo de player de outra
 * TODO: Meter botão de filtragem (ver se filtra localmente ou no servidor)
 * TODO: Meter para quando consultar a team aparecer no profile os dados reais da Team
 * TODO: Meter os botões de send MatchInvite caso seja uma Team a aceder há lista
 * TODO: Meter os botões de send MemberShipRequest caso seja um player sem equipa a consultar a lista
 * */
/**
 * Ver como faço para daqui reutilizar a lista, os filtros ver depois
 * */
@Composable
fun ListTeamScreen(navHostController: NavHostController){
    var filters by remember { mutableStateOf(FiltersListTeam()) }
    var filtersExpanded by remember { mutableStateOf(false) }
    val listRanks = RankNameDto.generateExampleRanks()

    val allTeams = remember { ItemTeamInfoDto.generateExampleTeams() }
    val filteredList = remember(allTeams, filters) {
        filterTeamList(allTeams, filters)
    }

    ListSurface(
        list = allTeams,
        filterSection = {
            FilterSection(
                isExpanded = filtersExpanded,
                onToggleExpand = { filtersExpanded = !filtersExpanded },
                content = {
                    FiltersListTeamContent(
                        filters = filters,
                        onFiltersChange = { filters = it },
                        listRanks = listRanks,
                        modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp)
                    )
                }
            )
        },
        listItems = { team ->
            ListTeam(
                team = team,
                navHostController = navHostController
            )
        },
        messageEmptyList = stringResource(id = R.string.list_teams_empty)
    )
}

/**
 * Filtros da secção de Filtros
 * */
@Composable
private fun FiltersListTeamContent(
    filters: FiltersListTeam,
    onFiltersChange: (FiltersListTeam) -> Unit,
    listRanks: List<RankNameDto>,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_name_team),
                    value = filters.name,
                    onValueChange = { onFiltersChange(filters.copy(name = it)) },
                    maxLenght = TeamConst.MAX_NAME_LENGTH,
                    modifier = Modifier.weight(1f)
                )
                LabelTextField(
                    label = stringResource(id = R.string.filter_city),
                    value = filters.city,
                    maxLenght = GeneralConst.MAX_CITY_LENGTH,
                    onValueChange = { onFiltersChange(filters.copy(city = it)) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        val selectedRankDto = listRanks.find { it.name == filters.rank }
        FilterRow(
            content = {
                // TODO: Modificar a selectBox do rank para que os ranks sejam carregados da BD
                LabelSelectBox(
                    label = "Rank",
                    list = listRanks,
                    selectedValue = selectedRankDto ?: listRanks.first(),
                    itemToString = { rankDto ->
                        rankDto.name
                    },
                    onSelectItem = { rankDto ->
                        onFiltersChange(filters.copy(rank = rankDto.name))
                    },
                    modifier = Modifier.weight(1f)
                )

                NumberFilterField(
                    label = stringResource(id = R.string.filter_min_points_Team),
                    value = filters.minPoint,
                    onValueChange = { onFiltersChange(filters.copy(minPoint = it)) },
                    min = TeamConst.MIN_NUMBER_POINTS,
                    max = TeamConst.MAX_NUMBER_POINTS,
                    modifier = Modifier.weight(1f)
                )
            }
        )
        
        FilterRow(
            content = {
                NumberFilterField(
                    label = stringResource(id = R.string.filter_max_points_Team),
                    value = filters.maxPoint,
                    min = TeamConst.MIN_NUMBER_POINTS,
                    max = TeamConst.MAX_NUMBER_POINTS,
                    onValueChange = { onFiltersChange(filters.copy(maxPoint = it)) },
                    modifier = Modifier.weight(1f)
                )

                NumberFilterField(
                    label = stringResource(id = R.string.filter_min_average_age_team),
                    value = filters.minAge,
                    min = TeamConst.MIN_AVERAGE_AGE,
                    max = TeamConst.MAX_AVERAGE_AGE,
                    onValueChange = { onFiltersChange(filters.copy(minAge = it)) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
           content = {
               NumberFilterField(
                   label = stringResource(id = R.string.filter_max_average_age_team),
                   value = filters.maxAge,
                   min = TeamConst.MIN_AVERAGE_AGE,
                   max = TeamConst.MAX_AVERAGE_AGE,
                   onValueChange = { onFiltersChange(filters.copy(maxAge = it)) },
                   modifier = Modifier.weight(1f)
               )

               NumberFilterField(
                   label = stringResource(id = R.string.filter_min_members_team),
                   value = filters.minNumberMembers,
                   min = TeamConst.MIN_MEMBERS,
                   max = TeamConst.MAX_MEMBERS,
                   onValueChange = { onFiltersChange(filters.copy(minNumberMembers = it)) },
                   modifier = Modifier.weight(1f)
               )
           }
        )

        FilterRow(
            content = {
                NumberFilterField(
                    label = stringResource(id = R.string.filter_max_members_team),
                    value = filters.maxNumberMembers,
                    min = TeamConst.MIN_MEMBERS,
                    max = TeamConst.MAX_MEMBERS,
                    onValueChange = { onFiltersChange(filters.copy(maxNumberMembers = it)) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterApplyButton(
            onClick = {
                // TODO: Fazer o pedido ao endpoint com os 'filters'
            },
            modifier = Modifier.fillMaxWidth()
        )
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
    min: Int = 0,
    max: Int = Int.MAX_VALUE,
    modifier: Modifier = Modifier
) {
    val textValue by remember(value) {
        mutableStateOf(value?.toString() ?: "")
    }

    LabelTextField(
        label = label,
        value = textValue,
        onValueChange = { onValueChange(it.toIntOrNull())},
        modifier = modifier,
        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number)
    )
}

@Composable
private fun ListTeam(
    team: ItemTeamInfoDto,
    navHostController: NavHostController
) {
    GenericListItem(
        item = team,
        title = { it.name },
        overline = {
            ListTeamOverline(team = team)
        },
        supporting = {
            AddressRow(address = team.city)
            NumMembersTeamRow(numMembers = team.numberMembers)
        },
        leading = {
            ImageList(image = team.logoTeam)
        },
        trailing = {
            ListTeamTrailing(
                team = team,
                navHostController = navHostController
            )
        }
    )
}

@Composable
private fun ListTeamOverline(team: ItemTeamInfoDto) {
    Text(
        text = buildAnnotatedString {
            pushStyle(SpanStyle(fontWeight = FontWeight.Bold))
            append("Rank: ${team.rank}")
            pop()

            append("  ")

            pushStyle(SpanStyle(color = MaterialTheme.colorScheme.primary))
            append("(${team.points} Pts)")
            pop()
        },
        style = MaterialTheme.typography.bodyMedium,
        maxLines = 1,
        overflow = TextOverflow.Ellipsis
    )
}

@Composable
private fun ListTeamTrailing(
    team: ItemTeamInfoDto,
    navHostController: NavHostController
) {
    val typeUser by remember { mutableStateOf(false) }

    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        ListSendMemberShipRequestButton(
            sendMemberShipRequest = {
                if (typeUser) {
                    navHostController.navigate(route = MembershipRequestRoutes.SEND_MEMBERSHIP_REQUEST) {
                        launchSingleTop = true
                    }
                } else {
                    navHostController.navigate(route = Routes.TeamRoutes.SEND_MATCH_INVITE.route) {
                        launchSingleTop = true
                    }
                }
            }
        )

        ShowMoreInfoButton(
            showMoreDetails = {
                val idTeam = team.id
                navHostController.navigate(route = "${Routes.TeamRoutes.TEAM_PROFILE.route}/{${idTeam}}") {
                    launchSingleTop = true
                }
            }
        )
    }
}

/**
 * Criterios de Filtragem
 * */
private fun filterTeamList(
    teams: List<ItemTeamInfoDto>,
    filters: FiltersListTeam
): List<ItemTeamInfoDto> {
    val minPoints = filters.minPoint
    val maxPoints = filters.maxPoint
    val minAge = filters.minAge
    val maxAge = filters.maxAge
    val minNumberMembers = filters.minNumberMembers
    val maxNumberMembers = filters.maxNumberMembers

    return teams.filter { team ->
        val nameFilterPassed = filters.name.isNullOrEmpty() ||
                team.name.contains(filters.name, ignoreCase = true)

        val rankFilterPassed = filters.rank.isNullOrEmpty() ||
                team.name.contains(filters.rank, ignoreCase = true)

        val cityFilterPassed = filters.city.isNullOrEmpty() ||
                team.city.contains(filters.city, ignoreCase = true)

        val minPointFilterPassed = (minPoints == null) || (team.points >= minPoints)
        val maxPointFilterPassed = (maxPoints == null) || (team.points <= maxPoints)

        val minAgeFilterPassed = (minAge == null) || (team.averageAge >= minAge)
        val maxAgeFilterPassed = (maxAge == null) || (team.averageAge <= maxAge)

        val minNumberMembersFilterPassed = (minNumberMembers == null) || (team.numberMembers >= minNumberMembers)
        val maxNumberMembersFilterPassed = (maxNumberMembers == null) || (team.numberMembers <= maxNumberMembers)

        nameFilterPassed && rankFilterPassed && cityFilterPassed && minPointFilterPassed
                && maxPointFilterPassed && minAgeFilterPassed && maxAgeFilterPassed &&
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
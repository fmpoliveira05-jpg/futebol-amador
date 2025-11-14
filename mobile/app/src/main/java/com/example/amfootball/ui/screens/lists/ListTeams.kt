package com.example.amfootball.ui.screens.lists

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.ExperimentalAnimationApi
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
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Groups
import androidx.compose.material.icons.filled.LocationOn
import androidx.compose.material.icons.filled.Send
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
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
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.dtos.filters.FiltersListTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterHeader
import com.example.amfootball.ui.components.lists.InfoRow
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.FilterTeamActions
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.SendMemberShipRequestButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.buttons.TextButtonModel
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.viewModel.lists.ListTeamViewModel
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
fun ListTeamScreen(
    navHostController: NavHostController,
    viewModel: ListTeamViewModel = viewModel()
) {
    var filtersExpanded by remember { mutableStateOf(false) }
    val listRanks = viewModel.listRank.value
    val listTeam = viewModel.listTeams.value

    val filterStates by viewModel.uiFilterState.collectAsState()

    val filtersActions = FilterTeamActions(
        onNameChange = viewModel::onNameChange,
        onCityChange = viewModel::onCityChange,
        onRankChange = viewModel::onRankChange,
        onMinPointChange = viewModel::onMinPointChange,
        onMaxPointChange = viewModel::onMaxPointChange,
        onMinAgeChange = viewModel::onMinAgeChange,
        onMaxAgeChange = viewModel::onMaxAgeChange,
        onMinNumberMembersChange = viewModel::onMinNumberMembersChange,
        onMaxNumberMembersChange = viewModel::onMaxNumberMembersChange,
        onApplyFilters = viewModel::applyFilters,
        onClearFilters = viewModel::clearFilters
    )

    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            item {
                FilterSection(
                    isExpanded = filtersExpanded,
                    onToggleExpand = { filtersExpanded = !filtersExpanded },
                    state = filterStates,
                    actions = filtersActions,
                    listRanks = listRanks
                )
                Spacer(Modifier.height(16.dp))
            }

            items(listTeam) { team ->
                ListTeam(
                    team = team,
                    onClickSendMembership = { viewModel.sendMemberShipRequest(
                        idTeam = team.Id,
                        navHostController = navHostController
                    ) },
                    onClickSendMatchInvite = { viewModel.sendMatchInvite(
                        idTeam = team.Id,
                        navHostController = navHostController) },
                    showMoreDetails = { viewModel.ShowMore(
                        idTeam = team.Id,
                        navHostController = navHostController
                    ) }
                )
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
    state: FiltersListTeamDto,
    actions: FilterTeamActions,
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

            //Filtros todos
            AnimatedVisibility(visible = isExpanded) {
                FiltersListTeamContent(
                    state = state,
                    actions = actions,
                    listRanks = listRanks,
                    modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
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
    state: FiltersListTeamDto,
    actions: FilterTeamActions,
    listRanks: List<RankNameDto>,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        Row(modifier = Modifier.fillMaxWidth()) {
            LabelTextField(
                label = stringResource(id = R.string.filter_name_team),
                value = state.name,
                onValueChange = { actions.onNameChange(it) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            LabelTextField(
                label = stringResource(id = R.string.filter_city),
                value = state.city,
                onValueChange = { actions.onCityChange(it) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            LabelSelectBox(
                label = "Rank",
                list = listRanks,
                selectedValue = state.rank ?: "",
                itemToString = { rankDto ->
                    rankDto.Name
                },
                onSelectItem = { rankDto ->
                    actions.onRankChange(rankDto.Name)
                },
                modifier = Modifier.weight(1f)
            )

            Spacer(Modifier.weight(1f))
            Spacer(Modifier.width(8.dp))

            NumberFilterField(
                label = stringResource(id = R.string.filter_min_points_Team),
                value = state.minPoint,
                onValueChange = { actions.onMinPointChange(it) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            NumberFilterField(
                label = stringResource(id = R.string.filter_max_points_Team),
                value = state.maxPoint,
                onValueChange = { actions.onMaxPointChange(it) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            NumberFilterField(
                label = stringResource(id = R.string.filter_min_average_age_team),
                value = state.minAge,
                onValueChange = { actions.onMinAgeChange(it) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            NumberFilterField(
                label = stringResource(id = R.string.filter_max_average_age_team),
                value = state.maxAge,
                onValueChange = { actions.onMaxAgeChange(it) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            NumberFilterField(
                label = stringResource(id = R.string.filter_min_members_team),
                value = state.minNumberMembers,
                onValueChange = { actions.onMinNumberMembersChange(it) },
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(Modifier.height(8.dp))

        Row(modifier = Modifier.fillMaxWidth()) {
            NumberFilterField(
                label = stringResource(id = R.string.filter_max_members_team),
                value = state.maxNumberMembers,
                onValueChange = { actions.onMaxNumberMembersChange(it) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
            Spacer(Modifier.weight(1f))
        }

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            onApplyFiltersClick = actions.onApplyFilters,
            onClearFilters = actions.onClearFilters,
            modifier = Modifier.weight(1f)
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

//Passar metodos
@Composable
private fun ListTeam(
    team: ItemTeamInfoDto,
    onClickSendMembership: () -> Unit,
    onClickSendMatchInvite: () -> Unit,
    showMoreDetails: () -> Unit
) {
    ListItem(
        headlineContent = { //Conteudo Principal
            Text(
                text= team.Name,
                style = MaterialTheme.typography.titleLarge,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis)
        },
        overlineContent = { //Aparece em cima do titulo
            Text(
                text = buildAnnotatedString {
                    pushStyle(SpanStyle(fontWeight = FontWeight.Bold))
                    append("Rank: ${team.Rank}")
                    pop()

                    append("  ")

                    pushStyle(SpanStyle(color = MaterialTheme.colorScheme.primary))
                    append("(${team.Points} Pts)")
                    pop()
                },
                style = MaterialTheme.typography.bodyMedium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        },
        supportingContent = { //Aparece em baixo (Descrição e cidade)
            SupportingContent(team = team)
        },
        leadingContent = { // imagem
            Icon(
                imageVector = Icons.Default.Star,
                contentDescription = "Logo Team",
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(40.dp)
            )
        },
        trailingContent = { // Tudo que aparece há direita (Botão Ver Detalher + Send MatchInvite no caso da team ou no caso do player send MemberShipRequest)
            TrailingContent(
                onClickSendMembership = onClickSendMembership,
                onClickSendMatchInvite = onClickSendMatchInvite,
                showMoreDetails = showMoreDetails
            )
        },
    )
}

@Composable
private fun SupportingContent(
    team: ItemTeamInfoDto
) {
    Column {
        Spacer(Modifier.height(8.dp))
        InfoRow(
            icon = Icons.Default.LocationOn,
            text = team.City,
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(Modifier.height(4.dp))
        InfoRow(
            icon = Icons.Default.Groups,
            text = "${team.NumberMembers} ${stringResource(id = R.string.list_teams_members)}"
        )
    }
}
@Composable
private fun TrailingContent(
    onClickSendMembership: () -> Unit,
    onClickSendMatchInvite: () -> Unit,
    showMoreDetails: () -> Unit
) {
    val typeUser by remember { mutableStateOf(false) }

    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        if(typeUser) {
            SendMemberShipRequestButton(
                onClickSendMembership = { onClickSendMembership() }
            )
        } else {
            TextButtonModel(
                onClick = { onClickSendMatchInvite() },
                imageVector = Icons.Default.Send,
                contentDescription = "Send Match Invite",
                text = "Match Invite",
            )
        }

        ShowMoreInfoButton(
            showMoreDetails = { showMoreDetails() }
        )
    }
}

@Preview(
    name = "Lista de Equipa da equipa - PT",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "List Team Screen of team- EN",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewListTeamScreenTeam() {
    ListTeamScreen(rememberNavController())
}

@Preview(
    name = "Lista de Equipa de jogador - PT",
    locale = "pt-rPT",
    showBackground = true)
@Preview(
    name = "List Team Screen of player- EN",
    locale = "en",
    showBackground = true)
@Composable
fun PreviewListTeamScreenPlayer() {
    ListTeamScreen(rememberNavController())
}
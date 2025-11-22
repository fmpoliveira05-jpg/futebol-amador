package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.padding
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
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterTeamActions
import com.example.amfootball.data.actions.itemsList.ItemsListTeamAction
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.ListSendMemberShipRequestButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.NumberTextField
import com.example.amfootball.ui.components.lists.AddressRow
import com.example.amfootball.ui.components.lists.FilterNameTeamTextField
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.NumMembersTeamRow
import com.example.amfootball.ui.viewModel.lists.ListTeamViewModel
import com.example.amfootball.utils.GeneralConst
import com.example.amfootball.utils.TeamConst

/**
 * TODO: Quando carregar da API os dados ter em atenção e quando for um tipo de players carregar as teams de uma forma e outro tipo de player de outra
 * TODO: Meter botão de filtragem (ver se filtra localmente ou no servidor)
 * TODO: Meter para quando consultar a team aparecer no profile os dados reais da Team
 * TODO: Meter os botões de send MatchInvite caso seja uma Team a aceder há lista
 * TODO: Meter os botões de send MemberShipRequest caso seja um player sem equipa a consultar a lista
 * */

/**
 * Ecrã principal para a listagem de equipas de Futebol Americano.
 *
 * Este Composable é responsável por:
 * 1. Coletar o estado da UI do ViewModel (filtros, lista de equipas e ranks).
 * 2. Configurar as ações de filtragem e interação com os itens da lista.
 * 3. Gerir a exibição da secção de filtros (expandido/colapsado).
 * 4. Renderizar a lista utilizando o componente genérico [ListSurface].
 *
 * @param navHostController Controlador de navegação para transitar entre ecrãs.
 * @param viewModel O ViewModel responsável pela lógica de negócio e estado da lista.
 */
@Composable
fun ListTeamScreen(
    navHostController: NavHostController,
    viewModel: ListTeamViewModel = viewModel()
){
    val filters by viewModel.uiFilterState.collectAsStateWithLifecycle()
    val listTeams by viewModel.listTeams.collectAsStateWithLifecycle()
    val listRanks by viewModel.listRank.collectAsStateWithLifecycle()
    /*
    Isto vou meter uma variavel no viewModel para guardar offline os filtros da pessoa
    val filteredList = remember(listTeams, filters) {
        filterTeamList(listTeams, filters)
    }
    * */

    // Definição das ações de atualização dos filtros
    val filtersActions = FilterTeamActions(
        onNameChange = viewModel::onNameChange,
        onCityChange = viewModel::onCityChange,
        onMinAgeChange = viewModel::onMinAgeChange,
        onMaxAgeChange = viewModel::onMaxAgeChange,
        onMinPointChange = viewModel::onMinPointChange,
        onMaxPointChange = viewModel::onMaxPointChange,
        onMinNumberMembersChange = viewModel::onMinNumberMembersChange,
        onMaxNumberMembersChange = viewModel::onMaxNumberMembersChange,
        onRankChange = viewModel::onRankChange,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::applyFilters,
            onFilterClean = viewModel::clearFilters
        )
    )

    // Definição das ações para cada item da lista (equipa)
    val itemListActions = ItemsListTeamAction(
        onSendMemberShipRequest = viewModel::sendMemberShipRequest,
        onSendMatchInvite = viewModel::sendMatchInvite,
        onShowMore = viewModel::showMore
    )

    var filtersExpanded by remember { mutableStateOf(false) }

    ListSurface(
        list = listTeams,
        filterSection = {
            FilterSection(
                isExpanded = filtersExpanded,
                onToggleExpand = { filtersExpanded = !filtersExpanded },
                content = {
                    FiltersListTeamContent(
                        filters = filters,
                        filtersActions = filtersActions,
                        listRanks = listRanks,
                        modifier = Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp)
                    )
                }
            )
        },
        listItems = { team ->
            ListTeam(
                team = team,
                itemActions = itemListActions,
                navHostController = navHostController
            )
        },
        messageEmptyList = stringResource(id = R.string.list_teams_empty)
    )
}

/**
 * Conteúdo interno da secção de filtros para equipas.
 *
 * Apresenta campos para filtrar por:
 * - Nome e Cidade.
 * - Rank (via SelectBox).
 * - Intervalos de Pontos (Min/Max).
 * - Intervalos de Idade Média (Min/Max).
 * - Intervalos de Número de Membros (Min/Max).
 *
 * @param filters Estado atual dos filtros [FiltersListTeam].
 * @param filtersActions Callbacks para atualizar os valores dos filtros.
 * @param listRanks Lista de Ranks disponíveis para seleção.
 * @param modifier Modificador de layout.
 */
@Composable
private fun FiltersListTeamContent(
    filters: FiltersListTeam,
    filtersActions: FilterTeamActions,
    listRanks: List<RankNameDto>,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        FilterRow(
            content = {
                FilterNameTeamTextField(
                    nameTeam = filters.name,
                    onNameTeamChange = { filtersActions.onNameChange(it) },
                    isError = false,
                    errorMessage = "",
                    modifier = Modifier.weight(1f)
                )

                LabelTextField(
                    label = stringResource(id = R.string.filter_city),
                    value = filters.city,
                    maxLenght = GeneralConst.MAX_CITY_LENGTH,
                    onValueChange = { filtersActions.onCityChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        val selectedRankDto = listRanks.find { it.name == filters.rank }
        FilterRow(
            content = {
                LabelSelectBox(
                    label = "Rank",
                    list = listRanks,
                    selectedValue = selectedRankDto ?: listRanks.first(),
                    itemToString = { it.name },
                    onSelectItem = { filtersActions.onRankChange(it.name)},
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                NumberTextField(
                    label = stringResource(id = R.string.filter_min_points_Team),
                    value = filters.minPoint,
                    onValueChange = { filtersActions.onMinPointChange(it) },
                    min = TeamConst.MIN_NUMBER_POINTS,
                    max = TeamConst.MAX_NUMBER_POINTS,
                    modifier = Modifier.weight(1f)
                )

                NumberTextField(
                    label = stringResource(id = R.string.filter_max_points_Team),
                    value = filters.maxPoint,
                    min = TeamConst.MIN_NUMBER_POINTS,
                    max = TeamConst.MAX_NUMBER_POINTS,
                    onValueChange = { filtersActions.onMaxPointChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )
        
        FilterRow(
            content = {
                NumberTextField(
                    label = stringResource(id = R.string.filter_min_average_age_team),
                    value = filters.minAge,
                    min = TeamConst.MIN_AVERAGE_AGE,
                    max = TeamConst.MAX_AVERAGE_AGE,
                    onValueChange = { filtersActions.onMinAgeChange(it) },
                    modifier = Modifier.weight(1f)
                )

                NumberTextField(
                    label = stringResource(id = R.string.filter_max_average_age_team),
                    value = filters.maxAge,
                    min = TeamConst.MIN_AVERAGE_AGE,
                    max = TeamConst.MAX_AVERAGE_AGE,
                    onValueChange = { filtersActions.onMaxAgeChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
           content = {
               NumberTextField(
                   label = stringResource(id = R.string.filter_min_members_team),
                   value = filters.minNumberMembers,
                   min = TeamConst.MIN_MEMBERS,
                   max = TeamConst.MAX_MEMBERS,
                   onValueChange = { filtersActions.onMinNumberMembersChange(it) },
                   modifier = Modifier.weight(1f)
               )

               NumberTextField(
                   label = stringResource(id = R.string.filter_max_members_team),
                   value = filters.maxNumberMembers,
                   min = TeamConst.MIN_MEMBERS,
                   max = TeamConst.MAX_MEMBERS,
                   onValueChange = { filtersActions.onMaxNumberMembersChange(it) },
                   modifier = Modifier.weight(1f)
               )
           }
        )

        LineClearFilterButtons(
            buttonsActions = filtersActions.buttonActions,
            modifier = Modifier.weight(1f)
        )
    }
}

/**
 * Representação individual de uma Equipa na lista.
 *
 * Utiliza [GenericListItem] para estruturar a informação:
 * - Logo (Leading)
 * - Nome (Title)
 * - Rank e Pontos (Overline)
 * - Cidade e Nº de Membros (Supporting)
 * - Botões de Ação (Trailing)
 *
 * @param team DTO contendo as informações da equipa.
 * @param itemActions Ações disponíveis para este item (convidar, ver mais).
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun ListTeam(
    team: ItemTeamInfoDto,
    itemActions: ItemsListTeamAction,
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
                itemActions = itemActions,
                navHostController = navHostController
            )
        }
    )
}

/**
 * Componente para exibir o texto de topo (Overline) do item da lista.
 *
 * Formata o texto usando [buildAnnotatedString]:
 * - "Rank: [Valor]" em Negrito.
 * - "([Valor] Pts)" na cor primária do tema.
 */
@Composable
private fun ListTeamOverline(team: ItemTeamInfoDto) {
    Text(
        text = buildAnnotatedString {
            pushStyle(SpanStyle(fontWeight = FontWeight.Bold))
            append("Rank: ${team.rank}")
            pop()

            append("\n")

            pushStyle(SpanStyle(color = MaterialTheme.colorScheme.primary))
            append("(${team.points} Pts)")
            pop()
        },
        style = MaterialTheme.typography.bodyMedium,
        overflow = TextOverflow.Ellipsis
    )
}

/**
 * Componente lateral direito (Trailing) do item da lista.
 *
 * Contém os botões de ação rápida:
 * 1. Enviar pedido de adesão ou convite de jogo (dependendo do [typeUser]).
 * 2. Ver mais detalhes da equipa.
 *
 * @param team A equipa associada.
 * @param itemActions As ações a serem executadas.
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun ListTeamTrailing(
    team: ItemTeamInfoDto,
    itemActions: ItemsListTeamAction,
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
                    itemActions.onSendMemberShipRequest(team.id, navHostController)
                } else {
                    itemActions.onSendMatchInvite(team.id, navHostController)
                }
            }
        )

        ShowMoreInfoButton(
            showMoreDetails = {
                itemActions.onShowMore(team.id, navHostController)
            }
        )
    }
}

/**
 * Criterios de Filtragem
 *
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
 */


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
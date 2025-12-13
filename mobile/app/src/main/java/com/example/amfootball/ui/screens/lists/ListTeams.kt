package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
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
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.core.utils.GeneralConst
import com.example.amfootball.core.utils.TeamConst
import com.example.amfootball.data.events.UiState
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.data.remote.dtos.rank.RankNameDto
import com.example.amfootball.data.remote.dtos.team.ItemTeamInfoDto
import com.example.amfootball.domains.enums.UserRole
import com.example.amfootball.domains.errors.filtersError.FilterTeamError
import com.example.amfootball.ui.actions.filters.ButtonFilterActions
import com.example.amfootball.ui.actions.filters.FilterTeamActions
import com.example.amfootball.ui.actions.itemsList.ItemsListTeamAction
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.ListSendMemberShipRequestButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.inputFields.NumberTextField
import com.example.amfootball.ui.components.lists.AddressRow
import com.example.amfootball.ui.components.lists.AverageAgeRow
import com.example.amfootball.ui.components.lists.FilterNameTeamTextField
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.NumMembersTeamRow
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.navigation.objects.Routes
import com.example.amfootball.ui.previewsMocks.ListTeamMocks
import com.example.amfootball.ui.viewModel.lists.ListTeamViewModel

//TODO: Meter Botão Ver Mais
/**
 * Ecrã principal para a listagem de equipas de Futebol Americano (Stateful Screen).
 *
 * Este Composable atua como o contentor de estado:
 * 1. Instancia e coleta os fluxos ([FiltersListTeam], [ItemTeamInfoDto], [UiState]) do [ListTeamViewModel].
 * 2. Define as ações ([FilterTeamActions], [ItemsListTeamAction]) que ligam a UI à lógica de negócio.
 * 3. Passa os dados puros para o [ListTeamContent] renderizar.
 *
 * @param navHostController Controlador de navegação para transitar entre ecrãs.
 * @param viewModel O ViewModel injetado via Hilt.
 */
@Composable
fun ListTeamScreen(
    navHostController: NavHostController,
    viewModel: ListTeamViewModel = hiltViewModel()
) {
    val filters by viewModel.uiFilterState.collectAsStateWithLifecycle()
    val filtersError by viewModel.filterError.collectAsStateWithLifecycle()
    val listTeams by viewModel.uiList.collectAsStateWithLifecycle()
    val listRanks by viewModel.listRank.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val role by viewModel.role.collectAsStateWithLifecycle()
    val sentRequests by viewModel.sentRequestIds.collectAsStateWithLifecycle()

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

    val itemListActions = ItemsListTeamAction(
        onSendMemberShipRequest = viewModel::sendMemberShipRequest,
        onSendMatchInvite = { idTeam, nameTeam ->
            navHostController.navigate(route = "${Routes.TeamRoutes.SEND_MATCH_INVITE.route}/${idTeam}/${nameTeam}") {
                launchSingleTop = true
            }
        },
        onShowMore = { idTeam ->
            navHostController.navigate(route = "${Routes.TeamRoutes.TEAM_PROFILE.route}/$idTeam") {
                launchSingleTop = true
            }
        }
    )

    ListTeamContent(
        isOnline = isOnline,
        listTeams = listTeams,
        filters = filters,
        filtersActions = filtersActions,
        listRanks = listRanks,
        itemListActions = itemListActions,
        uiState = uiState,
        onRetry = { viewModel.retry() },
        role = role,
        filtersError = filtersError,
        navHostController = navHostController,
        sentRequests = sentRequests
    )
}

/**
 * Conteúdo Visual da Lista de Equipas (Stateless Content).
 *
 * Responsável apenas pela renderização da UI. Envolve a lista com um [LoadingPage] para
 * gerir estados de UI e um [OfflineBanner].
 *
 * @param isOnline Estado da conectividade (controla o [OfflineBanner]).
 * @param listTeams Lista de equipas a exibir.
 * @param filters Estado atual dos filtros.
 * @param filtersActions Ações de manipulação dos filtros.
 * @param listRanks Lista de Ranks para o dropdown.
 * @param itemListActions Ações de interação com os itens da lista.
 * @param uiState Estado de loading e erro.
 * @param role O papel do utilizador logado.
 * @param onRetry Callback para tentar recarregar os dados em caso de erro.
 * @param navHostController Controlador de navegação.
 * @param sentRequests Conjunto de IDs de equipas para as quais já existe um pedido de adesão pendente.
 */
@Composable
private fun ListTeamContent(
    isOnline: Boolean,
    listTeams: List<ItemTeamInfoDto>,
    filters: FiltersListTeam,
    filtersError: FilterTeamError,
    filtersActions: FilterTeamActions,
    listRanks: List<RankNameDto>,
    itemListActions: ItemsListTeamAction,
    uiState: UiState,
    role: UserRole,
    onRetry: () -> Unit,
    navHostController: NavHostController,
    sentRequests: Set<String>
) {
    var filtersExpanded by remember { mutableStateOf(false) }
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = onRetry,
        content = {
            Column(modifier = Modifier.fillMaxSize()) {
                OfflineBanner(isVisible = !isOnline)

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
                                    filtersError = filtersError,
                                    modifier = Modifier.padding(
                                        start = 16.dp,
                                        end = 16.dp,
                                        bottom = 16.dp
                                    )
                                )
                            }
                        )
                    },
                    listItems = { team ->
                        ListTeam(
                            team = team,
                            itemActions = itemListActions,
                            role = role,
                            navHostController = navHostController,
                            sentRequests = sentRequests
                        )
                    },
                    messageEmptyList = stringResource(id = R.string.list_teams_empty)
                )
            }
        }
    )
}

/**
 * Conteúdo interno da secção de filtros para equipas.
 *
 * Agrupa todos os campos de filtro (Nome, Cidade, Rank, Pontos, Idade Média, Membros)
 * e os botões de ação ([LineClearFilterButtons]).
 *
 * @param filters Estado atual dos filtros [FiltersListTeam].
 * @param filtersActions Callbacks para atualizar os valores dos filtros.
 * @param listRanks Lista de Ranks disponíveis para seleção.
 * @param filtersError Erros de validação associados aos campos de filtro.
 * @param modifier Modificador de layout.
 */
@Composable
private fun FiltersListTeamContent(
    filters: FiltersListTeam,
    filtersError: FilterTeamError,
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
                    isError = filtersError.nameError != null,
                    errorMessage = filtersError.nameError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                LabelTextField(
                    label = stringResource(id = R.string.filter_city),
                    value = filters.city,
                    maxLenght = GeneralConst.MAX_CITY_LENGTH,
                    onValueChange = { filtersActions.onCityChange(it) },
                    isError = filtersError.cityError != null,
                    errorMessage = filtersError.cityError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
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
                    onSelectItem = { rankDto ->
                        if (rankDto.id == "0") {
                            filtersActions.onRankChange("")
                        } else {
                            filtersActions.onRankChange(rankDto.name)
                        }
                    },
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
                    isError = filtersError.minPointError != null,
                    errorMessage = filtersError.minPointError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                NumberTextField(
                    label = stringResource(id = R.string.filter_max_points_Team),
                    value = filters.maxPoint,
                    min = TeamConst.MIN_NUMBER_POINTS,
                    max = TeamConst.MAX_NUMBER_POINTS,
                    onValueChange = { filtersActions.onMaxPointChange(it) },
                    isError = filtersError.maxPointError != null,
                    errorMessage = filtersError.maxPointError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
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
                    isError = filtersError.minAgeError != null,
                    errorMessage = filtersError.minAgeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                NumberTextField(
                    label = stringResource(id = R.string.filter_max_average_age_team),
                    value = filters.maxAge,
                    min = TeamConst.MIN_AVERAGE_AGE,
                    max = TeamConst.MAX_AVERAGE_AGE,
                    onValueChange = { filtersActions.onMaxAgeChange(it) },
                    isError = filtersError.maxAgeError != null,
                    errorMessage = filtersError.maxAgeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
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
                    isError = filtersError.minNumberMembersError != null,
                    errorMessage = filtersError.minNumberMembersError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                NumberTextField(
                    label = stringResource(id = R.string.filter_max_members_team),
                    value = filters.maxNumberMembers,
                    min = TeamConst.MIN_MEMBERS,
                    max = TeamConst.MAX_MEMBERS,
                    onValueChange = { filtersActions.onMaxNumberMembersChange(it) },
                    isError = filtersError.maxNumberMembersError != null,
                    errorMessage = filtersError.maxNumberMembersError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
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
 * Representação individual de uma Equipa na lista (GenericListItem).
 *
 * @param team DTO contendo as informações da equipa.
 * @param itemActions Ações disponíveis para este item (convidar, ver mais).
 * @param role O papel do utilizador logado.
 * @param navHostController Controlador de navegação.
 * @param sentRequests Conjunto de IDs de equipas para as quais já existe um pedido de adesão.
 */
@Composable
private fun ListTeam(
    team: ItemTeamInfoDto,
    itemActions: ItemsListTeamAction,
    role: UserRole,
    navHostController: NavHostController,
    sentRequests: Set<String>
) {
    GenericListItem(
        item = team,
        title = { it.name },
        overline = {
            ListTeamOverline(team = team)
        },
        supporting = {
            AddressRow(address = team.city)
            AverageAgeRow(age = team.averageAge)
            NumMembersTeamRow(numMembers = team.numberMembers)
        },
        leading = {
            StringImageList(
                image = team.logoTeam,
                contentDescription = stringResource(
                    id = R.string.logo_team_name,
                    R.string.logo_team,
                    team.name
                )
            )
        },
        trailing = {
            ListTeamTrailing(
                team = team,
                itemActions = itemActions,
                role = role,
                isRequestSent = sentRequests.contains(team.id),
                navHostController = navHostController
            )
        }
    )
}

/**
 * Componente para exibir o texto de topo (Overline) do item da lista.
 *
 * Formata o texto para destacar o Rank e os Pontos da equipa.
 */
@Composable
private fun ListTeamOverline(team: ItemTeamInfoDto) {
    Text(
        text = buildAnnotatedString {
            pushStyle(SpanStyle(fontWeight = FontWeight.Bold))
            append("Rank: ${team.rank.name}")
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
 * Contém os botões de ação rápida (Enviar pedido/convite e Ver mais),
 * com visibilidade dependente do [role] do utilizador e do estado da equipa.
 *
 * @param team A equipa associada.
 * @param itemActions As ações a serem executadas.
 * @param role O papel do utilizador logado.
 * @param navHostController Controlador de navegação.
 * @param isRequestSent Indica se já foi enviado um pedido de adesão a esta equipa.
 */
@Composable
private fun ListTeamTrailing(
    team: ItemTeamInfoDto,
    itemActions: ItemsListTeamAction,
    role: UserRole,
    navHostController: NavHostController,
    isRequestSent: Boolean,
) {
    val idTeam = team.id

    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        val validaRolePlayerWithouTeam = role == UserRole.PLAYER_WITHOUT_TEAM && team.numberMembers < TeamConst.MAX_MEMBERS
        val validateRoleAdminTeam = role == UserRole.ADMIN_TEAM && team.numberMembers >= TeamConst.MIN_MEMBERS_TO_MATCH

        if ((validaRolePlayerWithouTeam || validateRoleAdminTeam) && !isRequestSent) {
            ListSendMemberShipRequestButton(
                sendMemberShipRequest = {
                    when (role) {
                        UserRole.PLAYER_WITHOUT_TEAM -> {
                            itemActions.onSendMemberShipRequest(idTeam)
                        }
                        UserRole.ADMIN_TEAM -> {
                            itemActions.onSendMatchInvite(idTeam, team.name)
                        }
                        else -> {}
                    }
                }
            )
        }

        ShowMoreInfoButton(
            showMoreDetails = {
                itemActions.onShowMore(idTeam)
            }
        )
    }
}

@Preview(name = "1. Administrador de Equipa - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "1. Admin Team - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListTeamContentAdmin() {
    ListTeamContent(
        isOnline = true,
        listTeams = ListTeamMocks.mockTeams,
        filters = FiltersListTeam(),
        filtersActions = ListTeamMocks.mockFiltersActions,
        listRanks = ListTeamMocks.mockRanks,
        itemListActions = ListTeamMocks.mockItemActions,
        uiState = UiState(isLoading = false),
        onRetry = {},
        navHostController = rememberNavController(),
        filtersError = FilterTeamError(),
        role = UserRole.ADMIN_TEAM,
        sentRequests = emptySet()
    )
}

@Preview(name = "2. Jogador sem Equipa - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "2. Player Without Team - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListTeamContentPlayerWithoutTeam() {
    ListTeamContent(
        isOnline = true,
        listTeams = ListTeamMocks.mockTeams,
        filters = FiltersListTeam(),
        filtersActions = ListTeamMocks.mockFiltersActions,
        listRanks = ListTeamMocks.mockRanks,
        itemListActions = ListTeamMocks.mockItemActions,
        uiState = UiState(isLoading = false),
        onRetry = {},
        navHostController = rememberNavController(),
        filtersError = FilterTeamError(),
        role = UserRole.PLAYER_WITHOUT_TEAM,
        sentRequests = emptySet()
    )
}

@Preview(name = "3. Jogador - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "3. Player - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListTeamContentPlayer() {
    ListTeamContent(
        isOnline = true,
        listTeams = ListTeamMocks.mockTeams,
        filters = FiltersListTeam(),
        filtersError = FilterTeamError(),
        filtersActions = ListTeamMocks.mockFiltersActions,
        listRanks = ListTeamMocks.mockRanks,
        itemListActions = ListTeamMocks.mockItemActions,
        uiState = UiState(isLoading = false),
        onRetry = {},
        navHostController = rememberNavController(),
        role = UserRole.MEMBER_TEAM,
        sentRequests = emptySet()
    )
}

@Preview(name = "4. Vazia - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "4. Empty - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListTeamContentEmpty() {
    ListTeamContent(
        isOnline = true,
        listTeams = emptyList(),
        filters = FiltersListTeam(),
        filtersActions = ListTeamMocks.mockFiltersActions,
        listRanks = ListTeamMocks.mockRanks,
        itemListActions = ListTeamMocks.mockItemActions,
        uiState = UiState(isLoading = false),
        onRetry = {},
        navHostController = rememberNavController(),
        filtersError = FilterTeamError(),
        role = UserRole.PLAYER_WITHOUT_TEAM,
        sentRequests = emptySet()
    )
}
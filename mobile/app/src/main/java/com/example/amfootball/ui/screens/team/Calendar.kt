package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.MoreVert
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
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
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterCalendarActions
import com.example.amfootball.data.actions.itemsList.ItemsCalendarActions
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.dtos.support.PitchInfo
import com.example.amfootball.data.dtos.support.TeamStatisticsDto
import com.example.amfootball.data.enums.MatchResult
import com.example.amfootball.data.errors.filtersError.FilterCalendarError
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.MatchActionsMenu
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterIsCompetiveMatch
import com.example.amfootball.ui.components.lists.FilterIsFinishMatch
import com.example.amfootball.ui.components.lists.FilterIsHomeMatch
import com.example.amfootball.ui.components.lists.FilterMaxDateGamePicker
import com.example.amfootball.ui.components.lists.FilterMinDateGamePicker
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.components.notification.ToastHandler
import com.example.amfootball.ui.viewModel.team.CalendarTeamViewModel
import com.example.amfootball.utils.Patterns
import com.example.amfootball.utils.TeamConst
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

/**
 * Ecrã principal do Calendário da Equipa.
 *
 * Responsável por orquestrar o estado da ViewModel, gerir os filtros,
 * exibir a lista de partidas e lidar com erros e estados de carregamento.
 *
 * @param navHostController Controlador de navegação para transições entre ecrãs.
 * @param viewModel ViewModel injetada via Hilt que contém a lógica de negócio.
 */
@Composable
fun CalendarScreen(
    navHostController: NavHostController,
    viewModel: CalendarTeamViewModel = hiltViewModel()
) {
    val filters by viewModel.filter.collectAsStateWithLifecycle()
    val list by viewModel.list.collectAsStateWithLifecycle()
    val filterError by viewModel.uiErrors.collectAsStateWithLifecycle()
    val filterActions = FilterCalendarActions(
        onNameChange = viewModel::onNameChange,
        onMinDateGameChange = viewModel::onMinDateGameChange,
        onMaxDateGameChange = viewModel::onMaxDateGameChange,
        onGameLocalChange = viewModel::onGameLocalChange,
        onTypeMatchChange = viewModel::onTypeMatchChange,
        onFinishMatch = viewModel::onIsFinishedChange,
        onButtonFilterActions = ButtonFilterActions(
            onFilterApply = viewModel::onApplyFilter,
            onFilterClean = viewModel::onClearFilter
        )
    )

    val itemsListAction = ItemsCalendarActions(
        onCancelMatch = viewModel::onCancelMatch,
        onPostPoneMatch = viewModel::onPostPoneMatch,
        onStartMatch = viewModel::onStartMatch,
        onFinishMatch = viewModel::onFinishMatch
    )

    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    ToastHandler(
        toastMessage = uiState.toastMessage,
        onToastShown = viewModel::onToastShown
    )

    CalendarContent(
        list = list,
        filters = filters,
        filterActions = filterActions,
        filterError = filterError,
        uiState = uiState,
        retry = viewModel::loadCalendar,
        isOnline = isOnline,
        itemsListAction = itemsListAction,
        navHostController = navHostController
    )
}

/**
 * Conteúdo visual do ecrã de calendário.
 *
 * Exibe o banner offline, a secção de filtros (expansível) e a lista de partidas.
 */
@Composable
private fun CalendarContent(
    list: List<InfoMatchCalendar>,
    filters: FilterCalendar,
    filterActions: FilterCalendarActions,
    filterError: FilterCalendarError,
    uiState: UiState,
    retry: () -> Unit,
    isOnline: Boolean,
    itemsListAction: ItemsCalendarActions,
    navHostController: NavHostController
) {
    var isExpanded by remember { mutableStateOf(false) }

    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = retry,
        content = {
            OfflineBanner(isVisible = !isOnline)

            ListSurface(
                list = list,
                filterSection = {
                    FilterSection(
                        isExpanded = isExpanded,
                        onToggleExpand = { isExpanded = !isExpanded },
                        content = { paddingModifier ->
                            FiltersCalendarContent(
                                filters = filters,
                                filterActions = filterActions,
                                filterError = filterError,
                                modifier = paddingModifier,
                            )
                        }
                    )
                },
                listItems = { match ->
                    ListMatchCalendarItem(
                        match = match,
                        itemsListAction = itemsListAction,
                        navHostController = navHostController,
                        modifier = Modifier.fillMaxWidth()
                    )
                },
            )
        }
    )
}

/**
 * Painel que contém os campos de filtro do calendário.
 */
@Composable
private fun FiltersCalendarContent(
    filters: FilterCalendar,
    filterActions: FilterCalendarActions,
    filterError: FilterCalendarError,
    modifier: Modifier = Modifier
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_name_team),
                    value = filters.opponentName,
                    maxLenght = TeamConst.MAX_NAME_LENGTH,
                    onValueChange = { filterActions.onNameChange(it) },
                    modifier = Modifier.weight(1f),
                    isError = filterError.opponentNameError != null,
                    errorMessage = filterError.opponentNameError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    }
                )

                FilterIsHomeMatch(
                    selectedValue = filters.isHome,
                    onSelectItem = { filterActions.onGameLocalChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterMinDateGamePicker(
                    minDateGame = filters.minGameDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDateGameChange(it) },
                    isError = filterError.minGameDateError != null,
                    errorMessage = filterError.minGameDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMaxDateGamePicker(
                    maxDateGame = filters.maxGameDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDateGameChange(it)},
                    isError = filterError.minGameDateError != null,
                    errorMessage = filterError.maxGameDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterIsCompetiveMatch(
                    selectedValue = filters.typeMatch,
                    onSelectItem = { filterActions.onTypeMatchChange(it) },
                    modifier = Modifier.weight(1f)
                )

                FilterIsFinishMatch(
                    selectedValue = filters.isFinish,
                    onSelectItem = { filterActions.onFinishMatch(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        LineClearFilterButtons(
            buttonsActions = filterActions.onButtonFilterActions,
            modifier = Modifier.weight(1f)
        )
    }
}

/**
 * Item individual da lista que representa uma partida (Card).
 */
@Composable
private fun ListMatchCalendarItem(
    match: InfoMatchCalendar,
    itemsListAction: ItemsCalendarActions,
    navHostController: NavHostController,
    modifier: Modifier = Modifier
) {
    ElevatedCard(modifier = modifier) {
        Column(
            modifier = Modifier.padding(12.dp)
        ) {
            TitleMatchCalendar(
                match = match,
                itensListAction = itemsListAction,
                navHostController = navHostController
            )

            Spacer(Modifier.height(8.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                ColumnTeams(
                    match = match,
                    modifier = Modifier.weight(2.5f) // Mais espaço para nomes longos
                )

                Spacer(modifier = Modifier.width(8.dp)) // Espaço entre colunas

                ColumnDataMatch(
                    match = match,
                    modifier = Modifier.weight(1.3f) // Espaço suficiente para a data não quebrar
                )
            }
        }
    }
}

/**
 * Cabeçalho do Card da partida: Tipo de Jogo e Menu de Opções.
 */
@Composable
private fun TitleMatchCalendar(
    match: InfoMatchCalendar,
    itensListAction: ItemsCalendarActions,
    navHostController: NavHostController
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = stringResource(match.typeMatch.stringId),
            style = MaterialTheme.typography.labelMedium,
            color = MaterialTheme.colorScheme.primary,
            modifier = Modifier.weight(1f)
        )

        OptionsMatch(
            match = match,
            itensListAction = itensListAction,
            navHostController = navHostController
        )
    }
}

/**
 * Menu dropdown com ações disponíveis para a partida (Iniciar, Terminar, Adiar, Cancelar).
 */
@Composable
private fun OptionsMatch(
    match: InfoMatchCalendar,
    itensListAction: ItemsCalendarActions,
    navHostController: NavHostController
) {
    var isMenuExpanded by remember { mutableStateOf(false) }
    Box {
        IconButton(onClick = { isMenuExpanded = !isMenuExpanded }) {
            Icon(
                imageVector = Icons.Default.MoreVert,
                contentDescription = "Opções"
            )
        }

        MatchActionsMenu(
            expanded = isMenuExpanded,
            onDismissRequest = { isMenuExpanded = false },
            onStartMatch = {
                itensListAction.onStartMatch(match.idMatch)
            },
            onFinishMatch = {
                itensListAction.onFinishMatch(
                    match.idMatch,
                    navHostController
                )
            },
            onPostPoneMatch = {
                itensListAction.onPostPoneMatch(
                    match.idMatch,
                    navHostController
                )
            },
            onCancelMatch = {
                itensListAction.onCancelMatch(
                    match.idMatch,
                    navHostController
                )
            }
        )
    }
}

/**
 * Coluna com as informações das duas equipas (Nome, Imagem, Golos).
 */
@Composable
private fun ColumnTeams(
    match: InfoMatchCalendar,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.spacedBy(4.dp)
    ) {
        TeamInfoRow(
            team = match.team,
            matchResult = match.matchResult,
            image = match.team.image
        )

        TeamInfoRow(
            team = match.opponent,
            matchResult = match.matchResult,
            image = match.opponent.image
        )
    }
}

/**
 * Coluna com a data do jogo e estado textual.
 */
@Composable
private fun ColumnDataMatch(
    match: InfoMatchCalendar,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.End
    ) {
        Text(
            text = stringResource(id = match.matchStatus.stringId),
            style = MaterialTheme.typography.labelMedium
        )

        Text(
            text = match.formattedDate,
            style = MaterialTheme.typography.bodyMedium)
    }
}

/**
 * Linha que exibe imagem e nome de uma equipa, e os golos (se aplicável).
 */
@Composable
private fun TeamInfoRow(
    team: TeamStatisticsDto,
    matchResult: MatchResult,
    image: String? = ""
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        StringImageList(
            image = image,
        )

        Text(
            text = team.name,
            style = MaterialTheme.typography.bodyMedium,
            maxLines = 1,
            overflow = TextOverflow.Ellipsis,
            modifier = Modifier.weight(1f)
        )

        if (matchResult != MatchResult.UNDEFINED) {
            Text(
                text = team.numGoals.toString(),
                style = MaterialTheme.typography.titleMedium,
                fontWeight = androidx.compose.ui.text.font.FontWeight.Bold,
                modifier = Modifier.padding(start = 8.dp)
            )
        }
    }
}

// ----------------------------------------------------------------
// MOCK DATA & PREVIEWS
// ----------------------------------------------------------------

private val mockFilterActions = FilterCalendarActions(
    onNameChange = {},
    onMinDateGameChange = {},
    onMaxDateGameChange = {},
    onGameLocalChange = {},
    onTypeMatchChange = {},
    onFinishMatch = {},
    onButtonFilterActions = ButtonFilterActions(
        onFilterApply = {},
        onFilterClean = {}
    )
)

private val mockItemActions = ItemsCalendarActions(
    onCancelMatch = { _, _ -> },
    onPostPoneMatch = { _, _ -> },
    onStartMatch = {},
    onFinishMatch = { _, _ -> }
)

private val mockCalendarList: List<InfoMatchCalendar>
    get() = listOf(
        InfoMatchCalendar(
            idMatch = "1",
            matchStatusId = 2, // 2 = DONE
            rawGameDate = LocalDateTime.now().minusDays(3).toString(), // String
            typeMatchBool = true, // Competitivo
            matchResultId = 0, // Vitória
            pitchGame = PitchInfo(
                name = "Estádio Municipal",
                address = "Rua Principal",
            ),
            team = TeamStatisticsDto(
                idTeam = "t1",
                name = "Minha Equipa",
                image = "",
                numGoals = 3,
            ),
            opponent = TeamStatisticsDto(
                idTeam = "t2",
                name = "Dragões FC",
                image = "",
                numGoals = 1,
            ),
            isHome = false
        ),

        InfoMatchCalendar(
            idMatch = "2",
            matchStatusId = 0, // 0 = SCHEDULED
            rawGameDate = LocalDateTime.now().plusDays(5).toString(), // String
            typeMatchBool = false,
            matchResultId = -1, // Undefined
            pitchGame = PitchInfo(
                name = "Campo de Treinos",
                address = "Avenida Secundária",
            ),
            team = TeamStatisticsDto(
                idTeam = "t1",
                name = "Minha Equipa",
                image = "",
                numGoals = 0,
            ),
            opponent = TeamStatisticsDto(
                idTeam = "t3",
                name = "Leões da Serra",
                image = "",
                numGoals = 0,
            ),
            isHome = true
        )
    )

@Preview(name = "1. Lista Normal - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewCalendarContentNormal() {
    CalendarContent(
        list = mockCalendarList,
        filters = FilterCalendar(),
        filterActions = mockFilterActions,
        filterError = FilterCalendarError(),
        uiState = UiState(isLoading = false),
        isOnline = true,
        retry = {},
        itemsListAction = mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "2. Lista Vazia - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewCalendarContentEmpty() {
    CalendarContent(
        list = emptyList(),
        filters = FilterCalendar(),
        filterActions = mockFilterActions,
        filterError = FilterCalendarError(),
        uiState = UiState(isLoading = false),
        isOnline = true,
        retry = {},
        itemsListAction = mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "3. Offline Mode - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewCalendarContentOffline() {
    CalendarContent(
        list = mockCalendarList,
        filters = FilterCalendar(),
        filterActions = mockFilterActions,
        filterError = FilterCalendarError(),
        uiState = UiState(isLoading = false),
        isOnline = false,
        retry = {},
        itemsListAction = mockItemActions,
        navHostController = rememberNavController()
    )
}
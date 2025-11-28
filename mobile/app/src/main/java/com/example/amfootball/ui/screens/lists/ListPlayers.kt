package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPlayersActions
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.errors.filtersError.FilterPlayersErrors
import com.example.amfootball.data.mocks.Lists.ListPlayers
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.ListSendMemberShipRequestButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.AddressRow
import com.example.amfootball.ui.components.lists.AgeRow
import com.example.amfootball.ui.components.lists.FilterCityTextField
import com.example.amfootball.ui.components.lists.FilterListPosition
import com.example.amfootball.ui.components.lists.FilterMaxAgeTextField
import com.example.amfootball.ui.components.lists.FilterMinAgeTextField
import com.example.amfootball.ui.components.lists.FilterNamePlayerTextField
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PositionRow
import com.example.amfootball.ui.components.lists.SizeRow
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.theme.AMFootballTheme
import com.example.amfootball.ui.viewModel.lists.ListPlayerViewModel
import com.example.amfootball.utils.PlayerConst

/**
 * Ecrã principal de Listagem de Jogadores.
 *
 * É um contentor "Stateful" que interage com o [ListPlayerViewModel] para obter dados,
 * gerir o estado da UI e processar eventos de navegação.
 *
 * @param navHostController Controlador de navegação para transitar para o perfil do jogador.
 * @param viewModel ViewModel injetada via Hilt que contém toda a lógica de negócio e estados.
 */
@Composable
fun ListPlayersScreen(
    navHostController: NavHostController,
    viewModel: ListPlayerViewModel = hiltViewModel()
) {
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val filters by viewModel.uiFilters.collectAsStateWithLifecycle()
    val filtersError by viewModel.filterError.collectAsStateWithLifecycle()
    val list by viewModel.uiList.collectAsStateWithLifecycle()
    val listPosition by viewModel.uiListPositions.collectAsStateWithLifecycle()
    val showMorePlayersVisible by viewModel.showMoreButtonVisible.collectAsStateWithLifecycle()

    val filterActions = FilterListPlayersActions(
        onNameChange = viewModel::onNameChange,
        onCityChange = viewModel::onCityChange,
        onMinAgeChange = viewModel::onMinAgeChange,
        onMaxAgeChange = viewModel::onMaxAgeChange,
        onPositionChange = viewModel::onPositionChange,
        onMinSizeChange = viewModel::onMinSizeChange,
        onMaxSizeChange = viewModel::onMaxSizeChange,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::filterApply,
            onFilterClean = viewModel::cleanFilters
        )
    )

    ListPlayersContent(
        isOnline = isOnline,
        uiState = uiState,
        list = list,
        filters = filters,
        filtersError = filtersError,
        listPosition = listPosition,
        onRetry = { viewModel.retry() },
        filterActions = filterActions,
        onSendMembership = { id -> viewModel.sendMembershipRequest(id) },
        onShowMore = { id ->
            viewModel.showMore(idPlayer = id, navHostController = navHostController)
        },
        isValidShowMore = showMorePlayersVisible,
        showMoreItems = { viewModel.loadMorePlayers() }
    )
}

/**
 * Conteúdo visual da lista de jogadores ("Stateless").
 *
 * Responsável por desenhar a estrutura da página:
 * - Banner de estado Offline.
 * - Secção de Filtros expansível.
 * - Lista de cartões de jogadores.
 * - Botão "Mostrar Mais".
 *
 * @param isOnline Indica se o dispositivo tem conexão à rede.
 * @param uiState Estado global da UI (Loading, Error, etc).
 * @param list Lista de jogadores a exibir.
 * @param filters Estado atual dos campos de filtro.
 * @param filtersError Estado dos erros de validação dos filtros.
 * @param listPosition Lista de posições disponíveis para o dropdown.
 * @param onRetry Callback para tentar recarregar os dados em caso de erro.
 * @param isValidShowMore Se true, mostra o botão para carregar mais jogadores.
 * @param showMoreItems Callback acionado ao clicar em "Mostrar Mais".
 * @param filterActions Ações de alteração dos inputs de filtro.
 * @param onSendMembership Callback para enviar pedido de adesão a um jogador.
 * @param onShowMore Callback para navegar para o detalhe do jogador.
 */
@Composable
fun ListPlayersContent(
    isOnline: Boolean,
    uiState: UiState,
    list: List<InfoPlayerDto>,
    filters: FilterListPlayer,
    filtersError: FilterPlayersErrors,
    listPosition: List<Position?>,
    onRetry: () -> Unit,
    isValidShowMore: Boolean,
    showMoreItems: () -> Unit,
    filterActions: FilterListPlayersActions,
    onSendMembership: (String) -> Unit,
    onShowMore: (String) -> Unit
) {
    var filtersExpanded by remember { mutableStateOf(false) }

    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = onRetry,
        content = {
            OfflineBanner(isVisible = !isOnline)

            ListSurface(
                list = list,
                filterSection = {
                    FilterSection(
                        isExpanded = filtersExpanded,
                        onToggleExpand = { filtersExpanded = !filtersExpanded },
                        content = { paddingModifier ->
                            // Nota: A função FilterListPlayerContent precisa ser pública ou estar neste ficheiro
                            FilterListPlayerContent(
                                filters = filters,
                                filtersError = filtersError,
                                filterActions = filterActions,
                                listPosition = listPosition,
                                modifier = paddingModifier
                            )
                        }
                    )
                },
                listItems = { player ->
                    ItemListPlayer(
                        player = player,
                        sendMemberShipRequest = { onSendMembership(player.id) },
                        showMore = { onShowMore(player.id) }
                    )
                },
                isValidShowMore = isValidShowMore,
                showMoreItems = showMoreItems,
                messageEmptyList = stringResource(id = R.string.list_player_empty)
            )
        }
    )
}

/**
 * Formulário com os campos de filtro para a pesquisa de jogadores.
 */
@Composable
private fun FilterListPlayerContent(
    filters: FilterListPlayer,
    filtersError: FilterPlayersErrors,
    filterActions: FilterListPlayersActions,
    listPosition: List<Position?>,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        FilterRow(
            content = {
                FilterNamePlayerTextField(
                    playerName = filters.name,
                    onPlayerNameChange = { filterActions.onNameChange(it) },
                    isError = filtersError.nameError != null,
                    errorMessage = filtersError.nameError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterCityTextField(
                    city = filters.city,
                    onCityChange = { filterActions.onCityChange(it) },
                    isError = filtersError.cityError != null,
                    errorMessage = filtersError.cityError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterListPosition(
                    listPosition = listPosition,
                    selectPosition = filters.position,
                    onSelectPosition = { filterActions.onPositionChange(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterMaxAgeTextField(
                    maxAge = filters.maxAge?.toString(),
                    onMaxAgeChange = { filterActions.onMaxAgeChange(it.toIntOrNull()) },
                    isError = filtersError.maxAgeError != null,
                    errorMessage = filtersError.maxAgeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMinAgeTextField(
                    minAge = filters.minAge?.toString(),
                    onMinAgeChange = { filterActions.onMinAgeChange(it.toIntOrNull()) },
                    isError = filtersError.minAgeError != null,
                    errorMessage = filtersError.minAgeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())},
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_min_size),
                    value = filters.minSize?.toString(),
                    onValueChange = { filterActions.onMinSizeChange(it.toIntOrNull()) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    minLenght = PlayerConst.MIN_HEIGHT,
                    maxLenght = PlayerConst.MAX_HEIGHT,
                    isError = filtersError.minSizeError != null,
                    errorMessage = filtersError.minSizeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                LabelTextField(
                    label = stringResource(id = R.string.filter_max_size),
                    value = filters.maxSize?.toString(),
                    onValueChange = { filterActions.onMaxSizeChange(it.toIntOrNull()) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    minLenght = PlayerConst.MIN_HEIGHT,
                    maxLenght = PlayerConst.MAX_HEIGHT,
                    isError = filtersError.maxSizeError != null,
                    errorMessage = filtersError.maxSizeError?.let {
                        stringResource(it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            buttonsActions = filterActions.buttonActions,
            modifier = Modifier.weight(1f)
        )
    }
}

/**
 * Item individual da lista (Cartão do Jogador).
 */
@Composable
private fun ItemListPlayer(
    player: InfoPlayerDto,
    sendMemberShipRequest: () -> Unit,
    showMore: () -> Unit,
) {
    GenericListItem(
        item = player,
        title = { it.name },
        leading = {
            StringImageList(
                image = player.image,
            )
        },
        supporting = {
            Column {
                AgeRow(age = player.age)
                AddressRow(address = player.address)
                PositionRow(position = player.position)
                SizeRow(height = player.heigth)
            }
        },
        trailing = {
            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.End
            ) {
                if (!player.haveTeam) {
                    ListSendMemberShipRequestButton(sendMemberShipRequest = sendMemberShipRequest)
                }

                ShowMoreInfoButton(
                    showMoreDetails = showMore,
                    contentDescription = stringResource(id = R.string.list_teams_view_team)
                )
            }
        }
    )
}

// ----------------------------------------------------------------
// PREVIEWS
// ----------------------------------------------------------------
/**
 * Preview 1: Lista Normal com dados.
 * Mostra como a lista aparece quando tudo corre bem (Online).
 */
@Preview(name = "1. Normal - EN", locale = "en", showBackground = true)
@Preview(name = "1. Normal - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun ListPlayersPreview() {
    AMFootballTheme {
        ListPlayersContent(
            isOnline = true,
            uiState = UiState(isLoading = false),
            list = ListPlayers.list,
            filters = FilterListPlayer(),
            filtersError = FilterPlayersErrors(),
            listPosition = ListPlayers.Positions,
            onRetry = {},
            filterActions = ListPlayers.Actions,
            onSendMembership = {},
            onShowMore = {},
            isValidShowMore = true,
            showMoreItems = {}
        )
    }
}

/**
 * Preview 2: Lista Vazia.
 * Testa a mensagem de "sem jogadores" (stringResource(R.string.list_player_empty)).
 */
@Preview(name = "2. Vazia - EN", locale = "en", showBackground = true)
@Preview(name = "2. Vazia - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun ListPlayersEmptyPreview() {
    AMFootballTheme {
        ListPlayersContent(
            isOnline = true,
            uiState = UiState(isLoading = false),
            list = emptyList(),
            filters = FilterListPlayer(),
            filtersError = FilterPlayersErrors(),
            listPosition = ListPlayers.Positions,
            onRetry = {},
            filterActions = ListPlayers.Actions,
            onSendMembership = {},
            onShowMore = {},
            isValidShowMore = false,
            showMoreItems = {}
        )
    }
}

/**
 * Preview 3: Modo Offline.
 * Testa o banner de "Sem conexão" no topo da lista.
 */
@Preview(name = "3. Offline - EN", locale = "en", showBackground = true)
@Preview(name = "3. Offline - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun ListPlayersOfflinePreview() {
    AMFootballTheme {
        ListPlayersContent(
            isOnline = false,
            uiState = UiState(isLoading = false),
            list = ListPlayers.list,
            filters = FilterListPlayer(),
            filtersError = FilterPlayersErrors(),
            listPosition = ListPlayers.Positions,
            onRetry = {},
            filterActions = ListPlayers.Actions,
            onSendMembership = {},
            onShowMore = {},
            isValidShowMore = true,
            showMoreItems = {}
        )
    }
}

/**
 * Preview 4: Estado de Carregamento.
 * Mostra o indicador de progresso (LoadingPage).
 */
@Preview(name = "4. Loading - EN", locale = "en", showBackground = true)
@Preview(name = "4. Loading - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun ListPlayersLoadingPreview() {
    AMFootballTheme {
        ListPlayersContent(
            isOnline = true,
            uiState = UiState(isLoading = true),
            list = emptyList(),
            filters = FilterListPlayer(),
            filtersError = FilterPlayersErrors(),
            listPosition = ListPlayers.Positions,
            onRetry = {},
            filterActions = ListPlayers.Actions,
            onSendMembership = {},
            onShowMore = {},
            isValidShowMore = false,
            showMoreItems = {}
        )
    }
}

/**
 * Preview 5: Estado de Erro.
 * Testa o ecrã de erro com o botão de Retry.
 */
@Preview(name = "5. Erro - EN", locale = "en", showBackground = true)
@Preview(name = "5. Erro - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun ListPlayersErrorPreview() {
    AMFootballTheme {
        ListPlayersContent(
            isOnline = true,
            uiState = UiState(isLoading = false, errorMessage = "Erro de conexão ao servidor"),
            list = emptyList(),
            filters = FilterListPlayer(),
            filtersError = FilterPlayersErrors(),
            listPosition = ListPlayers.Positions,
            onRetry = {},
            filterActions = ListPlayers.Actions,
            onSendMembership = {},
            onShowMore = {},
            isValidShowMore = false,
            showMoreItems = {}
        )
    }
}
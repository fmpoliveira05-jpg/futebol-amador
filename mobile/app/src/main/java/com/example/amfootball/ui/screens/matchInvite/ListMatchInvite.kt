package com.example.amfootball.ui.screens.matchInvite

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMatchInviteActions
import com.example.amfootball.data.actions.itemsList.ItemListMatchIniviteActions
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.errors.filtersError.FilterMatchInviteError
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.AcceptButton
import com.example.amfootball.ui.components.buttons.EditButton
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.RejectButton
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.DateRow
import com.example.amfootball.ui.components.lists.FilterMaxDatePicker
import com.example.amfootball.ui.components.lists.FilterMinDatePicker
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PitchAddressRow
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.viewModel.matchInvite.ListMatchInviteViewModel
import com.example.amfootball.utils.Patterns
import java.time.format.DateTimeFormatter

/**
 * Ecrã de Listagem de Convites de Jogo Recebidos.
 *
 * Este componente "Stateful" atua como o ponto central para a equipa gerir os desafios recebidos
 * de outras equipas. Permite filtrar a lista por remetente ou data e interagir com cada convite.
 *
 * Responsabilidades:
 * 1. Coleta o estado do ViewModel (Lista de Convites, Filtros, Erros, Estado UI).
 * 2. Configura as callbacks de ação para filtros e itens da lista.
 * 3. Delega a renderização visual para [ListMatchInviteContent].
 *
 * @param navHostController Controlador de navegação para transitar para detalhes ou ecrã de negociação.
 * @param viewModel ViewModel injetado via Hilt que fornece os dados e lógica de negócio.
 */
@Composable
fun ListMatchInviteScreen(
    navHostController: NavHostController,
    viewModel: ListMatchInviteViewModel = hiltViewModel()
) {
    val filters by viewModel.uiFilters.collectAsStateWithLifecycle()
    val filterError by viewModel.filterError.collectAsStateWithLifecycle()

    val filterActions = FilterMatchInviteActions(
        onSenderNameChange = viewModel::onNameSenderChange,
        onMinDateSelected = viewModel::onMinDateChange,
        onMaxDateSelected = viewModel::onMaxDateChange,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::onApplyFilter,
            onFilterClean = viewModel::onFilterClear
        )
    )

    val list by viewModel.uiList.collectAsStateWithLifecycle()
    val itemsListActions = ItemListMatchIniviteActions(
        acceptMatchInvite = viewModel::acceptMatchInvite,
        rejectMatchInvite = viewModel::rejectMatchInvite,
        negociateMatchInvite = viewModel::negociateMatchInvite,
        showMoreDetails = viewModel::showMoreDetails
    )

    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    ListMatchInviteContent(
        uiState = uiState,
        isOnline = isOnline,
        filters = filters,
        filterActions = filterActions,
        filterError = filterError,
        list = list,
        itemsListActions = itemsListActions,
        navHostController = navHostController
    )
}

/**
 * Conteúdo visual da lista de convites (Stateless).
 *
 * Estrutura o layout principal do ecrã, incluindo:
 * - Banner de status offline.
 * - Gestão de estado de carregamento.
 * - Secção de filtros expansível.
 * - Lista de cartões de convite.
 *
 * @param uiState Estado global da UI (Loading/Erro).
 * @param isOnline Estado da conectividade.
 * @param filters Estado atual dos filtros aplicados.
 * @param filterActions Ações para atualizar filtros.
 * @param filterError Erros de validação nos campos de filtro.
 * @param list A lista de convites [InfoMatchInviteDto] a exibir.
 * @param itemsListActions Ações disponíveis para cada item da lista.
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun ListMatchInviteContent(
    uiState: UiState,
    isOnline: Boolean,
    filters: FilterMatchInvite,
    filterActions: FilterMatchInviteActions,
    filterError: FilterMatchInviteError,
    list: List<InfoMatchInviteDto>,
    itemsListActions: ItemListMatchIniviteActions,
    navHostController: NavHostController
) {
    var filtersExpanded by remember { mutableStateOf(false) }

    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = {},
        content = {
            OfflineBanner(isVisible = !isOnline)

            ListSurface(
                list = list,
                filterSection = {
                    FilterSection(
                        isExpanded = filtersExpanded,
                        onToggleExpand = { filtersExpanded = !filtersExpanded },
                        content = {
                            FilterListMatchInvite(
                                filters = filters,
                                filterActions = filterActions,
                                filterError = filterError,
                                modifier = Modifier.padding(
                                    start = 16.dp,
                                    end = 16.dp,
                                    bottom = 16.dp
                                ),
                            )
                        }
                    )
                },
                listItems = { invite ->
                    ItemListMatchInivite(
                        matchInvite = invite,
                        itemsListActions = itemsListActions,
                        navHostController = navHostController
                    )
                },
                messageEmptyList = stringResource(R.string.list_match_invite_empty)
            )
        }
    )

}

/**
 * Painel de conteúdo dos filtros de pesquisa para convites.
 *
 * Permite filtrar por:
 * - Nome da equipa remetente.
 * - Intervalo de datas do jogo proposto.
 *
 * @param filters Valores atuais dos filtros.
 * @param filterError Erros de validação associados.
 * @param filterActions Callbacks para alteração de valores.
 * @param modifier Modificador de layout.
 */
@Composable
private fun FilterListMatchInvite(
    filters: FilterMatchInvite,
    filterError: FilterMatchInviteError,
    filterActions: FilterMatchInviteActions,
    modifier: Modifier = Modifier
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_sender_name),
                    value = filters.senderName ?: "",
                    onValueChange = { filterActions.onSenderNameChange(it) },
                    isError = filterError.senderNameError != null,
                    errorMessage = filterError.senderNameError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterMinDatePicker(
                    value = filters.minDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMinDateSelected(it) },
                    isError = filterError.minDateError != null,
                    errorMessage = filterError.minDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMaxDatePicker(
                    value = filters.maxDate?.format(displayFormatter) ?: "",
                    onDateSelected = { filterActions.onMaxDateSelected(it) },
                    isError = filterError.maxDateError != null,
                    errorMessage = filterError.maxDateError?.let {
                        stringResource(id = it.messageId, *it.args.toTypedArray())
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
 * Item individual da lista de convites de jogo.
 *
 * Renderiza um cartão detalhado com:
 * - Nome e Logo da equipa adversária.
 * - Local do jogo (Campo e Morada).
 * - Data proposta.
 * - Ações Rápidas: Aceitar, Rejeitar, Negociar (Editar) e Ver Detalhes.
 *
 * @param matchInvite O DTO com os dados do convite.
 * @param itemsListActions Ações disponíveis para este item.
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun ItemListMatchInivite(
    matchInvite: InfoMatchInviteDto,
    itemsListActions: ItemListMatchIniviteActions,
    navHostController: NavHostController
) {
    GenericListItem(
        item = matchInvite,
        title = { it.opponent.name },
        leading = {
            StringImageList(
                image = matchInvite.opponent.image,
                contentDescription = stringResource(
                    id = R.string.logo_team_name,
                    R.string.logo_team,
                    matchInvite.opponent.name
                )
            )
        },
        supporting = {
            Column {
                PitchAddressRow(ptichAdrress = matchInvite.pitchGame)

                DateRow(
                    date = matchInvite.gameDate.format(
                        DateTimeFormatter.ofPattern(
                            Patterns.DATE,
                        )
                    )
                )
            }
        },
        trailing = {
            ShowMoreInfoButton(
                showMoreDetails = {
                    itemsListActions.showMoreDetails(
                        matchInvite.id,
                        navHostController
                    )
                },
                contentDescription = stringResource(id = R.string.list_teams_view_team)
            )
        },
        underneathItems = {
            AcceptButton(accept = { itemsListActions.acceptMatchInvite(matchInvite.id) })
            EditButton(
                edit = { itemsListActions.negociateMatchInvite(matchInvite.id, navHostController) },
                contentDescription = stringResource(id = R.string.negotiate_button_description)
            )
            RejectButton(reject = { itemsListActions.rejectMatchInvite(matchInvite.id) })
        }
    )
}

@Preview(
    name = "Lista de convite de partida - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "List Match Invite - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewListMatchInvite() {
    ListMatchInviteScreen(
        navHostController = rememberNavController(),
    )
}
package com.example.amfootball.ui.screens.lists

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CalendarMonth
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
import com.example.amfootball.data.actions.filters.FilterMemberShipRequestActions
import com.example.amfootball.data.actions.itemsList.ItemsMemberShipRequest
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.errors.filtersError.FilterMemberShipRequestError
import com.example.amfootball.data.filters.FilterMemberShipRequest
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterMaxDatePicker
import com.example.amfootball.ui.components.lists.FilterMinDatePicker
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.InfoRow
import com.example.amfootball.ui.components.lists.ItemAcceptRejectAndShowMore
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.StringImageList
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.viewModel.memberShipRequest.ListMemberShipRequestViewModel
import com.example.amfootball.utils.Patterns
import com.example.amfootball.utils.UserConst
import java.time.format.DateTimeFormatter

//TODO: Falta adaptar isto para quando for admin mostrar uns memberShipRequest e se for player outros
/**
 * Ecrã de Listagem de Pedidos de Adesão (Membership Requests).
 *
 * Este componente "Stateful" (com estado) atua como o ponto de entrada para visualizar e gerir
 * pedidos pendentes de entrada em equipas ou convites a jogadores.
 *
 * Responsabilidades:
 * 1. Observar o estado do ViewModel (Lista, Filtros, Erros, UI State).
 * 2. Construir as ações de filtro e de item (callbacks).
 * 3. Delegar a renderização visual para [ContentListMemberShipRequest].
 *
 * **Nota de Implementação (TODO):**
 * Atualmente exibe a mesma lista independentemente do papel do utilizador.
 * Futuramente, deve adaptar-se para diferenciar entre "Pedidos recebidos pela Equipa" (Visão Admin)
 * e "Convites recebidos pelo Jogador" (Visão Player).
 *
 * @param navHostController Controlador de navegação para transitar para detalhes ou aceitar pedidos.
 * @param viewModel ViewModel injetado via Hilt que fornece os dados e lógica de negócio.
 */
@Composable
fun ListMemberShipRequest(
    navHostController: NavHostController,
    viewModel: ListMemberShipRequestViewModel = hiltViewModel(),
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()
    val filters by viewModel.uiFilterState.collectAsStateWithLifecycle()
    val filterError by viewModel.uiFilterErrorState.collectAsStateWithLifecycle()
    val list by viewModel.uiList.collectAsStateWithLifecycle()
    val filterActions = FilterMemberShipRequestActions(
        onSenderNameChange = viewModel::onSenderNameChanged,
        onMinDateSelected = viewModel::onMinDateSelected,
        onMaxDateSelected = viewModel::onMaxDateSelected,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::applyFilters,
            onFilterClean = viewModel::clearFilters
        ),
    )

    val itemsActions = ItemsMemberShipRequest(
        acceptMemberShipRequest = viewModel::acceptMemberShipRequest,
        rejectMemberShipRequest = viewModel::rejectMemberShipRequest,
        showMore = viewModel::showMore
    )

    ContentListMemberShipRequest(
        uiState = uiState,
        isOnline = isOnline,
        filters = filters,
        filterError = filterError,
        filterActions = filterActions,
        list = list,
        itemsActions = itemsActions,
        navHostController = navHostController
    )
}

/**
 * Conteúdo visual da lista de pedidos (Stateless).
 *
 * Responsável por estruturar o layout do ecrã, incluindo:
 * - Banner Offline.
 * - Estado de Carregamento.
 * - Secção de Filtros Expansível.
 * - Lista de Itens.
 *
 * @param uiState Estado global da UI (Loading/Erro).
 * @param isOnline Estado da conectividade.
 * @param filters Estado atual dos filtros aplicados.
 * @param filterError Erros de validação nos campos de filtro.
 * @param filterActions Ações para atualizar filtros.
 * @param list A lista de [MembershipRequestInfoDto] a exibir.
 * @param itemsActions Ações para interagir com cada item da lista.
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun ContentListMemberShipRequest(
    uiState: UiState,
    isOnline: Boolean,
    filters: FilterMemberShipRequest,
    filterError: FilterMemberShipRequestError,
    filterActions: FilterMemberShipRequestActions,
    list: List<MembershipRequestInfoDto>,
    itemsActions: ItemsMemberShipRequest,
    navHostController: NavHostController,
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
                            FilterListMemberShipRequestContent(
                                filters = filters,
                                filterError = filterError,
                                filterActions = filterActions,
                                modifier = Modifier.padding(
                                    start = 16.dp,
                                    end = 16.dp,
                                    bottom = 16.dp
                                )
                            )
                        }
                    )
                },
                listItems = { request ->
                    ListMemberShipRequestContent(
                        membershipRequest = request,
                        itemsActions = itemsActions,
                        navHostController = navHostController
                    )
                },
                messageEmptyList = stringResource(id = R.string.list_membership_request_empty)
            )
        }
    )

}

/**
 * Painel de conteúdo dos filtros de pesquisa.
 *
 * Agrupa os campos de filtro (Nome do remetente, Datas) em linhas organizadas.
 *
 * @param filters Valores atuais dos filtros.
 * @param filterError Erros de validação associados.
 * @param filterActions Callbacks para alteração de valores.
 * @param modifier Modificador de layout.
 */
@Composable
private fun FilterListMemberShipRequestContent(
    filters: FilterMemberShipRequest,
    filterError: FilterMemberShipRequestError,
    filterActions: FilterMemberShipRequestActions,
    modifier: Modifier = Modifier,
) {
    val displayFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)

    Column(modifier = modifier) {
        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_sender_name),
                    value = filters.senderName ?: "",
                    maxLenght = UserConst.MAX_NAME_LENGTH,
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
 * Item individual da lista de pedidos de adesão.
 *
 * Renderiza um cartão com:
 * - Título dinâmico (Nome do Jogador ou da Equipa, dependendo de quem enviou).
 * - Imagem da Equipa.
 * - Data de envio.
 * - Botões de ação (Aceitar, Rejeitar, Ver Mais).
 *
 * @param membershipRequest O DTO com os dados do pedido.
 * @param itemsActions Ações disponíveis para este item.
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun ListMemberShipRequestContent(
    membershipRequest: MembershipRequestInfoDto,
    itemsActions: ItemsMemberShipRequest,
    navHostController: NavHostController
) {
    var receiver = ""
    var sender = ""

    if (membershipRequest.isPlayerSender) {
        sender = membershipRequest.player.id
        receiver = membershipRequest.team.id
    } else {
        sender = membershipRequest.team.id
        receiver = membershipRequest.player.id
    }

    GenericListItem(
        item = membershipRequest,
        title = { entity ->
            if (membershipRequest.isPlayerSender) {
                entity.player.name
            } else {
                entity.team.name
            }
        },
        leading = {
            StringImageList(
                image = membershipRequest.team.image,
                contentDescription = stringResource(
                    id = R.string.logo_team_name,
                    stringResource(R.string.logo_team),
                    membershipRequest.team.name
                )
            )
        },
        supporting = {
            Column {
                InfoRow(
                    icon = Icons.Default.CalendarMonth,
                    text = membershipRequest.dateSend.format(
                        DateTimeFormatter.ofPattern(
                            Patterns.DATE,
                        )
                    ),
                    modifier = Modifier.fillMaxWidth()
                )
            }
        },
        trailing = {
            ItemAcceptRejectAndShowMore(
                accept = {
                    itemsActions.acceptMemberShipRequest(
                        receiver,
                        membershipRequest.id,
                        membershipRequest.isPlayerSender,
                        navHostController
                    )
                },
                reject = {
                    itemsActions.rejectMemberShipRequest(
                        receiver,
                        membershipRequest.id,
                        membershipRequest.isPlayerSender,
                    )
                },
                showMore = {
                    itemsActions.showMore(
                        sender,
                        membershipRequest.isPlayerSender,
                        navHostController,
                    )
                }
            )
        }
    )
}

@Preview(
    name = "Lista de pedidos de adesão Jogador - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "List MemberShip Request player - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewListMemberShipRequestPlayerScreen() {
    ListMemberShipRequest(rememberNavController())
}


@Preview(
    name = "Lista de pedidos de adesão Jogador - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "List MemberShip Request player - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewListMemberShipRequestTeamScreen() {
    ListMemberShipRequest(rememberNavController())
}
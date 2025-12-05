package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowDownward
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Upgrade
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
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
import com.example.amfootball.data.actions.filters.FilterMemberTeamAction
import com.example.amfootball.data.actions.itemsList.ItemsListMemberAction
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import com.example.amfootball.data.errors.filtersError.FilterMembersFilterError
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.mocks.lists.ListMembersMocks
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.lists.AgeRow
import com.example.amfootball.ui.components.lists.FilterHeader
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
import com.example.amfootball.ui.components.lists.TypeMemberRow
import com.example.amfootball.ui.components.notification.OfflineBanner
import com.example.amfootball.ui.viewModel.team.ListMembersViewModel

//TODO: Corrigir previews + toast de network + retry
/**
 * Ecrã de Gestão e Listagem de Membros da Equipa.
 *
 * Este é um componente Stateful (com estado) que exibe a lista completa de jogadores e staff da equipa,
 * permitindo filtragem por nome, idade, posição e cargo.
 *
 * **Requisitos de Permissão:** Este ecrã é tipicamente acedido apenas por Administradores da Equipa.
 *
 * @param navHostController Controlador de navegação para detalhes do jogador.
 * @param viewModel ViewModel injetado via Hilt para gerir o estado de filtros e a lista de membros.
 */
@Composable
fun ListMembersScreen(
    navHostController: NavHostController,
    viewModel: ListMembersViewModel = hiltViewModel()
) {
    val list by viewModel.uiList.collectAsStateWithLifecycle()
    val listTypeMember by viewModel.uiListTypeMember.collectAsStateWithLifecycle()
    val listPosition by viewModel.uiListPositions.collectAsStateWithLifecycle()
    val filters by viewModel.uiFilter.collectAsStateWithLifecycle()
    val filtersErrors by viewModel.uiErrorFilters.collectAsStateWithLifecycle()
    val filterAction = FilterMemberTeamAction(
        onTypeMemberChange = viewModel::onTypeMemberChange,
        onNameChange = viewModel::onNameChange,
        onMinAgeChange = viewModel::onMinAgeChange,
        onMaxAgeChange = viewModel::onMaxAgeChange,
        onPositionChange = viewModel::onPositionChange,
        buttonActions = ButtonFilterActions(
            onFilterApply = viewModel::onApplyFilter,
            onFilterClean = viewModel::onClearFilter
        )
    )

    val itemsListActions = ItemsListMemberAction(
        onPromoteMember = viewModel::onPromoteMember,
        onDemoteMember = viewModel::onDemoteMember,
        onRemovePlayer = viewModel::onRemovePlayer,
        onShowMoreInfo = viewModel::onShowMoreInfo
    )

    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val isOnline by viewModel.isOnline.collectAsStateWithLifecycle()

    ListMemberContent(
        filters = filters,
        filterActions = filterAction,
        filtersErrors = filtersErrors,
        list = list,
        listTypeMember = listTypeMember,
        listPosition = listPosition,
        uiState = uiState,
        isOnline = isOnline,
        itemsListActions = itemsListActions,
        navHostController = navHostController,
    )
}

/**
 * Conteúdo visual da lista de membros (Stateless).
 *
 * Responsável por estruturar o layout, incluindo o wrapper de [LoadingPage] e o componente
 * principal [ListSurface] que renderiza a lista e a secção de filtros.
 *
 * @param uiState Estado da UI (Loading, Erros).
 * @param isOnline Estado da conectividade.
 * @param filters Valores atuais dos filtros.
 * @param filterActions Callbacks para filtros.
 * @param filtersErrors Erros de validação nos filtros.
 * @param list A lista de membros [MemberTeamDto].
 * @param listTypeMember Opções de filtro para Tipo de Membro.
 * @param listPosition Opções de filtro para Posição.
 * @param itemsListActions Callbacks de ação do item (promover, remover).
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun ListMemberContent(
    uiState: UiState,
    isOnline: Boolean,
    filters: FilterMembersTeam,
    filterActions: FilterMemberTeamAction,
    filtersErrors: FilterMembersFilterError,
    list: List<MemberTeamDto>,
    listTypeMember: List<TypeMember?>,
    listPosition: List<Position?>,
    itemsListActions: ItemsListMemberAction,
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
                        header = {
                            FilterHeader(
                                isExpanded = filtersExpanded,
                                onToggleExpand = { filtersExpanded = !filtersExpanded })
                        },
                        content = { paddingModifier ->
                            FilterListMemberContent(
                                filters = filters,
                                filterActions = filterActions,
                                filterErros = filtersErrors,
                                listTypeMember = listTypeMember,
                                listPosition = listPosition,
                                modifier = paddingModifier
                            )
                        }
                    )
                },
                listItems = { member ->
                    ListMemberItem(
                        member = member,
                        promote = { itemsListActions.onPromoteMember(member.id) },
                        despromote = { itemsListActions.onDemoteMember(member.id) },
                        remove = { itemsListActions.onRemovePlayer(member.id) },
                        showMore = { itemsListActions.onShowMoreInfo(member.id, navHostController) }
                    )
                },
                messageEmptyList = stringResource(id = R.string.list_members_empty)
            )
        }
    )
}

/**
 * Painel que contém todos os campos de filtro para a lista de membros.
 *
 * @param filters Valores atuais dos filtros.
 * @param filterActions Callbacks para alteração de valores.
 * @param filterErros Erros de validação associados.
 * @param listTypeMember Opções para o filtro de Cargo (Admin/Player).
 * @param listPosition Opções para o filtro de Posição.
 * @param modifier Modificador de layout.
 */
@Composable
private fun FilterListMemberContent(
    filters: FilterMembersTeam,
    filterActions: FilterMemberTeamAction,
    filterErros: FilterMembersFilterError,
    listTypeMember: List<TypeMember?>,
    listPosition: List<Position?>,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        FilterRow(
            content = {
                FilterNamePlayerTextField(
                    playerName = filters.name ?: "",
                    onPlayerNameChange = { filterActions.onNameChange(it) },
                    isError = filterErros.nameError != null,
                    errorMessage = filterErros.nameError?.let {
                        stringResource(
                            id = it.messageId,
                            *it.args.toTypedArray()
                        )
                    },
                    modifier = Modifier.weight(1f)
                )
            }
        )

        FilterRow(
            content = {
                FilterMinAgeTextField(
                    minAge = filters.minAge?.toString(),
                    onMinAgeChange = { filterActions.onMinAgeChange(it.toIntOrNull()) },
                    isError = filterErros.minAgeError != null,
                    errorMessage = filterErros.minAgeError?.let {
                        stringResource(
                            id = it.messageId,
                            *it.args.toTypedArray()
                        )
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterMaxAgeTextField(
                    maxAge = filters.maxAge?.toString() ?: "",
                    onMaxAgeChange = { filterActions.onMaxAgeChange(it.toIntOrNull()) },
                    isError = filterErros.maxAgeError != null,
                    errorMessage = filterErros.maxAgeError?.let {
                        stringResource(
                            id = it.messageId,
                            *it.args.toTypedArray()
                        )
                    },
                    modifier = Modifier.weight(1f)
                )
            },
        )

        FilterRow(
            content = {
                LabelSelectBox(
                    label = stringResource(id = R.string.filter_type_member),
                    list = listTypeMember,
                    selectedValue = filters.typeMember,
                    onSelectItem = { filterActions.onTypeMemberChange(it) },
                    itemToString = { position ->
                        if (position == null) {
                            stringResource(id = R.string.filter_selectbox_all)
                        } else {
                            stringResource(id = position.stringId)
                        }
                    },
                    modifier = Modifier.weight(1f)
                )

                FilterListPosition(
                    listPosition = listPosition,
                    selectPosition = filters.position,
                    onSelectPosition = { filterActions.onPositionChange(it) },
                    modifier = Modifier.weight(1f)
                )
            },
        )

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            buttonsActions = filterActions.buttonActions,
            modifier = Modifier.weight(1f)
        )
    }
}

/**
 * Item individual da lista de membros.
 *
 * Renderiza o [GenericListItem] com todas as informações de perfil, e adiciona
 * os botões de ação na seção "Trailing".
 *
 * @param member DTO do membro da equipa.
 * @param promote Ação para promover o membro a Admin.
 * @param despromote Ação para despromover o Admin a membro.
 * @param remove Ação para remover/expulsar o membro da equipa.
 * @param showMore Ação para ver mais informações sobre o membro.
 */
@Composable
private fun ListMemberItem(
    member: MemberTeamDto,
    promote: () -> Unit,
    despromote: () -> Unit,
    remove: () -> Unit,
    showMore: () -> Unit,
) {
    GenericListItem(
        item = member,
        title = { it.name },
        leading = {
            StringImageList(
                image = member.image,
                contentDescription = stringResource(
                    id = R.string.proflie_image_name,
                    R.string.proflie_image,
                    member.name
                )
            )
        },
        supporting = {
            MemberSupportingInfo(member = member)
        },
        trailing = {
            MemberTrailingButtons(
                typeMember = member.typeMember,
                promote = promote,
                despromote = despromote,
                remove = remove,
                showMore = showMore
            )
        }
    )
}

/**
 * Coluna de informações secundárias exibidas abaixo do nome do membro.
 *
 * @param member DTO do membro.
 */
@Composable
private fun MemberSupportingInfo(member: MemberTeamDto) {
    Column {
        AgeRow(age = member.age)
        PositionRow(position = member.position)
        TypeMemberRow(typeMember = member.typeMember)
        SizeRow(height = member.height)
    }
}

/**
 * Botões de ação (trailing content) para gestão de membros.
 *
 * A visibilidade dos botões de promoção/despromoção é controlada pelo `typeMember` do item:
 * - Se for [TypeMember.PLAYER]: Mostra o botão "Promover".
 * - Se for [TypeMember.ADMIN_TEAM]: Mostra o botão "Despromover".
 * O botão de "Remover" (Expulsar) e "Ver Mais" é sempre visível.
 *
 * @param typeMember O cargo atual do membro (para controlo visual).
 * @param promote Ação para promover.
 * @param despromote Ação para despromover.
 * @param remove Ação para remover.
 * @param showMore Ação para ver detalhes.
 */
@Composable
private fun MemberTrailingButtons(
    typeMember: TypeMember,
    promote: () -> Unit,
    despromote: () -> Unit,
    remove: () -> Unit,
    showMore: () -> Unit
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        modifier = Modifier.padding(start = 8.dp)
    ) {
        when (typeMember) {
            TypeMember.PLAYER -> {
                IconButton(onClick = promote) {
                    Icon(
                        imageVector = Icons.Filled.Upgrade,
                        contentDescription = stringResource(id = R.string.accept_button_description),
                        tint = MaterialTheme.colorScheme.primary
                    )
                }
            }

            TypeMember.ADMIN_TEAM -> {
                IconButton(onClick = despromote) {
                    Icon(
                        imageVector = Icons.Filled.ArrowDownward,
                        contentDescription = stringResource(id = R.string.reject_button_description),
                        tint = MaterialTheme.colorScheme.error
                    )
                }
            }
        }

        IconButton(onClick = remove) {
            Icon(
                imageVector = Icons.Default.Delete,
                contentDescription = stringResource(id = R.string.remove_button_player_description),
                tint = MaterialTheme.colorScheme.error
            )
        }

        ShowMoreInfoButton(
            showMoreDetails = showMore,
            contentDescription = stringResource(id = R.string.list_teams_view_team)
        )
    }
}

@Preview(name = "1. Lista Normal - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "1. List Normal - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListMemberContent_Normal() {
    ListMemberContent(
        filters = FilterMembersTeam(),
        filterActions = ListMembersMocks.mockFilterActions,
        filtersErrors = FilterMembersFilterError(),
        list = ListMembersMocks.mockMembers,
        listTypeMember = ListMembersMocks.mockListTypes,
        listPosition = ListMembersMocks.mockListPositions,
        uiState = UiState(isLoading = false),
        isOnline = true,
        itemsListActions = ListMembersMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "2. Lista Vazia - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "2. List Empty - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListMemberContent_Empty() {
    ListMemberContent(
        filters = FilterMembersTeam(),
        filterActions = ListMembersMocks.mockFilterActions,
        filtersErrors = FilterMembersFilterError(),
        list = emptyList(),
        listTypeMember = ListMembersMocks.mockListTypes,
        listPosition = ListMembersMocks.mockListPositions,
        uiState = UiState(isLoading = false),
        isOnline = true,
        itemsListActions = ListMembersMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "3. Loading - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "3. Loading - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListMemberContent_Loading() {
    ListMemberContent(
        filters = FilterMembersTeam(),
        filterActions = ListMembersMocks.mockFilterActions,
        filtersErrors = FilterMembersFilterError(),
        list = emptyList(),
        listTypeMember = ListMembersMocks.mockListTypes,
        listPosition = ListMembersMocks.mockListPositions,
        uiState = UiState(isLoading = true),
        isOnline = true,
        itemsListActions = ListMembersMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "4. Erro - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "4. Error - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListMemberContent_Error() {
    ListMemberContent(
        filters = FilterMembersTeam(),
        filterActions = ListMembersMocks.mockFilterActions,
        filtersErrors = FilterMembersFilterError(),
        list = emptyList(),
        listTypeMember = ListMembersMocks.mockListTypes,
        listPosition = ListMembersMocks.mockListPositions,
        uiState = UiState(isLoading = false, errorMessage = "Falha ao conectar ao servidor."),
        isOnline = true,
        itemsListActions = ListMembersMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "5. Offline Banner - PT", locale = "pt-rPT", showBackground = true)
@Composable
fun PreviewListMemberContent_Offline() {
    ListMemberContent(
        filters = FilterMembersTeam(),
        filterActions = ListMembersMocks.mockFilterActions,
        filtersErrors = FilterMembersFilterError(),
        list = ListMembersMocks.mockMembers,
        listTypeMember = ListMembersMocks.mockListTypes,
        listPosition = ListMembersMocks.mockListPositions,
        uiState = UiState(isLoading = false),
        isOnline = false,
        itemsListActions = ListMembersMocks.mockItemActions,
        navHostController = rememberNavController()
    )
}
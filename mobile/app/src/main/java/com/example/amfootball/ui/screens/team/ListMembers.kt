package com.example.amfootball.ui.screens.team

import android.net.Uri
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
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMemberTeamAction
import com.example.amfootball.data.actions.itemsList.ItemsListMemberAction
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import com.example.amfootball.data.errors.filtersError.FilterMembersFilterError
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
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PositionRow
import com.example.amfootball.ui.components.lists.SizeRow
import com.example.amfootball.ui.components.lists.TypeMemberRow
import com.example.amfootball.ui.viewModel.team.ListMembersViewModel

@Composable
fun ListMembersScreen(
    navHostController: NavHostController,
    viewModel: ListMembersViewModel = viewModel()
) {
    val list by viewModel.uiList.observeAsState(initial = emptyList())
    val listTypeMember by viewModel.uiListTypeMember.observeAsState(initial = emptyList())
    val listPosition by viewModel.uiListPositions.observeAsState(initial = emptyList())

    val filters by viewModel.uiFilter.observeAsState(initial = FilterMembersTeam())
    val filtersErrors by viewModel.uiErrorFilters.observeAsState(initial = FilterMembersFilterError())
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

    ListMemberContent(
        filters = filters,
        filterActions = filterAction,
        filtersErrors = filtersErrors,
        list = list,
        listTypeMember = listTypeMember,
        listPosition = listPosition,
        itemsListActions = itemsListActions,
        navHostController = navHostController,
    )
}

@Composable
private fun ListMemberContent(
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

    ListSurface(
        list = list,
        filterSection = {
            FilterSection(
                isExpanded = filtersExpanded,
                onToggleExpand = { filtersExpanded = !filtersExpanded },
                header = {
                    FilterHeader(isExpanded = filtersExpanded, onToggleExpand = { filtersExpanded = !filtersExpanded })
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
        FilterRow (
            content = {
                FilterNamePlayerTextField(
                    playerName = filters.name ?: "",
                    onPlayerNameChange = { filterActions.onNameChange(it) },
                    isError = filterErros.nameError != null,
                    errorMessage = filterErros.nameError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) },
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
                    errorMessage = filterErros.minAgeError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) },
                    modifier = Modifier.weight(1f)
                )

                FilterMaxAgeTextField(
                    maxAge = filters.maxAge?.toString() ?: "",
                    onMaxAgeChange = { filterActions.onMaxAgeChange(it.toIntOrNull()) },
                    isError = filterErros.maxAgeError != null,
                    errorMessage = filterErros.maxAgeError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) },
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
            ImageList(
                image = member.image,
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

@Composable
private fun MemberSupportingInfo(member: MemberTeamDto) {
    Column {
        AgeRow(age = member.age)
        PositionRow(position = member.position)
        TypeMemberRow(typeMember = member.typeMember)
        SizeRow(size = member.size)
    }
}

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
            TypeMember.ADMIN_TEAM ->  {
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


private val mockMembers = listOf(
    MemberTeamDto(
        id = "1",
        name = "João Silva",
        age = 25,
        position = Position.FORWARD,
        typeMember = TypeMember.ADMIN_TEAM,
        image = Uri.EMPTY,
        size = 1.85
    ),
    MemberTeamDto(
        id = "2",
        name = "Pedro Santos",
        age = 22,
        position = Position.DEFENDER,
        typeMember = TypeMember.PLAYER,
        image = Uri.EMPTY,
        size = 1.78
    ),
    MemberTeamDto(
        id = "3",
        name = "Miguel Costa",
        age = 28,
        position = Position.MIDFIELDER,
        typeMember = TypeMember.PLAYER,
        image = Uri.EMPTY,
        size = 1.92
    )
)

private val mockFilterActions = FilterMemberTeamAction(
    {}, {}, {}, {}, {}, ButtonFilterActions({}, {})
)

private val mockItemActions = ItemsListMemberAction(
    {}, {}, {}, { _, _ -> }
)

// ----------------------------------------------------------------
// PREVIEWS
// ----------------------------------------------------------------

@Preview(name = "1. Lista Normal - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "1. List Normal - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListMemberContent_Normal() {
    ListMemberContent(
        filters = FilterMembersTeam(),
        filterActions = mockFilterActions,
        filtersErrors = FilterMembersFilterError(),
        list = mockMembers,
        listTypeMember = emptyList(), // Pode popular se tiveres um enum.values().toList()
        listPosition = emptyList(),   // Pode popular se tiveres um enum.values().toList()
        itemsListActions = mockItemActions,
        navHostController = rememberNavController()
    )
}

@Preview(name = "2. Lista Vazia - PT", locale = "pt-rPT", showBackground = true)
@Preview(name = "2. List Empty - EN", locale = "en", showBackground = true)
@Composable
fun PreviewListMemberContent_Empty() {
    ListMemberContent(
        filters = FilterMembersTeam(),
        filterActions = mockFilterActions,
        filtersErrors = FilterMembersFilterError(),
        list = emptyList(),
        listTypeMember = emptyList(),
        listPosition = emptyList(),
        itemsListActions = mockItemActions,
        navHostController = rememberNavController()
    )
}
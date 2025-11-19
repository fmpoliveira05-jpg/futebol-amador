package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardOptions
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
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMemberTeamAction
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import com.example.amfootball.data.errors.filtersError.FilterMembersFilterError
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.AgeRow
import com.example.amfootball.ui.components.lists.FilterHeader
import com.example.amfootball.ui.components.lists.FilterRow
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.components.lists.GenericListItem
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.components.lists.ListSurface
import com.example.amfootball.ui.components.lists.PositionRow
import com.example.amfootball.ui.components.lists.SizeRow
import com.example.amfootball.ui.components.lists.TypeMemberRow
import com.example.amfootball.ui.viewModel.team.ListMembersViewModel
import com.example.amfootball.utils.UserConst

@Composable
fun ListMembersScreen(
    navHostController: NavHostController,
    viewModel: ListMembersViewModel = viewModel()
) {
    val filters by viewModel.uiFilter.observeAsState(initial = FilterMembersTeam())
    val filtersErrors by viewModel.uiErrorFilters.observeAsState(initial = FilterMembersFilterError())
    val list by viewModel.uiList.observeAsState(initial = emptyList())
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

    val listTypeMember by viewModel.uiListTypeMember.observeAsState(initial = emptyList())
    val listPosition by viewModel.uiListPositions.observeAsState(initial = emptyList())
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
                        filterActions = filterAction,
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
                promote = { viewModel.onPromoteMember(idPlayer = member.id) },
                despromote = { viewModel.onDemoteMember(idAdmin = member.id) },
                remove = { viewModel.onRemovePlayer(idPlayer = member.id) },
                showMore = { viewModel.onShowMoreInfo(
                    idUser = member.id,
                    navHostController = navHostController)
                }
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
                LabelTextField(
                    label = stringResource(id = R.string.filter_name),
                    value = filters.name ?: "",
                    maxLenght = UserConst.MAX_NAME_LENGTH,
                    onValueChange = { filterActions.onNameChange(it) },
                    modifier = Modifier.weight(1f),
                    isError = filterErros.nameError != null,
                    errorMessage = filterErros.nameError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) }
                )
            }
        )

        FilterRow(
            content = {
                LabelTextField(
                    label = stringResource(id = R.string.filter_min_age),
                    value = filters.minAge?.toString() ?: "",
                    minLenght = UserConst.MIN_AGE,
                    maxLenght = UserConst.MAX_AGE,
                    onValueChange = { filterActions.onMinAgeChange(it.toIntOrNull()) },
                    isError = filterErros.minAgeError != null,
                    errorMessage = filterErros.minAgeError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    modifier = Modifier.weight(1f)
                )

                LabelTextField(
                    label = stringResource(id = R.string.filter_max_age),
                    value = filters.maxAge?.toString() ?: "",
                    minLenght = UserConst.MIN_AGE,
                    maxLenght = UserConst.MAX_AGE,
                    isError = filterErros.maxAgeError != null,
                    errorMessage = filterErros.maxAgeError?.let { stringResource(id = it.messageId, *it.args.toTypedArray()) },
                    onValueChange = { filterActions.onMaxAgeChange(it.toIntOrNull()) },
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

                LabelSelectBox(
                    label = stringResource(id = R.string.filter_position),
                    list = listPosition,
                    selectedValue = filters.position,
                    onSelectItem = { filterActions.onPositionChange(it) },
                    itemToString = { typeMember ->
                        if (typeMember == null) {
                            stringResource(id = R.string.filter_selectbox_all)
                        } else {
                            stringResource(id = typeMember.stringId)
                        }
                    },
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
                //contentDescription = stringResource(id = R.string.remove_button_description),
                contentDescription = "Remove",
                tint = MaterialTheme.colorScheme.error
            )
        }

        ShowMoreInfoButton(
            showMoreDetails = showMore,
            contentDescription = stringResource(id = R.string.list_teams_view_team)
        )
    }
}


@Preview(
    name = "Lista de Memberos - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "Members List - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun PreviewListMembers() {
    ListMembersScreen(rememberNavController())
}
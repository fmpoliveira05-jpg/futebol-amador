package com.example.amfootball.ui.screens.team

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.actions.filters.FilterMemberTeamAction
import com.example.amfootball.data.dtos.filters.FilterMembersTeam
import com.example.amfootball.data.enums.TypeMember
import com.example.amfootball.ui.components.buttons.LineClearFilterButtons
import com.example.amfootball.ui.components.inputFields.LabelSelectBox
import com.example.amfootball.ui.components.inputFields.LabelTextField
import com.example.amfootball.ui.components.lists.FilterHeader
import com.example.amfootball.ui.components.lists.FilterSection
import com.example.amfootball.ui.viewModel.team.ListMembersViewModel

@Composable
fun ListMembersScreen(
    navHostController: NavHostController,
    viewModel: ListMembersViewModel = viewModel()
) {
    val filters by viewModel.uiFilter.collectAsState()
    val list by viewModel.uiList.collectAsState()
    val filterAction = FilterMemberTeamAction(
        onTypeMemberChange = viewModel::onTypeMemberChange,
        onNameChange = viewModel::onNameChange,
        onMinAgeChange = viewModel::onMinAgeChange,
        onMaxAgeChange = viewModel::onMaxAgeChange,
        onPositionChange = viewModel::onPositionChange,
        onApplyFilter = viewModel::onApplyFilter,
        onClearFilter = viewModel::onClearFilter,
    )

    var filtersExpanded by remember { mutableStateOf(false) }

    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            item {
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
                            modifier = paddingModifier
                        )
                    }

                )
                Spacer(Modifier.height(16.dp))
            }

            /**
             * items(filteredList) { team ->
             *                 ListTeam(team = team, navHostController = navHostController)
             *                 Spacer(Modifier.height(12.dp))
             *             }
             * */
        }
    }
}

@Composable
private fun FilterListMemberContent(
    filters: FilterMembersTeam,
    filterActions: FilterMemberTeamAction,
    modifier: Modifier = Modifier
) {
    Column(modifier = modifier) {
        Row(modifier = Modifier.fillMaxWidth()) {
            LabelTextField(
                label = "Nome",
                value = filters.name,
                onValueChange = { filterActions.onNameChange(it) },
                modifier = Modifier.weight(1f)
            )
            Spacer(Modifier.width(8.dp))
        }

        //Corrigir aqui as cenas de não ter (it)
        Row(modifier = Modifier.fillMaxWidth()) {
            LabelTextField(
                label = "Idade Min.",
                value = filters.minAge.toString(),
                onValueChange = { filterActions.onMinAgeChange },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                modifier = Modifier.weight(1f)
            )

            Spacer(Modifier.width(8.dp))

            LabelTextField(
                label = "Idade Max.",
                value = filters.maxAge.toString(),
                onValueChange = { filterActions.onMaxAgeChange },
                modifier = Modifier.weight(1f)
            )
        }

        Row(modifier = Modifier.fillMaxWidth()) {
            //TODO: Trocar para selectBox

            val typeMemberOptions = listOf(
                null,
                TypeMember.PLAYER,
                TypeMember.ADMIN_TEAM
            )
            LabelSelectBox(
                label = "Tipo de membro",
                list = typeMemberOptions,
                selectedValue = filters.typeMember?.displayName ?: "Todos",
                onSelectItem = {filterActions.onTypeMemberChange(it) },
                itemToString = { it?.displayName ?: "Todos" },
                modifier = Modifier.weight(1f)
            )



            Spacer(Modifier.width(8.dp))

            //TODO: Trocar para selectBox(positions)
            /**
            LabelTextField(
            label = "Tipo de membro",
            value = filters.typeMember.toString(),
            onValueChange = { filterActions.onIsAdminChange },
            modifier = Modifier.weight(1f)
            )
             * */

        }

        Spacer(Modifier.height(16.dp))

        LineClearFilterButtons(
            onApplyFiltersClick = filterActions.onApplyFilter,
            onClearFilters = filterActions.onClearFilter,
            modifier = Modifier.weight(1f)
        )

    }

}

@Preview
@Composable
fun PreviewListMembers() {
    ListMembersScreen(rememberNavController())
}
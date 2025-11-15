package com.example.amfootball.data.actions.filters

import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

data class FilterMemberTeamAction(
    val onTypeMemberChange: (TypeMember?) -> Unit,
    val onNameChange: (String) -> Unit,
    val onMinAgeChange: (Int?) -> Unit,
    val onMaxAgeChange: (Int?) -> Unit,
    val onPositionChange: (Position?) -> Unit,
    val onApplyFilter: () -> Unit,
    val onClearFilter: () -> Unit,
)

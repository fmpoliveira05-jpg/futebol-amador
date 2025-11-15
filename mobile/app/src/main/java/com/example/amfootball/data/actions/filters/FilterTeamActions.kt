package com.example.amfootball.data.actions.filters

data class FilterTeamActions(
    val onNameChange: (String) -> Unit,
    val onCityChange: (String) -> Unit,
    val onRankChange: (String) -> Unit,
    val onMinPointChange: (Int?) -> Unit,
    val onMaxPointChange: (Int?) -> Unit,
    val onMinAgeChange: (Int?) -> Unit,
    val onMaxAgeChange: (Int?) -> Unit,
    val onMinNumberMembersChange: (Int?) -> Unit,
    val onMaxNumberMembersChange: (Int?) -> Unit,
    val buttonActions: ButtonFilterActions
)
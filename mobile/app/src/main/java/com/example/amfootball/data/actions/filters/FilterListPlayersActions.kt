package com.example.amfootball.data.actions.filters

import com.example.amfootball.data.enums.Position

data class FilterListPlayersActions(
    val onNameChange: (String) -> Unit,
    val onCityChange: (String) -> Unit,
    val onMinAgeChange: (Int?) -> Unit,
    val onMaxAgeChange: (Int?) -> Unit,
    val onPositionChange: (Position?) -> Unit,
    val onMinSizeChange: (Int?) -> Unit,
    val onMaxSizeChange: (Int?) -> Unit,
    val buttonActions: ButtonFilterActions
)

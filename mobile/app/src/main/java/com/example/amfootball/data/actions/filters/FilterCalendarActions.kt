package com.example.amfootball.data.actions.filters

import com.example.amfootball.data.enums.TypeMatch

data class FilterCalendarActions(
    val onNameChange: (String) -> Unit,
    val onMinDateGameChange: (Long) -> Unit,
    val onMaxDateGameChange: (Long) -> Unit,
    val onGameLocalChange: (Boolean?) -> Unit,
    val onTypeMatchChange: (TypeMatch?) -> Unit,
    val onIsFinishedChange: (Boolean?) -> Unit,
    val onButtonFilterActions: ButtonFilterActions
)

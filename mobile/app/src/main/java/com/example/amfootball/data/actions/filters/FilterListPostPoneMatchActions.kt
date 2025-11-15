package com.example.amfootball.data.actions.filters

data class FilterListPostPoneMatchActions(
    val onOpponentNameChange: (String) -> Unit,
    val onIsHomeChange: (Boolean?) -> Unit,
    val onMinDateGameChange: (Long) -> Unit,
    val onMaxDateGameChange: (Long) -> Unit,
    val onMinDatePostPoneChange: (Long) -> Unit,
    val onMaxDatePostPoneChange: (Long) -> Unit,
    val buttonFilterActions: ButtonFilterActions
)



package com.example.amfootball.data.actions.filters

data class FilterMemberShipRequestActions(
    val onSenderNameChange: (newName: String) -> Unit,
    val onMinDateSelected: (newMinDate: Long) -> Unit,
    val onMaxDateSelected: (newMaxDate: Long) -> Unit,
    val buttonActions: ButtonFilterActions
)

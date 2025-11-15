package com.example.amfootball.data.actions.filters

data class ButtonFilterActions(
    val onFilterApply: () -> Unit,
    val onFilterClean: () -> Unit,
)

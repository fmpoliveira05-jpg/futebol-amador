package com.example.amfootball.data.actions.forms

import androidx.navigation.NavHostController

data class FormMatchInviteActions(
    val onGameDateChange: (newDate: Long) -> Unit,
    val onTimeGameChange: (newTime: String) -> Unit,
    val onLocalGameChange: (isHome: Boolean) -> Unit,
    val onSubmitForm: (navHostController: NavHostController) -> Unit
)
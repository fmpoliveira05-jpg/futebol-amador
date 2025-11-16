package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

data class ItensCalendarActions(
    val onCancelMatch: (String, NavHostController) -> Unit,
    val onPostPoneMatch: (String, NavHostController) -> Unit,
    val onStartMatch: (String) -> Unit,
    val onFinishMatch: (String, NavHostController) -> Unit,
)

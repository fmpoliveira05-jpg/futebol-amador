package com.example.amfootball.data.actions.forms

import androidx.navigation.NavHostController

data class FormFinishMatchActions(
    val onNumGoalsTeamChange: (newNumGoalsTeam: Int) -> Unit,
    val onNumGoalsOpponentChange: (numGoalsOpponent: Int) -> Unit,
    val onSubmitForm: (NavHostController) -> Unit,
)

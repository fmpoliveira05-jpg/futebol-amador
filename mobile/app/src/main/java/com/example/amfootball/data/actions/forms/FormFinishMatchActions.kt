package com.example.amfootball.data.actions.forms

data class FormFinishMatchActions(
    val onNumGoalsTeamChange: (newNumGoalsTeam: Int) -> Unit,
    val onNumGoalsOpponentChange: (numGoalsOpponent: Int) -> Unit,
    val onSubmitForm: () -> Unit,
)

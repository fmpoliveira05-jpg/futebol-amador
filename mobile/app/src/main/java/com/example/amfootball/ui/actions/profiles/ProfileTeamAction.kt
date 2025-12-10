package com.example.amfootball.ui.actions.profiles

data class ProfileTeamAction(
    val onEditClick: () -> Unit,
    val onDeleteClick: () -> Unit,
    val retry: () -> Unit
)

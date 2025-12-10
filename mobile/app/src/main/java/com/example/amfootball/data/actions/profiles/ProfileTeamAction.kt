package com.example.amfootball.data.actions.profiles

data class ProfileTeamAction(
    val onEditClick: () -> Unit,
    val onDeleteClick: () -> Unit,
    val retry: () -> Unit
)

package com.example.amfootball.data.actions.lists

import androidx.navigation.NavHostController
import androidx.compose.runtime.State

data class LeadBoardActions(
    val onShowMore: (idTeam: String, navHostController: NavHostController) -> Unit,
    val isValidShowMoreTeams: () -> State<Boolean>,
    val onLoadMoreTeams: () -> Unit
)

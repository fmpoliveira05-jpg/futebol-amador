package com.example.amfootball.ui.actions.homePageActions

data class HomePageActions(
    val onNavigateCreateTeam: () -> Unit,
    val onNavigationToRequests: () -> Unit,
    val onNavigateToListTeams: () -> Unit,
    val onNavigateToTeamHome: () -> Unit
)

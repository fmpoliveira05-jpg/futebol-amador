package com.example.amfootball.data.actions.homePageActions

data class HomePageActions(
    val onNavigateCreateTeam: () -> Unit,
    val onNavigationToRequests: () -> Unit,
    val onNavigateToListTeams: () -> Unit,
    val onNavigateToTeamHome: () -> Unit
)

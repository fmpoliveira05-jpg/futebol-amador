package com.example.amfootball.data.actions.homePageActions

data class HomePageTeamActions(
    val onNavigateCasualMatch: () -> Unit,
    val onNavigateRankedMatch: () -> Unit,
    val onNavigateCalendar: () -> Unit,
    val onNavigateMembers: () -> Unit
)
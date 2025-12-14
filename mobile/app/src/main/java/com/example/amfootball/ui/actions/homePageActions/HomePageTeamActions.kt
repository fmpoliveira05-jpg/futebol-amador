package com.example.amfootball.ui.actions.homePageActions

data class HomePageTeamActions(
    val onNavigateCasualMatch: () -> Unit,
    val onNavigateRankedMatch: () -> Unit,
    val onNavigateCalendar: () -> Unit,
    val onNavigateMembers: () -> Unit,
    val onLeaveTeam: () -> Unit
)
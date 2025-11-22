package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

data class ItemsListTeamAction(
    val onSendMatchInvite: (idTeam: String, navHostController: NavHostController) -> Unit,
    val onSendMemberShipRequest: (idTeam: String, navHostController: NavHostController) -> Unit,
    val onShowMore: (idTeam: String, navHostController: NavHostController) -> Unit
)

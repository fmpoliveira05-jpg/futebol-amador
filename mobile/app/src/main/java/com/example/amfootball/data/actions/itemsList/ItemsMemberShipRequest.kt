package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

data class ItemsMemberShipRequest (
    val acceptMemberShipRequest: (idReceiver: String, idRequest: String,
                                  isPlayerSender: Boolean, navHostController: NavHostController) -> Unit,
    val rejectMemberShipRequest: (idReceiver: String, idRequest: String, isPlayerSender: Boolean) -> Unit,
    val showMore: (idSender: String, isPlayerSender: Boolean, navHostController: NavHostController) -> Unit
)
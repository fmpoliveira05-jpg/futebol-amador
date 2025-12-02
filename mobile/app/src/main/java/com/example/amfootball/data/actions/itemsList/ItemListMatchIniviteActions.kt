package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

data class ItemListMatchIniviteActions(
    val acceptMatchInvite: (idMatchInvite: String) -> Unit,
    val rejectMatchInvite: (idMatchInvite: String) -> Unit,
    val negociateMatchInvite: (idMatchInvite: String, navHostController: NavHostController) -> Unit,
    val showMoreDetails: (idMatchInvite: String, navHostController: NavHostController) -> Unit
)
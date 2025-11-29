package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

data class ItemsListPostPoneMatchActions(
    val acceptPostPoneMatch: (idPostPoneMatch: String) -> Unit,
    val rejectPostPoneMatch: (idPostPoneMatch: String) -> Unit,
    val showMoreInfo: (idOpponent: String, navHostController: NavHostController) -> Unit
)

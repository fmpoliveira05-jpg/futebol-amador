package com.example.amfootball.ui.actions.itemsList

data class ItemListPlayerActions(
    val onSendMembership: (playerId: String) -> Unit,
    val onShowMore: (onSucess: () -> Unit) -> Unit
)
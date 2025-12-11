package com.example.amfootball.ui.actions.lists

import kotlinx.coroutines.flow.StateFlow

data class ShowMoreItensAction(
    val isValidShowMore: () -> StateFlow<Boolean>,
    val onLoadMore: () -> Unit
)
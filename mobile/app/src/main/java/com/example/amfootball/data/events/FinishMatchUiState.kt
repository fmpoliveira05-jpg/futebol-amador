package com.example.amfootball.data.events

sealed class FinishMatchUiState {
    object Loading : FinishMatchUiState()
    object Connecting : FinishMatchUiState()
    object WaitingForConfirmation : FinishMatchUiState()
    object MatchFinished : FinishMatchUiState()
    data class Error(val msg: String) : FinishMatchUiState()
}
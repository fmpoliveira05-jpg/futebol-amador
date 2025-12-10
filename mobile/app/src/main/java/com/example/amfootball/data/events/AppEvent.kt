package com.example.amfootball.data.events

sealed interface AppEvent {
    data class TeamDeleted(val message: String) : AppEvent
    data object UserLoggedOut : AppEvent
}
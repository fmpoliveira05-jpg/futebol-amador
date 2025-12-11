package com.example.amfootball.data.remote.dtos

data class CalendarEvent(
    val title: String,
    val description: String?,
    val start: Long,
    val end: Long,
    val location: String?
)
package com.example.amfootball.data.filters

import java.time.LocalDateTime

data class FilterMatchInvite(
    val senderName: String? = null,
    val minDate: LocalDateTime? = null,
    val maxDate: LocalDateTime? = null
)
package com.example.amfootball.data.dtos.filters

import java.time.LocalDateTime

data class FilterPostPoneMatch(
    val nameOpponent: String? = null,
    val isHome: Boolean? = null,
    val minDataGame: LocalDateTime? = null,
    val maxDateGame: LocalDateTime? = null,
    val minDatePostPone: LocalDateTime? = null,
    val maxDatePostPone: LocalDateTime? = null
)
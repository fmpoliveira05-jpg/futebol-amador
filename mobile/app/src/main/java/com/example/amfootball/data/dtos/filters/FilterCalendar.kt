package com.example.amfootball.data.dtos.filters

import com.example.amfootball.data.enums.TypeMatch
import java.time.LocalDateTime

data class FilterCalendar(
    val opponentName: String? = null,
    val minGameDate: LocalDateTime? = null,
    val maxGameDate: LocalDateTime? = null,
    val gameLocale: Boolean? = null,
    val typeMatch: TypeMatch? = null,
    val isFinish: Boolean? = null,
)

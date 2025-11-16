package com.example.amfootball.data.dtos.suporrtDto

import com.example.amfootball.data.enums.MatchResult

data class TeamStatisticsDto(
    val id: String,
    val infoTeam: TeamDto,
    val numGoals: Int = 0,
    val matchResult: MatchResult = MatchResult.UNDEFINED,
)

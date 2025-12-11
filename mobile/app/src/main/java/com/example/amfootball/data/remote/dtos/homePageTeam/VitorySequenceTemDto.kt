package com.example.amfootball.data.remote.dtos.homePageTeam

import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.domains.enums.match.MatchResult
data class VitorySequenceTemDto(
    val opponent: TeamDto,
    val result: String,
    val matchResultId: Int
) {
    val matchResult: MatchResult
        get() = when (matchResultId) {
            0 -> MatchResult.WIN
            1 -> MatchResult.DRAW
            2 -> MatchResult.LOSE
            else -> MatchResult.UNDEFINED
        }
}
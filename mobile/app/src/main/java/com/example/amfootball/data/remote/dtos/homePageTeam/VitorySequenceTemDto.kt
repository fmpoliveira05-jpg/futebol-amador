package com.example.amfootball.data.remote.dtos.homePageTeam

import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.domains.enums.match.MatchResult
import com.google.gson.annotations.SerializedName

data class VitorySequenceTemDto(
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamDto = TeamDto(),
    @SerializedName("Result", alternate = ["result"])
    val result: String = "",
    @SerializedName("MatchResult", alternate = ["matchResult"])
    val matchResultId: Int = 0
) {
    val matchResult: MatchResult
        get() = when (matchResultId) {
            0 -> MatchResult.WIN
            1 -> MatchResult.LOSE
            2 -> MatchResult.DRAW
            else -> MatchResult.UNDEFINED
        }
}
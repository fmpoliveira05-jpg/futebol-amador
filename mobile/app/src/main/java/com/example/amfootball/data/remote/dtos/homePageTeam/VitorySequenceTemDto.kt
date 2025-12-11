package com.example.amfootball.data.remote.dtos.homePageTeam

import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.domains.enums.match.MatchResult
import com.google.gson.annotations.SerializedName

data class VitorySequenceTemDto(
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamDto,
    @SerializedName("Result")
    val result: String,
    @SerializedName("MatchResult", alternate = ["matchResult"])
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
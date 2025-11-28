package com.example.amfootball.data.dtos.postponeMatch

import com.example.amfootball.data.dtos.support.TeamDto
import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime

data class PostponeDto(
    @SerializedName("IdMatch", alternate = ["idMatch"])
    val idMatch: String,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val gameDate: LocalDateTime,
    @SerializedName("PostPoneDate", alternate = ["postPoneDate"])
    val postponeDate: LocalDateTime,
    @SerializedName("Team", alternate = ["team"])
    val team: TeamDto,
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamDto,
)
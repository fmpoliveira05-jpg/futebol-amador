package com.example.amfootball.data.remote.dtos.postponeMatch

import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime

data class PostPoneReturn(
    @SerializedName("IdMatch")
    val idMatch: String,
    @SerializedName("StatusPostPone")
    val gameDate: LocalDateTime,
    @SerializedName("NameTeam")
    val nameTeam: String,
    @SerializedName("NameOpponent")
    val nameOpponent: String,
    @SerializedName("NamePitch")
    val namePitch: String,
)

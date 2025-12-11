package com.example.amfootball.data.remote.dtos.postponeMatch

import com.example.amfootball.domains.enums.StatusPostPone
import com.google.gson.annotations.SerializedName

data class PostPoneResponse(
    @SerializedName("IdMatch")
    val idMatch: String,
    @SerializedName("StatusPostPone")
    val statusPostPone: StatusPostPone,
    @SerializedName("IdTeam")
    val idTeam: String,
    @SerializedName("IdOpponent")
    val idOpponent: String
)
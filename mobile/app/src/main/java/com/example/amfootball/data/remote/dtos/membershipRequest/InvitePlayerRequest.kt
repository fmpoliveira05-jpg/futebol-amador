package com.example.amfootball.data.remote.dtos.membershipRequest

import com.google.gson.annotations.SerializedName

data class InvitePlayerRequest(
    @SerializedName("PlayerId", alternate = ["playerId"])
    val playerId: String
)

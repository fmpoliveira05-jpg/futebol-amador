package com.example.amfootball.data.remote.dtos.membershipRequest

import com.google.gson.annotations.SerializedName

data class InviteTeamRequest(
    @SerializedName("TeamId")
    val teamId: String
)

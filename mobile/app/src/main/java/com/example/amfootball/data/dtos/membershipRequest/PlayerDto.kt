package com.example.amfootball.data.dtos.membershipRequest

import com.google.gson.annotations.SerializedName

data class PlayerDto(
    @SerializedName("Id", alternate = ["id"])
    val id: String,
    @SerializedName("Name", alternate = ["name"])
    val name: String
)

package com.example.amfootball.data.dtos.player

import com.google.gson.annotations.SerializedName

data class PlayerProfileDto(
    val name: String,
    val dateOfBirth: String?,
    val icon: String? = null,
    val address: String,
    val phoneNumber: String,
    val email: String,
    val position: String,
    val height: Int,
    @SerializedName("TeamName")
    val team: String
)
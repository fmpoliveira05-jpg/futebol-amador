package com.example.amfootball.data.dtos.player

import com.google.gson.annotations.SerializedName
import java.time.LocalDate

data class CreateProfileDto(
    @SerializedName("Name")
    val userName: String,
    @SerializedName("DateOfBirth")
    val dateOfBirth: String,
    @SerializedName("Phone")
    val phone: String,
    @SerializedName("Email")
    val email: String,
    @SerializedName("Password")
    val password: String,
    @SerializedName("Address")
    val address: String,
    @SerializedName("Position")
    val position: Int,
    @SerializedName("Height")
    val height: Int
)

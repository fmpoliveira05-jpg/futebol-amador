package com.example.amfootball.data.dtos

import com.google.gson.annotations.SerializedName

data class FullProfileDto(
    val userProfile: UserProfileDto,
    val playerProfile: PlayerProfileDto
    )

data class PlayerProfileDto(
    val userName: String,
    val teamName: String,
    val position: String,
    val isAdmin: Boolean,
    val height: Int
)

data class UserProfileDto(
    val userName: String,
    val dateOfBirth: String,
    val phone: String,
    val email: String,
    val address: String,
)

data class CreateProfileDto(
    @SerializedName("Name")
    val userName: String,
    @SerializedName("DateOfBirth")
    val dateOfBirth: String,
    @SerializedName("Phone")
    val phone: String,
    @SerializedName("Email")
    val email: String,
    @SerializedName("Address")
    val address: String,
    @SerializedName("Position")
    val position: Int,
    @SerializedName("Height")
    val height: Int
)
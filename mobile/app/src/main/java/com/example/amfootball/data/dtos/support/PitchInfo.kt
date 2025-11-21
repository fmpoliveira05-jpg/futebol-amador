package com.example.amfootball.data.dtos.support

import com.google.gson.annotations.SerializedName

data class PitchInfo(
    @SerializedName("Name", alternate = ["name"])
    val name: String = "",
    @SerializedName("Address", alternate = ["address"])
    val address: String = ""
)
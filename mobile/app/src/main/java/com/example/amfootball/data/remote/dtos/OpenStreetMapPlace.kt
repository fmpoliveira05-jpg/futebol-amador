package com.example.amfootball.data.remote.dtos

import com.google.gson.annotations.SerializedName

data class OpenStreetMapPlace(
    @SerializedName("place_id") val placeId: Long,
    @SerializedName("display_name") val displayName: String,
    @SerializedName("lat") val lat: String,
    @SerializedName("lon") val lon: String
)
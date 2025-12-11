package com.example.amfootball.domains.enums

import com.google.gson.annotations.SerializedName

enum class StatusPostPone(val value: Int) {
    @SerializedName("0")
    REJECTED(0),
    @SerializedName("1")
    ACCEPTED(1);
}
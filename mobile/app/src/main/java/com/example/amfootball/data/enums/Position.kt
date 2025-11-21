package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R
import com.google.gson.annotations.SerializedName

enum class Position(@StringRes val stringId: Int) {
    @SerializedName("0", alternate = ["GoalKeeper", "Goalkeeper"]) // Aceita nº 0 ou String
    GOALKEEPER(stringId = R.string.position_goalkeeper),
    @SerializedName("1", alternate = ["Defender"])
    DEFENDER(stringId = R.string.position_defender),
    @SerializedName("2", alternate = ["Midfielder"])
    MIDFIELDER(stringId = R.string.position_midfields),
    @SerializedName("3", alternate = ["Forward"])
    FORWARD(stringId = R.string.position_forward),
}
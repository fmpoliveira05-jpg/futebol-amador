package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R

enum class Position(@StringRes val stringId: Int) {
    FORWARD(stringId = R.string.position_forward),
    MIDFIELDER(stringId = R.string.position_midfields),
    DEFENDER(stringId = R.string.position_defender),
    GOALKEEPER(stringId = R.string.position_goalkeeper),
}
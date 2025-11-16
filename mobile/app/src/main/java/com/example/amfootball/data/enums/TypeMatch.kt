package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R

enum class TypeMatch(@StringRes val stringId: Int) {
    COMPETITIVE(stringId = R.string.type_match_competitive),
    CASUAL(stringId =  R.string.type_match_casual)
}
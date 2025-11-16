package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R
enum class MatchResult(@StringRes val stringId: Int) {
    WIN(stringId = R.string.match_result_win),
    DRAW(stringId = R.string.match_result_draw),
    LOSE(stringId = R.string.match_result_lose),
    UNDEFINED(stringId = R.string.match_result_undefined),
}
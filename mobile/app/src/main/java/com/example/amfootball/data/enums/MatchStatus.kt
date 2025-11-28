package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R

enum class MatchStatus(@StringRes val stringId: Int) {
    SCHEDULED(R.string.match_status_scheduled),
    IN_PROGRESS(R.string.match_status_in_progress),
    DONE(R.string.match_status_done),
    POST_PONED(R.string.match_status_post_poned),
    CANCELED(R.string.match_status_canceled)
}
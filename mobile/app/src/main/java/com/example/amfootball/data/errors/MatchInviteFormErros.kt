package com.example.amfootball.data.errors

import androidx.annotation.StringRes

data class MatchInviteFormErros(
    @StringRes val dateError: Int? = null,
    @StringRes val timeError: Int? = null
)
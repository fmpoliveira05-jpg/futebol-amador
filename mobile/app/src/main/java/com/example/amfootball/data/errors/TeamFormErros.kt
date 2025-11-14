package com.example.amfootball.data.errors

import androidx.annotation.StringRes

data class TeamFormErros(
    @StringRes val nameError: Int? = null,
    @StringRes val descriptionError: Int? = null,
    @StringRes val pitchNameError: Int? = null,
    @StringRes val pitchAddressError: Int? = null
)

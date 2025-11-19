package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

data class FilterPlayersErrors(
    val nameError: ErrorMessage? = null,
    val cityError: ErrorMessage? = null,
    val minAgeError: ErrorMessage? = null,
    val maxAgeError: ErrorMessage? = null,
    val minSizeError: ErrorMessage? = null,
    val maxSizeError: ErrorMessage? = null,
)

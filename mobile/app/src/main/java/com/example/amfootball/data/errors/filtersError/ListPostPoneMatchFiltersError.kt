package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

data class ListPostPoneMatchFiltersError(
    val nameOpponentError: ErrorMessage? = null,
    val minDateGameError: ErrorMessage? = null,
    val maxDateGameError: ErrorMessage? = null,
    val minDatePostPoneError: ErrorMessage? = null,
    val maxDatePostPoneError: ErrorMessage? = null
)

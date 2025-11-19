package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

data class FilterMemberShipRequestError(
    val senderNameError: ErrorMessage? = null,
    val minDateError: ErrorMessage? = null,
    val maxDateError: ErrorMessage? = null
)

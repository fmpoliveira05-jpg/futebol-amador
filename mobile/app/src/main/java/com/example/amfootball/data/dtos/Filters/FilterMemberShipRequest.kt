package com.example.amfootball.data.dtos.Filters

import java.util.Date

data class FilterMemberShipRequest(
    val senderName: String?,

    val minDate: Date?,

    val maxDate: Date?
)

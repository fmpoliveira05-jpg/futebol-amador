package com.example.amfootball.data.filters

import com.example.amfootball.data.enums.Position

data class FilterListPlayerDto(
    val name: String? = null,
    val city: String? = null,
    val minAge: Int? = null,
    val maxAge: Int? = null,
    val position: Position? = null,
    val minSize: Int? = null,
    val maxSize: Int? = null,
)
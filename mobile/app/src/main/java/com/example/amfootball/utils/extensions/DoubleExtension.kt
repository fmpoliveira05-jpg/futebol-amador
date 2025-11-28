package com.example.amfootball.utils.extensions

import java.util.Locale

fun Double.toOneDecimal(): String {
    return String.format(Locale.getDefault(), "%.1f", this)
}
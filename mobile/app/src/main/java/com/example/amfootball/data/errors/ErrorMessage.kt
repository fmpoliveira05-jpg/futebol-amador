package com.example.amfootball.data.errors

data class ErrorMessage(
    val messageId: Int,
    val args: List<Any> = emptyList()
)

package com.example.amfootball.data.dtos.chat

import androidx.compose.ui.graphics.Color

data class GroupMessageDto(
    val id: String,
    val text: String,
    val senderName: String,
    val senderColor: Color,
    val isSentByMe: Boolean,
    val timestamp: String
) {

}

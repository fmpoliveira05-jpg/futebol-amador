package com.example.amfootball.ui.previewsMocks

import com.example.amfootball.data.remote.dtos.chat.ChatRoom
import com.example.amfootball.data.remote.dtos.chat.MessageDto

object ChatListMocks{
    val mockChatRooms = listOf(
        ChatRoom(id = "1", name = "Equipa A"),
        ChatRoom(id = "2", name = "Treinadores"),
        ChatRoom(id = "3", name = "Bernardo Silva")
    )
}

object ChatMocks{
    val mockMessagesPt = listOf(
        MessageDto(text = "Boas! Tudo bem?", senderId = "other"),
        MessageDto(text = "Tudo tranquilo! E contigo?", senderId = "me"),
        MessageDto(text = "Bora jogar logo?", senderId = "other"),
    )

    val mockMessagesEn = listOf(
        MessageDto(text = "Hey! How are you?", senderId = "other"),
        MessageDto(text = "I'm good! And you?", senderId = "me"),
        MessageDto(text = "Ready for the match?", senderId = "other"),
    )
}
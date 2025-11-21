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
    companion object {
        fun generateExempleChat():List<GroupMessageDto> {
            return listOf(
                GroupMessageDto(
                    id = "1",
                    text = "Boas pessoal, a que horas é o jogo?",
                    senderName = "Eu",
                    senderColor = Color.Gray,
                    isSentByMe = true,
                    timestamp = "18:30"
                ),
                GroupMessageDto(
                    id = "2",
                    text = "Acho que ficou marcado para as 20h.",
                    senderName = "João Silva",
                    senderColor = Color(0xFF6200EE), // Roxo
                    isSentByMe = false,
                    timestamp = "18:32"
                ),
                GroupMessageDto(
                    id = "3",
                    text = "Confirmado! Não se esqueçam das chuteiras.",
                    senderName = "Pedro Santos",
                    senderColor = Color(0xFF03DAC5), // Verde Água
                    isSentByMe = false,
                    timestamp = "18:35"
                ),
                GroupMessageDto(
                    id = "4",
                    text = "Vou levar a bola nova.",
                    senderName = "Eu",
                    senderColor = Color.Gray,
                    isSentByMe = true,
                    timestamp = "18:36"
                ),
                GroupMessageDto(
                    id = "5",
                    text = "Grande máquina! Até logo.",
                    senderName = "Rui Costa",
                    senderColor = Color(0xFFB00020), // Vermelho
                    isSentByMe = false,
                    timestamp = "18:40"
                )
            )
        }
    }
}

package com.example.amfootball.data.dtos.chat

data class MessageDto(
    val id: String,
    val text: String,
    val isSentByMe: Boolean,
    val timestamp: String
) {
    companion object{
        fun generateMessageDtoList(): List<MessageDto> {
            val mockMessageDtos = listOf(
                MessageDto("1", "Olá! Tudo bem?", isSentByMe = false, "10:00"),
                MessageDto("2", "Tudo ótimo! E com você?", isSentByMe = true, "10:01"),
                MessageDto("3", "Estou bem também, obrigado por perguntar.", isSentByMe = false, "10:01"),
                MessageDto("4", "Você viu o novo layout do app? Achei incrível!", isSentByMe = false, "10:02"),
                MessageDto("5", "Vi sim! O time de design fez um trabalho excelente.", isSentByMe = true, "10:03"),
                MessageDto("6", "Com certeza. A navegação está muito mais fluida.", isSentByMe = true, "10:03"),
                MessageDto("7", "Verdade!", isSentByMe = false, "10:04")
            )
            return mockMessageDtos
        }
    }
}

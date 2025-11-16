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
        fun generateExempleChat(): List<GroupMessageDto> {
            val colorAdm = Color(0xFFE91E63)
            val colorGoleiro = Color(0xFF2196F3)
            val colorRiso = Color(0xFFFF9800)
            val colorMe = Color(0xFF4CAF50)

            val mockMessages = listOf(
                GroupMessageDto("1", "Fala galera! Lista de Domingo. Quem vai?", "Luiz (Adm)", colorAdm, false, "09:30"),
                GroupMessageDto("2", "1 - Luiz\n2 - ...", "Luiz (Adm)", colorAdm, false, "09:30"),
                GroupMessageDto("3", "Tô dentro! 🙋‍♂️", "João", colorGoleiro, false, "09:32"),
                GroupMessageDto("4", "Coloca meu nome aí. O goleiro confirmou?", "Marcão", colorRiso, false, "09:35"),
                GroupMessageDto("5", "Confirmou sim, ele vem.", "Luiz (Adm)", colorAdm, false, "09:36"),
                GroupMessageDto("6", "Deixem somente ver se consigo!", "Eu", colorMe, true, "09:40"),
                GroupMessageDto("7", "Tambem irei.Podem contar comigo!", "Luiz (Adm)", colorAdm, false, "09:41"),
                GroupMessageDto("8", "Vou levar meu primo, joga muito.", "Marcão", colorRiso, false, "09:45"),
                GroupMessageDto("9", "Se jogar igual você tamo ferrado kkkkk", "João", colorGoleiro, false, "09:46"),
                GroupMessageDto("10", "Domingo não consigo, podemos remarcar?", "Eu", colorMe, true, "09:50")
            )
            return mockMessages
        }
    }
}

package com.example.amfootball.data.dtos.chat

import androidx.compose.ui.graphics.Color
import com.google.firebase.firestore.DocumentId

data class MessageDto(
    @DocumentId val id: String = "",
    val text: String = "",
    val senderId: String = "",
    val timestamp: Any? = null,

){
    companion object {
        fun generateExempleChat(): List<MessageDto> {
            val colorAdm = Color(0xFFE91E63)
            val colorGoleiro = Color(0xFF2196F3)
            val colorRiso = Color(0xFFFF9800)
            val colorMe = Color(0xFF4CAF50)

            val mockMessages = listOf(
                MessageDto("1", "Fala galera! Lista de Domingo. Quem vai?", "Luiz (Adm)", "09:30"),
                MessageDto("2", "1 - Luiz\n2 - ...", "Luiz (Adm)", "09:30"),
                MessageDto("3", "Tô dentro! 🙋‍♂️", "João", "09:32"),
                MessageDto("4", "Coloca meu nome aí. O goleiro confirmou?", "Marcão", "09:35"),
                MessageDto("5", "Confirmou sim, ele vem.", "Luiz (Adm)", "09:36"),
                MessageDto("6", "Deixem somente ver se consigo!", "Eu", "09:40"),
                MessageDto("7", "Tambem irei.Podem contar comigo!", "Luiz (Adm)", "09:41"),
                MessageDto("8", "Vou levar meu primo, joga muito.", "Marcão", "09:45"),
                MessageDto("9", "Se jogar igual você tamo ferrado kkkkk", "João", "09:46"),
                MessageDto("10", "Domingo não consigo, podemos remarcar?", "Eu", "09:50")
            )
            return mockMessages
        }
    }
}

data class ChatRoom(
    @DocumentId val id: String = "",
    val name: String = "",
    val members: List<String> = emptyList(),
    val createdBy: String = ""
){
    constructor() : this("", "", emptyList(), "")
}


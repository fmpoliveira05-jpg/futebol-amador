package com.example.amfootball.data.dtos.chat

data class ItemListChatDto(
    val id: Int,
    val name: String,
    val lastMessage: String,
    val time: String,
    val unreadCount: Int = 0,
    val isOnline: Boolean = false
) {
    companion object{
        fun generateListChat(): List<ItemListChatDto> {
            val sampleChats = listOf(
                ItemListChatDto(1, "FooFigthers VS Gorilazz", "João: É possivel remarcar para domingo as 10?", "10:30", 2, true),
                ItemListChatDto(2, "The Simpsons VS Friends", "Antonio: Sexta-feira não poderei.", "09:15", 5),
                ItemListChatDto(3, "João Dev", "O PR foi aprovado, pode fazer o merge.", "Ontem", 0, true),
                ItemListChatDto(4, "Suporte Técnico", "Seu ticket #4829 foi resolvido.", "Ontem", 0),
                ItemListChatDto(5, "Maria Oliveira", "Enviei as fotos do projeto.", "Segunda", 1),
                ItemListChatDto(6, "Bruno Academia", "Bora treinar hoje?", "Domingo", 0),
                ItemListChatDto(7, "Clube do Livro", "A próxima leitura será 'Duna'.", "Sábado", 12),
                ItemListChatDto(8, "Lucas Design", "Vou te mandar o Figma atualizado.", "Sexta", 0, true)
            )
            return sampleChats
        }
    }
}
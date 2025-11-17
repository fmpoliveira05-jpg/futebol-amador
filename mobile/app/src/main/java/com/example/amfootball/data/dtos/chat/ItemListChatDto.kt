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
            )
            return sampleChats
        }
    }
}
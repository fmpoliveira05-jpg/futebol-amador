package com.example.amfootball.data.dtos.chat

/**
 * Objeto de Transferência de Dados (DTO) que representa o resumo de uma conversa na lista de chats.
 *
 * Este objeto é otimizado para a visualização em listas (Inbox), contendo apenas os metadados
 * essenciais para apresentar uma pré-visualização da sala de chat, como o snippet da última mensagem
 * e contadores de notificações.
 *
 * @property id O identificador único numérico da conversa/sala.
 * @property name O título de exibição da conversa (geralmente o nome das equipas, ex: "Equipa A vs Equipa B").
 * @property lastMessage O texto da última mensagem trocada, usado para pré-visualização (snippet) na lista.
 * @property time A representação textual da hora ou data da última mensagem (ex: "10:30" ou "Ontem").
 * @property unreadCount O número de mensagens não lidas nesta conversa (Badge count). A predefinição é 0.
 * @property isOnline Indica se o interlocutor ou a sala está ativa/online (Status indicator). A predefinição é `false`.
 */
data class ItemListChatDto(
    val id: Int,
    val name: String,
    val lastMessage: String,
    val time: String,
    val unreadCount: Int = 0,
    val isOnline: Boolean = false
)
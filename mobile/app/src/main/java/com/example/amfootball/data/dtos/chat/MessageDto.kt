package com.example.amfootball.data.dtos.chat

import com.google.firebase.firestore.DocumentId

/**
 * Representa um documento de mensagem individual armazenado na coleção de chat do Firestore.
 *
 * Esta classe funciona como um POJO (Plain Old Java Object) para o mapeamento automático de dados
 * pelo SDK do Firebase. Todos os campos possuem valores por defeito para garantir a existência
 * de um construtor vazio (no-argument constructor), requisito obrigatório para a deserialização do Firestore.
 *
 * @property id O identificador único do documento da mensagem.
 * A anotação [@DocumentId] indica que este campo não é guardado dentro do JSON, mas é
 * preenchido automaticamente com o ID do documento ao ler da base de dados.
 *
 * @property text O conteúdo textual da mensagem enviada.
 *
 * @property senderId O identificador único (UID) do utilizador ou equipa que enviou a mensagem.
 *
 * @property timestamp O carimbo de data/hora da mensagem.
 * É definido como [Any]? para suportar dois tipos de dados distintos:
 * 1. `FieldValue.serverTimestamp()` durante a escrita (para que o servidor defina a hora).
 * 2. `com.google.firebase.Timestamp` durante a leitura (formato em que o Firestore devolve os dados).
 */
data class MessageDto(
    @DocumentId val id: String = "",
    val text: String = "",
    val senderId: String = "",
    val timestamp: Any? = null,
)

/**
 * Representa um documento de sala de chat (Chat Room) no Firestore.
 *
 * Define a estrutura de um grupo ou conversa, contendo os metadados da sala e a lista de participantes.
 *
 * @property id O identificador único da sala de chat.
 * Preenchido automaticamente através da anotação [@DocumentId] com a chave do documento.
 *
 * @property name O nome de exibição da sala (ex: "Negociações - Final").
 *
 * @property members Uma lista de Strings contendo os IDs dos utilizadores ou equipas que
 * pertencem a esta sala. Utilizado frequentemente para regras de segurança (Security Rules)
 * e filtragem de queries (ex: `array-contains`).
 */
data class ChatRoom(
    @DocumentId val id: String = "",
    val name: String = "",
    val members: List<String> = emptyList()
)
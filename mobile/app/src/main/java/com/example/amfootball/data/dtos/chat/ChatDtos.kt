package com.example.amfootball.data.dtos.chat

/**
 * Estrutura de dados enviada no corpo (body) de um pedido para criar uma nova sala de chat.
 *
 * Este DTO contém as informações essenciais para inicializar um grupo de conversação no backend,
 * definindo o seu nome e quais as equipas participantes.
 *
 * @property roomName O nome de exibição da sala de chat a ser criada (ex: "Liga de Verão", "Negociação").
 * @property teamIds Uma lista contendo os identificadores únicos (IDs) das equipas que serão adicionadas a esta sala.
 */
data class CreateRoomRequest(
    val roomName: String,
    val teamIds: List<String>
)

/**
 * Estrutura de dados recebida como resposta após a criação bem-sucedida de uma sala de chat.
 *
 * @property roomId O identificador único gerado pelo backend para a sala recém-criada.
 * Este ID deve ser utilizado posteriormente para subscrever tópicos de socket ou enviar mensagens.
 */
data class CreateRoomResponse(
    val roomId: String
)
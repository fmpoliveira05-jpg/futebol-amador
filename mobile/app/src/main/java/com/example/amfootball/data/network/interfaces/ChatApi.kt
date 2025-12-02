package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.chat.CreateRoomRequest
import com.example.amfootball.data.dtos.chat.CreateRoomResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

/**
 * Interface de API responsável pelas funcionalidades de comunicação e Chat.
 *
 * Gere o ciclo de vida das salas de conversação (criação e recuperação)
 * permitindo a troca de mensagens entre jogadores ou grupos.
 */
interface ChatApi {
    /**
     * Cria uma nova sala de chat privada ou retorna uma existente.
     *
     * Endpoint: POST api/chat/create-room
     *
     * @param request O corpo do pedido contendo os IDs dos participantes.
     * @return [Response] contendo o ID da sala criada ou recuperada.
     */
    @POST("${BaseEndpoints.chatApi}/create-room")
    suspend fun createChatRoom(
        @Body request: CreateRoomRequest
    ): Response<CreateRoomResponse>
}
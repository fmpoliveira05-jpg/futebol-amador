package com.example.amfootball.data.Interfaces

import com.example.amfootball.data.Dtos.CreateRoomRequest
import com.example.amfootball.data.Dtos.CreateRoomResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.Header
import retrofit2.http.POST

// Isto é apenas um contrato, não tem lógica
interface ApiBackend {

    @POST("/api/chat/create-room")
    suspend fun createChatRoom(
        @Header("Authorization") token: String,
        @Body request: CreateRoomRequest
    ): Response<CreateRoomResponse>
}
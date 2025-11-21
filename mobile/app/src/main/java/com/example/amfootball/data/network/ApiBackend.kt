package com.example.amfootball.data.network

import com.example.amfootball.data.dtos.chat.CreateRoomRequest
import com.example.amfootball.data.dtos.chat.CreateRoomResponse
import com.example.amfootball.data.dtos.CreateProfileDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.dtos.team.ProfileTeamInfoDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

// Isto é apenas um contrato, não tem lógica
interface ApiBackend{
    /**
     * Chama o nosso endpoint de backend /api/users/create-profile
     * para sincronizar o perfil do Firebase com a nossa base de dados SQL Server.
     *
     * @param authToken O token JWT obtido do Firebase
     * @param request O corpo da requisição com os dados do perfil
     */
    //confirmar se não precisa da / antes da api na rota
    @POST("api/Player/create-profile")
    suspend fun createProfile(
        @Body request: CreateProfileDto
    ): Response<Unit>

    @POST("/api/chat/create-room")
    suspend fun createChatRoom(
        @Body request: CreateRoomRequest
    ): Response<CreateRoomResponse>

    @GET("api/Player/get-my-profile")
    suspend fun getMyProfile(
    ): Response<CreateProfileDto>

    //Player
    /**
    * Vai buscar há API um player através do seu ID
    * */
    @GET("api/Player/{playerId}")
    suspend fun getPlayerProfile(
        @Path("playerId") playerId: String
    ): Response<PlayerProfileDto>


    //Teams
    /**
     * Vai buscar há API uma equipa pelo ID
     * */
    @GET("api/Team/{id}")
    suspend fun getTeamProfile(
        @Path("id") teamId: String
    ): Response<ProfileTeamInfoDto>

}
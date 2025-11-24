package com.example.amfootball.data.network

import com.example.amfootball.data.dtos.chat.CreateRoomRequest
import com.example.amfootball.data.dtos.chat.CreateRoomResponse
import com.example.amfootball.data.dtos.CreateProfileDto
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.QueryMap

/**
 * Interface de definição da API Retrofit.
 *
 * Contém todos os endpoints para comunicação com o Backend.
 * Esta interface não contém lógica, apenas a assinatura dos métodos HTTP (GET, POST, etc.).
 */
interface ApiBackend{
    // ============================================================================================
    //  PERFIL & AUTENTICAÇÃO
    // ============================================================================================

    /**
     * Sincroniza o perfil criado no Firebase com a base de dados SQL Server do Backend.
     * Deve ser chamado logo após o registo inicial.
     *
     * Endpoint: POST api/Player/create-profile
     *
     * @param request O DTO contendo os dados iniciais do perfil do utilizador.
     * @return [Response] vazia (Unit) indicando sucesso (200 OK) ou falha.
     */
    //confirmar se não precisa da / antes da api na rota
    @POST("api/Player/create-profile")
    suspend fun createProfile(
        @Body request: CreateProfileDto
    ): Response<Unit>

    /**
     * Obtém o perfil do utilizador atualmente autenticado.
     * O token de autenticação é enviado automaticamente no Header via Interceptor.
     *
     * Endpoint: GET api/Player/get-my-profile
     *
     * @return [Response] contendo os dados do perfil do próprio utilizador.
     */
    @GET("api/Player/get-my-profile")
    suspend fun getMyProfile(
    ): Response<CreateProfileDto>

    // ============================================================================================
    //  CHAT
    // ============================================================================================

    /**
     * Cria uma nova sala de chat privada ou retorna uma existente.
     *
     * Endpoint: POST api/chat/create-room
     *
     * @param request O corpo do pedido contendo os IDs dos participantes.
     * @return [Response] contendo o ID da sala criada ou recuperada.
     */
    @POST("api/chat/create-room")
    suspend fun createChatRoom(
        @Body request: CreateRoomRequest
    ): Response<CreateRoomResponse>

    // ============================================================================================
    //  PLAYERS (JOGADORES)
    // ============================================================================================

    /**
     * Obtém os detalhes públicos completos de um jogador específico.
     *
     * Endpoint: GET api/Player/details/{playerId}
     *
     * @param playerId O identificador único (UUID) do jogador.
     * @return [Response] contendo o perfil detalhado do jogador [PlayerProfileDto].
     */
    @GET("api/Player/details/{playerId}")
    suspend fun getPlayerProfile(
        @Path("playerId") playerId: String
    ): Response<PlayerProfileDto>

    /**
     * Pesquisa e lista jogadores com base em filtros.
     *
     * Endpoint: GET api/Player/listPlayers
     *
     * @param filters Um mapa de parâmetros de consulta (Query Params).
     * Ex: ["name" -> "Joao", "city" -> "Porto"].
     * @return [Response] com uma lista resumida de jogadores [InfoPlayerDto].
     */
    @GET("api/Player/listPlayers")
    suspend fun getPlayersList(
        @QueryMap filters: Map<String, String>
    ): Response<List<InfoPlayerDto>>

    // ============================================================================================
    //  TEAMS (EQUIPAS)
    // ============================================================================================

    /**
     * Cria uma nova equipa na plataforma.
     *
     * Endpoint: POST api/Team
     *
     * @param team O DTO com os dados do formulário de criação da equipa.
     * @return [Response] contendo o ID da nova equipa (String).
     */
    @POST("api/Team")
    suspend fun createTeam(
        @Body team: FormTeamDto
    ): Response<String>

    @PUT("api/Team/{teamId}")
    suspend fun updateTeam(
        @Path("teamId") teamId: String,
        @Body team: FormTeamDto
    ): Response<FormTeamDto>

    /**
     * Obtém o perfil completo e detalhado de uma equipa.
     * Inclui dados do estádio, ranking e estatísticas.
     *
     * Endpoint: GET api/Team/{id}
     *
     * @param teamId O identificador único (UUID) da equipa.
     * @return [Response] contendo o perfil da equipa [ProfileTeamDto].
     */
    @GET("api/Team/{id}")
    suspend fun getTeamProfile(
        @Path("id") teamId: String
    ): Response<ProfileTeamDto>

    /**
     * Pesquisa e lista equipas com base em filtros.
     *
     * Endpoint: GET api/Team/listTeams
     *
     * @param filters Um mapa de parâmetros de consulta para filtragem.
     * Ex: ["NameTeam" -> "Lions", "City" -> "Lisboa"].
     * @return [Response] com uma lista resumida de equipas [ItemTeamInfoDto].
     */
    @GET("api/Team/listTeams")
    suspend fun getListTeam(
        @QueryMap filters: Map<String, String>
    ): Response<List<ItemTeamInfoDto>>
}
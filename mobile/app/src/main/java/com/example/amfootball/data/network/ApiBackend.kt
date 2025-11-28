package com.example.amfootball.data.network

import com.example.amfootball.data.dtos.chat.CreateRoomRequest
import com.example.amfootball.data.dtos.chat.CreateRoomResponse
import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.SendMatchInviteDto
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.dtos.player.LoginDto
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.filters.FilterMembersTeam
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.HTTP
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
    ): Response<PlayerProfileDto>

    @POST("api/User/login")
    suspend fun loginUser(
        @Body request: LoginDto
    ): Response<PlayerProfileDto>
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


    //TODO: Validar se a Response é Unit ou se é um DTO
    @POST("api/Player/{playerId}/membership-requests/send")
    suspend fun sendMemberShipRequestToTeam(
        @Path("playerId") playerId: String,
        @Body teamId: String
    ): Response<MembershipRequestInfoDto>

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

    /**
     * Atualiza os dados de uma equipa existente.
     *
     * Este método envia um pedido PUT para substituir ou atualizar as informações
     * da equipa identificada pelo [teamId].
     *
     * @param teamId O ID único da equipa a ser atualizada.
     * @param team O objeto [FormTeamDto] contendo os novos dados da equipa (nome, sigla, etc.).
     * @return Um [Response] contendo o [FormTeamDto] com os dados atualizados confirmados pelo servidor.
     */
    @PUT("api/Team/{teamId}")
    suspend fun updateTeam(
        @Path("teamId") teamId: String,
        @Body team: FormTeamDto
    ): Response<FormTeamDto>

    @GET("api/Team/opponent/{teamId}")
    suspend fun getOpponentTeam(
        @Path("teamId") teamId: String
    ): Response<TeamDto>


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

    /**
     * Obtém a lista de membros de uma equipa específica, aplicando filtros de pesquisa.
     *
     * @param teamId O ID da equipa da qual se pretende listar os membros.
     * @param filter O objeto [FilterMembersTeam] contendo os critérios de filtragem (nome, idade, posição, etc.).
     * @return Um [Response] contendo uma lista de [MemberTeamDto] correspondente aos filtros.
     */
    @GET("api/Team/{teamId}/members")
    suspend fun getListMembers(
        @Path("teamId") teamId: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<MemberTeamDto>>

    /**
     * Remove (expulsa) um jogador de uma equipa.
     *
     * Apenas administradores da equipa devem ter permissão para executar esta ação.
     *
     * @param teamId O ID da equipa.
     * @param playerId O ID do jogador que será removido da equipa.
     */
    @DELETE("api/Team/{teamId}/members/{playerIdToRemove}")
    suspend fun removePlayerforTeam(
        @Path("teamId") teamId: String,
        @Path("playerIdToRemove") playerId: String
    ): Response<Unit>

    /**
     * Promove um membro da equipa a Administrador.
     *
     * Altera o tipo de membro de PLAYER para ADMIN_TEAM.
     *
     * @param teamId O ID da equipa.
     * @param playerId O ID do jogador que será promovido a administrador.
     */
    @PUT("api/Team/{teamId}/members/{playerId}/promote/{playerIdToPromote}")
    suspend fun promotePlayer(
        @Path("teamId") teamId: String,
        @Path("playerIdToPromote") playerId: String
    ): Response<Unit>

    /**
     * Despromove um Administrador da equipa a membro normal.
     *
     * Altera o tipo de membro de ADMIN_TEAM para PLAYER.
     *
     * @param teamId O ID da equipa.
     * @param playerId O ID do administrador que perderá os privilégios.
     */
    @PUT("api/Team/{teamId}/members/{playerId}/demote/{adminIdToDemote}")
    suspend fun desmoteAdmin(
        @Path("teamId") teamId: String,
        @Path("adminIdToDemote") playerId: String
    ): Response<Unit>

    //Calendar
    @GET("api/Calendar/{idTeam}")
    suspend fun getCalendar(
        @Path("idTeam") idTeam: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<InfoMatchCalendar>>

    @GET("api/Calendar/{idTeam}/{idMatch}")
    suspend fun getMatchTeam(
        @Path("idTeam") idTeam: String,
        @Path("idMatch") idMatch: String
    ): Response<MatchInviteDto>

    @PUT("api/Calendar/{idTeam}/PostponeMatch")
    suspend fun postponeMatch(
        @Path("idTeam") idTeam: String,
        @Body postponeMatch: PostPoneMatchDto
    ): Response<Unit>

    @HTTP(method = "DELETE", path = "api/Calendar/{idTeam}/CancelMatch/{idMatch}", hasBody = true)
    suspend fun cancelMatch(
        @Path("idTeam") idTeam: String,
        @Path("idMatch") idMatch: String,
        @Body description: String
    ): Response<Unit>

    //MatchInvite
    @POST("api/MatchInvite/{idTeam}/match-invites")
    suspend fun sendMatchInvite(
        @Path("idTeam") idTeam: String,
        @Body matchInivite: SendMatchInviteDto
    ): Response<InfoMatchInviteDto>

    @PUT("api/MatchInvite/{idTeam}/Negociate")
    suspend fun negotiateMatch(
        @Path("idTeam") idTeam: String,
        @Body matchInivite: SendMatchInviteDto
    ): Response<InfoMatchInviteDto>

    @POST("api/Team/{teamId}/membership-requests/send")
    suspend fun sendMemberShipRequestToPlayer(
        @Path("teamId") teamId: String,
        @Body playerId: String
    ): Response<MembershipRequestInfoDto>
}
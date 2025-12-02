package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.QueryMap

interface PlayerApi {
    /**
     * Obtém os detalhes públicos completos de um jogador específico.
     *
     * Endpoint: GET api/Player/details/{playerId}
     *
     * @param playerId O identificador único (UUID) do jogador.
     * @return [Response] contendo o perfil detalhado do jogador [PlayerProfileDto].
     */
    @GET("${BaseEndpoints.playerApi}/details/{playerId}")
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
    @GET("${BaseEndpoints.playerApi}/listPlayers")
    suspend fun getPlayersList(
        @QueryMap filters: Map<String, String>
    ): Response<List<InfoPlayerDto>>


    //TODO: Validar se a Response é Unit ou se é um DTO
    /**
     * Envia um pedido de adesão de um jogador para uma equipa específica.
     *
     * Endpoint: POST api/Player/{playerId}/membership-requests/send
     *
     * @param playerId O identificador único (UUID) do jogador que envia o pedido.
     * @param teamId O identificador único (UUID) da equipa alvo (enviado no corpo da requisição).
     * @return [Response] contendo a informação do pedido de adesão criado [MembershipRequestInfoDto].
     */
    @POST("${BaseEndpoints.playerApi}/{playerId}/membership-requests/send")
    suspend fun sendMemberShipRequestToTeam(
        @Path("playerId") playerId: String,
        @Body teamId: String
    ): Response<MembershipRequestInfoDto>

    /**
     * Envia um convite de recrutamento (Membership Request) de uma Equipa para um Jogador.
     *
     * Diferente do jogador pedir para entrar (Join Request), aqui é a equipa (Capitão/Admin)
     * que toma a iniciativa de convidar um jogador específico para se juntar ao plantel.
     *
     * Endpoint: POST api/Team/{teamId}/membership-requests/send
     *
     * @param teamId O identificador único (UUID) da equipa que está a recrutar.
     * @param playerId O identificador único (UUID) do jogador alvo do convite (enviado no corpo da requisição).
     * @return [Response] contendo a informação do pedido de adesão gerado [MembershipRequestInfoDto].
     */
    @POST("api/Team/{teamId}/membership-requests/send")
    suspend fun sendMemberShipRequestToPlayer(
        @Path("teamId") teamId: String,
        @Body playerId: String
    ): Response<MembershipRequestInfoDto>
}
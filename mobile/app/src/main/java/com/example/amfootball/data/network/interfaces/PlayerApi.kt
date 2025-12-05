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

/**
 * Interface de definição dos endpoints da API Retrofit relacionados com a gestão de Jogadores.
 *
 * Esta interface cobre operações de leitura de perfil, pesquisa de mercado (listagem filtrada)
 * e gestão de pedidos de adesão (Membership Requests) bidirecionais (Jogador -> Equipa e Equipa -> Jogador).
 */
interface PlayerApi {
    /**
     * Obtém os detalhes públicos completos de um jogador específico.
     *
     * Permite visualizar a ficha técnica, histórico e informações de contacto de um jogador.
     *
     * **Endpoint:** `GET api/Player/details/{playerId}`
     *
     * @param playerId O identificador único (UUID) do jogador a consultar.
     * @return [Response] contendo o objeto detalhado [PlayerProfileDto].
     */
    @GET("${BaseEndpoints.PLAYER_API}/details/{playerId}")
    suspend fun getPlayerProfile(
        @Path("playerId") playerId: String
    ): Response<PlayerProfileDto>

    /**
     * Pesquisa e lista jogadores com base em critérios de filtragem dinâmicos.
     *
     * Utilizado na funcionalidade de "Mercado de Jogadores" para encontrar atletas livres,
     * por posição, idade, localização, etc.
     *
     * **Endpoint:** `GET api/Player/listPlayers`
     *
     * @param filters Um mapa chave-valor contendo os parâmetros de consulta (Query Params).
     * Os filtros nulos no DTO de origem devem ser omitidos deste mapa.
     * Exemplo: `{"Name": "Silva", "Position": "GOALKEEPER"}`.
     *
     * @return [Response] contendo uma lista resumida de [InfoPlayerDto], otimizada para visualização em listas.
     */
    @GET("${BaseEndpoints.PLAYER_API}/listPlayers")
    suspend fun getPlayersList(
        @QueryMap filters: Map<String, String>
    ): Response<List<InfoPlayerDto>>


    /**
     * Envia um pedido de adesão (Join Request) de um Jogador para uma Equipa.
     *
     * Representa a ação em que o **Jogador toma a iniciativa** de se candidatar a uma equipa.
     *
     * **Endpoint:** `POST api/Player/{playerId}/membership-requests/send`
     *
     * @param playerId O ID do jogador que está a enviar o pedido (extraído da sessão atual).
     * @param teamId O ID da equipa destinatária, enviado no corpo (Body) da requisição.
     * @return [Response] com os detalhes do pedido criado [MembershipRequestInfoDto].
     */
    @POST("${BaseEndpoints.PLAYER_API}/{playerId}/membership-requests/send")
    suspend fun sendMemberShipRequestToTeam(
        @Path("playerId") playerId: String,
        @Body teamId: String
    ): Response<MembershipRequestInfoDto>

    /**
     * Envia um convite de recrutamento (Recruitment Invite) de uma Equipa para um Jogador.
     *
     * Representa a ação em que a **Equipa (Capitão/Admin) toma a iniciativa** de convidar um jogador
     * para se juntar ao plantel.
     *
     * **Endpoint:** `POST api/Team/{teamId}/membership-requests/send`
     *
     * @param teamId O ID da equipa que está a fazer o convite.
     * @param playerId O ID do jogador convidado, enviado no corpo (Body) da requisição.
     * @return [Response] com os detalhes do convite criado [MembershipRequestInfoDto].
     */
    @POST("api/Team/{teamId}/membership-requests/send")
    suspend fun sendMemberShipRequestToPlayer(
        @Path("teamId") teamId: String,
        @Body playerId: String
    ): Response<MembershipRequestInfoDto>
}
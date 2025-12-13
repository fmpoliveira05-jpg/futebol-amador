package com.example.amfootball.data.interfaces.api

import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.BaseEndpoints
import com.example.amfootball.data.remote.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.remote.dtos.matchInivite.SendMatchInviteDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.HTTP
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.QueryMap

/**
 * Interface de API responsável pela **gestão de Convites** no sistema.
 *
 * Esta interface lida principalmente com **Convites de Jogo (Match Invites)**: Desafios
 * entre equipas para agendar partidas, incluindo o fluxo de negociação (envio, aceitação, recusa e atualização).
 *
 * **Nota:** O fluxo de Convites de Recrutamento deve ser implementado numa interface separada,
 * se necessário, ou as operações devem ser adicionadas aqui com uma distinção clara.
 */
interface MatchInviteApi {
    /**
     * Envia um convite de jogo (desafio) de uma equipa para outra.
     *
     * Este é o primeiro passo para agendar um jogo amigável ou competitivo.
     * O convite contém a proposta inicial de data, hora e local.
     *
     * Endpoint: POST api/MatchInvite/{idTeam}/match-invites
     *
     * @param idTeam O identificador único (UUID) da equipa que está a enviar o desafio (Remetente).
     * @param matchInvite O DTO [SendMatchInviteDto] contendo os detalhes do desafio (ID da equipa adversária, data, local).
     * @return [Response] contendo a informação do convite criado [MatchInviteDto].
     */
    @POST("${BaseEndpoints.MATCH_INVITE_API}/{${Arguments.ID_TEAM}}/match-invites")
    suspend fun sendMatchInvite(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @Body dto: SendMatchInviteDto
    ): Response<MatchInviteDto>

    /**
     * **Aceita** um convite de jogo recebido.
     *
     * Ao aceitar, o convite transita para o estado de **"Aceite"** e o jogo pode ser agendado
     * ou criado no calendário de ambas as equipas, finalizando o processo de negociação.
     *
     * Endpoint: `POST api/MatchInvite/{idTeam}/AcceptMatchInvite`
     *
     * @param idTeam O identificador da equipa que está a aceitar o convite (**Recetora**).
     * @param matchInviteId O ID (String) do convite que está a ser aceite.
     * @return [Response] contendo o DTO [SendMatchInviteDto] (possivelmente a confirmação dos termos finais) ou um DTO de confirmação.
     */
    @POST("${BaseEndpoints.MATCH_INVITE_API}/{${Arguments.ID_TEAM}}/AcceptMatchInvite")
    suspend fun acceptMatchInvite(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @Body matchInviteId: String
    ): Response<SendMatchInviteDto>

    /**
     * **Recusa** um convite de jogo recebido.
     *
     * O convite transita para o estado de **"Recusado"** ou é removido, terminando o desafio.
     *
     * Endpoint: `DELETE api/MatchInvite/{idTeam}/RefuseMatchInvite`
     *
     * @param idTeam O identificador da equipa que está a recusar o convite (**Recetora**).
     * @param matchInviteId O ID (String) do convite que está a ser recusado.
     * @return [Response] com um corpo vazio ([Unit]) em caso de sucesso (código 204 No Content).
     */
    @HTTP(
        method = "DELETE",
        path = "${BaseEndpoints.MATCH_INVITE_API}/{${Arguments.ID_TEAM}}/RefuseMatchInvite",
        hasBody = true
    )
    suspend fun refuseMatchInvite(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @Body matchInviteId: String
    ): Response<Unit>

    /**
     * Negocia ou atualiza as condições de um convite de jogo existente.
     *
     * Utilizado quando a equipa desafiada não aceita os termos iniciais e propõe
     * uma contraproposta (ex: altera a hora ou o local), ou quando a equipa remetente
     * ajusta o convite antes de ser aceite.
     *
     * Endpoint: PUT api/MatchInvite/{idTeam}/Negociate
     *
     * @param idTeam O identificador da equipa que está a realizar a negociação.
     * @param matchInvite O DTO [SendMatchInviteDto] com os novos termos propostos para o jogo.
     * @return [Response] contendo os detalhes do convite atualizado [MatchInviteDto].
     */
    @PUT("${BaseEndpoints.MATCH_INVITE_API}/{${Arguments.ID_TEAM}}/Negociate")
    suspend fun negotiateMatch(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @Body matchInvite: SendMatchInviteDto
    ): Response<MatchInviteDto>

    /**
     * **Obtém a lista de convites** de jogo associados a uma equipa.
     *
     * Pode ser utilizado para listar convites enviados, recebidos, aceites, ou recusados,
     * dependendo dos parâmetros de consulta fornecidos.
     *
     * Endpoint: `GET api/MatchInvite/{idTeam}`
     *
     * @param idTeam O identificador único da equipa cujos convites se pretende listar.
     * @param fitlers O mapa de parâmetros de consulta (`@QueryMap`) para filtrar os convites
     * (ex: estado do convite, tipo de convite - enviado/recebido).
     * **Nota:** O nome do parâmetro `matchInviteId` sugere que o endpoint
     * também poderia ser usado para obter um único convite.
     * @return [Response] contendo a lista de convites [List]<[MatchInviteDto]> que satisfazem os critérios.
     */
    @GET("${BaseEndpoints.MATCH_INVITE_API}/{${Arguments.ID_TEAM}}")
    suspend fun getMatchInviteList(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @QueryMap fitlers: Map<String, String>
    ): Response<List<MatchInviteDto>>

    /**
     * Obtém os detalhes de um **único convite de jogo** específico.
     *
     * Endpoint: `GET api/MatchInvite/{idTeam}/{idMatchInvite}`
     *
     * @param idTeam O identificador da equipa que está a consultar (para validação de acesso).
     * @param idMatchInvite O identificador único (UUID) do convite a ser recuperado.
     * @return [Response] contendo os detalhes completos do convite [MatchInviteDto].
     */
    @GET("${BaseEndpoints.MATCH_INVITE_API}/{${Arguments.ID_TEAM}}/{${Arguments.ID_MATCH_INVITE}}")
    suspend fun getMatchInvite(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @Path(Arguments.ID_MATCH_INVITE) idMatchInvite: String
    ): Response<MatchInviteDto>
}
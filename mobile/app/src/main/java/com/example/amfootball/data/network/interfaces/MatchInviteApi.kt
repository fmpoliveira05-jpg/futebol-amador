package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.SendMatchInviteDto
import com.example.amfootball.data.dtos.membershipRequest.MembershipRequestInfoDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path

/**
 * Interface de API responsável pela gestão de Convites.
 *
 * Esta interface lida com dois tipos distintos de interações:
 * 1. **Convites de Jogo (Match Invites):** Desafios entre equipas para agendar partidas, incluindo o fluxo de negociação.
 * 2. **Convites de Recrutamento:** Ações onde uma equipa convida diretamente um jogador para integrar o seu plantel.
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
     * @param matchInivite O DTO [SendMatchInviteDto] contendo os detalhes do desafio (ID da equipa adversária, data, local).
     * @return [Response] contendo a informação do convite criado [InfoMatchInviteDto].
     */
    @POST("api/MatchInvite/{idTeam}/match-invites")
    suspend fun sendMatchInvite(
        @Path("idTeam") idTeam: String,
        @Body matchInivite: SendMatchInviteDto
    ): Response<InfoMatchInviteDto>

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
     * @param matchInivite O DTO [SendMatchInviteDto] com os novos termos propostos para o jogo.
     * @return [Response] contendo os detalhes do convite atualizado [InfoMatchInviteDto].
     */
    @PUT("api/MatchInvite/{idTeam}/Negociate")
    suspend fun negotiateMatch(
        @Path("idTeam") idTeam: String,
        @Body matchInivite: SendMatchInviteDto
    ): Response<InfoMatchInviteDto>
}
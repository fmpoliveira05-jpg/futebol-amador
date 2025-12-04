package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.SendMatchInviteDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.HTTP
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.QueryMap

/**
 * Interface de API responsável pela gestão do Calendário de Jogos e Eventos da Equipa.
 *
 * Esta interface centraliza as operações relacionadas com a agenda da equipa, permitindo:
 * - Visualizar a lista de jogos (passados e futuros).
 * - Consultar detalhes específicos de uma partida.
 * - Gerir alterações de agendamento (Adiar jogos).
 * - Cancelar partidas agendadas (com justificação).
 */
interface CalendarApi {
    /**
     * Obtém o calendário de jogos de uma equipa, permitindo filtragem por período ou estado.
     *
     * Este endpoint é utilizado para popular a vista de calendário ou lista de jogos.
     * Suporta filtros via Query Params para selecionar intervalos de datas (ex: mês/ano)
     * ou tipos de jogos.
     *
     * Endpoint: GET api/Calendar/{idTeam}
     *
     * @param idTeam O identificador único (UUID) da equipa cujo calendário se pretende consultar.
     * @param filters Um mapa de filtros de consulta.
     * @return [Response] contendo uma lista de [InfoMatchCalendar] com os resumos dos jogos encontrados.
     */
    @GET("${BaseEndpoints.calendarApi}/{idTeam}")
    suspend fun getCalendar(
        @Path("idTeam") idTeam: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<InfoMatchCalendar>>

    /**
     * Obtém os detalhes completos de um jogo específico no contexto de uma equipa.
     *
     * Retorna informações detalhadas sobre a partida, incluindo o estado do convite,
     * equipa adversária, local, hora e metadados de gestão.
     *
     * Endpoint: GET api/Calendar/{idTeam}/{idMatch}
     *
     * @param idTeam O identificador da equipa que está a visualizar o jogo (para contexto de permissões/visão).
     * @param idMatch O identificador único do jogo/partida.
     * @return [Response] contendo o objeto [MatchInviteDto] com os detalhes da partida e estado do convite.
     */
    @GET("${BaseEndpoints.calendarApi}/{idTeam}/{idMatch}")
    suspend fun getMatchTeam(
        @Path("idTeam") idTeam: String,
        @Path("idMatch") idMatch: String
    ): Response<MatchInviteDto>

    /**
     * Adia um jogo agendado, propondo ou definindo uma nova data e hora.
     *
     * Esta operação é utilizada quando uma equipa necessita de reagendar uma partida.
     * Envia os dados da nova data através do corpo da requisição.
     *
     * Endpoint: PUT api/Calendar/{idTeam}/PostponeMatch
     *
     * @param idTeam O identificador da equipa que está a solicitar o adiamento.
     * @param postponeMatch O DTO [PostPoneMatchDto] contendo:
     * - O ID do jogo a adiar.
     * - A nova data e hora propostas.
     * - O motivo do adiamento (opcional).
     * @return [Response] vazia (Unit) indicando sucesso na operação.
     */
    @PUT("${BaseEndpoints.calendarApi}/{idTeam}/PostponeMatch")
    suspend fun postponeMatch(
        @Path("idTeam") idTeam: String,
        @Body postponeMatch: SendMatchInviteDto
    ): Response<Unit>

    /**
     * Cancela definitivamente um jogo agendado, exigindo uma justificação.
     *
     * **Nota Técnica:** Utiliza a anotação genérica `@HTTP` em vez de `@DELETE` porque
     * o padrão HTTP DELETE, por convenção, não deve conter corpo (Body).
     * No entanto, esta API requer o envio de uma `description` (motivo) no corpo da requisição
     * para processar o cancelamento corretamente.
     *
     * Endpoint: DELETE api/Calendar/{idTeam}/CancelMatch/{idMatch} (com Body)
     *
     * @param idTeam O identificador da equipa que está a cancelar o jogo.
     * @param idMatch O identificador do jogo a ser cancelado.
     * @param description A string enviada no corpo (Body) contendo o motivo/justificação do cancelamento.
     * @return [Response] vazia (Unit) indicando que o jogo foi cancelado com sucesso.
     */
    @HTTP(method = "DELETE", path = "${BaseEndpoints.calendarApi}/{idTeam}/CancelMatch/{idMatch}", hasBody = true)
    suspend fun cancelMatch(
        @Path("idTeam") idTeam: String,
        @Path("idMatch") idMatch: String,
        @Body description: String
    ): Response<Unit>
}
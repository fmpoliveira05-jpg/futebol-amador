package com.example.amfootball.data.services

import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.SendMatchInviteDto
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.interfaces.CalendarApi
import com.example.amfootball.utils.handleApiError
import com.example.amfootball.utils.safeApiCallWithNotReturn
import com.example.amfootball.utils.safeApiCallWithReturn
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável pela gestão da agenda e calendário de jogos de uma equipa.
 *
 * Esta classe atua como repositório de dados, isolando a comunicação com a [CalendarApi].
 * Utiliza as funções utilitárias `safeApiCall` para padronizar o tratamento de erros
 * e reduzir a repetição de código (boilerplate).
 *
 * @property calendarApi A interface Retrofit injetada para comunicação com o backend.
 */
@Singleton
class CalendarService @Inject constructor(
    private val calendarApi: CalendarApi
) {
    /**
     * Obtém a lista de jogos do calendário de uma equipa, aplicando filtros opcionais.
     *
     * Delega a execução para [safeApiCallWithReturn], garantindo que o retorno é uma lista válida
     * e lançando exceção caso o corpo da resposta seja nulo ou a chamada falhe.
     *
     * @param teamId O identificador único da equipa.
     * @param filter Critérios de pesquisa (ex: data, local), convertidos internamente para Query Map.
     * @return Uma lista de [InfoMatchCalendar].
     * @throws Exception Propagada automaticamente pelo utilitário em caso de erro de API ou Rede.
     */
    suspend fun getCalendar(teamId: String, filter: FilterCalendar?): List<InfoMatchCalendar> {
        val filters = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            calendarApi.getCalendar(idTeam = teamId, filters = filters)
        }
    }

    /**
     * Obtém os detalhes completos de um jogo ou convite específico.
     *
     * @param teamId O ID da equipa que consulta o detalhe.
     * @param matchId O ID único do jogo.
     * @return O objeto [MatchInviteDto] com os dados do jogo.
     * @throws Exception Se o jogo não for encontrado ou o corpo da resposta vier vazio.
     */
    suspend fun getMatchTeam(teamId: String, matchId: String): MatchInviteDto {
        return safeApiCallWithReturn {
            calendarApi.getMatchTeam(idTeam = teamId, idMatch = matchId)
        }
    }

    /**
     * Envia um pedido de adiamento (remarcação) para um jogo.
     *
     * Utiliza [safeApiCallWithNotReturn] pois esta operação não devolve dados (apenas confirmação HTTP 2xx).
     * O corpo da resposta é ignorado, validando-se apenas o sucesso da operação.
     *
     * @param teamId O ID da equipa requerente.
     * @param postponeMatch O DTO com a proposta de nova data.
     * @throws Exception Se o servidor rejeitar o pedido (ex: 400 Bad Request).
     */
    suspend fun postPoneMatch(teamId: String, postponeMatch: SendMatchInviteDto) {
        try {
            val response = calendarApi.postponeMatch(idTeam = teamId, postponeMatch = postponeMatch)

            if (!response.isSuccessful) {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw e
        }
    }

    /**
     * Cancela um jogo agendado.
     *
     * Utiliza [safeApiCallWithNotReturn] para uma operação do tipo "Fire-and-Forget".
     *
     * @param teamId O ID da equipa que cancela.
     * @param matchId O ID do jogo a cancelar.
     * @param description O motivo do cancelamento.
     * @throws Exception Se não for possível cancelar o jogo (ex: permissões ou estado inválido).
     */
    suspend fun cancelMatch(teamId: String, matchId: String, description: String) {
        safeApiCallWithNotReturn {
            calendarApi.cancelMatch(idTeam = teamId, idMatch = matchId, description = description)
        }
    }
}

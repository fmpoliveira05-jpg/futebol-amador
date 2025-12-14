package com.example.amfootball.data.remote.services

import com.example.amfootball.core.utils.safeApiCallWithNotReturn
import com.example.amfootball.core.utils.safeApiCallWithReturn
import com.example.amfootball.data.filters.FilterPostPoneMatch
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.interfaces.api.PostPoneMatchApi
import com.example.amfootball.data.remote.dtos.postponeMatch.PostPoneResponse
import com.example.amfootball.data.remote.dtos.postponeMatch.PostPoneReturn
import com.example.amfootball.data.remote.dtos.postponeMatch.PostponeDto
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável por gerir as solicitações de adiamento de jogos (Postpone Match).
 *
 * Atua como a camada de repositório para todas as interações relacionadas a pedidos
 * de adiamento, aceitação e rejeição, comunicando-se com a [PostPoneMatchApi].
 * Utiliza utilitários de chamadas seguras ([safeApiCallWithReturn] e [safeApiCallWithNotReturn])
 * para encapsular a lógica de tratamento de exceções de rede e HTTP.
 *
 * @property postPoneMatchApi A interface Retrofit injetada para comunicação com o backend.
 */
@Singleton
class PostPoneMatchService @Inject constructor(
    private val postPoneMatchApi: PostPoneMatchApi
) {

    /**
     * Obtém uma lista de solicitações de adiamento de jogo para uma equipa específica.
     *
     * Permite a filtragem das solicitações (ex: por estado PENDING, ACCEPTED) usando [FilterPostPoneMatch].
     *
     * @param teamId O identificador da equipa cujas solicitações de adiamento são solicitadas.
     * @param filter O filtro opcional [FilterPostPoneMatch] para restringir os resultados. Se for `null`,
     * retorna todas as solicitações.
     * @return [List] de [PostponeDto] contendo as informações das solicitações de adiamento.
     * @throws Exception Propagada em caso de falha na comunicação ou resposta inválida.
     */
    suspend fun getPostPoneMatch(teamId: String, filter: FilterPostPoneMatch?): List<PostponeDto> {
        val filters = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            postPoneMatchApi.getPostPoneMatch(idTeam = teamId, filters = filters)
        }
    }

    /**
     * Aceita uma solicitação de adiamento de jogo.
     *
     * Esta ação altera o estado da solicitação no backend para aceite e geralmente requer
     * que a equipa tenha permissão para realizar tal ação.
     *
     * @param teamId O identificador da equipa que está a aceitar o adiamento.
     * @param postPone O DTO [PostPoneResponse] contendo o identificador do pedido e informações necessárias para a aceitação.
     * @return [PostPoneReturn] contendo o status final e detalhes da operação de aceitação.
     * @throws Exception Propagada em caso de falha na comunicação ou se a operação for inválida (ex: pedido já aceite).
     */
    suspend fun acceptPostPoneMatch(teamId: String, postPone: PostPoneResponse): PostPoneReturn {
        return safeApiCallWithReturn {
            postPoneMatchApi.acceptPostPoneMatch(idTeam = teamId, postPone = postPone)
        }
    }

    /**
     * Rejeita uma solicitação de adiamento de jogo.
     *
     * Esta ação altera o estado da solicitação no backend para rejeitada. A função utiliza
     * [safeApiCallWithNotReturn] uma vez que não se espera um corpo de resposta.
     *
     * @param teamId O identificador da equipa que está a rejeitar o adiamento.
     * @param postPone O DTO [PostPoneResponse] contendo o identificador do pedido a ser rejeitado.
     * @throws Exception Propagada em caso de falha na comunicação.
     */
    suspend fun rejectPostPoneMatch(teamId: String, postPone: PostPoneResponse) {
        safeApiCallWithNotReturn {
            postPoneMatchApi.rejectPostPoneMatch(idTeam = teamId, postPone = postPone)
        }
    }
}
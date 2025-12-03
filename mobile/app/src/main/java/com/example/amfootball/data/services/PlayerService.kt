package com.example.amfootball.data.services

import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.interfaces.PlayerApi
import com.example.amfootball.utils.safeApiCallWithNotReturn
import com.example.amfootball.utils.safeApiCallWithReturn
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável pela gestão de dados e operações relacionadas com Jogadores.
 *
 * Esta classe encapsula a lógica de consulta de perfis, pesquisa de mercado (listagens filtradas)
 * e interações de recrutamento dirigidas a jogadores.
 *
 * A gestão de erros HTTP e validação de respostas é delegada nos utilitários [safeApiCallWithReturn]
 * e [safeApiCallWithNotReturn], garantindo um código mais limpo e seguro.
 *
 * @property playerApi A interface Retrofit injetada para comunicação com o backend.
 */
@Singleton
class PlayerService @Inject constructor(
    private val playerApi: PlayerApi
) {

    /**
     * Obtém o perfil detalhado de um jogador específico.
     *
     * Utiliza [safeApiCallWithReturn] para garantir que o retorno é um [PlayerProfileDto] válido.
     *
     * @param playerId O identificador único (UUID) do jogador a consultar.
     * @return O objeto [PlayerProfileDto] contendo todos os detalhes públicos do jogador.
     * @throws Exception Propagada automaticamente se o jogador não for encontrado (404) ou ocorrer erro de rede.
     */
    suspend fun getPlayerProfile(playerId: String): PlayerProfileDto {
        return safeApiCallWithReturn {
            playerApi.getPlayerProfile(playerId = playerId)
        }
    }

    /**
     * Pesquisa e lista jogadores com base em filtros (Mercado de Jogadores).
     *
     * Converte o objeto de filtro [FilterListPlayer] num mapa de parâmetros de consulta (Query Map)
     * antes de invocar a API de forma segura.
     *
     * @param filter Objeto com critérios de pesquisa (idade, posição, etc.), ou `null` para listar todos.
     * @return Uma lista de [InfoPlayerDto] com os resultados da pesquisa.
     * @throws Exception Se ocorrer falha na comunicação com o servidor.
     */
    suspend fun getListPlayer(filter: FilterListPlayer?): List<InfoPlayerDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            playerApi.getPlayersList(filters = filterMap)
        }
    }

    /**
     * Envia um convite de recrutamento (Membership Request) de uma equipa para um jogador.
     *
     * Utiliza [safeApiCallWithNotReturn] para executar a ação sem esperar dados de retorno,
     * validando apenas o sucesso da operação (HTTP 2xx).
     *
     * **Nota de Arquitetura:**
     * Embora este método esteja no `PlayerService` (pois o alvo é um jogador),
     * semanticamente representa uma ação de uma Equipa (`TeamService`).
     * Considerar mover para `TeamService` numa futura refatoração para manter a coesão por "Emissor da ação".
     *
     * @param teamId O ID da equipa que está a recrutar.
     * @param idPlayer O ID do jogador que vai receber o convite.
     * @throws Exception Se o convite falhar ou for rejeitado pelo servidor.
     */
    suspend fun sendMemberShipRequestToPlayer(teamId: String, idPlayer: String) {
        safeApiCallWithNotReturn {
            playerApi.sendMemberShipRequestToPlayer(teamId = teamId, playerId = idPlayer)
        }
    }
}
package com.example.amfootball.data.remote.services

import com.example.amfootball.core.utils.safeApiCallWithNotReturn
import com.example.amfootball.core.utils.safeApiCallWithReturn
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.filters.FilterMemberShipRequest
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.interfaces.api.PlayerApi
import com.example.amfootball.data.remote.dtos.membershipRequest.InvitePlayerRequest
import com.example.amfootball.data.remote.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.remote.dtos.membershipRequest.RequestMemberShip
import com.example.amfootball.data.remote.dtos.player.InfoPlayerDto
import com.example.amfootball.data.remote.dtos.player.PlayerProfileDto
import com.example.amfootball.domains.enums.pages.ListPlayerMode
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável pela gestão de dados e operações relacionadas com **Jogadores**.
 *
 * Esta classe atua como intermediária entre a UI/Repositórios e a API [PlayerApi], encapsulando:
 * - Consulta e atualização de perfis de jogadores.
 * - Pesquisa de mercado (listagens filtradas e jogadores sem equipa).
 * - Gestão de convites e pedidos de adesão (Membership Requests).
 *
 * Todas as chamadas de rede são protegidas pelos wrappers [safeApiCallWithReturn] e
 * [safeApiCallWithNotReturn] para gestão centralizada de exceções.
 *
 * @property playerApi Interface Retrofit injetada para comunicação com o backend.
 */
@Singleton
class PlayerService @Inject constructor(
    private val playerApi: PlayerApi
) {
    /**
     * Obtém o perfil detalhado de um jogador específico.
     *
     * @param playerId O identificador único (UUID) do jogador a consultar.
     * @return O objeto [PlayerProfileDto] contendo todos os detalhes públicos e estatísticos do jogador.
     * @throws Exception Propagada automaticamente se o jogador não for encontrado (404) ou ocorrer erro de rede.
     */
    suspend fun getPlayerProfile(playerId: String): PlayerProfileDto {
        return safeApiCallWithReturn {
            playerApi.getPlayerProfile(playerId = playerId)
        }
    }

    /**
    * Pesquisa e lista jogadores com base em filtros e no modo de visualização.
    *
    * Permite dois modos de operação baseados no [ListPlayerMode]:
    * 1. **Geral:** Lista todos os jogadores com base nos filtros.
    * 2. **Sem Equipa:** Lista apenas jogadores disponíveis para recrutamento (requer [teamId]).
    *
    * @param teamId O ID da equipa (obrigatório apenas se o [mode] for [ListPlayerMode.PLAYER_WITHOU_TEAM]).
    * @param mode Define o contexto da listagem (ex: Mercado geral ou Recrutamento).
    * @param filter Objeto com critérios de pesquisa (idade, posição, localização), ou `null`.
    * @return Uma lista de [InfoPlayerDto] com os resultados resumidos da pesquisa.
    */
    suspend fun getListPlayer(teamId: String?, mode: ListPlayerMode, filter: FilterListPlayer?): List<InfoPlayerDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            if (!teamId.isNullOrEmpty() && mode == ListPlayerMode.PLAYER_WITHOU_TEAM) {
                playerApi.getPlayersWithoutTeamList(teamId = teamId, filters = filterMap)
            } else {
                playerApi.getPlayersList(filters = filterMap)
            }
        }
    }

    /**
     * Envia um **convite de recrutamento** de uma equipa para um jogador.
     *
     * **Nota:** Esta ação cria um pedido pendente que o jogador deverá aceitar ou rejeitar.
     *
     * @param teamId O ID da equipa que está a enviar o convite.
     * @param idPlayer O ID do jogador alvo do recrutamento.
     * @return O objeto [MembershipRequestInfoDto] representando o pedido criado.
     */
    suspend fun sendMemberShipRequestToPlayer(teamId: String, idPlayer: String): MembershipRequestInfoDto {
        return safeApiCallWithReturn {
            val request = InvitePlayerRequest(playerId = idPlayer)
            playerApi.sendMemberShipRequestToPlayer(teamId = teamId, request = request)
        }
    }

    /**
     * Lista os pedidos de adesão/convites associados a um jogador.
     *
     * Pode listar tanto convites recebidos (de equipas) como pedidos enviados (pelo jogador).
     *
     * @param playerId O ID do jogador.
     * @param filter Filtros opcionais (ex: Estado do pedido: PENDENTE, ACEITE, REJEITADO).
     * @return Uma lista de [MembershipRequestInfoDto].
     */
    suspend fun listMemberShipRequest(playerId: String, filter: FilterMemberShipRequest? = null): List<MembershipRequestInfoDto> {
        return safeApiCallWithReturn {
            val filters = filter?.toQueryMap() ?: emptyMap()
            playerApi.listMemberShipRequest(playerId = playerId, filters = filters)
        }
    }

    /**
     * Aceita um pedido de adesão ou convite pendente.
     *
     * Ao aceitar, o jogador passa a fazer parte da equipa associada ao pedido.
     *
     * @param playerId O ID do jogador que está a aceitar.
     * @param requestId O ID do pedido/convite a ser aceite.
     * @return O objeto [MembershipRequestInfoDto] com o estado atualizado.
     */
    suspend fun acceptMemberShipRequest(playerId: String, requestId: String): MembershipRequestInfoDto {
        return safeApiCallWithReturn {
            val request = RequestMemberShip(requestId = requestId)
            playerApi.acceptMemberShipRequest(playerId = playerId, request = request)
        }
    }

    /**
     * Rejeita um pedido de adesão ou convite pendente.
     *
     * @param playerId O ID do jogador que está a rejeitar.
     * @param requestId O ID do pedido/convite a ser rejeitado.
     */
    suspend fun rejectMemberShipRequest(playerId: String, requestId: String) {
        safeApiCallWithNotReturn {
            playerApi.rejectMemberShipRequest(playerId = playerId, requestId = requestId)
        }
    }

    /**
     * Atualiza os dados do perfil do jogador.
     *
     * Extrai o ID do jogador diretamente do objeto [playerProfile].
     *
     * @param playerProfile O DTO contendo os dados atualizados. Deve conter um `loginResponseDto.localId` válido.
     * @throws IllegalArgumentException Se o ID do jogador não estiver presente no DTO.
     */
    suspend fun updatePlayerProfile(playerProfile: PlayerProfileDto) {
        safeApiCallWithNotReturn {
            playerApi.updatePlayer(
                playerId = playerProfile.loginResponseDto!!.localId,
                player = playerProfile
            )
        }
    }

    /**
     * Apaga permanentemente o perfil do jogador.
     *
     * @param playerId O ID do jogador a eliminar.
     */
    suspend fun deletePlayerProfile(playerId: String) {
        safeApiCallWithNotReturn {
            playerApi.deletePlayer(playerId = playerId)
        }
    }

    /**
     * Remove o jogador da equipa atual (Sair da Equipa).
     *
     * Após o sucesso desta operação, o jogador ficará sem equipa associada no backend.
     *
     * @param playerId O ID do jogador que vai sair da equipa.
     * @return O objeto [InfoPlayerDto] atualizado, refletindo o estado sem equipa.
     *
     * @see [com.example.amfootball.data.local.SessionManager] Deve ser invocado após o sucesso para limpar o teamId localmente.
     */
    suspend fun leaveTeam(playerId: String): InfoPlayerDto {
        return safeApiCallWithReturn {
            playerApi.leaveTeam(playerId = playerId)
        }
    }
}
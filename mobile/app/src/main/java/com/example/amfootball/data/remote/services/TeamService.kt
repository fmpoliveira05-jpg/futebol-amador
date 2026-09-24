package com.example.amfootball.data.remote.services

import com.example.amfootball.core.utils.safeApiCallWithNotReturn
import com.example.amfootball.core.utils.safeApiCallWithReturn
import com.example.amfootball.data.filters.FilterMemberShipRequest
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.interfaces.api.PlayerApi
import com.example.amfootball.data.interfaces.api.TeamApi
import com.example.amfootball.data.remote.dtos.homePageTeam.HomePageTeamDto
import com.example.amfootball.data.remote.dtos.membershipRequest.InviteTeamRequest
import com.example.amfootball.data.remote.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.remote.dtos.membershipRequest.RequestMemberShip
import com.example.amfootball.data.remote.dtos.leadboard.InfoTeamLeadboard
import com.example.amfootball.data.remote.dtos.player.MemberTeamDto
import com.example.amfootball.data.remote.dtos.team.FormTeamDto
import com.example.amfootball.data.remote.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.remote.dtos.team.ProfileTeamDto
import com.example.amfootball.data.remote.dtos.team.toFormTeamDto
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável pela lógica de negócio e manipulação de dados de **Equipas**.
 *
 * Esta classe centraliza todas as interações com a [TeamApi], incluindo:
 * - Operações CRUD (Criar, Ler, Atualizar, Apagar).
 * - Gestão de estatísticas e visualização de perfis.
 * - Gestão de Membros (Plantel, Staff, Promoções e Expulsões).
 * - Gestão de Pedidos de Adesão (Membership Requests).
 *
 * A gestão de erros e validação de respostas HTTP é delegada nos utilitários [safeApiCallWithReturn]
 * e [safeApiCallWithNotReturn], garantindo consistência e segurança.
 *
 * @property teamApi A interface Retrofit injetada para comunicação com o backend.
 */
@Singleton
class TeamService @Inject constructor(
    private val teamApi: TeamApi,
    private val playerApi: PlayerApi
) {

    /**
     * Obtém o perfil completo de uma equipa para visualização detalhada.
     *
     * @param teamId O ID da equipa.
     * @return [ProfileTeamDto] contendo estatísticas, membros, próximos jogos e histórico.
     * @throws Exception Propagada automaticamente em caso de erro de API ou rede.
     */
    suspend fun getTeamProfile(teamId: String): ProfileTeamDto {
        return safeApiCallWithReturn {
            teamApi.getTeamProfile(teamId = teamId)
        }
    }

    /**
     * Obtém a tabela de classificação (Leaderboard).
     **
     * @return [InfoTeamLeadboard] representando a classificação.
     */
    suspend fun getLeaderBoard(): List<InfoTeamLeadboard> {
        return safeApiCallWithReturn {
            teamApi.getLeaderBoard()
        }
    }


    /**
     * Obtém os dados resumidos para a "Home Page" ou Dashboard da equipa.
     *
     * @param teamId O ID da equipa.
     * @return [HomePageTeamDto] com métricas rápidas e atalhos de gestão.
     */
    suspend fun getHomePageTeam(teamId: String): HomePageTeamDto {
        return safeApiCallWithReturn {
            teamApi.getHomePageTeam(teamId = teamId)
        }
    }

    /**
     * Obtém os dados de uma equipa formatados especificamente para o formulário de edição.
     *
     * Fluxo:
     * 1. Obtém o perfil completo via [getTeamProfile].
     * 2. Converte para [FormTeamDto] usando a extensão [toFormTeamDto].
     *
     * @param teamId O ID da equipa a editar.
     * @return [FormTeamDto] preenchido com os dados atuais.
     */
    suspend fun getTeamToUpdate(teamId: String): FormTeamDto {
        return safeApiCallWithReturn {
            teamApi.getDataToUpdateTeam(teamId = teamId)
        }
    }

    /**
     * Pesquisa e lista equipas com base em filtros.
     *
     * @param filter Objeto com critérios de pesquisa (Localização, Nome, Nível) ou `null` para todas.
     * @return Lista de [ItemTeamInfoDto] com os resultados resumidos.
     */
    suspend fun getListTeam(filter: FiltersListTeam?): List<ItemTeamInfoDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            teamApi.getListTeam(filters = filterMap)
        }
    }

    /**
     * Lista equipas elegíveis para convite de jogos amigáveis.
     *
     * @param teamId O ID da minha equipa (para excluir da lista ou aplicar lógica de proximidade).
     * @param filter Filtros de pesquisa.
     * @return Lista de [ItemTeamInfoDto].
     */
    suspend fun getListTeamMatchInvite(teamId: String, filter: FiltersListTeam?): List<ItemTeamInfoDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            teamApi.getListTeamMatchInvite(teamId = teamId, filters = filterMap)
        }
    }

    /**
     * Lista as equipas que têm interações de pedidos de adesão (Membership) com um jogador específico.
     *
     * @param playerId O ID do jogador.
     * @param filter Filtros opcionais.
     * @return Lista de [ItemTeamInfoDto] representando as equipas envolvidas nos pedidos.
     */
    suspend fun getListTeamMemberShipRequest(playerId: String, filter: FiltersListTeam?): List<ItemTeamInfoDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            teamApi.getListTeamMembershipRequest(playerId = playerId, filters = filterMap)
        }
    }

    /**
     * Um **Jogador** envia um pedido para se juntar a uma **Equipa**.
     *
     * @param playerId O ID do jogador que faz o pedido.
     * @param teamId O ID da equipa alvo.
     * @return O objeto [MembershipRequestInfoDto] criado.
     */
    suspend fun playerSendMembershipRequestToTeam(playerId: String, teamId: String): MembershipRequestInfoDto {
        return safeApiCallWithReturn {
            val request = InviteTeamRequest(teamId)
            playerApi.sendMemberShipRequestToTeam(playerId = playerId, request = request)
        }
    }

    /**
     * Lista os pedidos de adesão recebidos pela equipa (ex: Jogadores que querem entrar).
     *
     * @param teamId O ID da equipa (Administrador).
     * @param filter Filtros (ex: Mostrar apenas PENDENTES).
     * @return Lista de [MembershipRequestInfoDto].
     */
    suspend fun getListMemberShipRequest(teamId: String, filter: FilterMemberShipRequest?): List<MembershipRequestInfoDto> {
        return safeApiCallWithReturn {
            val filters = filter?.toQueryMap() ?: emptyMap()
            teamApi.listMemberShipRequest(teamId = teamId, filters = filters)
        }
    }

    /**
     * A Equipa **Aceita** um pedido de adesão de um jogador.
     *
     * @param teamId O ID da equipa.
     * @param requestId O ID do pedido a aceitar.
     */
    suspend fun acceptMemberShipRequest(teamId: String, requestId: String) {
        safeApiCallWithNotReturn {
            val request = RequestMemberShip(requestId = requestId)
            teamApi.acceptMemberShipRequest(teamId = teamId, request = request)
        }
    }

    /**
     * A Equipa **Rejeita** um pedido de adesão.
     *
     * @param teamId O ID da equipa.
     * @param requestId O ID do pedido a rejeitar.
     */
    suspend fun rejectMemberShipRequest(teamId: String, requestId: String) {
        safeApiCallWithNotReturn {
            teamApi.rejectMemberShipRequest(teamId = teamId, requestId = requestId)
        }
    }

    /**
     * Cria uma nova equipa na plataforma.
     *
     * @param team O DTO contendo os dados do formulário (Nome, Emblema, Campo, etc.).
     * @return O [FormTeamDto] retornado pelo servidor, contendo o ID gerado.
     */
    suspend fun createTeam(team: FormTeamDto): FormTeamDto {
        return safeApiCallWithReturn {
            teamApi.createTeam(team = team)
        }
    }

    /**
     * Atualiza os dados de uma equipa existente.
     *
     * @param teamId O ID da equipa a atualizar.
     * @param team O DTO com os novos dados.
     * @return O [FormTeamDto] atualizado (eco do servidor).
     */
    suspend fun updateTeam(teamId: String, team: FormTeamDto): FormTeamDto {
        return safeApiCallWithReturn {
            teamApi.updateTeam(teamId = teamId, team = team)
        }
    }

    /**
     * Elimina permanentemente uma equipa.
     *
     * @param teamId O identificador da equipa a remover.
     */
    suspend fun deleteTeam(teamId: String) {
        safeApiCallWithNotReturn {
            teamApi.deleteTeam(teamId = teamId)
        }
    }

    /**
     * Lista os membros (jogadores e staff) de uma equipa.
     *
     * @param teamId O ID da equipa.
     * @param filter Critérios de filtragem (ex: Por Posição, Por Nome).
     * @return Lista de [MemberTeamDto].
     */
    suspend fun getListMembers(teamId: String, filter: FilterMembersTeam?): List<MemberTeamDto> {
        return safeApiCallWithReturn {
            val filters = filter?.toQueryMap() ?: emptyMap()

            teamApi.getListMembers(teamId = teamId, filters = filters)
        }
    }

    /**
     * Promove um jogador a **Administrador/Capitão** da equipa.
     *
     * @param teamId O ID da equipa.
     * @param playerPromoteId O ID do jogador a promover.
     */
    suspend fun promotePlayer(teamId: String, playerPromoteId: String) {
        safeApiCallWithNotReturn {
            teamApi.promotePlayer(teamId = teamId, playerId = playerPromoteId)
        }
    }

    /**
     * Despromove um Administrador para membro normal (remove privilégios de gestão).
     *
     * @param teamId O ID da equipa.
     * @param adminDemoteId O ID do administrador a despromover.
     */
    suspend fun demoteAdmin(teamId: String, adminDemoteId: String) {
        safeApiCallWithNotReturn {
            teamApi.desmoteAdmin(teamId = teamId, playerId = adminDemoteId)
        }
    }

    /**
     * Remove (expulsa) um jogador da equipa.
     *
     * @param teamId O ID da equipa.
     * @param playerId O ID do jogador a remover.
     */
    suspend fun removePlayerTeam(teamId: String, playerId: String) {
        safeApiCallWithNotReturn {
            teamApi.removePlayerforTeam(teamId = teamId, playerId = playerId)
        }
    }
}

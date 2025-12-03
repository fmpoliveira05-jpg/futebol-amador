package com.example.amfootball.data.services

import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.dtos.team.toFormTeamDto
import com.example.amfootball.data.filters.FilterMembersTeam
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.interfaces.TeamApi
import com.example.amfootball.utils.safeApiCallWithNotReturn
import com.example.amfootball.utils.safeApiCallWithReturn
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Repositório responsável pela lógica de negócio e manipulação de dados de Equipas.
 *
 * Esta classe centraliza todas as operações relacionadas com equipas, desde a visualização de perfil (CRUD),
 * gestão de membros (promover, despromover, remover) até à pesquisa filtrada.
 *
 * A gestão de erros e validação de respostas HTTP é delegada nos utilitários [safeApiCallWithReturn]
 * e [safeApiCallWithNotReturn], garantindo um código limpo e livre de tratamento de erros repetitivo.
 *
 * @property teamApi A interface Retrofit injetada para comunicação com o backend.
 */
@Singleton
class TeamService @Inject constructor(
    private val teamApi: TeamApi
) {

    /**
     * Obtém o perfil completo de uma equipa para visualização detalhada.
     *
     * @param teamId O ID da equipa.
     * @return [ProfileTeamDto] contendo estatísticas, membros e informações gerais.
     * @throws Exception Propagada automaticamente em caso de erro de API ou rede.
     */
    suspend fun getTeamProfile(teamId: String): ProfileTeamDto {
        return safeApiCallWithReturn {
            teamApi.getTeamProfile(teamId = teamId)
        }
    }

    /**
     * Obtém os dados de uma equipa formatados para o formulário de edição.
     *
     * Este método demonstra um padrão eficiente:
     * 1. Obtém o perfil completo da API de forma segura.
     * 2. Aplica a função de extensão `.toFormTeamDto()` no resultado bem-sucedido.
     *
     * @param teamId O ID da equipa a editar.
     * @return [FormTeamDto] pronto para preencher os campos do formulário.
     */
    suspend fun getTeamToUpdate(teamId: String): FormTeamDto {
        return safeApiCallWithReturn {
            teamApi.getTeamProfile(teamId = teamId)
        }.toFormTeamDto()
    }

    /**
     * Obtém o nome e dados básicos de uma equipa adversária.
     * Útil para exibir cabeçalhos ou resumos em listas de jogos.
     *
     * @param teamId O ID da equipa adversária.
     * @return [TeamDto] com informações essenciais (Nome, ID, Logo).
     */
    suspend fun getNameTeam(teamId: String): TeamDto {
        return safeApiCallWithReturn {
            teamApi.getOpponentTeam(teamId = teamId)
        }
    }

    /**
     * Obtém uma lista de equipas com base em critérios de filtro (Pesquisa).
     *
     * Converte o objeto de filtros em Query Map antes de invocar a API.
     *
     * @param filter Objeto de filtros ou `null` para listar todas.
     * @return Lista de [ItemTeamInfoDto] resumidos.
     */
    suspend fun getListTeam(filter: FiltersListTeam?): List<ItemTeamInfoDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            teamApi.getListTeam(filters = filterMap)
        }
    }

    /**
     * Cria uma nova equipa na plataforma.
     *
     * @param team O DTO com os dados do formulário de criação.
     * @return O ID da nova equipa gerada (String).
     */
    suspend fun createTeam(team: FormTeamDto): String {
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
     * Lista os membros (jogadores/staff) de uma equipa, com filtros opcionais.
     *
     * @param teamId O ID da equipa.
     * @param filter Critérios de filtragem (cargo, nome, etc.).
     * @return Lista de [MemberTeamDto].
     */
    suspend fun getListMembers(teamId: String, filter: FilterMembersTeam?): List<MemberTeamDto> {
        val filters = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            teamApi.getListMembers(teamId = teamId, filters = filters)
        }
    }

    //TODO: Testar
    /**
     * Promove um membro a Administrador da equipa.
     *
     * Utiliza [safeApiCallWithNotReturn] para uma operação sem retorno de dados.
     *
     * @param teamId O ID da equipa.
     * @param playerPromoteId O ID do jogador a promover.
     */
    suspend fun promotePlayer(teamId: String, playerPromoteId: String) {
        safeApiCallWithNotReturn {
            teamApi.promotePlayer(teamId = teamId, playerId = playerPromoteId)
        }
    }

    //TODO: Testar
    /**
     * Despromove um Administrador para membro normal.
     *
     * @param teamId O ID da equipa.
     * @param adminDemoteId O ID do administrador a despromover.
     */
    suspend fun demoteAdmin(teamId: String, adminDemoteId: String) {
        safeApiCallWithNotReturn {
            teamApi.desmoteAdmin(teamId = teamId, playerId = adminDemoteId)
        }
    }

    //TODO: Testar
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
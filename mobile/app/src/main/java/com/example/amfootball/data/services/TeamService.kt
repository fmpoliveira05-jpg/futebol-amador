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
import com.example.amfootball.utils.handleApiError
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Repositório responsável por toda a lógica de dados relacionada com as Equipas.
 * Atua como a única fonte de verdade (Single Source of Truth) para o [ListTeamViewModel] e [TeamFormViewModel].
 *
 * Gere as chamadas à API, tratamento de erros HTTP e transformação de dados (DTOs).
 */
@Singleton
class TeamService @Inject constructor(
    private val teamApi: TeamApi
) {
    /**
     * Obtém o perfil completo de uma equipa para visualização.
     *
     * @param teamId O ID da equipa.
     * @return [ProfileTeamDto] com os dados detalhados.
     * @throws Exception Se houver erro de rede ("Sem conexão") ou erro da API (404, 500).
     */
    suspend fun getTeamProfile(teamId: String): ProfileTeamDto {
        return try {
            val response = teamApi.getTeamProfile(teamId = teamId)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    /**
     * Obtém os dados de uma equipa formatados especificamente para o formulário de edição.
     *
     * Reutiliza o endpoint de [getTeamProfile] mas converte o resultado para [FormTeamDto].
     *
     * @param teamId O ID da equipa a editar.
     * @return [FormTeamDto] pronto para preencher os campos da UI.
     */
    suspend fun getTeamToUpdate(teamId: String): FormTeamDto {
        return try {
            val response = teamApi.getTeamProfile(teamId = teamId)

            if (response.isSuccessful && response.body() != null) {
                val fullProfile = response.body()!!

                fullProfile.toFormTeamDto()
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    suspend fun getNameTeam(teamId: String): TeamDto {
        try {
            val response = teamApi.getOpponentTeam(teamId = teamId)

            if (response.isSuccessful && response.body() != null) {
                return response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    /**
     * Obtém uma lista filtrada de equipas.
     *
     * @param filter Objeto contendo os filtros aplicados (opcional).
     * @return Uma lista de [ItemTeamInfoDto].
     */
    suspend fun getListTeam(filter: FiltersListTeam?): List<ItemTeamInfoDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return try {
            val response = teamApi.getListTeam(filters = filterMap)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    /**
     * Cria uma nova equipa.
     *
     * @param team O DTO contendo os dados do formulário.
     * @return O ID da nova equipa criada (String).
     */
    suspend fun createTeam(team: FormTeamDto): String {
        return try {
            val response = teamApi.createTeam(team = team)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    /**
     * Atualiza os dados de uma equipa existente.
     *
     * @param teamId O ID da equipa a atualizar.
     * @param team O DTO com os novos dados.
     * @return O [FormTeamDto] atualizado retornado pelo servidor.
     */
    suspend fun updateTeam(teamId: String, team: FormTeamDto): FormTeamDto {
        return try {
            val response = teamApi.updateTeam(teamId = teamId, team = team)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    //TODO: Testar
    /**
     * Obtém a lista de membros de uma equipa específica, aplicando filtros de pesquisa.
     *
     * Faz um pedido à API enviando os critérios definidos em [filter] (ex: nome, idade, posição).
     * Se a resposta for bem-sucedida, retorna a lista de membros. Caso contrário, lança uma exceção
     * com a mensagem de erro do backend.
     *
     * @param teamId O ID da equipa da qual se pretende listar os membros.
     * @param filter O objeto [FilterMembersTeam] com os critérios de filtragem.
     * @return Uma lista de [MemberTeamDto] correspondente aos filtros aplicados.
     * @throws Exception Se ocorrer erro de rede ("Sem conexão") ou se a API retornar um erro (lançado via [handleApiError]).
     */
    suspend fun getListMembers(teamId: String, filter: FilterMembersTeam?): List<MemberTeamDto> {
        return try {
            val filters = filter?.toQueryMap() ?: emptyMap()

            val response = teamApi.getListMembers(teamId = teamId, filters = filters)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    //TODO: Testar
    /**
     * Promove um membro da equipa a Administrador.
     *
     * Envia um pedido para alterar o tipo de membro do jogador alvo para 'ADMIN_TEAM'.
     * Não retorna valor; o sucesso é indicado pela ausência de exceções.
     *
     * @param teamId O ID da equipa.
     * @param playerPromoteId O ID do jogador que será promovido.
     * @throws Exception Se o jogador não existir, se quem pede não tiver permissões, ou em caso de erro de rede.
     */
    suspend fun promotePlayer(teamId: String, playerPromoteId: String) {
        try {
            val response = teamApi.promotePlayer(teamId = teamId, playerId = playerPromoteId)

            if (!response.isSuccessful) {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    //TODO: Testar
    /**
     * Despromove um Administrador da equipa a membro normal.
     *
     * Retira os privilégios de gestão do administrador alvo, alterando o seu tipo para 'PLAYER'.
     *
     * @param teamId O ID da equipa.
     * @param adminDemoteId O ID do administrador que será despromovido.
     * @throws Exception Se ocorrer erro na API (ex: tentar despromover o dono da equipa) ou erro de conexão.
     */
    suspend fun demoteAdmin(teamId: String, adminDemoteId: String) {
        try {
            val response = teamApi.desmoteAdmin(teamId = teamId, playerId = adminDemoteId)

            if (!response.isSuccessful) {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    //TODO: Testar
    /**
     * Remove (expulsa) um jogador ou membro da equipa.
     *
     * Esta ação remove permanentemente o utilizador da lista de membros da equipa.
     *
     * @param teamId O ID da equipa.
     * @param playerId O ID do jogador a remover.
     * @throws Exception Se o utilizador não puder ser removido ou em caso de falha de rede.
     */
    suspend fun removePlayerTeam(teamId: String, playerId: String) {
        try {
            val response = teamApi.removePlayerforTeam(teamId = teamId, playerId = playerId)

            if (!response.isSuccessful) {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }
}
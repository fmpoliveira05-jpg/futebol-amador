package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.filters.FilterMembersTeam
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.QueryMap

/**
 * Interface de API responsável pela gestão integral das Equipas.
 *
 * Esta interface centraliza todas as operações relacionadas com equipas, incluindo:
 * - Ciclo de vida da equipa (Criação e Edição).
 * - Visualização de perfis públicos e de adversários.
 * - Pesquisa e listagem de equipas.
 * - Gestão de membros (Listagem, Expulsão, Promoção e Despromoção de cargos).
 */
interface TeamApi {
    /**
     * Cria uma nova equipa na plataforma.
     *
     * O utilizador que executa este pedido torna-se automaticamente o Administrador da equipa criada.
     *
     * Endpoint: POST api/Team
     *
     * @param team O DTO [FormTeamDto] contendo os dados essenciais para a criação (Nome, Sigla, Cidade, etc.).
     * @return [Response] contendo o Identificador Único (UUID) gerado para a nova equipa.
     */
    @POST(BaseEndpoints.teamApi)
    suspend fun createTeam(
        @Body team: FormTeamDto
    ): Response<String>

    /**
     * Atualiza os dados informativos de uma equipa existente.
     *
     * Permite alterar campos como nome, emblema, cidade ou estádio.
     * Apenas administradores da equipa têm permissão para realizar esta operação.
     *
     * Endpoint: PUT api/Team/{teamId}
     *
     * @param teamId O identificador único (UUID) da equipa a ser atualizada.
     * @param team O objeto [FormTeamDto] com os novos dados a persistir.
     * @return [Response] contendo o objeto atualizado [FormTeamDto] conforme salvo no servidor.
     */
    @PUT("${BaseEndpoints.teamApi}/{teamId}")
    suspend fun updateTeam(
        @Path("teamId") teamId: String,
        @Body team: FormTeamDto
    ): Response<FormTeamDto>

    /**
     * Obtém uma visão simplificada de uma equipa, otimizada para contextos de adversário.
     *
     * Geralmente utilizado em ecrãs de agendamento de jogos ou histórico de partidas,
     * onde não é necessário o perfil completo, apenas dados básicos (Nome, Sigla, ID).
     *
     * Endpoint: GET api/Team/opponent/{teamId}
     *
     * @param teamId O identificador único (UUID) da equipa alvo.
     * @return [Response] contendo os dados básicos da equipa [TeamDto].
     */
    @GET("${BaseEndpoints.teamApi}/opponent/{teamId}")
    suspend fun getOpponentTeam(
        @Path("teamId") teamId: String
    ): Response<TeamDto>


    /**
     * Obtém o perfil público completo e detalhado de uma equipa.
     *
     * Inclui todas as informações relevantes: dados do estádio, ranking, estatísticas de vitórias/derrotas,
     * e outros metadados para exibição na página de detalhe da equipa.
     *
     * Endpoint: GET api/Team/{id}
     *
     * @param teamId O identificador único (UUID) da equipa.
     * @return [Response] contendo o perfil detalhado da equipa [ProfileTeamDto].
     */
    @GET("${BaseEndpoints.teamApi}/{id}")
    suspend fun getTeamProfile(
        @Path("id") teamId: String
    ): Response<ProfileTeamDto>

    /**
     * Pesquisa e lista equipas registadas na plataforma com base em filtros dinâmicos.
     *
     * Útil para o diretório de equipas ou pesquisa global.
     *
     * Endpoint: GET api/Team/listTeams
     *
     * @param filters Um mapa de chave-valor contendo os critérios de pesquisa.
     * Ex: `mapOf("NameTeam" to "Lions", "City" to "Lisboa")`.
     * @return [Response] com uma lista resumida de equipas [ItemTeamInfoDto] correspondentes.
     */
    @GET("${BaseEndpoints.teamApi}/listTeams")
    suspend fun getListTeam(
        @QueryMap filters: Map<String, String>
    ): Response<List<ItemTeamInfoDto>>

    /**
     * Lista os membros (jogadores e staff) de uma equipa específica.
     *
     * Permite filtrar os membros dentro da equipa (ex: procurar por nome ou posição específica).
     *
     * Endpoint: GET api/Team/{teamId}/members
     *
     * @param teamId O identificador da equipa.
     * @param filters Mapa de filtros convertidos de [FilterMembersTeam] (ex: nome, posição).
     * @return [Response] contendo a lista de membros [MemberTeamDto].
     */
    @GET("${BaseEndpoints.teamApi}/{teamId}/members")
    suspend fun getListMembers(
        @Path("teamId") teamId: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<MemberTeamDto>>

    /**
     * Remove (expulsa) um jogador de uma equipa.
     *
     * Esta ação revoga a afiliação do jogador com a equipa imediatamente.
     * Requer privilégios de Administrador da equipa.
     *
     * Endpoint: DELETE api/Team/{teamId}/members/{playerIdToRemove}
     *
     * @param teamId O identificador da equipa.
     * @param playerId O identificador do jogador a ser removido.
     * @return [Response] vazia (Unit) em caso de sucesso.
     */
    @DELETE("${BaseEndpoints.teamApi}/{teamId}/members/{playerIdToRemove}")
    suspend fun removePlayerforTeam(
        @Path("teamId") teamId: String,
        @Path("playerIdToRemove") playerId: String
    ): Response<Unit>

    /**
     * Promove um membro da equipa ao cargo de Administrador (Co-Admin).
     *
     * O membro passará a ter permissões de gestão (editar equipa, aceitar pedidos, gerir jogos).
     * O status muda de 'PLAYER' para 'ADMIN_TEAM'.
     *
     * Endpoint: PUT api/Team/{teamId}/members/{playerId}/promote/{playerIdToPromote}
     *
     * @param teamId O identificador da equipa.
     * @param playerId O identificador do membro que receberá a promoção.
     * @return [Response] vazia (Unit) confirmando a promoção.
     */
    @PUT("${BaseEndpoints.teamApi}/{teamId}/members/{playerId}/promote/{playerIdToPromote}")
    suspend fun promotePlayer(
        @Path("teamId") teamId: String,
        @Path("playerIdToPromote") playerId: String
    ): Response<Unit>

    /**
     * Retira os privilégios de administrador de um membro, despromovendo-o.
     *
     * O membro volta a ser um jogador regular sem permissões de gestão.
     * O status muda de 'ADMIN_TEAM' para 'PLAYER'.
     *
     * Endpoint: PUT api/Team/{teamId}/members/{playerId}/demote/{adminIdToDemote}
     *
     * @param teamId O identificador da equipa.
     * @param playerId O identificador do administrador que será despromovido.
     * @return [Response] vazia (Unit) confirmando a despromoção.
     */
    @PUT("${BaseEndpoints.teamApi}/{teamId}/members/{playerId}/demote/{adminIdToDemote}")
    suspend fun desmoteAdmin(
        @Path("teamId") teamId: String,
        @Path("adminIdToDemote") playerId: String
    ): Response<Unit>
}
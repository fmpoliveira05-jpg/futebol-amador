package com.example.amfootball.data.interfaces.api

import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.BaseEndpoints
import com.example.amfootball.data.remote.dtos.homePageTeam.HomePageTeamDto
import com.example.amfootball.data.remote.dtos.membershipRequest.InviteTeamRequest
import com.example.amfootball.data.remote.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.remote.dtos.membershipRequest.RequestMemberShip
import com.example.amfootball.data.remote.dtos.player.MemberTeamDto
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.data.remote.dtos.team.FormTeamDto
import com.example.amfootball.data.remote.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.remote.dtos.team.ProfileTeamDto
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
     * Obtém os dados principais para o painel de controlo (Dashboard) da equipa.
     *
     * Este endpoint agrega informações vitais para a página inicial da equipa, como
     * próximos jogos, estatísticas rápidas e notificações pendentes.
     *
     * Endpoint: GET api/Team/homeTeam/{idTeam}
     *
     * @param teamId O identificador único (UUID) da equipa.
     * @return [Response] contendo o objeto [HomePageTeamDto] com os dados do dashboard.
     */
    @GET("${BaseEndpoints.TEAM_API}/homeTeam/{${Arguments.ID_TEAM}}")
    suspend fun getHomePageTeam(
        @Path(Arguments.ID_TEAM) teamId: String
    ): Response<HomePageTeamDto>


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
    @POST(BaseEndpoints.TEAM_API)
    suspend fun createTeam(
        @Body team: FormTeamDto
    ): Response<FormTeamDto>

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
    @PUT("${BaseEndpoints.TEAM_API}/{${Arguments.TEAM_ID}}")
    suspend fun updateTeam(
        @Path(Arguments.TEAM_ID) teamId: String,
        @Body team: FormTeamDto
    ): Response<FormTeamDto>


    @GET("${BaseEndpoints.TEAM_API}/DataUpdate/{${Arguments.TEAM_ID}}")
    suspend fun getDataToUpdateTeam(
        @Path(Arguments.TEAM_ID) teamId: String,
    ): Response<FormTeamDto>

    /**
     * Ponto de acesso (Endpoint) para apagar uma equipa através do seu ID.
     *
     * Esta chamada HTTP DELETE notifica o backend para remover permanentemente a equipa especificada.
     *
     * @param teamId O identificador único (ID) da equipa a ser eliminada, extraído do Path da URL.
     * @return Um objeto [Response<Unit>], onde:
     * - [Unit] significa que não é esperado corpo de resposta (sucesso é indicado pelo código HTTP, ex: 204 No Content).
     * - [Response] permite verificar o status HTTP (sucesso ou erro).
     */
    @DELETE("${BaseEndpoints.TEAM_API}/{${Arguments.TEAM_ID}}")
    suspend fun deleteTeam(
        @Path(Arguments.TEAM_ID) teamId: String
    ): Response<Unit>

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
    @GET("${BaseEndpoints.TEAM_API}/opponent/{${Arguments.TEAM_ID}}")
    suspend fun getOpponentTeam(
        @Path(Arguments.TEAM_ID) teamId: String
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
    @GET("${BaseEndpoints.TEAM_API}/{id}")
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
    @GET("${BaseEndpoints.TEAM_API}/listTeams")
    suspend fun getListTeam(
        @QueryMap filters: Map<String, String>
    ): Response<List<ItemTeamInfoDto>>

    /**
     * Pesquisa equipas especificamente para o envio de convites de jogo (Match Invites).
     *
     * Diferente da listagem genérica, este endpoint pode aplicar regras de negócio específicas,
     * como excluir a própria equipa do utilizador ou equipas indisponíveis.
     *
     * Endpoint: GET api/Team/{teamId}/search
     *
     * @param teamId O identificador da equipa que está a realizar a pesquisa (a equipa desafiante).
     * @param filters Filtros de pesquisa (e.g., nome da equipa adversária, localização).
     * @return [Response] com a lista de equipas adversárias potenciais [ItemTeamInfoDto].
     */
    @GET("${BaseEndpoints.TEAM_API}/{${Arguments.TEAM_ID}}/search")
    suspend fun getListTeamMatchInvite(
        @Path(Arguments.TEAM_ID) teamId: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<ItemTeamInfoDto>>

    /**
     * Lista as equipas disponíveis para um jogador enviar um pedido de adesão.
     *
     * Permite que um jogador sem equipa (Free Agent) pesquise equipas para se juntar.
     *
     * Endpoint: GET api/Player/{playerId}/listTeamsToMemberShipRequest
     *
     * @param playerId O identificador do jogador que está à procura de equipa.
     * @param filters Filtros de pesquisa (e.g., nome da equipa, cidade).
     * @return [Response] com a lista de equipas [ItemTeamInfoDto].
     */
    @GET("${BaseEndpoints.PLAYER_API}/{${Arguments.PLAYER_ID}}/listTeamsToMemberShipRequest")
    suspend fun getListTeamMembershipRequest(
        @Path(Arguments.PLAYER_ID) playerId: String,
        @QueryMap filters: Map<String, String>,
    ): Response<List<ItemTeamInfoDto>>

    /**
     * Envia um pedido de adesão (Membership Request) ou um convite para uma equipa.
     *
     * Inicia o processo de associação entre um utilizador e uma equipa.
     *
     * Endpoint: POST api/Team/{teamId}/membership-requests/send
     *
     * @param teamId O identificador da equipa alvo.
     * @param request O corpo do pedido [InviteTeamRequest], contendo os detalhes do convite/pedido.
     * @return [Response] contendo os detalhes do pedido criado [MembershipRequestInfoDto].
     */
    @POST("${BaseEndpoints.TEAM_API}/{${Arguments.TEAM_ID}}/membership-requests/send")
    suspend fun sendMemberShipRequest(
        @Path(Arguments.TEAM_ID) teamId: String,
        @Body request: InviteTeamRequest
    ): Response<MembershipRequestInfoDto>

    /**
     * Lista os pedidos de adesão pendentes recebidos por uma equipa.
     *
     * Usado pelos administradores da equipa para visualizar quem solicitou entrar na equipa.
     *
     * Endpoint: GET api/Team/{teamId}/membership-request
     *
     * @param teamId O identificador da equipa.
     * @param filters Filtros opcionais (e.g., filtrar por estado do pedido).
     * @return [Response] com a lista de pedidos [MembershipRequestInfoDto].
     */
    @GET("${BaseEndpoints.TEAM_API}/{${Arguments.TEAM_ID}}/membership-request")
    suspend fun listMemberShipRequest(
        @Path(Arguments.TEAM_ID) teamId: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<MembershipRequestInfoDto>>

    /**
     * Aceita um pedido de adesão pendente.
     *
     * Confirma a entrada de um jogador na equipa.
     *
     * Endpoint: POST api/Team/{teamId}/membership-request/accept
     *
     * @param teamId O identificador da equipa que está a aceitar o pedido.
     * @param request O objeto [RequestMemberShip] contendo o ID do pedido/jogador a aceitar.
     * @return [Response] vazia indicando sucesso.
     */
    @POST("${BaseEndpoints.TEAM_API}/{${Arguments.TEAM_ID}}/membership-request/accept")
    suspend fun acceptMemberShipRequest(
        @Path(Arguments.TEAM_ID) teamId: String,
        @Body request: RequestMemberShip
    ): Response<Unit>

    /**
     * Rejeita um pedido de adesão pendente.
     *
     * Recusa a entrada de um jogador na equipa e remove o pedido da lista de pendentes.
     *
     * Endpoint: DELETE api/Team/{teamId}/membership-request/{requestId}/reject
     *
     * @param teamId O identificador da equipa.
     * @param requestId O identificador único do pedido de adesão a rejeitar.
     * @return [Response] vazia indicando sucesso.
     */
    @DELETE("${BaseEndpoints.TEAM_API}/{${Arguments.TEAM_ID}}/membership-request/{${Arguments.REQUEST_ID}}/reject")
    suspend fun rejectMemberShipRequest(
        @Path(Arguments.TEAM_ID) teamId: String,
        @Path(Arguments.REQUEST_ID) requestId: String,
    ): Response<Unit>

    /**
     * Lista os membros (jogadores e staff) de uma equipa específica.
     *
     * Permite filtrar os membros dentro da equipa (ex: procurar por nome ou posição específica).
     *
     * Endpoint: GET api/Team/{teamId}/members
     *
     * @param teamId O identificador da equipa.
     * @param filters Mapa de filtros convertidos de [com.example.amfootball.data.filters.FilterMembersTeam] (ex: nome, posição).
     * @return [Response] contendo a lista de membros [MemberTeamDto].
     */
    @GET("${BaseEndpoints.TEAM_API}/{teamId}/members")
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
    @DELETE("${BaseEndpoints.TEAM_API}/{teamId}/members/{playerIdToRemove}")
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
     * Endpoint: PUT api/Team/{teamId}/members/promote/{playerIdToPromote}
     *
     * @param teamId O identificador da equipa.
     * @param playerId O identificador do membro que receberá a promoção.
     * @return [Response] vazia (Unit) confirmando a promoção.
     */
    @PUT("${BaseEndpoints.TEAM_API}/{teamId}/members/promote/{playerIdToPromote}")
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
     * Endpoint: PUT api/Team/{teamId}/members/demote/{adminIdToDemote}
     *
     * @param teamId O identificador da equipa.
     * @param playerId O identificador do administrador que será despromovido.
     * @return [Response] vazia (Unit) confirmando a despromoção.
     */
    @PUT("${BaseEndpoints.TEAM_API}/{teamId}/members/demote/{adminIdToDemote}")
    suspend fun desmoteAdmin(
        @Path("teamId") teamId: String,
        @Path("adminIdToDemote") playerId: String
    ): Response<Unit>
}
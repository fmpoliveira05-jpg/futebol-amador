using Application.DTOs.Filters;
using Application.DTOs.Team;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio e Controlo de Integridade para a entidade [Team].
    /// 
    /// Esta interface define as regras de validação síncrona que garantem a validade dos dados e a
    /// aplicação de permissões (RBAC) antes que qualquer transação seja executada na base de dados.
    /// </summary>
    public interface ITeamValidator
    {
        /// <summary>
        /// Valida as condições para a criação de uma nova equipa (unicidade de nome, existência do Rank padrão e status do jogador criador).
        /// </summary>
        /// <param name="createTeamDto">O DTO com os dados da nova equipa.</param>
        /// <param name="rank">O Rank padrão a ser atribuído.</param>
        /// <param name="team">Equipa existente com o mesmo nome (se houver).</param>
        /// <param name="playerCreating">O jogador que está a criar a equipa.</param>
        void CreateTeamValidation(CreateTeamDto createTeamDto, Rank rank, Team? team, Player? playerCreating);

        /// <summary>
        /// Valida as condições para a atualização dos dados de uma equipa (ex: unicidade do novo nome).
        /// </summary>
        /// <param name="existingTeamNewName">A equipa encontrada com o novo nome (para verificação de unicidade).</param>
        /// <param name="team">A entidade da equipa a ser atualizada.</param>
        void UpdateTeamValidation(Team? existingTeamNewName, Team? team);

        /// <summary>
        /// Valida as condições para a eliminação de uma equipa (existência e ausência de partidas ativas).
        /// </summary>
        /// <param name="team">A entidade da equipa a ser eliminada (carregada com o calendário e partidas).</param>
        void DeleteTeamValidation(Team? team);

        /// <summary>
        /// Valida se a entidade de detalhes da equipa existe (não é nula) após a consulta.
        /// </summary>
        /// <param name="team">O DTO de detalhes da equipa.</param>
        void GetTeamByIdValidation(TeamDetailsDto team);

        /// <summary>
        /// Valida se a coleção de equipas retornada não é nula nem vazia.
        /// </summary>
        /// <param name="Teams">A coleção de equipas.</param>
        void GetAllTeamsValidation(IEnumerable<Team?> Teams);

        /// <summary>
        /// Valida se a equipa procurada existe (não é nula).
        /// </summary>
        /// <param name="team">A entidade da equipa.</param>
        void ValidateTeamSearch(Team? team);

        /// <summary>
        /// Valida as condições e permissões para remover (expulsar) um jogador de uma equipa.
        /// </summary>
        /// <remarks>
        /// **Regra RBAC:** Aplica a regra de antiguidade se o alvo for um Admin.
        /// </remarks>
        /// <param name="team">A equipa.</param>
        /// <param name="playerRemoving">O administrador que está a remover/expulsar.</param>
        /// <param name="playerRemoved">O jogador que está a ser removido.</param>
        void RemovePlayerFromTeamValidation(Team? team, Player? playerRemoving, Player? playerRemoved);

        /// <summary>
        /// Valida a existência da equipa antes de obter a lista de membros.
        /// </summary>
        /// <param name="team">A equipa.</param>
        void GetTeamMembersValidation(Team? team);

        /// <summary>
        /// Valida as condições para promover um membro a Administrador da equipa.
        /// </summary>
        /// <remarks>
        /// Verifica o limite máximo de administradores na equipa.
        /// </remarks>
        /// <param name="team">A equipa.</param>
        /// <param name="memberToPromote">O membro a ser promovido.</param>
        /// <param name="memberPromoting">O admin que executa a promoção.</param>
        void PromoteMemberToAdminValidation(Team? team, Player? memberToPromote, Player? memberPromoting);

        /// <summary>
        /// Valida as regras de despromoção de um Administrador a Membro regular.
        /// </summary>
        /// <remarks>
        /// **Regra RBAC:** Aplica a regra de antiguidade: o Admin que despromove deve ser mais antigo que o alvo.
        /// </remarks>
        /// <param name="team">A equipa.</param>
        /// <param name="adminToDemote">O admin alvo da despromoção.</param>
        /// <param name="adminDemoting">O admin que executa a ação.</param>
        void DemoteAdminToMemberValidation(Team? team, Player? adminToDemote, Player? adminDemoting);

        /// <summary>
        /// Valida a existência da equipa antes de obter a lista de pedidos de adesão.
        /// </summary>
        /// <param name="team">A equipa (carregada com MembershipRequests).</param>
        void GetMembershipRequestsValidation(Team? team);

        /// <summary>
        /// Valida o pedido de aprovação de adesão/convite.
        /// </summary>
        /// <remarks>
        /// Verifica a existência da equipa e se o pedido está pendente na coleção da equipa.
        /// </remarks>
        /// <param name="team">A equipa.</param>
        /// <param name="requestToDelete">O ID do pedido de adesão.</param>
        void ApproveMembershipRequestValidation(Team? team, Guid requestToDelete);

        /// <summary>
        /// Valida o pedido de rejeição de adesão/convite.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <param name="requestToDelete">O ID do pedido de adesão.</param>
        void RejectMembershipRequestValidation(Team? team, Guid requestToDelete);

        /// <summary>
        /// Valida um novo pedido de adesão (Join Request ou Convite de Recrutamento).
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Não pode haver pedido pendente (duplicado) e a equipa não pode estar cheia.
        /// </remarks>
        /// <param name="mr">O pedido de adesão (se já existir).</param>
        /// <param name="team">A equipa alvo.</param>
        void SendMembershipRequestValidation(MembershipRequest? mr, Team? team);

        /// <summary>
        /// Valida a existência da equipa antes de obter a agenda.
        /// </summary>
        /// <param name="team">A equipa.</param>
        void GetTeamScheduleValidation(Team? team);

        /// <summary>
        /// Valida o ID da equipa (não pode ser Guid.Empty) antes de iniciar uma pesquisa específica.
        /// </summary>
        /// <param name="idTeam">O ID (GUID) da equipa.</param>
        void ValidateVariableSearchTeam(Guid idTeam);

        /// <summary>
        /// Valida o ID da equipa de pesquisa e a consistência dos filtros de listagem.
        /// </summary>
        /// <param name="idTeam">O ID (GUID) da equipa.</param>
        /// <param name="filter">O DTO de filtros.</param>
        void ValidateVaribleSearchTeamWithFilters(Guid idTeam, FilterListTeamDto filter);

        /// <summary>
        /// Valida a consistência do intervalo de filtros numéricos (Pontos, Idade Média, Contagem de Membros).
        /// </summary>
        /// <param name="filter">O DTO de filtros de listagem de equipas.</param>
        void ValidateFilterTeams(FilterListTeamDto? filter);

        /// <summary>
        /// Valida os filtros de pesquisa para jogadores sem equipa (Agentes Livres).
        /// </summary>
        /// <param name="filter">O DTO de filtros.</param>
        void ValidateFiltersGetPlayersWithout(FilterTeamDto? filter);
    }
}
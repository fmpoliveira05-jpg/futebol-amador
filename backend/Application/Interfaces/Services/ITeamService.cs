using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão completa da entidade [Team].
    /// 
    /// Esta interface define os métodos de lógica de negócio para o ciclo de vida de uma equipa,
    /// incluindo criação, atualização de perfil, gestão de membros (promoção/remoção) e funções de pesquisa.
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// Cria e regista uma nova equipa na plataforma.
        /// </summary>
        /// <remarks>
        /// Esta operação deve validar que o jogador criador ([adminUserId]) não tem equipa e designá-lo como o primeiro Admin.
        /// </remarks>
        /// <param name="teamDto">DTO com os dados da equipa a criar.</param>
        /// <param name="adminUserId">ID do jogador autenticado que está a criar a equipa.</param>
        /// <returns>Uma tarefa assíncrona que retorna o ID (GUID) da nova equipa criada.</returns>
        Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, string adminUserId);

        /// <summary>
        /// Obtém os dados detalhados do perfil de uma equipa, incluindo Pitch e a lista de membros.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <returns>O DTO [TeamDetailsDto] com os detalhes do perfil.</returns>
        Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);

        /// <summary>
        /// Atualiza as informações básicas (Nome, Descrição, Pitch) de uma equipa.
        /// </summary>
        /// <remarks>
        /// A operação requer que o [currentUserId] seja um administrador da equipa.
        /// </remarks>
        /// <param name="teamId">ID da equipa a ser atualizada.</param>
        /// <param name="dto">DTO com os novos dados.</param>
        /// <param name="currentUserId">ID do utilizador que está a executar a atualização.</param>
        Task UpdateTeamInfoAsync(Guid teamId, CreateTeamDto dto, string currentUserId);

        /// <summary>
        /// Elimina uma equipa permanentemente.
        /// </summary>
        /// <remarks>
        /// **Transação:** Requer que o [currentUserId] seja um administrador. Remove a afiliação de todos os membros e elimina a entidade.
        /// </remarks>
        /// <param name="teamId">ID da equipa a eliminar.</param>
        /// <param name="currentUserId">ID do utilizador (Admin) que executa a eliminação.</param>
        Task DeleteTeamAsync(Guid teamId, string currentUserId);

        /// <summary>
        /// Obtém um DTO básico da equipa (Nome e ID) para uso como adversário ou em resumos.
        /// </summary>
        /// <param name="teamId">O ID (GUID) da equipa.</param>
        /// <returns>O DTO [TeamDto] ou null.</returns>
        Task<TeamDto> getOpponent(Guid teamId);

        /// <summary>
        /// Obtém a lista de todas as equipas (sem exclusão de origem), aplicando filtros de pesquisa.
        /// </summary>
        /// <param name="filters">Filtros de pesquisa (Nome, Rank, Pontos, etc.).</param>
        /// <returns>Lista de [InfoTeamsDto] resumidos.</returns>
        Task<List<InfoTeamsDto>> GetListTeams(FilterListTeamDto? filters);

        /// <summary>
        /// Obtém a lista de todos os membros de uma equipa.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <returns>Lista de [PlayerDetailsDto] (membros).</returns>
        Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId);

        /// <summary>
        /// Obtém a lista de membros de uma equipa com filtros aplicados (ex: por Posição, Idade).
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="filters">Filtros de jogador.</param>
        /// <returns>Lista filtrada de [PlayerDetailsDto].</returns>
        Task<List<PlayerDetailsDto>> GetTeamPlayersAsyncWithFilters(Guid teamId, FilterTeamPlayers filters);

        /// <summary>
        /// Remove (expulsa) um jogador de uma equipa.
        /// </summary>
        /// <remarks>
        /// Requer permissão de Admin. O jogador removido torna-se agente livre.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerIdToRemove">ID do jogador a ser expulso.</param>
        /// <param name="playerRemovingId">ID do administrador que executa a remoção.</param>
        Task RemovePlayerFromTeamAsync(Guid teamId, string playerIdToRemove, string playerRemovingId);

        /// <summary>
        /// Promove um membro da equipa a Administrador.
        /// </summary>
        /// <remarks>
        /// Requer permissão de Admin para promover. Atualiza a flag [IsAdmin] do jogador.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerIdToPromoteId">ID do jogador a ser promovido.</param>
        /// <param name="playerPromotingId">ID do administrador que executa a ação.</param>
        Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerPromotingId);

        /// <summary>
        /// Despromove um administrador para o estatuto de membro regular.
        /// </summary>
        /// <remarks>
        /// Requer permissão de Admin. Aplica a regra de **Antiguidade** (o admin que despromove deve ser mais antigo).
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="adminIdToDemote">ID do administrador a ser despromovido.</param>
        /// <param name="currentAdminId">ID do administrador que executa a ação.</param>
        Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string currentAdminId);

        /// <summary>
        /// Obtém uma lista de equipas elegíveis para Match Invite/Desafio, excluindo a equipa de origem (sem filtros).
        /// </summary>
        /// <param name="idTeam">O ID da equipa que está a fazer a pesquisa.</param>
        /// <returns>Lista de [InfoTeamsDto] (equipas alvo).</returns>
        Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam);

        /// <summary>
        /// Obtém uma lista de equipas elegíveis para Match Invite/Desafio, aplicando filtros complexos.
        /// </summary>
        /// <param name="idTeam">O ID da equipa que está a fazer a pesquisa.</param>
        /// <param name="filters">Filtros de pesquisa (Pontos, Rank, Idade Média, Localidade).</param>
        /// <returns>Lista de [InfoTeamsDto] filtrados.</returns>
        Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters);

        /// <summary>
        /// Obtém a lista completa de Jogadores Agentes Livres (sem equipa) sem filtros.
        /// </summary>
        /// <returns>Lista de [PlayerWithoutTeamInfoDto].</returns>
        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam();

        /// <summary>
        /// Obtém a lista de Jogadores Agentes Livres (sem equipa) aplicando filtros.
        /// </summary>
        /// <param name="filter">Filtros de pesquisa (Altura, Posição, Nome, Cidade).</param>
        /// <returns>Lista filtrada de [PlayerWithoutTeamInfoDto].</returns>
        Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterTeamDto filter);
    }
}
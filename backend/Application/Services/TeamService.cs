using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela lógica de negócio e gestão de dados da entidade [Team].
    /// 
    /// Centraliza todas as operações relacionadas com equipas, membros e perfis de gestão (CRUD, promoções, filtros),
    /// atuando como o ponto de orquestração entre Repositórios, Validadores e serviços de Notificação.
    /// </summary>
    public class TeamService : ITeamService
    {
        #region Initialize
        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly IRankRepository RankRepository;
        private readonly ITeamValidator TeamValidator;
        private readonly IMembershipRequestRepository MembershipRequestRepository;
        private readonly IPlayerValidator PlayerValidator;
        private readonly IPlayerAuthorizationValidator AuthorizationValidator;
        private readonly INotificationService notificationService;

        /// <summary>
        /// Construtor do TeamService.
        /// </summary>
        /// <param name="teamRepository">Repositório de Equipas.</param>
        /// <param name="playerRepository">Repositório de Jogadores.</param>
        /// <param name="unityOfWork">Unidade de Trabalho para gerir transações.</param>
        /// <param name="teamValidator">Validador de Regras de Negócio de Equipa.</param>
        /// <param name="rankRepository">Repositório de Ranks (para obter o rank padrão).</param>
        /// <param name="membershipRequestRepository">Repositório de Pedidos de Adesão.</param>
        /// <param name="playerValidator">Validador de Jogadores.</param>
        /// <param name="authorizationValidator">Validador de Controlo de Acesso (RBAC).</param>
        /// <param name="notificationService">Serviço de Hub para envio de notificações em tempo real.</param>
        public TeamService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IRankRepository rankRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IPlayerValidator playerValidator,
            IPlayerAuthorizationValidator authorizationValidator,
            INotificationService notificationService)
        {
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            UnityOfWork = unityOfWork;
            TeamValidator = teamValidator;
            RankRepository = rankRepository;
            AuthorizationValidator = authorizationValidator;
            this.notificationService = notificationService;
        }
        #endregion

        #region CRUD Team

        /// <summary>
        /// Cria uma nova equipa e designa o jogador criador como Administrador.
        /// </summary>
        /// <remarks>
        /// **Transação:**
        /// 1. Valida se o jogador pode criar uma equipa (não tem equipa).
        /// 2. Cria a entidade [Team] (com Calendar, Pitch, Rank e 0 pontos).
        /// 3. Atualiza o jogador criador ([playerCreating]) para ser Admin e membro da nova equipa.
        /// 4. Persiste as alterações ([UnityOfWork.SaveChangesAsync]).
        /// </remarks>
        /// <param name="teamDto">Dados da equipa a criar.</param>
        /// <param name="playerId">ID do jogador autenticado que está a criar.</param>
        /// <returns>O ID (GUID) da nova equipa criada.</returns>
        public async Task<CreateTeamDto> CreateTeamAsync(CreateTeamDto teamDto, string playerId)
        {
            var playerCreating = await PlayerRepository.GetPlayerByIdAsync(playerId);
            AuthorizationValidator.ValidatePlayerAutorizationWithoutTeam(playerCreating);

            var existingTeam = await TeamRepository.GetTeamByNameAsync(teamDto.Name);
            var rank = await RankRepository.GetDefaultRankAsync();

            TeamValidator.CreateTeamValidation(teamDto, rank, existingTeam, playerCreating);

            var newTeam = new Team(
                teamDto.Name,
                teamDto.Description,
                teamDto.icon,
                new Pitch(teamDto.HomePitch.Name, teamDto.HomePitch.Address),
                rank
            );

            playerCreating.IsAdmin = true;
            playerCreating.IdTeam = newTeam.Id;
            playerCreating.Team = newTeam;
            playerCreating.IsAdminLastChangedAt = DateTime.UtcNow;
            newTeam.Members ??= new List<Player>();
            newTeam.Members.Add(playerCreating);

            await TeamRepository.AddAsync(newTeam);

            PlayerRepository.UpdatePlayer(playerCreating);

            await UnityOfWork.SaveChangesAsync();

            teamDto.Id = newTeam.Id;
            return teamDto;
        }

        /// <summary>
        /// Elimina uma equipa permanentemente.
        /// </summary>
        /// <remarks>
        /// **Regras:** Apenas administradores podem eliminar. A equipa não pode ter partidas agendadas/ativas.
        /// **Side-Effects:** Remove a afiliação (IdTeam = null, IsAdmin = false) de todos os membros.
        /// </remarks>
        /// <param name="teamId">ID da equipa a ser eliminada.</param>
        /// <param name="currentUserId">ID do utilizador que está a executar a deleção (deve ser Admin).</param>
        public async Task DeleteTeamAsync(Guid teamId, string currentUserId)
        {
            var playerTryingToDelete = await PlayerRepository.GetPlayerByIdAsync(currentUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerTryingToDelete, teamId);

            var teamToDelete = await TeamRepository.GetTeamForDeletionAsync(teamId);

            TeamValidator.DeleteTeamValidation(teamToDelete);
            
            foreach (var member in teamToDelete.Members)
            {
                member.IdTeam = null;
                if (member.IsAdmin)
                {
                    member.IsAdmin = false;
                    member.IsAdminLastChangedAt = DateTime.UtcNow;
                }
            }

            /*TODO: Meter para todas as partidas dessa team serem cancelados pelo 
            motivo que a equipa foi eliminada
            */
            TeamRepository.DeleteTeam(teamToDelete);

            await UnityOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza as informações básicas (Nome, Descrição, Pitch) de uma equipa.
        /// </summary>
        /// <remarks>
        /// **Regras:** Apenas administradores podem atualizar. O novo nome deve ser único.
        /// </remarks>
        /// <param name="teamId">ID da equipa a ser atualizada.</param>
        /// <param name="dto">DTO com os novos dados.</param>
        /// <param name="currentUserId">ID do utilizador que está a atualizar (deve ser Admin).</param>
        public async Task<CreateTeamDto> UpdateTeamInfoAsync(Guid teamId, CreateTeamDto dto, string currentUserId)
        {
            var playerTryingToUpdate = await PlayerRepository.GetPlayerByIdAsync(currentUserId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerTryingToUpdate, teamId);

            var teamToUpdate = await TeamRepository.GetTeamForUpdateAsync(teamId);
            Team? teamWithSameName = null;

            if (dto.Name != null)
            {
                teamWithSameName = await TeamRepository.GetTeamByNameAsync(dto.Name);
            }

            TeamValidator.UpdateTeamValidation(teamWithSameName, teamToUpdate);

            teamToUpdate.Name = dto.Name ?? teamToUpdate.Name;
            teamToUpdate.Description = dto.Description ?? teamToUpdate.Description;
            teamToUpdate.Icon = dto.icon ?? teamToUpdate.Icon;
            teamToUpdate.Pitch.Name = dto.HomePitch.Name ?? teamToUpdate.Pitch.Name;
            teamToUpdate.Pitch.Address = dto.HomePitch.Address ?? teamToUpdate.Pitch.Address;

            TeamRepository.UpdateTeam(teamToUpdate);

            await UnityOfWork.SaveChangesAsync();
            return dto;
        }

        /// <summary>
        /// Obtém os dados detalhados do perfil de uma equipa.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <returns>O DTO [TeamDetailsDto] completo.</returns>
        public async Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamDetailsDtoAsync(teamId);

            TeamValidator.GetTeamByIdValidation(team);

            return team;
        }

        /// <summary>
        /// Obtém uma lista de equipas, aplicando filtros globais de pesquisa.
        /// </summary>
        /// <param name="filters">Filtros de pesquisa (Nome, Rank, Pontos, etc.).</param>
        /// <returns>Lista de [InfoTeamsDto] resumidos.</returns>
        public async Task<List<InfoTeamsDto>> GetListTeams(FilterListTeamDto? filters)
        {
            if (filters != null)
            {
                TeamValidator.ValidateFilterTeams(filters);
            }

            var listTeam = await TeamRepository.GetListTeams(filters);

            return listTeam;
        }

        #endregion

        #region Admins Manager

        /// <summary>
        /// Promove um membro da equipa a Administrador.
        /// </summary>
        /// <remarks>
        /// **Regras:** Apenas admins podem promover. A equipa não pode exceder o limite de admins.
        /// **Side-Effects:** Atualiza a flag [IsAdmin] do jogador e envia notificação.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerIdToPromoteId">ID do jogador a ser promovido.</param>
        /// <param name="playerIdToPromotingId">ID do administrador que executa a ação.</param>
        public async Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerIdToPromotingId)
        {
            var playerPromoting = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromotingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerPromoting, teamId);

            var playerToPromote = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromoteId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            
            TeamValidator.PromoteMemberToAdminValidation(existingTeam, playerToPromote, playerPromoting);

            await notificationService.SendUserAsync(playerIdToPromoteId, "Team Promotion", $"You have been promoted to admin of the team {existingTeam.Name}.");

            playerToPromote.IsAdmin = true;
            playerToPromote.IsAdminLastChangedAt = DateTime.UtcNow;

            await UnityOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Despromove um administrador para o estatuto de membro regular.
        /// </summary>
        /// <remarks>
        /// **Regras:** Apenas admins podem despromover. Aplica-se a regra de **Antiguidade** (o admin que despromove deve ser mais antigo).
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="adminIdToDemote">ID do administrador a ser despromovido.</param>
        /// <param name="adminDemotingId">ID do administrador que executa a ação.</param>
        public async Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string adminDemotingId)
        {
            var playerDemoting = await PlayerRepository.GetPlayerByIdAsync(adminDemotingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerDemoting, teamId);

            var playerToDemote = await PlayerRepository.GetPlayerByIdAsync(adminIdToDemote);
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            TeamValidator.DemoteAdminToMemberValidation(existingTeam, playerToDemote, playerDemoting);

            playerToDemote.IsAdmin = false;
            playerToDemote.IsAdminLastChangedAt = DateTime.UtcNow;
            await UnityOfWork.SaveChangesAsync();
            await notificationService.SendUserAsync(adminIdToDemote, "Team Demotion", $"You have been demoted to player of the team {existingTeam.Name}.");
        }

        #endregion

        #region Members Team

        /// <summary>
        /// Obtém a lista de membros de uma equipa para visualização.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <returns>Lista de [PlayerDetailsDto] (membros).</returns>
        public async Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            var today = DateTime.Today;
            var playerDtos = team.Members.Select(player => new PlayerDetailsDto
            {
                PlayerId = player.Id,
                Name = player.Name,
                Email = player.Email,
                PhoneNumber = player.Phone,
                IsAdmin = player.IsAdmin,
                Height = player.Height,
                Position = player.Position,
                DateOfBirth = player.DateOfBirth,
                Age = today.Year - player.DateOfBirth.Year,
                Team = player.IdTeam.HasValue ? new TeamDto
                {
                    IdTeam = player.IdTeam.Value,
                    Name = player.Team?.Name
                } : null,
                Address = player.Address,
            }).ToList();

            return playerDtos;
        }

        /// <summary>
        /// Obtém a lista de membros de uma equipa com filtros aplicados.
        /// </summary>
        /// <remarks>
        /// Delega a filtragem complexa (Idade, Posição, Nome, etc.) para o repositório, que a executa no servidor.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="filters">Filtros a aplicar.</param>
        /// <returns>Lista filtrada de [PlayerDetailsDto].</returns>
        public async Task<List<PlayerDetailsDto>> GetTeamPlayersAsyncWithFilters(Guid teamId, FilterTeamPlayers filters)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            return await TeamRepository.GetTeamPlayersDtoAsyncWithFilters(teamId, filters);
        }

        /// <summary>
        /// Remove (expulsa) um jogador de uma equipa.
        /// </summary>
        /// <remarks>
        /// **Regras:** Apenas admins podem remover. O admin que remove deve respeitar a regra de antiguidade se o alvo for Admin.
        /// **Side-Effects:** O jogador removido torna-se agente livre (IdTeam = null, IsAdmin = false).
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerIdToRemove">ID do jogador a ser expulso.</param>
        /// <param name="playerRemovingId">ID do administrador que executa a remoção.</param>
        public async Task RemovePlayerFromTeamAsync(Guid teamId, string playerIdToRemove, string playerRemovingId)
        {
            var playerRemoving = await PlayerRepository.GetPlayerByIdAsync(playerRemovingId);
            AuthorizationValidator.ValidatePlayerAutorizationIsAdmin(playerRemoving, teamId);

            var playerToRemove = await PlayerRepository.GetPlayerByIdAsync(playerIdToRemove);
            AuthorizationValidator.ValidatePlayerAutorizationIsMember(playerToRemove, teamId);

            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.RemovePlayerFromTeamValidation(existingTeam, playerRemoving, playerToRemove);

            existingTeam.Members.Remove(playerToRemove);
            playerToRemove.IdTeam = null;
            if (playerToRemove.IsAdmin)
            {
                playerToRemove.IsAdmin = false;
                playerToRemove.IsAdminLastChangedAt = DateTime.UtcNow;
            }

            await UnityOfWork.SaveChangesAsync();
            await notificationService.SendUserAsync(playerIdToRemove, "Team Ban", $"You have been removed from the team {existingTeam.Name}.");
        }

        #endregion

        #region List Team To MatchInvite

        /// <summary>
        /// Obtém uma lista de equipas elegíveis para receber um convite de partida (Match Invite) da equipa de origem.
        /// </summary>
        /// <param name="idTeam">ID da equipa de origem (remetente).</param>
        /// <returns>Lista de [InfoTeamsDto] (equipas alvo).</returns>
        public async Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam)
        {
            TeamValidator.ValidateVariableSearchTeam(idTeam);
            var team = await TeamRepository.GetTeamByIdAsync(idTeam);

            TeamValidator.ValidateTeamSearch(team);

            return await TeamRepository.GetListTeamsForTeams(idTeam);
        }

        /// <summary>
        /// Obtém uma lista de equipas elegíveis para Match Invite, aplicando filtros de pesquisa complexos.
        /// </summary>
        /// <param name="idTeam">ID da equipa de origem (remetente).</param>
        /// <param name="filters">Filtros a aplicar.</param>
        /// <returns>Lista filtrada de [InfoTeamsDto].</returns>
        public async Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters)
        {
            TeamValidator.ValidateVaribleSearchTeamWithFilters(idTeam, filters);
            var team = await TeamRepository.GetTeamByIdAsync(idTeam);

            TeamValidator.ValidateTeamSearch(team);

            return await TeamRepository.GetListTeamsByTeamsWithFilters(idTeam, filters);
        }

        /// <summary>
        /// Obtém uma lista de Agentes Livres (Jogadores sem equipa) aplicando filtros de pesquisa.
        /// </summary>
        /// <param name="filter">Filtros de jogador (Altura, Posição, Nome, Cidade).</param>
        /// <returns>Lista de [PlayerWithoutTeamInfoDto] filtrada.</returns>
        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterTeamDto filter)
        {
            TeamValidator.ValidateFiltersGetPlayersWithout(filter);

            return await TeamRepository.GetListPlayersWithoutTeamtWithFilters(filter);
        }

        /// <summary>
        /// Obtém a lista completa de Jogadores Agentes Livres (sem filtros).
        /// </summary>
        /// <returns>Lista completa de [PlayerWithoutTeamInfoDto].</returns>
        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam()
        {
            return await TeamRepository.GetListPlayersWithoutTeam();
        }

        #endregion

        /// <summary>
        /// Obtém os dados básicos (Nome, ID) de uma equipa.
        /// </summary>
        /// <param name="teamId">ID da equipa alvo.</param>
        /// <returns>DTO [TeamDto] com dados essenciais.</returns>
        /// <exception cref="ArgumentException">Se a equipa não for encontrada.</exception>
        public async Task<TeamDto> getOpponent(Guid teamId)
        {
            var team = await TeamRepository.GetOpponentTeamById(teamId);
            if(team == null)
            {
                throw new ArgumentException("A equipa não foi encontrada");
            }

            return team;
        }
    }
}
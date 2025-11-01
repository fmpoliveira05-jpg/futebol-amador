using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IUserRepository UserRepository;
        private readonly IUnityOfWork UnitOfWork;
        private readonly IRankRepository RankRepository;
        private readonly ITeamValidator TeamValidator;
        private readonly IMembershipRequestRepository MembershipRequestRepository;
        private readonly IPlayerValidator PlayerValidator;

        public TeamService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IUserRepository userRepository,
            IUnityOfWork unitOfWork,
            ITeamValidator teamValidator,
            IRankRepository rankRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IPlayerValidator playerValidator)
        {
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            UserRepository = userRepository;
            UnitOfWork = unitOfWork;
            TeamValidator = teamValidator;
            RankRepository = rankRepository;
            MembershipRequestRepository = membershipRequestRepository;
            PlayerValidator = playerValidator;
        }

        public async Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            var existingTeam = await TeamRepository.GetTeamForMembershipRequestAsync(teamId);
            var playerAccepting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);

            if (existingTeam.MembershipRequests == null)
                existingTeam.MembershipRequests = new List<MembershipRequests>();

            var requestToRemove = existingTeam?.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
            if (requestToRemove == null)
                throw new ValidationException($"A equipa com Id '{teamId}' não possui um pedido de adesão com Id '{requestId}'.");

            TeamValidator.ApproveMembershipRequestValidation(existingTeam, playerAccepting, requestId);

            var playerAccepted = await PlayerRepository.GetPlayerByIdAsync(requestToRemove.IdPlayer);

            existingTeam.MembershipRequests.Remove(requestToRemove);
            if (playerAccepted?.MembershipRequests != null)
            {
                var playerRequest = playerAccepted.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
                if (playerRequest != null)
                    playerAccepted.MembershipRequests.Remove(playerRequest);
            }

            playerAccepted.IdTeam = teamId;
            existingTeam.Members.Add(playerAccepted);

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid playerId)
        {
            var existingTeam = await TeamRepository.GetTeamByNameAsync(teamDto.Name);
            var rank = await RankRepository.GetDefaultRankAsync();
            var playerCreating = await PlayerRepository.GetPlayerByIdAsync(playerId);

            TeamValidator.CreateTeamValidation(teamDto, existingTeam, playerCreating);

            if (rank == null)
                throw new ValidationException("Não foi possível atribuir a classificação padrão à equipa.");

            var newTeam = new Teams(
                teamDto.Name,
                teamDto.Description,
                teamDto.icon,
                new Pitch(teamDto.HomePitch.Name, teamDto.HomePitch.Address),
                rank
            );

            await TeamRepository.AddAsync(newTeam);

            playerCreating.IsAdmin = true;
            playerCreating.IdTeam = newTeam.Id;
            playerCreating.Team = newTeam;
            newTeam.Members ??= new List<Player>();
            newTeam.Members.Add(playerCreating);

            PlayerRepository.UpdatePlayer(playerCreating);

            await UnitOfWork.SaveChangesAsync();
            return newTeam.Id;
        }

        public async Task DeleteTeamAsync(Guid teamId, Guid currentUserId)
        {
            var teamToDelete = await TeamRepository.GetTeamForDeletionAsync(teamId);
            var playerTryingToDelete = await PlayerRepository.GetPlayerByIdAsync(currentUserId);

            TeamValidator.DeleteTeamValidation(teamToDelete, playerTryingToDelete);
            foreach (var member in teamToDelete.Members)
            {
                member.IdTeam = null;
                if (member.IsAdmin)
                {
                    member.IsAdmin = false;
                    member.IsAdminLastChangedAt = DateTime.UtcNow;
                }
            }
            TeamRepository.DeleteTeam(teamToDelete);

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, Guid currentUserId)
        {
            var teamToUpdate = await TeamRepository.GetTeamForUpdateAsync(teamId);
            var playerTryingToUpdate = await PlayerRepository.GetPlayerByIdAsync(currentUserId);

            Teams teamWithSameName = null;

            if (dto.Name != null)
            {
                teamWithSameName = await TeamRepository.GetTeamByNameAsync(dto.Name);
            }

            TeamValidator.UpdateTeamValidation(teamWithSameName, teamToUpdate, playerTryingToUpdate);

            teamToUpdate.Name = dto.Name ?? teamToUpdate.Name;
            teamToUpdate.Description = dto.Description ?? teamToUpdate.Description;
            teamToUpdate.Icon = dto.icon ?? teamToUpdate.Icon;
            teamToUpdate.Pitch.Name = dto.PitchName ?? teamToUpdate.Pitch.Name;
            teamToUpdate.Pitch.Address = dto.PitchLocation ?? teamToUpdate.Pitch.Address;

            TeamRepository.UpdateTeam(teamToUpdate);

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid adminDemotingId)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToDemote = await PlayerRepository.GetPlayerByIdAsync(adminIdToDemote);
            var playerDemoting = await PlayerRepository.GetPlayerByIdAsync(adminDemotingId);

            TeamValidator.DemoteAdminToMemberValidation(existingTeam, playerToDemote, playerDemoting);

            playerToDemote.IsAdmin = false;
            playerToDemote.IsAdminLastChangedAt = DateTime.UtcNow;
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamDetailsDtoAsync(teamId);

            TeamValidator.GetTeamByIdValidation(team);

            return team;
        }

        public async Task<List<PlayerDetailsDto>> GetTeamPlayersAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            var playerDtos = team.Members.Select(player => new PlayerDetailsDto
            {
                Name = player.Name,
                Height = player.Height,
                IdTeam = player.IdTeam,
                Position = player.Position,
                IsAdmin = player.IsAdmin
            }).ToList();
            return playerDtos;
        }

        public async Task<List<PlayerDetailsDto>> GetTeamPlayersAsyncWithFilters(Guid teamId, FilterTeamPlayers filters)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            return await TeamRepository.GetTeamPlayersDtoAsyncWithFilters(teamId, filters);
        }

        public async Task RemovePlayerFromTeamAsync(Guid teamId, Guid playerIdToRemove, Guid playerRemovingId)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToRemove = await PlayerRepository.GetPlayerByIdAsync(playerIdToRemove);
            var playerRemoving = await PlayerRepository.GetPlayerByIdAsync(playerRemovingId);

            TeamValidator.RemovePlayerFromTeamValidation(existingTeam, playerRemoving, playerToRemove);

            existingTeam.Members.Remove(playerToRemove);
            playerToRemove.IdTeam = null;
            if (playerToRemove.IsAdmin)
            {
                playerToRemove.IsAdmin = false;
                playerToRemove.IsAdminLastChangedAt = DateTime.UtcNow;
            }

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromoteId, Guid playerIdToPromotingId)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToPromote = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromoteId);
            var playerPromoting = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromotingId);

            TeamValidator.PromoteMemberToAdminValidation(existingTeam, playerToPromote, playerPromoting);

            playerToPromote.IsAdmin = true;
            playerToPromote.IsAdminLastChangedAt = DateTime.UtcNow;

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            var existingTeam = await TeamRepository.GetTeamForMembershipRequestAsync(teamId);
            var playerRejecting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);

            if (existingTeam.MembershipRequests == null)
                existingTeam.MembershipRequests = new List<MembershipRequests>();

            var requestToRemove = existingTeam?.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
            if (requestToRemove == null)
                throw new ValidationException($"A equipa com Id '{teamId}' não possui um pedido de adesão com Id '{requestId}'.");

            TeamValidator.RejectMembershipRequestValidation(existingTeam, playerRejecting, requestId);

            var playerRejected = await PlayerRepository.GetPlayerByIdAsync(requestToRemove.IdPlayer);

            existingTeam.MembershipRequests.Remove(requestToRemove);

            if (playerRejected?.MembershipRequests != null)
            {
                var playerRequest = playerRejected.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
                if (playerRequest != null)
                    playerRejected.MembershipRequests.Remove(playerRequest);
            }

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, Guid adminUserId)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var adminConsulting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);

            if (existingTeam.MembershipRequests == null)
                existingTeam.MembershipRequests = new List<MembershipRequests>();

            if (!existingTeam.MembershipRequests.Any())
            {
                var fakeRequest = (MembershipRequests)Activator.CreateInstance(typeof(MembershipRequests), nonPublic: true)!;
                existingTeam.MembershipRequests.Add(fakeRequest);
            }

            TeamValidator.GetMembershipRequestsValidation(existingTeam, adminConsulting);

            return await TeamRepository.GetMembershipRequestsDtoAsync(teamId);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid teamId, Guid adminUserId, FilterMembershipRequestsTeam filters)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var admin = await PlayerRepository.GetPlayerByIdAsync(adminUserId);

            if (existingTeam.MembershipRequests == null)
                existingTeam.MembershipRequests = new List<MembershipRequests>();

            if (!existingTeam.MembershipRequests.Any())
            {
                var fakeRequest = (MembershipRequests)Activator.CreateInstance(typeof(MembershipRequests), nonPublic: true)!;
                existingTeam.MembershipRequests.Add(fakeRequest);
            }

            TeamValidator.GetMembershipRequestsValidation(existingTeam, admin);

            return await TeamRepository.GetMembershipRequestsDtoAsyncWithFilters(teamId, filters);
        }

        public Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<InfoTeamsDto>> SearchTeamsAsync(Guid idTeam)
        {
            TeamValidator.ValidateVariableSearchTeam(idTeam);
            var team = await TeamRepository.GetTeamByIdAsync(idTeam);

            TeamValidator.ValidateTeamSearch(team);


            return await TeamRepository.GetListTeamsForTeams(idTeam);
        }

        public async Task<List<InfoTeamsDto>> SearchTeamsWithFiltersAsync(Guid idTeam, FilterListTeamDto filters)
        {
            TeamValidator.ValidateVaribleSearchTeamWithFilters(idTeam, filters);
            var team = await TeamRepository.GetTeamByIdAsync(idTeam);

            //Quando houver players descomentar
            TeamValidator.ValidateTeamSearch(team);

            return await TeamRepository.GetListTeamsByTeamsWithFilters(idTeam, filters);
        }

        public async Task SendMembershipRequestAsync(Guid teamId, Guid playerIdToInvite, Guid adminUserId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var admin = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            var playerToInvite = await PlayerRepository.GetPlayerByIdAsync(playerIdToInvite);

            TeamValidator.SendMembershipRequestValidation(team, admin, playerToInvite);
            PlayerValidator.PlayerExists(playerToInvite);

            if (playerToInvite.IdTeam == teamId)
                throw new ValidationException("O jogador já pertence a esta equipa.");

            var existing = await MembershipRequestRepository
                .GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);
            if (existing != null)
                throw new ValidationException("Já existe um pedido/convite pendente entre a equipa e este jogador.");

            var invite = new MembershipRequests
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerIdToInvite,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };

            await MembershipRequestRepository.AddMembershipRequest(invite);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<List<TeamLeaderboardDto>> GetLeaderboardAsync()
        {
            var teams = await TeamRepository.GetTopTeamsAsync(100);
            return teams
                .Select((t, index) => new TeamLeaderboardDto
                {
                    Position = index + 1,
                    TeamName = t.TeamName,
                    CurrentPoints = t.CurrentPoints,
                    RankName = t.RankName
                })
                .ToList();
        }

    }
}
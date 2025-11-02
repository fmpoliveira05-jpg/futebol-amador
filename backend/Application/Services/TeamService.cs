using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
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
        private readonly IUnityOfWork UnityOfWork;
        private readonly IRankRepository RankRepository;
        private readonly ITeamValidator TeamValidator;
        private readonly IMembershipRequestRepository MembershipRequestRepository;
        private readonly IPlayerValidator PlayerValidator;

        public TeamService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IRankRepository rankRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IPlayerValidator playerValidator)
        {
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            UnityOfWork = unityOfWork;
            TeamValidator = teamValidator;
            RankRepository = rankRepository;
            MembershipRequestRepository = membershipRequestRepository;
            PlayerValidator = playerValidator;
        }

        #region CRUD Team
        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid playerId)
        {
            var existingTeam = await TeamRepository.GetTeamByNameAsync(teamDto.Name);
            var rank = await RankRepository.GetDefaultRankAsync();
            var playerCreating = await PlayerRepository.GetPlayerByIdAsync(playerId);

            TeamValidator.CreateTeamValidation(teamDto, rank, existingTeam, playerCreating);

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

            await UnityOfWork.SaveChangesAsync();
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

            await UnityOfWork.SaveChangesAsync();
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

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamDetailsDtoAsync(teamId);

            TeamValidator.GetTeamByIdValidation(team);

            return team;
        }

        #endregion

        #region Admins Manager
        public async Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromoteId, Guid playerIdToPromotingId)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToPromote = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromoteId);
            var playerPromoting = await PlayerRepository.GetPlayerByIdAsync(playerIdToPromotingId);

            TeamValidator.PromoteMemberToAdminValidation(existingTeam, playerToPromote, playerPromoting);

            playerToPromote.IsAdmin = true;
            playerToPromote.IsAdminLastChangedAt = DateTime.UtcNow;

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid adminDemotingId)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToDemote = await PlayerRepository.GetPlayerByIdAsync(adminIdToDemote);
            var playerDemoting = await PlayerRepository.GetPlayerByIdAsync(adminDemotingId);

            TeamValidator.DemoteAdminToMemberValidation(existingTeam, playerToDemote, playerDemoting);

            playerToDemote.IsAdmin = false;
            playerToDemote.IsAdminLastChangedAt = DateTime.UtcNow;
            await UnityOfWork.SaveChangesAsync();
        }

        #endregion

        #region MemberShipRequest
        public async Task<MemberShipRequestDto> SendMembershipRequestAsync(Guid teamId, Guid playerIdToInvite, Guid adminUserId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var admin = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            var playerToInvite = await PlayerRepository.GetPlayerByIdAsync(playerIdToInvite);
            var existing = await MembershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);

            TeamValidator.SendMembershipRequestValidation(existing, team, admin, playerToInvite);
            PlayerValidator.PlayerExists(playerToInvite);

            var invite = new MembershipRequests
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerIdToInvite,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };

            await MembershipRequestRepository.AddMembershipRequest(invite);
            await UnityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = invite.Id,
                PlayerId = invite.IdPlayer,
                PlayerName = invite.Player.Name,
                TeamId = invite.IdTeam,
                TeamName = invite.Team.Name,
                RequestDate = invite.InviteDate,
                IsPlayerSender = invite.IsPlayerSender
            };
        }

        public async Task<MemberShipRequestDto> AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            var existingTeam = await TeamRepository.GetTeamForMembershipRequestAsync(teamId)
                ?? throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

            var playerAccepting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            TeamValidator.ApproveMembershipRequestValidation(existingTeam, playerAccepting, requestId);

            var requestToRemove = existingTeam.MembershipRequests.FirstOrDefault(r => r.Id == requestId)
                ?? throw new ValidationException($"A equipa com Id '{teamId}' não possui um pedido de adesão com Id '{requestId}'.");

            var playerAccepted = await PlayerRepository.GetPlayerByIdAsync(requestToRemove.IdPlayer)
                ?? throw new ValidationException("O jogador associado ao pedido não foi encontrado.");

            var teamEntity = existingTeam;

            existingTeam.MembershipRequests.Remove(requestToRemove);

            playerAccepted.MembershipRequests?.Remove(requestToRemove);

            playerAccepted.IdTeam = teamId;
            existingTeam.Members.Add(playerAccepted);

            await UnityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = requestToRemove.Id,
                PlayerId = playerAccepted.Id,
                PlayerName = playerAccepted.Name,
                TeamId = teamEntity.Id,
                TeamName = teamEntity.Name,
                RequestDate = requestToRemove.InviteDate,
                IsPlayerSender = requestToRemove.IsPlayerSender
            };
        }
        public async Task<MemberShipRequestDto> RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            var existingTeam = await TeamRepository.GetTeamForMembershipRequestAsync(teamId)
                ?? throw new ValidationException($"A equipa com Id '{teamId}' não existe.");

            var playerRejecting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);
            TeamValidator.RejectMembershipRequestValidation(existingTeam, playerRejecting, requestId);

            var requestToRemove = existingTeam.MembershipRequests.FirstOrDefault(r => r.Id == requestId)
                ?? throw new ValidationException($"A equipa com Id '{teamId}' não possui um pedido de adesão com Id '{requestId}'.");

            var playerRejected = await PlayerRepository.GetPlayerByIdAsync(requestToRemove.IdPlayer)
                ?? throw new ValidationException("O jogador associado ao pedido não foi encontrado.");

            existingTeam.MembershipRequests.Remove(requestToRemove);

            playerRejected.MembershipRequests?.Remove(requestToRemove);

            await UnityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = requestToRemove.Id,
                PlayerId = playerRejected.Id,
                PlayerName = playerRejected.Name,
                TeamId = existingTeam.Id,
                TeamName = existingTeam.Name,
                RequestDate = requestToRemove.InviteDate,
                IsPlayerSender = requestToRemove.IsPlayerSender
            };
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, Guid adminUserId)
        {
            var existingTeam = await TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var adminConsulting = await PlayerRepository.GetPlayerByIdAsync(adminUserId);

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

        #endregion

        #region Members Team
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

            await UnityOfWork.SaveChangesAsync();
        }

        #endregion

        #region List Team To MatchInvite
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
        #endregion

        #region List Player To MemberShipRequest
        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeam()
        {
            var playersWithoutTeam = await PlayerRepository.GetPlayersWithoutTeamAsync();

            if (playersWithoutTeam == null || !playersWithoutTeam.Any())
                throw new NotFoundException("Não existem jogadores sem equipa no momento.");

            return playersWithoutTeam;
        }

        public async Task<List<PlayerWithoutTeamInfoDto>> GetPlayersWithoutTeamWithFilters(FilterPlayersWithoutTeamDto filter)
        {
            //Falta validar a autorização se for aqui
            TeamValidator.ValidateFiltersGetPlayersWithout(filter);

            return await TeamRepository.GetListPlayersWithoutTeamtWithFilters(filter);
        }

        #endregion
        public Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }

    }
}
using Application.DTOs.Match;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Validators;
using Domain.Entities;

using Domain.Exceptions;

namespace Application.Services
//criar/editar/remover equipas; gerir admins e membros.
{
    //verificar necessidade de ser public (rever protection level)
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IPlayerRepository UserRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly IRankRepository RankRepository;
        private readonly ITeamValidator TeamValidator;

        public TeamService(ITeamRepository teamRepository, IPlayerRepository playerRepository, IPlayerRepository userRepository, IUnityOfWork unitOfWork, ITeamValidator teamValidator, IRankRepository rankRepository)
        {
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            UserRepository = userRepository;
            UnityOfWork = unitOfWork;
            TeamValidator = teamValidator;
            RankRepository = rankRepository;

        }
        public async Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMembershipRequestAsync(teamId);
            var playerAcceptingTask = PlayerRepository.GetPlayerByIdAsync(adminUserId);

            await Task.WhenAll(existingTeamTask, playerAcceptingTask);

            var existingTeam = await existingTeamTask;
            var playerAccepting = await playerAcceptingTask;

            if (existingTeam.MembershipRequests == null) { 
                existingTeam.MembershipRequests = new List<MembershipRequests>();
            }

            var requestToRemove = existingTeam?.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
            if (requestToRemove == null)
            {
                throw new ValidationException($"A equipa com Id '{teamId}' não possui um pedido de adesão com Id '{requestId}'.");
            }

            TeamValidator.ApproveMembershipRequestValidation(existingTeam, playerAccepting, requestId);

            var playerAccepted = await PlayerRepository.GetPlayerByIdAsync(requestToRemove.IdPlayer);

            existingTeam.MembershipRequests.Remove(requestToRemove);
            //Apaga os pedidos de adesão que o jogador enviou e deixa os que o mesmo recebeu
            if (playerAccepted?.MembershipRequests != null)
            {
                foreach (MembershipRequests request in playerAccepted.MembershipRequests)
                {
                    if (request.IsPlayerSender)
                    {
                        playerAccepted.MembershipRequests.Remove(request);
                        break;
                    }
                }
            }

            playerAccepted.IdTeam = teamId;
            existingTeam.Members.Add(playerAccepted);

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, string playerId)
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

            playerCreating.IdTeam = newTeam.Id;
            playerCreating.IsAdmin = true;
            playerCreating.IsAdminLastChangedAt = DateTime.UtcNow;

            await TeamRepository.AddAsync(newTeam);
            await UnityOfWork.SaveChangesAsync();
            return newTeam.Id;
        }

        public async Task DeleteTeamAsync(Guid teamId, string currentUserId)
        {
            var teamToDeleteTask = TeamRepository.GetTeamForDeletionAsync(teamId);
            var playerTryingToDeleteTask = PlayerRepository.GetPlayerByIdAsync(currentUserId);
            await Task.WhenAll(teamToDeleteTask, playerTryingToDeleteTask);

            var teamToDelete = await teamToDeleteTask;
            var playerTryingToDelete = await playerTryingToDeleteTask;

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

        public async Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, string currentUserId)
        {
            var teamToUpdateTask = TeamRepository.GetTeamForUpdateAsync(teamId);
            var playerTryingToUpdateTask = PlayerRepository.GetPlayerByIdAsync(currentUserId);

            Task<Teams> teamWithSameNameTask = null; 
            Teams teamWithSameName = null;

            var tasksToAwait = new List<Task> { teamToUpdateTask, playerTryingToUpdateTask };

            if (dto.Name != null)
            {
                teamWithSameNameTask = TeamRepository.GetTeamByNameAsync(dto.Name);
                tasksToAwait.Add(teamWithSameNameTask);
            }

            await Task.WhenAll(tasksToAwait);

            var teamToUpdate = await teamToUpdateTask;
            var playerTryingToUpdate = await playerTryingToUpdateTask;

            if (teamWithSameNameTask != null)
            {
                teamWithSameName = await teamWithSameNameTask;
            }

            TeamValidator.UpdateTeamValidation(teamWithSameName, teamToUpdate, playerTryingToUpdate);

            teamToUpdate.Name = dto.Name ?? teamToUpdate.Name;
            teamToUpdate.Description = dto.Description ?? teamToUpdate.Description;
            teamToUpdate.Icon = dto.icon ?? teamToUpdate.Icon;
            teamToUpdate.Pitch.Name = dto.PitchName ?? teamToUpdate.Pitch.Name;
            teamToUpdate.Pitch.Address = dto.PitchLocation?? teamToUpdate.Pitch.Address;


            TeamRepository.UpdateTeam(teamToUpdate);

            await UnityOfWork.SaveChangesAsync();

        }

        public async Task DemoteAdminToPlayerAsync(Guid teamId, string adminIdToDemote, string adminDemotingId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToDemoteTask = PlayerRepository.GetPlayerByIdAsync(adminIdToDemote);
            var playerDemotingTask = PlayerRepository.GetPlayerByIdAsync(adminDemotingId);
            await Task.WhenAll(existingTeamTask, playerToDemoteTask, playerDemotingTask);

            var existingTeam = await existingTeamTask;
            var playerDemoted = await playerToDemoteTask;
            var playerDemoting = await playerDemotingTask;

            TeamValidator.DemoteAdminToMemberValidation(existingTeam, playerDemoted, playerDemoting);

            playerDemoted.IsAdmin = false;
            playerDemoted.IsAdminLastChangedAt = DateTime.UtcNow;
            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamDetailsDtoAsync(teamId);

            TeamValidator.GetTeamByIdValidation(team);

            return team;
        }

        public async Task<List<PlayerDetailsDTO>> GetTeamPlayersAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            var playerDtos = team.Members.Select(player => new PlayerDetailsDTO
            {
                Name = player.Name,
                Height = player.Height,
                IdTeam = player.IdTeam,
                Position = player.Position,
                IsAdmin = player.IsAdmin
            }).ToList();
            return playerDtos;

        }

        public async Task RemovePlayerFromTeamAsync(Guid teamId, string playerIdToRemove, string playerRemovingId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToRemoveTask = PlayerRepository.GetPlayerByIdAsync(playerIdToRemove);
            var playerRemovingTask = PlayerRepository.GetPlayerByIdAsync(playerRemovingId);
            await Task.WhenAll(existingTeamTask, playerToRemoveTask, playerRemovingTask);

            var existingTeam = await existingTeamTask;
            var playerToRemove = await playerToRemoveTask;
            var playerRemoving = await playerRemovingTask;

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

        public async Task PromotePlayerToAdminAsync(Guid teamId, string playerIdToPromoteId, string playerIdToPromotingId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToPromoteTask = PlayerRepository.GetPlayerByIdAsync(playerIdToPromoteId);
            var playerPromotingTask = PlayerRepository.GetPlayerByIdAsync(playerIdToPromotingId);
            await Task.WhenAll(existingTeamTask, playerToPromoteTask, playerPromotingTask);

            var existingTeam = await existingTeamTask;
            var playerToPromote = await playerToPromoteTask;
            var playerPromoting = await playerPromotingTask;

            TeamValidator.PromoteMemberToAdminValidation(existingTeam, playerToPromote, playerPromoting);

            playerToPromote.IsAdmin = true;
            playerToPromote.IsAdminLastChangedAt = DateTime.UtcNow;

            await UnityOfWork.SaveChangesAsync();
        }
        public async Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, string adminUserId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMembershipRequestAsync(teamId);
            var playerRejectingTask = PlayerRepository.GetPlayerByIdAsync(adminUserId);

            await Task.WhenAll(existingTeamTask, playerRejectingTask);

            var existingTeam = await existingTeamTask;
            var playerRejecting = await playerRejectingTask;

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

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, string adminUserId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var adminTask = PlayerRepository.GetPlayerByIdAsync(adminUserId);

            await Task.WhenAll(existingTeamTask, adminTask);

            var existingTeam = await existingTeamTask;
            var adminConsulting = await adminTask;

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



        public Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId)
        {
                        throw new NotImplementedException();
/*
            var existingTeamTask = await TeamRepository.GetTeamByIdAsync(teamId);

            TeamValidator.GetTeamScheduleValidation(existingTeamTask);
            var matchDtos = existingTeamTask.Calendar.Matches.Select(match => new MatchDto
            {
                MatchId = match.Id,
                AwayTeamName = match.Teams.Name, 
                HomeTeamName = match.HomeTeam.Name,
                MatchDate = match.MatchDate,
                PitchName = match.Pitch.Name,
                Location = match.Pitch.Address,
            }).ToList();
*/
        }


        public async Task<List<TeamSummaryDto>> SearchTeamsAsync(string playerId, TeamSearchFiltersDto filters)
        {
            var player = await PlayerRepository.GetPlayerByIdAsync(playerId);
            
            var query = TeamRepository.GetTeamsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(t => t.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.RankName))
            {
                query = query.Where(t => t.Rank.Name.Contains(filters.Name));
            }

            if (filters.MinAvgAge > 0)
            {
                query = query.Where(t => t.AverageAge > filters.MinAvgAge);
            }

            if (filters.MaxAvgAge < filters.MinAvgAge)
            {
                throw new Exception("Max average age must be higher or equal to Min Average Age.");
            }

            if (filters.MaxAvgAge > 0)
            {
                query = query.Where(t => t.AverageAge < filters.MaxAvgAge);
            }

            if (!string.IsNullOrWhiteSpace(filters.PitchAddress))
            {
                query = query.Where(t => t.Pitch.Address.Contains(filters.PitchAddress));
            }

            var teams = query
                .Select(t => new TeamSummaryDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    RankName = t.Rank.Name,
                    PlayerCount = t.Members.Count,
                })
                .ToList();

            if (player.IsAdmin)
            {
                teams.Where(t =>
                    t.PlayerCount > 11
                    );
            }else if (player.IdTeam == null)
                teams.Where(t =>
                    t.PlayerCount > 11
                    );

            return teams;
        }
    }
}

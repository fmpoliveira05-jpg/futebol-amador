using Application.DTOs.Match;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositorys;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Validators;
using Domain.Entities;
//using static Domain.Constants.ModelConstants;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
//criar/editar/remover equipas; gerir admins e membros.
{
    //verificar necessidade de ser public (rever protection level)
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IUserRepository UserRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly ITeamValidator TeamValidator;

        public TeamService(ITeamRepository teamRepository, IPlayerRepository playerRepository, IUserRepository userRepository, IUnityOfWork unitOfWork)
        {
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            UserRepository = userRepository;
            UnityOfWork = unitOfWork;
            TeamValidator = new TeamValidator();
        }

        public async Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMembershipRequestAsync(teamId);
            var playerAcceptingTask = PlayerRepository.GetPlayerByIdAsync(adminUserId);

            Task<Player> playerAcceptedTask = null;

            await Task.WhenAll(existingTeamTask, playerAcceptingTask);

            var existingTeam = await existingTeamTask;
            var playerRejecting = await playerAcceptingTask;

            Guid playerAcceptedId = Guid.Empty;
            var requestToRemove = existingTeam?.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
            if (requestToRemove != null)
            {
                playerAcceptedId = requestToRemove.IdPlayer;
            }

            playerAcceptedTask = PlayerRepository.GetPlayerByIdAsync(playerAcceptedId);

            TeamValidator.ApproveMembershipRequestValidation(existingTeam, playerRejecting, requestId);

            var playerAccepted = await playerAcceptedTask;

            if (requestToRemove != null)
            {
                existingTeam.MembershipRequests.Remove(requestToRemove);
            }


            if (playerAccepted != null)
            {
                var playerRequest = playerAccepted.MembershipRequests?.FirstOrDefault(r => r.Id == requestId);
                if (playerRequest != null)
                {
                    playerAccepted.MembershipRequests.Remove(playerRequest);
                    playerAccepted.idTeam = teamId;
                    existingTeam.Members.Add(playerAccepted);
                }
            }

            await UnityOfWork.SaveChangesAsync();

            if (playerAccepted == null)
            {
                throw new NotFoundException("O jogador que fez o pedido de adesão não foi encontrado.");
            }
        }

        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid creatorPlayerId)
        {

            var existingTeamTask = TeamRepository.GetTeamByNameAsync(teamDto.Name);
            var creatorPlayerTask = PlayerRepository.GetPlayerByIdAsync(creatorPlayerId);
            await Task.WhenAll(existingTeamTask, creatorPlayerTask);

            var existingTeam = await existingTeamTask;
            var creatorPlayer = await creatorPlayerTask;

            TeamValidator.CreateTeamValidation(teamDto, existingTeam, creatorPlayer);

            var pitch = new Pitch(
                teamDto.HomePitch.Name,
                teamDto.HomePitch.Address
            );

            var newTeam = new Teams(
                teamDto.Name,
                teamDto.Description,
                teamDto.icon,
                pitch
            );

            await TeamRepository.AddAsync(newTeam);


            await UnityOfWork.SaveChangesAsync();

            return newTeam.Id;
        }

        public async Task DeleteTeamAsync(Guid teamId, Guid currentUserId)
        {
            var teamToDeleteTask = TeamRepository.GetTeamForDeletionAsync(teamId);
            var playerTryingToDeleteTask = PlayerRepository.GetPlayerByIdAsync(currentUserId);
            await Task.WhenAll(teamToDeleteTask, playerTryingToDeleteTask);

            var teamToDelete = await teamToDeleteTask;
            var playerTryingToDelete = await playerTryingToDeleteTask;

            TeamValidator.DeleteTeamValidation(teamToDelete, playerTryingToDelete);

            TeamRepository.DeleteTeam(teamToDelete);

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, Guid currentUserId)
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

        public async Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid adminDemotingId)
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

        public async Task<List<PlayerDto>> GetTeamPlayersAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamForMemberManagementAsync(teamId);

            TeamValidator.GetTeamMembersValidation(team);

            var playerDtos = team.Members.Select(player => new PlayerDto
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                Height = player.Height,
                idTeam = player.idTeam,
                Position = player.Position,
                IsAdmin = player.IsAdmin
            }).ToList();
            return playerDtos;

        }

        public async Task RemovePlayerFromTeamAsync(Guid teamId, Guid playerIdToRemove, Guid playerRemovingId)
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
            playerToRemove.idTeam = null;
            if (playerToRemove.IsAdmin)
            {
                playerToRemove.IsAdmin = false;
                playerToRemove.IsAdminLastChangedAt = DateTime.UtcNow;
            }

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromoteId, Guid playerIdToPromotingId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToPromoteTask = PlayerRepository.GetPlayerByIdAsync(playerIdToPromoteId);
            var playerPromotingTask = PlayerRepository.GetPlayerByIdAsync(playerIdToPromotingId);
            await Task.WhenAll(existingTeamTask, playerToPromoteTask, playerPromotingTask);

            var existingTeam = await existingTeamTask;
            var playerToPromote = await playerToPromoteTask;
            var playerPromoting = await playerPromotingTask;

            TeamValidator.PromoteMemberToAdminValidation(existingTeam, playerToPromote, playerPromoting);

            existingTeam.Members.Remove(playerToPromote);
            playerToPromote.IsAdmin = true;
            playerToPromote.IsAdminLastChangedAt = DateTime.UtcNow;

            await UnityOfWork.SaveChangesAsync();
        }
        public async Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
        var existingTeamTask = TeamRepository.GetTeamForMembershipRequestAsync(teamId); 
        var playerRejectingTask = PlayerRepository.GetPlayerByIdAsync(adminUserId);
    
        Task<Player> playerRejectedTask = null; 

        await Task.WhenAll(existingTeamTask, playerRejectingTask);

        var existingTeam = await existingTeamTask;
        var playerRejecting = await playerRejectingTask;

        Guid playerRejectedId = Guid.Empty;
        var requestToRemove = existingTeam?.MembershipRequests
                                            .FirstOrDefault(r => r.Id == requestId);
        if (requestToRemove != null)
        {
            playerRejectedId = requestToRemove.IdPlayer;
        }

        playerRejectedTask = PlayerRepository.GetPlayerByIdAsync(playerRejectedId);

        TeamValidator.RejectMembershipRequestValidation(existingTeam, playerRejecting, requestId);

        var playerRejected = await playerRejectedTask;

        if (requestToRemove != null)
        {
            existingTeam.MembershipRequests.Remove(requestToRemove);
        }


        if (playerRejected != null)
        {
            var playerRequest = playerRejected.MembershipRequests?.FirstOrDefault(r => r.Id == requestId);
            if (playerRequest != null)
            {
                playerRejected.MembershipRequests.Remove(playerRequest);
            }
        }

        await UnityOfWork.SaveChangesAsync();
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, Guid adminUserId)
        {
            var existingTeamTask = TeamRepository.GetTeamForMemberManagementAsync(teamId);
            var adminTask = PlayerRepository.GetPlayerByIdAsync(teamId);

            await Task.WhenAll(existingTeamTask, adminTask);

            var existingTeam = await existingTeamTask;
            var adminConsulting = await adminTask;

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


        public Task<List<TeamSummaryDto>> SearchTeamsAsync(TeamSearchFiltersDto filters)
        {
            throw new NotImplementedException();
        }

    }
}

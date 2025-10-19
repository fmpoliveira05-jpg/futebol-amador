using Application.DTOs.Match;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
//using static Domain.Constants.ModelConstants;
using Domain.Exceptions;

namespace Application.Services
//criar/editar/remover equipas; gerir admins e membros.
{
    //verificar necessidade de ser public (rever protection level)
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository TeamRepository;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IUserRepository UserRepository;
        private readonly IUnitOfWork UnitOfWork;

        public TeamService(ITeamRepository teamRepository,IPlayerRepository playerRepository,IUserRepository userRepository,IUnitOfWork unitOfWork)
        {
            TeamRepository = teamRepository;
            PlayerRepository = playerRepository;
            UserRepository = userRepository;
            UnitOfWork = unitOfWork;
        }
        
        public Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid creatorPlayerId)
        {

            var existingTeam = TeamRepository.GetTeamByNameAsync(teamDto.Name);
            var creatorPlayer =  UserRepository.GetUserByIdAsync(creatorPlayerId);
            await Task.WhenAll(existingTeam, creatorPlayer);

            if (existingTeam != null)
            {
                throw new ValidationException($"Uma equipa com o nome '{teamDto.Name}' já existe.");
            }

            if (creatorPlayer == null)
            {
                throw new NotFoundException($"Utilizador com ID {creatorPlayerId} não encontrado.");
            }

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


            await UnitOfWork.SaveChangesAsync();

            return newTeam.Id;
        }

        public async Task DeleteTeamAsync(Guid teamId, Guid currentUserId)
        {
            var existingTeamTask = TeamRepository.GetTeamByIdAsync(teamId);
            var playerTryingToDeleteTask = PlayerRepository.GetPlayerByIdAsync(currentUserId);
            await Task.WhenAll(existingTeamTask, playerTryingToDeleteTask);

            var existingTeam = await existingTeamTask;
            var playerTryingToDelete = await playerTryingToDeleteTask;

            if (playerTryingToDelete == null)
            {
                throw new NotFoundException($"Utilizador com ID {currentUserId} não encontrado.");
            }
            //adicionar verificação se o player pertence à equipa
            if (!playerTryingToDelete.IsAdmin)
            {
                throw new ValidationException($"O player de id '{playerTryingToDelete.Id}' não é admnistrador da equipa.");
            }
            if (existingTeam == null)
            {
                throw new ValidationException($"O time não com o Id '{teamId}' não existe.");
            }


        }

        public Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid currentAdminId)
        {
            throw new NotImplementedException();
        }

        public async Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamByIdAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException($"Equipe com ID {teamId} não encontrada.");
            }
          
            var teamDetailsDto = new TeamDetailsDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                FoundationDate = team.DataFoundation,
                TotalPoints = team.CurrentPoints,
                RankName = team.Rank?.Name,
                PitchDto = $"{team.Pitch.Name}, {team.Pitch.Address}",
                Players = team.Members.Select(player => new PlayerDto
                {
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    Height = player.Height,
                    idTeam = player.idTeam,
                    Position = player.Position,
                    IsAdmin = player.IsAdmin
                }).ToList()
            };
            return teamDetailsDto;
        }

        public async Task<List<PlayerDto>> GetTeamPlayersAsync(Guid teamId)
        {
            var team = await TeamRepository.GetTeamByIdAsync(teamId);
            if (team == null)
            {
                throw new NotFoundException($"Equipe com ID {teamId} não encontrada.");
            }
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

        public Task KickPlayerFromTeamAsync(Guid teamId, Guid playerIdToKick, Guid adminUserId)
        {
            throw new NotImplementedException();
        }

        public Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromote, Guid currentAdminId)
        {
            throw new NotImplementedException();
        }

        public Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            throw new NotImplementedException();
        }

        Task<List<MemberShipRequestDto>> ITeamService.GetMembershipRequestsAsync(Guid teamId, Guid adminUserId)
        {
            throw new NotImplementedException();
        }

        Task<List<MatchDto>> ITeamService.GetTeamScheduleAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }

        Task<List<TeamSummaryDto>> ITeamService.SearchTeamsAsync(TeamSearchFiltersDto filters)
        {
            throw new NotImplementedException();
        }

        Task ITeamService.UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, Guid currentUserId)
        {
            throw new NotImplementedException();
        }
    }
}

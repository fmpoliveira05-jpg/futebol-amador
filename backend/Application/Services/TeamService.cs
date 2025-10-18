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
using static Domain.Constants.ModelConstants;
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

        public Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid creatorUserId)
        {

            var existingTeam = await TeamRepository.GetTeamByNameAsync(teamDto.Name);
            if (existingTeam != null)
            {
                // Lança uma exceção que o seu middleware apanha e transforma num 400 Bad Request
                throw new ValidationException($"Uma equipa com o nome '{teamDto.Name}' já existe.");
            }

            var creatorUser = await UserRepository.GetUserByIdAsync(creatorUserId);
            if (creatorUser == null)
            {
                throw new NotFoundException($"Utilizador com ID {creatorUserId} não encontrado.");
            }

            var newTeam = new Team(
                teamDto.Name,
                teamDto.Description,
                teamDto.FoundationDate,
                teamDto.HomePitchId
            );

 
            var newPlayer = new Player(
                creatorUser,  
                newTeam,      
                "Capitão",    
                true          
            );


            await TeamRepository.AddAsync(newTeam);
            await PlayerRepository.AddAsync(newPlayer);


            await UnitOfWork.SaveChangesAsync();

            return newTeam.Id;
        }

        public Task DeleteTeamAsync(Guid teamId, Guid currentUserId)
        {
            throw new NotImplementedException();
        }

        public Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid currentAdminId)
        {
            throw new NotImplementedException();
        }

        public Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PlayerDto>> GetTeamPlayersAsync(Guid teamId)
        {
            throw new NotImplementedException();
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

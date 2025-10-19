using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositorys;
using Application.Interfaces.Services;
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
        private readonly IUserRepository UserRepository;
        private readonly IUnityOfWork UnitOfWork;

        public TeamService(ITeamRepository teamRepository, IPlayerRepository playerRepository, 
            IUserRepository userRepository, IUnityOfWork unitOfWork)
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

            var existingTeam = await TeamRepository.GetTeamByNameAsync(teamDto.Name);
            if (existingTeam != null)
            {
                throw new ValidationException($"Uma equipa com o nome '{teamDto.Name}' já existe.");
            }

            var creatorPlayer = await UserRepository.GetUserByIdAsync(creatorPlayerId);
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

        public Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }
    }
}

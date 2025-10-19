using Application.DTOs.Player;
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

        public async Task<Guid> CreateTeamAsync(CreateTeamDto teamDto)
        {

            var existingTeam = await TeamRepository.GetTeamByNameAsync(teamDto.Name);
            if (existingTeam != null)
            {
                throw new ValidationException($"Uma equipa com o nome '{teamDto.Name}' já existe.");
            }

            //var creatorPlayer = await UserRepository.GetUserByIdAsync(creatorPlayerId);
            //if (creatorPlayer == null)
            //{
              //  throw new NotFoundException($"Utilizador com ID {creatorPlayerId} não encontrado.");
            //}

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
    }
}

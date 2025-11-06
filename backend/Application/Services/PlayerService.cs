using Application.DTOs.Filters;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        #region Initializer
        private readonly IPlayerRepository playerRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IPlayerValidator playerValidator;
        private readonly IUserRepository userRepository;
        private readonly ITeamService teamService;
        private readonly IPlayerAuthorizationValidator authorizationValidator;

        public PlayerService(IPlayerRepository playerRepository, ITeamRepository teamRepository,
            IUnityOfWork unitOfWork, IMembershipRequestRepository membershipRequestRepository,
            IPlayerValidator playerValidator, IUserRepository userRepository,
            IPlayerAuthorizationValidator authorizationValidator, ITeamService teamService)
        {
            this.playerRepository = playerRepository;
            this.teamRepository = teamRepository;
            this.unityOfWork = unitOfWork;
            this.playerValidator = playerValidator;
            this.userRepository = userRepository;
            this.authorizationValidator = authorizationValidator;
            this.teamService = teamService;
        }

        #endregion

        #region CRUD Player
        public async Task<string> CreatePlayerAsync(string userId, string email, CreatePlayerDto playerDto)
        {
            var player = new Player
            {
                //Id = userId,
                Name = playerDto.Name,
                DateOfBirth = playerDto.DateOfBirth,
                Address = playerDto.Address,
                Email = email,
                Phone = playerDto.Phone,
                Position = playerDto.Position,
                Height = playerDto.Height,
                CreationDate = DateTime.UtcNow
            };

            await playerRepository.AddAsync(player);

            await unityOfWork.SaveChangesAsync();

            return player.Id;
        }

        public async Task DeletePlayerAsync(string playerId)
        {
            //playerValidator.GetPlayerByIdValidator(playerId);
            var playerToDelete = await playerRepository.GetPlayerByIdAsync(playerId);

            playerRepository.DeletePlayer(playerToDelete);

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<PlayerDetailsDto> GetPlayerByIdAsync(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.GetPlayerByIdValidator(player);

            PlayerDetailsDto playerDetails = new PlayerDetailsDto
            {
                Name = player.Name,
                DateOfBirth = player.DateOfBirth,
                Address = player.Address,
                Position = player.Position,
                Height = player.Height,
                IdTeam = player.IdTeam
            };

            return playerDetails;
        }

        public async Task UpdatePlayerAsync(string playerId, UpdatePlayerDto dto)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            var existingPlayers = new User[] {
                await userRepository.GetUserByEmailAsync(dto.Email),
                await userRepository.GetUserByPhoneAsync(dto.Phone),
            };

            playerValidator.UpdatePlayerValidator(dto, player, existingPlayers);

            bool hasChange = hasChangePlayer(dto, player);
            playerValidator.ValidateHasChangeDataPlayer(hasChange);

            await unityOfWork.SaveChangesAsync();
        }

        #endregion

        #region Actions Player in Team
        //Falta tirar o player da lista de players do team
        public async Task<string> LeaveTeam(string playerId)
        {
            var existingPlayer = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.LeaveTeamValidator(existingPlayer);

            if (existingPlayer.IsAdmin)
            {
                if (existingPlayer.Team.Members.Count == 1)
                {
                    await teamService.DeleteTeamAsync((Guid)existingPlayer.IdTeam, existingPlayer.Id);
                }
                else
                {
                    var otherAdmin = existingPlayer.Team.Members
                        .FirstOrDefault(p => p.IsAdmin && p.Id != existingPlayer.Id);

                    if (otherAdmin == null)
                    {
                        var otherMembers = existingPlayer.Team.Members
                            .Where(p => p.Id != existingPlayer.Id)
                            .ToList();
                        var oldestDate = otherMembers.Min(p => p.CreationDate);
                        Player newAdmin = otherMembers.First(p => p.CreationDate == oldestDate);
                        newAdmin.IsAdmin = true;
                    }

                    existingPlayer.IsAdmin = false;
                }
            }

            string teamName = existingPlayer.Team.Name;

            existingPlayer.Team.Members.Remove(existingPlayer);
            existingPlayer.Team = null;
            existingPlayer.IdTeam = null;

            playerRepository.UpdatePlayer(existingPlayer);

            await unityOfWork.SaveChangesAsync();

            return teamName;
        }
        #endregion

        #region Lists
        public async Task<List<InfoTeamsDto>> GetListTeams()
        {
            return await teamRepository.GetListTeamsPlayer();
        }

        public async Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter)
        {
            playerValidator.ValidateFiltersListTeams(filter);
            return await teamRepository.GetListTeamsPlayersWithFilters(filter);
        }
        #endregion

        #region Private Methods
        private static bool hasChangePlayer(UpdatePlayerDto dto, Player player)
        {
            bool hasChange = false;

            if (dto.Name != player.Name)
            {
                player.Name = dto.Name;
                hasChange = true;
            }

            if (dto.DateOfBirth != player.DateOfBirth)
            {
                player.DateOfBirth = dto.DateOfBirth;
                hasChange = true;
            }

            if (dto.Address != player.Address)
            {
                player.Address = dto.Address;
                hasChange = true;
            }

            if (dto.Email != player.Email)
            {
                player.Email = dto.Email;
                hasChange = true;
            }

            if (dto.Phone != player.Phone)
            {
                player.Phone = dto.Phone;
                hasChange = true;
            }

            if (dto.Position != player.Position)
            {
                player.Position = dto.Position;
                hasChange = true;
            }

            if (dto.Height != player.Height)
            {
                player.Height = dto.Height;
                hasChange = true;
            }

            return hasChange;
        }

        #endregion
    }
}
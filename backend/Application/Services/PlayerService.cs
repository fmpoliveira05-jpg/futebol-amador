using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        #region Initializer
        private readonly IPlayerRepository playerRepository;
        private readonly IUserRepository userRepository;
        private readonly ITeamRepository teamRepository;  
        private readonly IUnityOfWork unityOfWork;    
        private readonly ITeamService teamService;
        private readonly IAuthService AuthService;
        private readonly IUserDataValidator UserDataValidator;
        private readonly IPlayerValidator playerValidator;
        private readonly ITeamValidator teamValidator;

        public PlayerService(IPlayerRepository playerRepository, ITeamRepository teamRepository,
            IUnityOfWork unitOfWork, IMembershipRequestRepository membershipRequestRepository,
            IPlayerValidator playerValidator, IUserRepository userRepository, 
            ITeamService teamService, IUserDataValidator userDataValidator,
            IAuthService authService, ITeamValidator teamValidator)
        {
            this.playerRepository = playerRepository;
            this.teamRepository = teamRepository;
            this.unityOfWork = unitOfWork;
            this.playerValidator = playerValidator;
            this.userRepository = userRepository;
            this.teamService = teamService;
            this.UserDataValidator = userDataValidator;
            this.AuthService = authService;
            this.teamValidator = teamValidator;
        }

        #endregion

        #region CRUD Player
        public async Task<string> CreatePlayerAsync(CreatePlayerDto playerDto)
        {
            UserDataValidator.PhoneNumberValidation(playerDto.Phone);
            UserDataValidator.EmailValidation(playerDto.Email);

            var userSamePhoneNumber = await playerRepository.GetPlayerByPhoneNumberAsync(playerDto.Phone);
            var userSameEmail = await playerRepository.GetPlayerByEmailAsync(playerDto.Email);

            playerValidator.CreatePlayerValidator(playerDto, userSamePhoneNumber, userSameEmail);

            var userId = await AuthService.RegisterUser(playerDto.Email, playerDto.Password, playerDto.Phone);

            UserDataValidator.CreateUserValidation(userId);

            var player = new Player
            {
                Id = userId,
                Name = playerDto.Name,
                DateOfBirth = playerDto.DateOfBirth,
                Address = playerDto.Address,
                Email = playerDto.Email,
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
            UserDataValidator.DeleteUserValidation(playerId);
            var playerToDelete = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.DeletePlayerValidator(playerToDelete);

            playerRepository.DeletePlayer(playerToDelete);

            AuthService.DeleteUserAsync(playerId);

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<UpdatePlayerDto> UpdatePlayerAsync(string playerId, UpdatePlayerDto dto)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            var existingPlayers = new User[] {
                await userRepository.GetUserByEmailAsync(dto.Email),
                await userRepository.GetUserByPhoneAsync(dto.Phone),
            };

            playerValidator.UpdatePlayerValidator(dto, player, existingPlayers);

            bool hasChange = await hasChangePlayer(dto, player);
            playerValidator.ValidateHasChangeDataPlayer(hasChange);

            await unityOfWork.SaveChangesAsync();

            player.Name = dto.Name;
            player.DateOfBirth = dto.DateOfBirth;
            player.Address = dto.Address;
            player.Email = dto.Email;
            player.Phone = dto.Phone;
            player.Position = dto.Position;
            player.Height = dto.Height;

            var updatedDto = new UpdatePlayerDto
            {
                playerId = player.Id,
                Name = player.Name,
                DateOfBirth = player.DateOfBirth,
                Address = player.Address,
                Email = player.Email,
                Phone = player.Phone,
                Position = player.Position,
                Height = player.Height
            };

            return updatedDto;
        }

        public async Task<PlayerDetailsDto> GetPlayerByIdAsync(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.GetPlayerByIdValidator(player);

            PlayerDetailsDto playerDetails = new PlayerDetailsDto
            {
                PlayerId = playerId,
                Name = player.Name,
                Email = player.Email,
                PhoneNumber = player.Phone,
                DateOfBirth = player.DateOfBirth,
                Address = player.Address,
                Position = player.Position,
                Height = player.Height,
                IdTeam = player.IdTeam,
                IsAdmin = player.IsAdmin,
                TeamName = player.Team?.Name
            };

            return playerDetails;
        }

        public async Task<List<InfoPlayerDto?>> ListPlayers(FilterTeamDto? filter)
        {
            teamValidator.ValidateFiltersGetPlayersWithout(filter);
           
            return await playerRepository.GetPlayersList(filter);
        }

        #endregion

        #region Actions Player in Team
        //Falta tirar o player da lista de players do team
        public async Task<InfoPlayerDto> LeaveTeam(string playerId)
        {
            var existingPlayer = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.LeaveTeamValidator(existingPlayer);

            var team = await teamRepository.GetTeamByIdAsync((Guid)existingPlayer.IdTeam);

            if (existingPlayer.IsAdmin)
            {
                if (team.Members.Count == 1)
                {
                    await teamService.DeleteTeamAsync((Guid)existingPlayer.IdTeam, existingPlayer.Id);
                }
                else
                {
                    var otherAdmin = team.Members
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

            string teamName = team.Name;

            team.Members.Remove(existingPlayer);
            existingPlayer.Team = null;
            existingPlayer.IdTeam = null;

            playerRepository.UpdatePlayer(existingPlayer);

            await unityOfWork.SaveChangesAsync();

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            int age = dateNow.Year - existingPlayer.DateOfBirth.Year;
            if (dateNow < existingPlayer.DateOfBirth.AddYears(age)) {
                age--;
            }

            var playerDto = new InfoPlayerDto
            {
                Id = existingPlayer.Id,
                Name = existingPlayer.Name,
                Address = existingPlayer.Address,
                Age = age,
                Position = existingPlayer.Position,
                Heigth = existingPlayer.Height,
                HaveTeam = existingPlayer.IdTeam != null
            };

            return playerDto;
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
        private async Task<bool> hasChangePlayer(UpdatePlayerDto dto, Player player)
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
                 await AuthService.UpdateEmailAsync(player.Id, dto.Email);
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
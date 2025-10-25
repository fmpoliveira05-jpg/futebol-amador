using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository playerRepository;
        private readonly ITeamService teamService;
        private readonly IUnityOfWork unitOfWork;
        private readonly IPlayerValidator playerValidator;

        public PlayerService(IPlayerRepository playerRepository, ITeamService teamService, IUnityOfWork unitOfWork, IPlayerValidator playerValidator)
        {
            this.playerRepository = playerRepository;
            this.teamService = teamService;
            this.unitOfWork = unitOfWork;
            this.playerValidator = playerValidator;
        }

        public async Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDto)
        {
            var existingPlayer = await playerRepository.GetPlayerByEmailAsync(playerDto.Email);

            playerValidator.CreatePlayerValidator(playerDto, existingPlayer);

            var player = new Player
            {
                Name = playerDto.Name,
                DateOfBirth = playerDto.DateOfBirth,
                Address = playerDto.Address,
                Email = playerDto.Email,
                Password = playerDto.Password,
                Phone = playerDto.Phone,
                Position = playerDto.Position,
                Height = playerDto.Height,
                CreationDate = DateTime.Now
            };

            await playerRepository.AddAsync(player);
            
            await unitOfWork.SaveChangesAsync();
            
            return player.Id;
        }

        public async Task DeletePlayerAsync(Guid playerId)
        {
            var playerToDelete = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.DeletePlayerValidator(playerToDelete);

            playerRepository.DeletePlayer(playerToDelete);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.PlayerExists(player);

            PlayerDetailsDTO playerDetails = new PlayerDetailsDTO
            {
                Name = player.Name,
                DateOfBirth = player.DateOfBirth,
                Address = player.Address,
                Position = player.Position,
                Height = player.Height
            };

            return playerDetails;
        }

        public async Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDTO dto)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            if (player == null)
            {
                throw new Exception($"Player with ID {playerId} not found.");
            }

            var emailExists = await playerRepository.GetPlayerByEmailAsync(dto.Email);

            playerValidator.UpdatePlayerValidator(dto, player, emailExists);
            
            if (emailExists != null)
            {
                throw new Exception($"The Email {dto.Email} is already being used.");
            }

            var updatedPlayer = new Player
            {
                Id = playerId,
                Name = dto.Name,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                Email = dto.Email,
                Phone = dto.Phone,
                Position = dto.Position,
                Height = dto.Height
            };

            playerRepository.UpdatePlayer(updatedPlayer);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<String> LeaveTeam(Guid playerId)
        {
            var existingPlayer = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.LeaveTeamValidator(existingPlayer);

            if (existingPlayer.IsAdmin)
            {
                if (existingPlayer.Team != null)
                {
                    if (existingPlayer.Team.Members.Count == 0)
                    {
                        await teamService.DeleteTeamAsync((Guid)existingPlayer.IdTeam, existingPlayer.Id);
                    }
                    
                    var otherAdmin = existingPlayer.Team.Members.First(p => p.IsAdmin == true && p.Id != existingPlayer.Id);

                    //if the team has no more admins, choose the oldest account player to be the new admin.
                    if (otherAdmin == null)
                    {
                        var oldestDate = existingPlayer.Team.Members.Min(p => p.CreationDate);
                        Player newAdmin = existingPlayer.Team.Members.First(p => p.CreationDate == oldestDate);
                        newAdmin.IsAdmin = true;
                    }
                }

                existingPlayer.IsAdmin = false;
            }

            string teamName = existingPlayer.Team.Name;

            existingPlayer.Team = null;
            existingPlayer.IdTeam = null;

            playerRepository.UpdatePlayer(existingPlayer);

            await unitOfWork.SaveChangesAsync();

            return teamName;
        }
    }
}

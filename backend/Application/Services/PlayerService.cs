using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository playerRepository;
        private readonly IUnitOfWork unitOfWork;

        public PlayerService(IPlayerRepository playerRepository, IUnitOfWork unitOfWork)
        {
            this.playerRepository = playerRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDto)
        {
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

            if (playerToDelete == null)
            {
                throw new Exception($"Player with ID {playerId} not found.");
            }    

            playerRepository.DeletePlayer(playerToDelete);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<PlayerDetailsDTO> GetPlayerByIdAsync(Guid playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            if (player == null)
            {
                throw new Exception($"Player with ID {playerId} not found.");
            }

            PlayerDetailsDTO playerDetails = new PlayerDetailsDTO
            {
                Id = playerId,
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

            var updatedPlayer = new Player
            {
                Id = playerId,
                Name = dto.Name,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                Position = dto.Position,
                Height = dto.Height
            };

            playerRepository.UpdatePlayer(updatedPlayer);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<String> LeaveTeam(Guid playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            if (player == null)
            {
                throw new Exception($"Player with ID {playerId} not found.");
            }

            string teamName = player.Team.Name;

            player.Team = null;
            player.idTeam = null;

            playerRepository.UpdatePlayer(player);

            await unitOfWork.SaveChangesAsync();

            return teamName;
        }
    }
}

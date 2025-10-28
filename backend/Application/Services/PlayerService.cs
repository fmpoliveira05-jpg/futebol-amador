using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository playerRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IUnityOfWork unitOfWork;

        public PlayerService(IPlayerRepository playerRepository, ITeamRepository teamRepository, IUnityOfWork unitOfWork)
        {
            this.playerRepository = playerRepository;
            this.teamRepository = teamRepository;
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
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            if (player == null)
            {
                throw new Exception($"Player with ID {playerId} not found.");
            }

            if (player.IsAdmin)
            {
                player.IsAdmin = false;

                if (player.Team != null)
                {
                    if (player.Team.Members.Count == 0)
                    {
                        //delete team, there are no more players.
                        //n sei como vou fazer isso, willkie disse q saberia fazer.
                    }
                    
                    var otherAdmin = player.Team.Members.First(p => p.IsAdmin == true);

                    //if the team has no more admins, choose the oldest account player to be the new admin.
                    if (otherAdmin == null)
                    {
                        var oldestDate = player.Team.Members.Min(p => p.CreationDate);
                        Player newAdmin = player.Team.Members.First(p => p.CreationDate == oldestDate);
                        newAdmin.IsAdmin = true;
                    }
                }

            }

            string teamName = player.Team.Name;

            player.Team = null;
            player.IdTeam = null;

            playerRepository.UpdatePlayer(player);

            await unitOfWork.SaveChangesAsync();

            return teamName;
        }
    }
}

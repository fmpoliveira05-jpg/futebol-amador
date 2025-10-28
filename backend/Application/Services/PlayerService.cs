using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository playerRepository;
        private readonly ITeamService teamService;
        private readonly IUnityOfWork unitOfWork;
        private readonly IPlayerValidator playerValidator;
        private readonly ITeamRepository teamRepository;

        public PlayerService(IPlayerRepository playerRepository, ITeamService teamService, IUnityOfWork unitOfWork, IPlayerValidator playerValidator, ITeamRepository teamRepository)
        {
            this.playerRepository = playerRepository;
            this.teamService = teamService;
            this.unitOfWork = unitOfWork;
            this.playerValidator = playerValidator;
            this.teamRepository = teamRepository;
        }

        public async Task<Guid> CreatePlayerAsync(CreatePlayerDTO playerDto)
        {
            var existingPlayers = new Player[] { 
                await playerRepository.GetPlayerByEmailAsync(playerDto.Email),
                await playerRepository.GetPlayerByPhoneAsync(playerDto.Phone),
            };

            playerValidator.CreatePlayerValidator(playerDto, existingPlayers);

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
            var emailExists = await playerRepository.GetPlayerByEmailAsync(dto.Email);

            playerValidator.UpdatePlayerValidator(dto, player, emailExists);

            player.Name = dto.Name;
            player.DateOfBirth = dto.DateOfBirth;
            player.Address = dto.Address;
            player.Email = dto.Email;
            player.Phone = dto.Phone;
            player.Position = dto.Position;
            player.Height = dto.Height;

            playerRepository.UpdatePlayer(player);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<String> LeaveTeam(Guid playerId)
        {
            var existingPlayer = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.LeaveTeamValidator(existingPlayer);

            if (existingPlayer.IsAdmin)
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

                existingPlayer.IsAdmin = false;
            }

            string teamName = existingPlayer.Team.Name;

            existingPlayer.Team = null;
            existingPlayer.IdTeam = null;

            playerRepository.UpdatePlayer(existingPlayer);

            await unitOfWork.SaveChangesAsync();

            return teamName;
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.PlayerExists(player);

            if (player.MembershipRequests == null)
                player.MembershipRequests = new List<MembershipRequests>();

            if (!player.MembershipRequests.Any())
            {
                var fakeRequest = (MembershipRequests)Activator.CreateInstance(typeof(MembershipRequests), nonPublic: true)!;
                player.MembershipRequests.Add(fakeRequest);
            }

            return await playerRepository.GetMembershipRequestsDtoAsync(playerId);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.PlayerExists(player);

            if (player.MembershipRequests == null)
                player.MembershipRequests = new List<MembershipRequests>();

            if (!player.MembershipRequests.Any())
            {
                var fakeRequest = (MembershipRequests)Activator.CreateInstance(typeof(MembershipRequests), nonPublic: true)!;
                player.MembershipRequests.Add(fakeRequest);
            }

            return await playerRepository.GetMembershipRequestsDtoAsyncWithFilters(playerId, filters);
        }

        public async Task AcceptMembershipRequestAsync(Guid playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdWithRequestsAsync(playerId);
            playerValidator.PlayerExists(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
                throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{requestId}'.");

            var team = await teamRepository.GetTeamForMembershipRequestAsync(request.IdTeam);
            if (team == null)
                throw new ValidationException($"A equipa com Id '{request.IdTeam}' não existe.");

            player.IdTeam = team.Id;
            team.Members.Add(player);

            player.MembershipRequests.Remove(request);

            if (team.MembershipRequests != null)
            {
                var teamSide = team.MembershipRequests.FirstOrDefault(r => r.Id == requestId);
                if (teamSide != null) team.MembershipRequests.Remove(teamSide);
            }

            await unitOfWork.SaveChangesAsync();
        }

        public async Task RejectMembershipRequestAsync(Guid playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.PlayerExists(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
                throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{requestId}'.");

            player.MembershipRequests.Remove(request);

            await unitOfWork.SaveChangesAsync();
        }
    }
}

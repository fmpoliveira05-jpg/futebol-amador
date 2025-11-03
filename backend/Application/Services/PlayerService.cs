using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository playerRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IUnityOfWork unitOfWork;
        private readonly IPlayerValidator playerValidator;

        public PlayerService(IPlayerRepository playerRepository, ITeamRepository teamRepository, IUnityOfWork unitOfWork)
        {
            this.playerRepository = playerRepository;
            this.teamRepository = teamRepository;
            this.unitOfWork = unitOfWork;
            this.playerValidator = new PlayerValidator();
        }

        public async Task<string> CreatePlayerAsync(string userId,string email,CreatePlayerDTO playerDto)
        {
            var player = new Player
            {
                Id = userId,
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
            
            await unitOfWork.SaveChangesAsync();
            
            return player.Id;
        }

        public async Task DeletePlayerAsync(string playerId)
        {
            var playerToDelete = await playerRepository.GetPlayerByIdAsync(playerId);

            if (playerToDelete == null)
            {
                throw new Exception($"Player with ID {playerId} not found.");
            }    

            playerRepository.DeletePlayer(playerToDelete);

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<PlayerDetailsDTO> GetPlayerByIdAsync(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.GetPlayerByIdValidator(player);

            if (player == null)
            {
                throw new Exception($"Player with ID {playerId} not found.");
            }

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

            var existingPlayers = new Users[] {
                await userRepository.GetUserByEmailAsync(dto.Email),
                await userRepository.GetUserByPhoneAsync(dto.Phone),
            };

            playerValidator.UpdatePlayerValidator(dto, player, existingPlayers);

            bool hasChange = hasChangePlayer(dto, player);
            playerValidator.ValidateHasChangeDataPlayer(hasChange);

            await unityOfWork.SaveChangesAsync();
        }

        //Falta tirar o player da lista de players do team
        public async Task<string> LeaveTeam(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            if (player == null)
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

            if (player.IsAdmin)
            {
                player.IsAdmin = false;

            existingPlayer.Team.Members.Remove(existingPlayer);
            existingPlayer.Team = null;
            existingPlayer.IdTeam = null;

                    //if the team has no more admins, choose the oldest account player to be the new admin.
                    if (otherAdmin == null)
                    {
                        var oldestDate = player.Team.Members.Min(p => p.CreationDate);
                        Player newAdmin = player.Team.Members.First(p => p.CreationDate == oldestDate);
                        newAdmin.IsAdmin = true;
                    }
            }

            

            string teamName = player.Team.Name;

            player.Team = null;
            player.IdTeam = null;

            playerRepository.UpdatePlayer(player);

            await unityOfWork.SaveChangesAsync();

            return teamName;
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.PlayerExists(player);

            return await playerRepository.GetMembershipRequestsDtoAsync(playerId);
        }

        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncWithFilters(Guid playerId, FilterMembershipRequestsPlayer filters)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.PlayerExists(player);

            return await playerRepository.GetMembershipRequestsDtoAsyncWithFilters(playerId, filters);
        }

        public async Task<MemberShipRequestDto> AcceptMembershipRequestAsync(Guid playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdWithRequestsAsync(playerId)
                ?? throw new ValidationException($"O jogador com Id '{playerId}' não existe.");

            playerValidator.PlayerExists(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId)
                ?? throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{requestId}'.");

            var team = await teamRepository.GetTeamForMembershipRequestAsync(request.IdTeam)
                ?? throw new ValidationException($"A equipa com Id '{request.IdTeam}' não existe.");

            player.IdTeam = team.Id;
            team.Members.Add(player);

            player.MembershipRequests.Remove(request);
            team.MembershipRequests?.Remove(request);

            await unityOfWork.SaveChangesAsync();

            var fullPlayer = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);
            var fullTeam = await teamRepository.GetTeamByIdAsync(request.IdTeam);

            return new MemberShipRequestDto
            {
                RequestId = request.Id,
                PlayerId = fullPlayer.Id,
                PlayerName = fullPlayer.Name,
                TeamId = fullTeam.Id,
                TeamName = fullTeam.Name,
                RequestDate = request.InviteDate,
                IsPlayerSender = request.IsPlayerSender
            };
        }

        public async Task<MemberShipRequestDto> RejectMembershipRequestAsync(Guid playerId, Guid requestId)
        {
            var player = await playerRepository.GetPlayerByIdWithRequestsAsync(playerId)
                ?? throw new ValidationException($"O jogador com Id '{playerId}' não existe.");

            playerValidator.PlayerExists(player);

            var request = player.MembershipRequests?.FirstOrDefault(r => r.Id == requestId)
                ?? throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{requestId}'.");

            var fullPlayer = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);
            var fullTeam = await teamRepository.GetTeamByIdAsync(request.IdTeam);

            player.MembershipRequests.Remove(request);

            await unityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = request.Id,
                PlayerId = fullPlayer.Id,
                PlayerName = fullPlayer.Name,
                TeamId = fullTeam.Id,
                TeamName = fullTeam.Name,
                RequestDate = request.InviteDate,
                IsPlayerSender = request.IsPlayerSender
            };
        }

        //faz validation de se o player ja tem equipa
        public async Task<MemberShipRequestDto> SendMembershipRequestAsync(Guid playerId, Guid teamId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);

            var existingRequest = await membershipRequestRepository
                .GetMembershipRequestByPlayerAndTeam(playerId, teamId);

            playerValidator.SendMembershipRequestValidator(player, team, existingRequest);

            var newRequest = new MembershipRequests
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerId,
                IdTeam = teamId,
                Player = player,
                Team = team,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = true
            };

            await membershipRequestRepository.AddMembershipRequest(newRequest);
            await unityOfWork.SaveChangesAsync();

            return new MemberShipRequestDto
            {
                RequestId = newRequest.Id,
                PlayerId = newRequest.IdPlayer,
                PlayerName = newRequest.Player.Name,
                TeamId = newRequest.IdTeam,
                TeamName = newRequest.Team.Name,
                RequestDate = newRequest.InviteDate,
                IsPlayerSender = newRequest.IsPlayerSender
            };
        }

        public async Task<List<InfoTeamsDto>> GetListTeams()
        {
            return await teamRepository.GetListTeamsPlayer();
        }

        public async Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter)
        {
            playerValidator.ValidateFiltersListTeams(filter);
            return await teamRepository.GetListTeamsPlayersWithFilters(filter);
        }

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
using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
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
        private readonly IUserRepository userRepository;
        private readonly ITeamService teamService;
        private readonly ITeamRepository teamRepository;
        private readonly IPlayerValidator playerValidator;
        private readonly IUnityOfWork unityOfWork;
        private readonly IPasswordHasher passwordHasher;
        private readonly IMembershipRequestRepository membershipRequestRepository;

        public PlayerService(
            IPlayerRepository playerRepository,
            ITeamService teamService,
            IUserRepository userRepository,
            IUnityOfWork unityOfWork,
            IPlayerValidator playerValidator,
            ITeamRepository teamRepository,
            IPasswordHasher passwordHasher,
            IMembershipRequestRepository membershipRequestRepository)
        {
            this.playerRepository = playerRepository;
            this.userRepository = userRepository;
            this.teamService = teamService;
            this.unityOfWork = unityOfWork;
            this.playerValidator = playerValidator;
            this.teamRepository = teamRepository;
            this.passwordHasher = passwordHasher;
            this.membershipRequestRepository = membershipRequestRepository;
        }

        public async Task<Guid> CreatePlayerAsync(CreatePlayerDto playerDto)
        {
            var existingPlayers = new Users[] {
                await userRepository.GetUserByEmailAsync(playerDto.Email),
                await userRepository.GetUserByPhoneAsync(playerDto.Phone),
            };

            playerValidator.CreatePlayerValidator(playerDto, existingPlayers);

            var player = new Player
            {
                Name = playerDto.Name,
                DateOfBirth = playerDto.DateOfBirth,
                Address = playerDto.Address,
                Email = playerDto.Email,
                Password = passwordHasher.HashPassword(playerDto.Password),
                Phone = playerDto.Phone,
                Position = playerDto.Position,
                Height = playerDto.Height,
                CreationDate = DateTime.UtcNow
            };

            await playerRepository.AddAsync(player);
            await unityOfWork.SaveChangesAsync();

            return player.Id;
        }

        public async Task DeletePlayerAsync(Guid playerId)
        {
            var playerToDelete = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.DeletePlayerValidator(playerToDelete);

            playerRepository.DeletePlayer(playerToDelete);

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<PlayerDetailsDto> GetPlayerByIdAsync(Guid playerId)
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

        public async Task UpdatePlayerAsync(Guid playerId, UpdatePlayerDto dto)
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

        public async Task<string> LeaveTeam(Guid playerId)
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

            playerValidator.PlayerExists(player);
            playerValidator.SendMembershipRequestValidator(player, team);

            if (team == null)
                throw new ValidationException("A equipa especificada não existe.");

            if (player.IdTeam == teamId)
                throw new ValidationException("O jogador já pertence a esta equipa.");

            var existingRequest = await membershipRequestRepository
                .GetMembershipRequestByPlayerAndTeam(playerId, teamId);

            if (existingRequest != null)
                throw new ValidationException("Já existe um pedido de adesão pendente para esta equipa.");

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
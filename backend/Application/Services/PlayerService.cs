using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using System.Data;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela lógica de negócio e gestão de dados da entidade [Player].
    /// 
    /// Centraliza todas as operações relacionadas com jogadores, incluindo criação, gestão de perfil,
    /// pesquisa de mercado e operações de afiliação a equipas.
    /// </summary>
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

        /// <summary>
        /// Construtor do PlayerService.
        /// </summary>
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

        /// <summary>
        /// Cria um novo perfil de jogador na plataforma.
        /// </summary>
        /// <remarks>
        /// **Transação Firebase-DB:**
        /// 1. Valida o formato de Email/Telefone.
        /// 2. Valida a unicidade de Email/Telefone.
        /// 3. Cria o utilizador no serviço de autenticação ([AuthService.RegisterUser]) para obter o UID.
        /// 4. Cria a entidade [Player] na base de dados relacional com o UID.
        /// 5. Persiste as alterações ([UnityOfWork.SaveChangesAsync]).
        /// </remarks>
        /// <param name="playerDto">DTO com os dados do jogador a criar.</param>
        /// <returns>O ID (string) do novo jogador criado.</returns>
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

        /// <summary>
        /// Elimina um perfil de jogador.
        /// </summary>
        /// <remarks>
        /// **Transação:** Elimina a entidade da DB e o utilizador do Firebase Auth.
        /// </remarks>
        /// <param name="playerId">ID do jogador a eliminar.</param>
        public async Task DeletePlayerAsync(string playerId)
        {
            UserDataValidator.DeleteUserValidation(playerId);
            var playerToDelete = await playerRepository.GetPlayerByIdAsync(playerId);

            playerValidator.DeletePlayerValidator(playerToDelete);

            playerRepository.DeletePlayer(playerToDelete);

            AuthService.DeleteUserAsync(playerId);

            await unityOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza as informações básicas de um jogador.
        /// </summary>
        /// <remarks>
        /// **Transação:** Atualiza os dados na DB relacional e sincroniza as alterações críticas (como Email) com o Firebase.
        /// </remarks>
        /// <param name="playerId">ID do jogador a atualizar.</param>
        /// <param name="dto">DTO com os novos dados.</param>
        /// <returns>O DTO [UpdatePlayerDto] atualizado, refletindo as alterações persistidas.</returns>
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

        /// <summary>
        /// Obtém o perfil detalhado de um jogador pelo ID.
        /// </summary>
        /// <param name="playerId">ID do jogador.</param>
        /// <returns>O DTO [PlayerDetailsDto] com os detalhes do jogador.</returns>
        public async Task<PlayerDetailsDto> GetPlayerByIdAsync(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            playerValidator.GetPlayerByIdValidator(player);
            
            var today = DateTime.Today;
            var playerDetails = new PlayerDetailsDto
            {
                PlayerId = playerId,
                Name = player.Name,
                Email = player.Email,
                PhoneNumber = player.Phone,
                DateOfBirth = player.DateOfBirth,
                Address = player.Address,
                Position = player.Position,
                Height = player.Height,
                Age = today.Year - player.DateOfBirth.Year,
                Team = player.IdTeam.HasValue ? new TeamDto
                {
                    IdTeam = player.IdTeam.Value,
                    Name = player.Team?.Name
                } : null,
                IsAdmin = player.IsAdmin,
            };

            return playerDetails;
        }

        /// <summary>
        /// Lista jogadores com filtros aplicados.
        /// </summary>
        /// <remarks>
        /// Utilizado para a funcionalidade de "Mercado de Jogadores".
        /// </remarks>
        /// <param name="filter">Filtros de pesquisa.</param>
        /// <returns>Lista de [InfoPlayerDto] (visão resumida).</returns>
        public async Task<List<InfoPlayerDto?>> ListPlayers(FilterTeamDto? filter)
        {
            teamValidator.ValidateFiltersGetPlayersWithout(filter);
           
            return await playerRepository.GetPlayersList(filter);
        }

        #endregion

        #region Actions Player in Team

        /// <summary>
        /// Remove a afiliação de um jogador à sua equipa atual.
        /// </summary>
        /// <remarks>
        /// **Lógica Complexa:** Se o jogador for Admin, o serviço tenta nomear o membro mais antigo como novo Admin se for o último Admin a sair. Se for o último membro da equipa (Admin ou não), a equipa é eliminada ([teamService.DeleteTeamAsync]).
        /// **Side-Effects:** Atualiza IsAdmin e IdTeam para null.
        /// </remarks>
        /// <param name="playerId">ID do jogador que está a sair.</param>
        /// <returns>DTO [InfoPlayerDto] do jogador agora agente livre.</returns>
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

        /// <summary>
        /// Obtém uma lista de todas as equipas elegíveis (com vagas) para que o jogador possa interagir.
        /// </summary>
        /// <remarks>
        /// Esta consulta é utilizada para popular a lista principal de pesquisa ou o mercado de equipas.
        /// </remarks>
        /// <returns>Lista de [InfoTeamsDto] com estatísticas resumidas das equipas.</returns>
        public async Task<List<InfoTeamsDto>> GetListTeams()
        {
            return await teamRepository.GetListTeamsPlayer();
        }

        /// <summary>
        /// Obtém uma lista de equipas elegíveis, aplicando filtros dinâmicos de pesquisa.
        /// </summary>
        /// <remarks>
        /// Executa a validação síncrona do filtro para garantir que os intervalos (ex: pontuação) são válidos 
        /// antes de executar a consulta à base de dados.
        /// </remarks>
        /// <param name="filter">O DTO contendo os critérios de filtragem (Nome, Rank, Pontos, Idade Média, Localidade).</param>
        /// <returns>Lista de [InfoTeamsDto] filtrados.</returns>
        public async Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter)
        {
            playerValidator.ValidateFiltersListTeams(filter);
            return await teamRepository.GetListTeamsPlayersWithFilters(filter);
        }
        #endregion

        public async Task<bool> UpdateDeviceTokenAsync(string userId, string token)
        {
            var user = await playerRepository.GetPlayerByIdAsync(userId);
            if (user == null) return false;

            user.DeviceToken = string.IsNullOrWhiteSpace(token) ? null : token;

            await unityOfWork.SaveChangesAsync();
            return true;
        }

        #region Private Methods

        /// <summary>
        /// Compara o DTO de atualização com a entidade existente para verificar se houve alguma alteração de dados.
        /// Aplica as alterações aos campos da entidade e chama a atualização de Email no Auth Service se necessário.
        /// </summary>
        /// <param name="dto">O DTO de atualização.</param>
        /// <param name="player">A entidade [Player] existente.</param>
        /// <returns><c>true</c> se pelo menos um campo foi alterado/sincronizado.</returns>
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
using Application.DTOs.Filters;
using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using System.Xml.Linq;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela lógica de negócio e ciclo de vida dos Pedidos de Adesão ([MembershipRequest]).
    /// 
    /// Esta classe gere a criação, aceitação e rejeição de pedidos de adesão, tanto quando o Jogador envia
    /// (Join Request) quanto quando a Equipa convida (Recruitment Invite).
    /// </summary>
    public class MembershipService : IMembershipRequestService
    {
        private readonly ITeamRepository teamRepository;
        private readonly IPlayerRepository playerRepository;
        private readonly IMembershipRequestRepository membershipRequestRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IPlayerValidator playerValidator;
        private readonly IPlayerAuthorizationValidator authorizationValidator;
        private readonly IMembershipValidator membershipValidator;
        private readonly INotificationService notificationService;
        private readonly INotificationFirebaseService notificationFirebaseService;

        /// <summary>
        /// Construtor do MembershipService.
        /// </summary>
        public MembershipService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IUnityOfWork unityOfWork,
            ITeamValidator teamValidator,
            IPlayerValidator playerValidator,
            IPlayerAuthorizationValidator authorizationValidator,
            IMembershipValidator membershipValidator,
            INotificationService notificationService,
            INotificationFirebaseService notificationFirebaseService)
        {
            this.teamRepository = teamRepository;
            this.playerRepository = playerRepository;
            this.membershipRequestRepository = membershipRequestRepository;
            this.unityOfWork = unityOfWork;
            this.playerValidator = playerValidator;
            this.authorizationValidator = authorizationValidator;
            this.membershipValidator = membershipValidator;
            this.notificationService = notificationService;
            this.notificationFirebaseService = notificationFirebaseService;
        }

        #region Pedidos de adesão da Team

        /// <summary>
        /// Envia um convite de recrutamento de uma Equipa para um Jogador.
        /// </summary>
        /// <remarks>
        /// **Regras:** O remetente deve ser um Admin. O jogador alvo não pode ter equipa. A equipa não pode estar cheia.
        /// **Side-Effects:** Envia notificação ao jogador alvo.
        /// </remarks>
        /// <param name="teamId">ID da equipa que convida.</param>
        /// <param name="playerIdToInvite">ID do jogador alvo.</param>
        /// <param name="sender">ID do administrador que executa a ação.</param>
        /// <returns>DTO do pedido criado.</returns>
        public async Task<MemberShipRequestDto> SendMembershipRequestTeam(Guid teamId, string playerIdToInvite, string sender)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);
            var playerToInvite = await playerRepository.GetPlayerByIdAsync(playerIdToInvite);
            var player = await playerRepository.GetPlayerByIdAsync(sender);
            var existing = await membershipRequestRepository.GetMembershipRequestByPlayerAndTeam(playerIdToInvite, teamId);

            membershipValidator.ValidateSendRequestByTeam(team, playerToInvite, existing, player);

            var invite = new MembershipRequest
            {
                Id = Guid.NewGuid(),
                IdPlayer = playerIdToInvite,
                IdTeam = teamId,
                InviteDate = DateTime.UtcNow,
                IsPlayerSender = false
            };

            await membershipRequestRepository.AddMembershipRequest(invite);
            await unityOfWork.SaveChangesAsync();
            await notificationService.SendUserAsync(playerIdToInvite, "New Team Invitation!", $"You've been invited to join the team {team.Name}!");

            return new MemberShipRequestDto
            {
                RequestId = invite.Id,
                Player = new PlayerDto
                {
                    Id = invite.IdPlayer,
                    Name = playerToInvite.Name
                },
                Team = new TeamDto {
                    IdTeam = invite.IdTeam,
                    Name = team.Name,
                },
                RequestDate = invite.InviteDate,
                IsPlayerSender = invite.IsPlayerSender
            };
        }

        /// <summary>
        /// Aceita um pedido de adesão (Join Request) por parte de um Administrador de Equipa.
        /// </summary>
        /// <remarks>
        /// **Transação:**
        /// 1. Valida a permissão do Admin e se a equipa está cheia.
        /// 2. Remove o [MembershipRequest].
        /// 3. Atualiza o [Player] com o novo [IdTeam] (afiliação).
        /// 4. Remove TODOS os pedidos pendentes associados a esse jogador (seja de outras equipas ou convites de recrutamento).
        /// 5. Notifica o jogador aceite.
        /// </remarks>
        /// <param name="teamId">ID da equipa que aceita.</param>
        /// <param name="requestId">ID do pedido a aceitar.</param>
        /// <param name="adminId">ID do administrador que executa a ação.</param>
        public async Task AcceptMembershipRequestTeam(Guid teamId, Guid requestId, string adminId)
        {
            var request = await membershipRequestRepository.GetMembershipRequestById(requestId);
            var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);
            var playerAccepting = await playerRepository.GetPlayerByIdAsync(adminId);

            membershipValidator.ValidateAcceptRequestByTeam(team, request, playerAccepting);

            var playerAccepted = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);

            membershipRequestRepository.RemoveMembershipRequest(request);

            playerAccepted.IdTeam = teamId;
            team.Members.Add(playerAccepted);

            await membershipRequestRepository.RemoveAllMemberShipRequestsOfPlayer(playerAccepted.Id);
            await RemoveAllMatchInviteTeam(team);
            await unityOfWork.SaveChangesAsync();
            await notificationService.SendUserAsync(request.IdPlayer, "Membership request Accepted!", $"Your request to join the team {team.Name} has been accepted!");

            SendNotificationAcceptMemberShipRequest(teamId, team.Name, playerAccepting.Id, playerAccepting.Name);
        }

        /// <summary>
        /// Rejeita um pedido de adesão (Join Request) por parte de um Administrador de Equipa.
        /// </summary>
        /// <remarks>
        /// **Transação:** Apenas remove o pedido e notifica o jogador, sem alterar a afiliação.
        /// </remarks>
        /// <param name="teamId">ID da equipa que rejeita.</param>
        /// <param name="requestId">ID do pedido a rejeitar.</param>
        /// <param name="adminId">ID do administrador que executa a ação.</param>
        public Task RejectMembershipRequestTeam(Guid teamId, Guid requestId, string adminId)
        {
            return membershipRequestRepository.GetMembershipRequestById(requestId).ContinueWith(async requestTask =>
            {
                var request = await requestTask;
                var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);
                var playerAccepting = await playerRepository.GetPlayerByIdAsync(adminId);

                membershipValidator.ValidateRejectRequestByTeam(team, request, playerAccepting);

                var playerAccepted = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);

                membershipRequestRepository.RemoveMembershipRequest(request);

                await unityOfWork.SaveChangesAsync();
                await notificationService.SendUserAsync(request.IdPlayer, "Membership request rejected.", $"Your request to join the team {team.Name} has been rejected.");
            }).Unwrap();
        }

        /// <summary>
        /// Obtém a lista de pedidos de adesão (Join Requests) recebidos por uma Equipa.
        /// </summary>
        /// <remarks>
        /// **Permissões:** Apenas Admins da equipa podem ver esta lista.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerId">ID do administrador que está a consultar.</param>
        /// <returns>Lista de DTOs dos pedidos de adesão pendentes.</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string playerId)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);

            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            membershipValidator.ValidateGetRequestsByTeam(team, player);

            return await membershipRequestRepository.GetMembershipRequestsByTeam(teamId);
        }

        /// <summary>
        /// Obtém a lista de pedidos de adesão recebidos por uma Equipa, aplicando filtros de pesquisa.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="filters">Filtros (Nome do Jogador, Data).</param>
        /// <param name="playerId">ID do administrador que está a consultar.</param>
        /// <returns>Lista filtrada de DTOs dos pedidos.</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters, string playerId)
        {
            var team = await teamRepository.GetTeamForMemberManagementAsync(teamId);

            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            membershipValidator.ValidateGetRequestsByTeam(team, player);

            return await membershipRequestRepository.GetMembershipRequestsByTeamWithFilters(teamId, filters);
        }


        #endregion

        #region Pedidos de adesão do Player

        /// <summary>
        /// Obtém a lista de Convites de Recrutamento (enviados por Equipas) recebidos por um Jogador.
        /// </summary>
        /// <remarks>
        /// **Permissões:** Apenas para jogadores sem equipa ([ValidatePlayerAutorizationWithoutTeam]).
        /// </remarks>
        /// <param name="playerId">ID do jogador alvo.</param>
        /// <returns>Lista de DTOs dos convites recebidos.</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayer(string playerId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            return await membershipRequestRepository.GetMembershipRequestsByPlayer(playerId);
        }

        /// <summary>
        /// Obtém a lista de Convites de Recrutamento recebidos por um Jogador, aplicando filtros.
        /// </summary>
        /// <param name="playerId">ID do jogador alvo.</param>
        /// <param name="filters">Filtros (Nome da Equipa, Data).</param>
        /// <returns>Lista filtrada de DTOs dos convites.</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);
            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            return await membershipRequestRepository.GetMembershipRequestsByPlayerWithFilters(playerId, filters);
        }

        /// <summary>
        /// Aceita um convite de recrutamento de uma Equipa por parte do Jogador.
        /// </summary>
        /// <remarks>
        /// **Transação:** O jogador é afiliado à equipa ([IdTeam] é definido). Todos os outros pedidos pendentes do jogador são eliminados.
        /// **Side-Effects:** Notifica os administradores da equipa.
        /// </remarks>
        /// <param name="playerId">ID do jogador que aceita.</param>
        /// <param name="requestId">ID do convite a ser aceite.</param>
        /// <returns>DTO do pedido aceito.</returns>
        public async Task<MemberShipRequestDto> AcceptMembershipRequestAsyncPlayer(string playerId, Guid requestId)
        {
            var membershipRequest = await membershipRequestRepository.GetMembershipRequestById(requestId);

            if (membershipRequest == null)
            {
                throw new ValidationException("O pedido de adesão não foi encontrado.");
            }

            var player = membershipRequest.Player;

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var team = await teamRepository.GetTeamForMembershipRequestAsync(membershipRequest.IdTeam);

            membershipValidator.ValidateAcceptRequestByPlayer(player, membershipRequest, team);

            player.IdTeam = team.Id;
            team.Members.Add(player);

            player.MembershipRequests?.Remove(membershipRequest);
            team.MembershipRequests?.Remove(membershipRequest);

            await membershipRequestRepository.RemoveAllMemberShipRequestsOfPlayer(player.Id);

            await RemoveAllMatchInviteTeam(team);

            await unityOfWork.SaveChangesAsync();

            var fullPlayer = await playerRepository.GetPlayerByIdAsync(membershipRequest.IdPlayer);
            var fullTeam = await teamRepository.GetTeamByIdAsync(membershipRequest.IdTeam);

            var teamAdmins = team.Members.Where(p => p.IsAdmin).ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(admin.Id, "Membership Invite Accepted", $"{player.Name} accepted your membership invite and is now part of the team!");
            }

            await SendNotificationAcceptMemberShipRequest(team.Id, team.Name, playerId, player.Name)
            return new MemberShipRequestDto
            {
                RequestId = membershipRequest.Id,
                Player = new PlayerDto
                {
                    Id = fullPlayer.Id,
                    Name = fullPlayer.Name
                },
                Team = new TeamDto
                {
                    IdTeam = fullTeam.Id,
                    Name = fullTeam.Name,
                },
                RequestDate = membershipRequest.InviteDate,
                IsPlayerSender = membershipRequest.IsPlayerSender
            };
        }

        /// <summary>
        /// Rejeita um convite de recrutamento de uma Equipa por parte do Jogador.
        /// </summary>
        /// <param name="playerId">ID do jogador que rejeita.</param>
        /// <param name="requestId">ID do convite a ser rejeitado.</param>
        public Task<MemberShipRequestDto> RejectMembershipRequestAsyncPlayer(string playerId, Guid requestId)
        {
            return membershipRequestRepository.GetMembershipRequestById(requestId).ContinueWith(async requestTask =>
            {
                var request = await requestTask;
                var player = await playerRepository.GetPlayerByIdAsync(playerId);

                authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);
                membershipValidator.ValidateRejectRequestByPlayer(request, player);

                player.MembershipRequests?.Remove(request);
                await unityOfWork.SaveChangesAsync();

                if (request?.Team != null)
                {
                    var teamAdmins = request.Team.Members.Where(p => p.IsAdmin).ToList();

                    foreach (var admin in teamAdmins)
                    {
                        await notificationService.SendUserAsync(admin.Id, "Membership Invite Rejected", $"{player.Name} rejected your membership invite.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("A equipa associada ao pedido de adesão não foi encontrada.");
                }

                var fullPlayer = await playerRepository.GetPlayerByIdAsync(request.IdPlayer);
                var fullTeam = await teamRepository.GetTeamByIdAsync(request.IdTeam);

                return new MemberShipRequestDto
                {
                    RequestId = request.Id,
                    Player = new PlayerDto
                    {
                        Id = fullPlayer.Id,
                        Name = fullPlayer.Name
                    },
                    Team = new TeamDto
                    {
                        IdTeam = fullTeam.Id,
                        Name = fullTeam.Name,
                    },
                    RequestDate = request.InviteDate,
                    IsPlayerSender = request.IsPlayerSender
                };
            }).Unwrap();
        }

        /// <summary>
        /// Envia um pedido de adesão (Join Request) de um Jogador para uma Equipa.
        /// </summary>
        /// <remarks>
        /// **Regras:** O jogador deve ser agente livre.
        /// **Side-Effects:** Notifica os administradores da equipa.
        /// </remarks>
        /// <param name="playerId">ID do jogador que envia o pedido.</param>
        /// <param name="teamId">ID da equipa alvo.</param>
        /// <returns>DTO do pedido criado.</returns>
        public async Task<MemberShipRequestDto> SendMembershipRequestAsyncPlayer(string playerId, Guid teamId)
        {
            var player = await playerRepository.GetPlayerByIdAsync(playerId);

            authorizationValidator.ValidatePlayerAutorizationWithoutTeam(player);

            var team = await teamRepository.GetTeamForMembershipRequestAsync(teamId);

            var existingRequest = await membershipRequestRepository
                .GetMembershipRequestByPlayerAndTeam(playerId, teamId);

            playerValidator.SendMembershipRequestValidator(player, team, existingRequest);

            var newRequest = new MembershipRequest
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
            
            var teamAdmins = team.Members
                .Where(p => p.IsAdmin == true)
                .ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(admin.Id, "New Membership Request", $"Your team received a new membership request from {player.Name}.");
            }

            return new MemberShipRequestDto
            {
                RequestId = newRequest.Id,
                Player = new PlayerDto
                {
                    Id = newRequest.IdPlayer,
                    Name = newRequest.Player.Name
                },
                Team = new TeamDto
                {
                    IdTeam = newRequest.IdTeam,
                    Name = newRequest.Team.Name,
                },
                RequestDate = newRequest.InviteDate,
                IsPlayerSender = newRequest.IsPlayerSender
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Verifica se a equipa atingiu o número máximo de membros e, se sim, remove todos os pedidos pendentes para essa equipa.
        /// </summary>
        /// <remarks>
        /// Utilizado após a aceitação de um novo membro para limpar a fila de recrutamento.
        /// </remarks>
        /// <param name="team">A equipa (carregada com membros).</param>
        private async Task RemoveAllMatchInviteTeam(Team team)
        {
            if (team.Members.Count >= ModelConstants.TeamConst.MaxMembers)
            {
                await membershipRequestRepository.RemoveAllMemberShipRequestsOfTeam(team.Id);
            }
        }

        private async Task SendNotificationAcceptMemberShipRequest(Guid teamId, string nameTeam, string playerId, string namePlayer)
        {
            var title = "Convite de adesão Aceite!";
            var textPlayer = $"O seu convite de adesão da equipa {nameTeam} foi aceite";
            var textTeam = $"O {namePlayer} é um novo jogador da equipa";

            var dataPayloadPlayer = new Dictionary<string, string>()
            {
                { "type", "ACCEPT_MEMBERSHIP_REQUEST_PLAYER" },
                { "teamId", teamId.ToString() },
                { "title", title },
                { "body", textPlayer }
            };

            var dataPayloadTeam = new Dictionary<string, string>()
            {
                { "type", "ACCEPT_MEMBERSHIP_REQUEST_TEAM" },
                { "teamId", teamId.ToString() },
                { "title", title },
                { "body", textTeam }
            };

            notificationFirebaseService.SendNotificationToUser(playerId, dataPayloadPlayer, title, textPlayer);
            await notificationFirebaseService.SendMulticastNotification(teamId, dataPayloadTeam, title, textTeam);
        }
        #endregion
    }
} 
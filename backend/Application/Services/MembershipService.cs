using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    // mudar o nome do ficheiro para MembershipRequestService
    public class MembershipService : IMembershipRequestService
    {
        private readonly IPlayerRepository playerRepository;
        private readonly ITeamRepository teamRepository;
        private readonly IMembershipRequestRepository membershipRequestRepository;
        private readonly IUnityOfWork unityOfWork;

        public MembershipService(
            IPlayerRepository playerRepository,
            ITeamRepository teamRepository,
            IMembershipRequestRepository membershipRequestRepository,
            IUnityOfWork unityOfWork)
        {
            this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            this.teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            this.membershipRequestRepository = membershipRequestRepository ?? throw new ArgumentNullException(nameof(membershipRequestRepository));
            this.unityOfWork = unityOfWork ?? throw new ArgumentNullException(nameof(unityOfWork));
        }

        public async Task<IEnumerable<MemberShipRequestDto>> GetRequestsReceivedByPlayerFromTeams(Guid idPlayer)
        {
            if (idPlayer == Guid.Empty)
                throw new BusinessRuleException("O id do jogador não pode estar vazio");

            var player = await playerRepository.GetPlayerByIdAsync(idPlayer);
            if (player == null)
                throw new ArgumentNullException("O jogador não foi encontrado");

            var list = await membershipRequestRepository.GetMembershipRequestsByPlayer(idPlayer);

            var invitesFromTeams = list
                .Where(m => m.IsPlayerSender == false)
                .Select(m => new MemberShipRequestDto
                {
                    RequestId = m.Id,
                    PlayerId = m.IdPlayer,
                    PlayerName = m.Player?.Name,
                    TeamId = m.IdTeam,
                    TeamName = m.Team?.Name,
                    RequestDate = m.InviteDate,
                    IsPlayerSender = m.IsPlayerSender,
                    Message = null
                });

            return invitesFromTeams;
        }

        public async Task<IEnumerable<MemberShipRequestDto>> GetRequestsSentByPlayer(Guid idPlayer)
        {
            if (idPlayer == Guid.Empty)
                throw new BusinessRuleException("O id do jogador não pode estar vazio");

            var player = await playerRepository.GetPlayerByIdAsync(idPlayer);
            if (player == null)
                throw new ArgumentNullException("O jogador não foi encontrado");

            var list = await membershipRequestRepository.GetMembershipRequestsByPlayer(idPlayer);

            var sentByPlayer = list
                .Where(m => m.IsPlayerSender == true)
                .Select(m => new MemberShipRequestDto
                {
                    RequestId = m.Id,
                    PlayerId = m.IdPlayer,
                    PlayerName = m.Player?.Name,
                    TeamId = m.IdTeam,
                    TeamName = m.Team?.Name,
                    RequestDate = m.InviteDate,
                    IsPlayerSender = m.IsPlayerSender,
                    Message = null
                });

            return sentByPlayer;
        }

        public async Task SendMembershipRequest(SendMembershipRequestDTO dto)
        {
            if (dto == null)
                throw new BusinessRuleException("DTO inválido");

            if (dto.IdPlayer == Guid.Empty)
                throw new BusinessRuleException("O id do jogador não pode estar vazio");

            if (dto.IdTeam == Guid.Empty)
                throw new BusinessRuleException("O id da equipa não pode estar vazio");

            bool senderFlag = dto.Sender ?? true;

            var player = await playerRepository.GetPlayerByIdAsync(dto.IdPlayer);
            if (player == null)
                throw new ArgumentNullException("O jogador não foi encontrado");

            var team = await teamRepository.GetTeamByIdAsync(dto.IdTeam);
            if (team == null)
                throw new ArgumentNullException("A equipa não foi encontrada");

            var existing = await membershipRequestRepository.GetMembershipRequestByPlayerAndTeam(dto.IdPlayer, dto.IdTeam);
            if (existing != null)
            {
                throw new BusinessRuleException("Já existe um pedido de adesão entre este jogador e esta equipa");
            }

            var membershipRequest = new MembershipRequests(player, team, senderFlag)
            {
            };

            await membershipRequestRepository.AddMembershipRequest(membershipRequest);

            player.MembershipRequests.Add(membershipRequest);

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<Player> AcceptMembershipRequest(Guid idTeam, Guid idMembershipRequest)
        {
            if (idTeam == Guid.Empty)
                throw new BusinessRuleException("O id da equipa não pode estar vazio");

            if (idMembershipRequest == Guid.Empty)
                throw new BusinessRuleException("O id do pedido não pode estar vazio");

            var request = await membershipRequestRepository.GetMembershipRequestById(idMembershipRequest);
            if (request == null)
                throw new ArgumentNullException("O pedido de adesão não existe");

            if (request.IdTeam != idTeam)
                throw new BusinessRuleException("O pedido de adesão não pertence a esta equipa");

            var player = request.Player;
            if (player == null)
                throw new ArgumentNullException("O jogador do pedido não foi encontrado");

            var team = request.Team;
            if (team == null)
                throw new ArgumentNullException("A equipa do pedido não foi encontrada");

            player.Team = team;
            player.IdTeam = team.Id;

            await membershipRequestRepository.DeleteMembershipRequest(request);

            await teamRepository.UpdateTeam(team);

            await unityOfWork.SaveChangesAsync();

            return player;
        }

        public async Task RefuseMembershipRequest(Guid idTeam, Guid idMembershipRequest)
        {
            if (idTeam == Guid.Empty)
                throw new BusinessRuleException("O id da equipa não pode estar vazio");

            if (idMembershipRequest == Guid.Empty)
                throw new BusinessRuleException("O id do pedido não pode estar vazio");

            var request = await membershipRequestRepository.GetMembershipRequestById(idMembershipRequest);
            if (request == null)
                throw new ArgumentNullException("O pedido de adesão não existe");

            if (request.IdTeam != idTeam)
                throw new BusinessRuleException("O pedido de adesão não pertence a esta equipa");

            await membershipRequestRepository.DeleteMembershipRequest(request);

            await unityOfWork.SaveChangesAsync();
        }
    }
}

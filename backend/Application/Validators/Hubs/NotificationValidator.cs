using Application.Interfaces.Repositories;
using Application.Interfaces.Validators.Hub;
using Domain.Entities;

namespace Application.Validators.Hubs
{
    /// <summary>
    /// Validador de segurança utilizado no [NotificationHub] para verificar se um utilizador
    /// tem permissão para subscrever o canal de notificações de uma equipa específica.
    /// 
    /// Implementa regras de segurança estritas baseadas na existência de entidades e na afiliação.
    /// </summary>
    public class NotificationValidator : INotificationValidator
    {
        /// <summary> Repositório para buscar dados da equipa. </summary>
        private readonly ITeamRepository teamRepository;

        /// <summary> Repositório para buscar dados do jogador. </summary>
        private readonly IPlayerRepository playerRepository;

        /// <summary>
        /// Construtor da classe [NotificationValidator].
        /// </summary>
        /// <param name="teamRepository">Repositório de Equipas, injetado via DI.</param>
        /// <param name="playerRepository">Repositório de Jogadores, injetado via DI.</param>
        public NotificationValidator(ITeamRepository teamRepository, IPlayerRepository playerRepository)
        {
            this.teamRepository = teamRepository;
            this.playerRepository = playerRepository;
        }

        /// <summary>
        /// Valida se um utilizador é membro da equipa alvo.
        /// </summary>
        /// <remarks>
        /// Regras verificadas (sequencialmente):
        /// <list type="number">
        ///     <item>A equipa (Team) deve existir.</item>
        ///     <item>O jogador (Player) deve existir (autenticado).</item>
        ///     <item>O jogador deve pertencer à coleção de membros dessa equipa (Afiliação).</item>
        /// </list>
        /// </remarks>
        /// <param name="teamId">O ID da equipa cujo canal de notificação o utilizador deseja subscrever.</param>
        /// <param name="userId">O ID do utilizador autenticado (obtido do contexto do Hub).</param>
        /// <exception cref="ArgumentException">Lançada se a equipa, o jogador não existirem, ou o jogador não pertencer à equipa.</exception>
        public async Task ValidateTeamMembershipAsync(Guid teamId, string? userId)
        {
            Team team = await teamRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                throw new ArgumentException("The team does not exist.");
            }

            Player player = await playerRepository.GetPlayerByIdAsync(userId);

            if (player == null)
            {
                throw new ArgumentException("The player does not exist.");
            }

            Player playerExist = team.Members.FirstOrDefault(p => p.Id == player.Id);

            if (playerExist == null)
            {
                throw new ArgumentException("The player does not belong to this team.");
            }
        }
    }
}
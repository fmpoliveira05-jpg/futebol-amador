using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators
{
    /// <summary>
    /// Validador de Regras de Negócio para o ciclo de vida dos pedidos de adesão e convites de recrutamento ([MembershipRequest]).
    /// 
    /// Esta classe verifica se as entidades existem, se o jogador tem permissão (Admin), e se o estado
    /// das entidades (ex: equipa cheia, pedido duplicado) é válido antes de prosseguir com a operação.
    /// </summary>
    public class MembershipValidator : IMembershipValidator
    {
        /// <summary>
        /// Construtor padrão da classe [MembershipValidator].
        /// </summary>
        public MembershipValidator() { }

        /// <summary>
        /// Valida o pedido de adesão enviado por um Jogador para uma Equipa (Join Request).
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>Jogador e Equipa alvos devem existir.</item>
        ///     <item>O jogador não pode pertencer já a uma equipa.</item>
        ///     <item>Não deve existir um pedido pendente entre este par.</item>
        /// </list>
        /// </remarks>
        /// <param name="player">O jogador que está a enviar o pedido.</param>
        /// <param name="team">A equipa alvo.</param>
        /// <param name="existingRequest">O pedido pendente existente (se houver).</param>
        /// <exception cref="ValidationException">Se o jogador já tiver equipa, o pedido for duplicado, ou entidades não existirem.</exception>
        public void ValidateSendRequestByPlayer(Player player, Team team, MembershipRequest? existingRequest)
        {
            if (player == null)
            {
                throw new ValidationException("O jogador não existe.");
            }

            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (player.IdTeam != null)
            {
                throw new ValidationException("O jogador já pertence a uma equipa.");
            }

            if (existingRequest != null)
            {
                throw new ValidationException("Já existe um pedido de adesão entre este jogador e esta equipa.");
            }
        }

        /// <summary>
        /// Valida o pedido de convite de recrutamento enviado por uma Equipa para um Jogador (Recruitment Invite).
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>O remetente ([player]) deve ser um Administrador.</item>
        ///     <item>A equipa não pode estar cheia ([ModelConstants.TeamConst.MaxMembers]).</item>
        ///     <item>O jogador convidado não pode pertencer a outra equipa.</item>
        ///     <item>Não deve existir um pedido pendente entre este par.</item>
        /// </list>
        /// </remarks>
        /// <param name="team">A equipa remetente.</param>
        /// <param name="invitedPlayer">O jogador alvo do convite.</param>
        /// <param name="existingRequest">O pedido pendente existente (se houver).</param>
        /// <param name="player">O administrador que executa o convite.</param>
        /// <exception cref="ValidationException">Se a equipa estiver cheia, o jogador já tiver equipa, ou o remetente não for Admin.</exception>
        public void ValidateSendRequestByTeam(Team team, Player invitedPlayer, MembershipRequest? existingRequest, Player player)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (team.Members.Count == Domain.Constants.ModelConstants.TeamConst.MaxMembers)
            {
                throw new ValidationException("A equipa já está cheia.");
            }

            if (invitedPlayer == null)
            {
                throw new ValidationException("O jogador a convidar não existe.");
            }

            if (invitedPlayer.Team != null && invitedPlayer.Team != player.Team)
            {
                throw new ValidationException("O jogador convidado já pertence a outra equipa.");
            }

            if (invitedPlayer.Team == player.Team)
            {
                throw new ValidationException("O jogador convidado já pertence à equipa.");
            }

            if (!player.IsAdmin)
            {
                throw new ValidationException("O jogador não é administrador da equipa.");
            }

            if (existingRequest != null)
            {
                throw new ValidationException("Já existe um pedido de adesão entre esta equipa e este jogador.");
            }
        }

        /// <summary>
        /// Valida o pedido de aceitação (Approve) de um pedido de adesão (Join Request) pela Equipa.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Equipa não pode estar cheia, o remetente da aceitação deve ser Admin, e o jogador alvo não pode ser já membro.
        /// </remarks>
        /// <param name="team">A equipa que está a aceitar.</param>
        /// <param name="request">O pedido [MembershipRequest] a ser aprovado.</param>
        /// <param name="playerAccepting">O administrador que executa a aceitação.</param>
        /// <exception cref="ValidationException">Se a equipa estiver cheia ou o jogador não for Admin.</exception>
        public void ValidateAcceptRequestByTeam(Team team, MembershipRequest request, Player playerAccepting)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (team.Members.Count == Domain.Constants.ModelConstants.TeamConst.MaxMembers)
            {
                throw new ValidationException("A equipa já está cheia.");
            }

            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }

            if (!playerAccepting.IsAdmin)
            {
                throw new ValidationException("O jogador que tenta aceitar o pedido não é administrador da equipa.");
            }

            if (team.Members.Any(m => m.Id == request.IdPlayer))
            {
                throw new ValidationException("O jogador já é membro desta equipa.");
            }
        }

        /// <summary>
        /// Valida o pedido de rejeição (Reject) de um pedido de adesão pela Equipa.
        /// </summary>
        /// <remarks>
        /// A regra principal é que o utilizador que rejeita tem de ser um Admin e o pedido tem de existir.
        /// </remarks>
        /// <param name="team">A equipa que está a rejeitar.</param>
        /// <param name="request">O pedido [MembershipRequest] a ser rejeitado.</param>
        /// <param name="player">O administrador que executa a rejeição.</param>
        /// <exception cref="ValidationException">Se a equipa não existir, o utilizador não for Admin, ou o pedido for nulo.</exception>
        public void ValidateRejectRequestByTeam(Team team, MembershipRequest request, Player player)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (!player.IsAdmin)
            {
                throw new ValidationException("O jogador não é administrador da equipa.");
            }

            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }
        }

        /// <summary>
        /// Valida se um utilizador tem permissão para visualizar os pedidos de adesão da equipa.
        /// </summary>
        /// <remarks>
        /// Apenas administradores e membros da equipa têm permissão para ver pedidos.
        /// </remarks>
        /// <param name="team">A equipa alvo.</param>
        /// <param name="player">O jogador que está a tentar consultar a lista.</param>
        /// <exception cref="ValidationException">Se o jogador não for administrador ou não pertencer à equipa.</exception>
        public void ValidateGetRequestsByTeam(Team team, Player player)
        {
            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (player.Team != team)
            {
                throw new ValidationException("O jogador não pertence à equipa.");
            }

            if (!player.IsAdmin)
            {
                throw new ValidationException("O jogador não é administrador da equipa.");
            }
        }

        /// <summary>
        /// Valida a aceitação de um convite de recrutamento pelo Jogador.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Jogador não pode pertencer a outra equipa e o pedido deve ser destinado a ele.
        /// </remarks>
        /// <param name="player">O jogador que aceita o convite.</param>
        /// <param name="request">O convite [MembershipRequest] a ser aceite.</param>
        /// <param name="team">A equipa que enviou o convite.</param>
        /// <exception cref="ValidationException">Se o jogador já tiver equipa ou o pedido não for destinado a ele.</exception>
        public void ValidateAcceptRequestByPlayer(Player player, MembershipRequest request, Team team)
        {
            if (player == null)
            {
                throw new ValidationException("O jogador não existe.");
            }

            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }

            if (team == null)
            {
                throw new ValidationException("A equipa não existe.");
            }

            if (player.IdTeam != null)
            {
                throw new ValidationException("O jogador já pertence a uma equipa.");
            }

            if (request.IdPlayer != player.Id)
            {
                throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{request.Id}' ou pertence a outra equipa.");
            }
        }

        /// <summary>
        /// Valida a rejeição de um convite de recrutamento pelo Jogador.
        /// </summary>
        /// <remarks>
        /// A regra principal é garantir que o pedido pertence ao jogador que está a tentar rejeitá-lo.
        /// </remarks>
        /// <param name="request">O pedido [MembershipRequest] a ser rejeitado.</param>
        /// <param name="player">O jogador que rejeita o convite.</param>
        /// <exception cref="ValidationException">Se o pedido for nulo ou não pertencer ao jogador.</exception>
        public void ValidateRejectRequestByPlayer(MembershipRequest request, Player player)
        {
            if (request == null)
            {
                throw new ValidationException("O pedido de adesão não existe.");
            }

            if (request.IdPlayer != player.Id)
            {
                throw new ValidationException($"O jogador não possui um pedido de adesão com Id '{request.Id}' ou pertence a outra equipa.");
            }
        }
    }
}
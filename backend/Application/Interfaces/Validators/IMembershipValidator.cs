using Domain.Entities;

namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio para o ciclo de vida dos Pedidos de Adesão ([MembershipRequest]).
    /// 
    /// Esta interface define as regras de validação síncrona que garantem a validade da operação (criação, aceitação, rejeição)
    /// com base no papel do utilizador (Admin, Player) e no estado das entidades (Equipa cheia, pedido duplicado).
    /// </summary>
    public interface IMembershipValidator
    {
        /// <summary>
        /// Valida o pedido de adesão enviado por um Jogador para uma Equipa (Join Request).
        /// </summary>
        /// <remarks>
        /// Regras verificadas: O jogador deve ser agente livre (sem equipa), a equipa deve existir, e o pedido não pode ser duplicado.
        /// </remarks>
        /// <param name="player">O jogador que está a enviar o pedido.</param>
        /// <param name="team">A equipa alvo.</param>
        /// <param name="existingRequest">O pedido pendente existente (se houver).</param>
        void ValidateSendRequestByPlayer(Player player, Team team, MembershipRequest? existingRequest);

        /// <summary>
        /// Valida o pedido de convite de recrutamento enviado por uma Equipa para um Jogador (Recruitment Invite).
        /// </summary>
        /// <remarks>
        /// Regras verificadas: O remetente deve ser um Admin, a equipa não pode estar cheia, e o jogador convidado não pode pertencer a outra equipa.
        /// </remarks>
        /// <param name="team">A equipa remetente.</param>
        /// <param name="invitedPlayer">O jogador alvo do convite.</param>
        /// <param name="existingRequest">O pedido pendente existente (se houver).</param>
        /// <param name="player">O administrador que executa o convite.</param>
        void ValidateSendRequestByTeam(Team team, Player invitedPlayer, MembershipRequest? existingRequest, Player player);

        /// <summary>
        /// Valida a aceitação de um pedido de adesão (Join Request) pela Equipa.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: A equipa que aceita não pode estar cheia e o utilizador que aceita deve ser Admin.
        /// </remarks>
        /// <param name="team">A equipa que está a aceitar.</param>
        /// <param name="request">O pedido [MembershipRequest] a ser aprovado.</param>
        /// <param name="playerAccepting">O administrador que executa a aceitação.</param>
        void ValidateAcceptRequestByTeam(Team team, MembershipRequest request, Player playerAccepting);

        /// <summary>
        /// Valida o pedido de rejeição (Reject) de um pedido de adesão pela Equipa.
        /// </summary>
        /// <remarks>
        /// A regra principal é que o utilizador que rejeita tem de ser um Admin e o pedido tem de existir.
        /// </remarks>
        /// <param name="team">A equipa que está a rejeitar.</param>
        /// <param name="request">O pedido [MembershipRequest] a ser rejeitado.</param>
        /// <param name="player">O administrador que executa a rejeição.</param>
        void ValidateRejectRequestByTeam(Team team, MembershipRequest request, Player player);

        /// <summary>
        /// Valida se um utilizador tem permissão para visualizar os pedidos de adesão da equipa.
        /// </summary>
        /// <remarks>
        /// Requer que o jogador ([player]) seja um administrador da equipa ([team]).
        /// </remarks>
        /// <param name="team">A equipa alvo.</param>
        /// <param name="player">O jogador que está a tentar consultar a lista.</param>
        void ValidateGetRequestsByTeam(Team team, Player player);

        /// <summary>
        /// Valida a aceitação de um convite de recrutamento pelo Jogador.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Jogador não pode pertencer a outra equipa e o pedido deve ser destinado a ele.
        /// </remarks>
        /// <param name="player">O jogador que aceita o convite.</param>
        /// <param name="request">O convite [MembershipRequest] a ser aceite.</param>
        /// <param name="team">A equipa que enviou o convite.</param>
        void ValidateAcceptRequestByPlayer(Player player, MembershipRequest request, Team team);

        /// <summary>
        /// Valida a rejeição de um convite de recrutamento pelo Jogador.
        /// </summary>
        /// <remarks>
        /// A regra principal é garantir que o pedido pertence ao jogador que está a tentar rejeitá-lo.
        /// </remarks>
        /// <param name="request">O pedido [MembershipRequest] a ser rejeitado.</param>
        /// <param name="player">O jogador que rejeita o convite.</param>
        void ValidateRejectRequestByPlayer(MembershipRequest request, Player player);
    }
}
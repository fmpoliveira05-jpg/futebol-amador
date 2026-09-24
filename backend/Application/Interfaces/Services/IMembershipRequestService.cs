using Application.DTOs.Filters;
using Application.DTOs.MemberShip;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão do ciclo de vida dos Pedidos de Adesão ([MembershipRequest]).
    /// 
    /// Esta interface abstrai a lógica de negócio para a criação, consulta, aceitação e rejeição de pedidos
    /// de afiliação, gerindo a comunicação bidirecional (Jogador &lt;-> Equipa).
    /// </summary>
    public interface IMembershipRequestService
    {
        /// <summary>
        /// Envia um convite de recrutamento da Equipa para um Jogador.
        /// </summary>
        /// <remarks>
        /// A lógica interna deve garantir que o remetente ([sender]) tem permissão (Admin) e que o jogador alvo está livre.
        /// </remarks>
        /// <param name="teamId">ID da equipa que convida.</param>
        /// <param name="playerIdToInvite">ID do jogador alvo.</param>
        /// <param name="sender">ID do utilizador (Admin) que executa a ação.</param>
        /// <returns>Um DTO [MemberShipRequestDto] com os detalhes do convite criado.</returns>
        Task<MemberShipRequestDto> SendMembershipRequestTeam(Guid teamId, string playerIdToInvite, string sender);

        /// <summary>
        /// Envia um pedido de adesão (Join Request) de um Jogador para uma Equipa.
        /// </summary>
        /// <remarks>
        /// O serviço deve validar que o jogador não tem equipa e que a equipa alvo não está cheia.
        /// </remarks>
        /// <param name="playerId">ID do jogador que envia o pedido (Remetente).</param>
        /// <param name="teamId">ID da equipa alvo.</param>
        /// <returns>Um DTO [MemberShipRequestDto] com os detalhes do pedido criado.</returns>
        Task<MemberShipRequestDto> SendMembershipRequestAsyncPlayer(string playerId, Guid teamId);

        /// <summary>
        /// Aceita um pedido de adesão (Join Request) por parte de um Administrador de Equipa.
        /// </summary>
        /// <remarks>
        /// **Transação:** Remove o pedido, afilia o jogador à equipa e limpa outros pedidos pendentes desse jogador.
        /// </remarks>
        /// <param name="teamId">ID da equipa que aceita.</param>
        /// <param name="requestId">ID do pedido a aceitar.</param>
        /// <param name="adminId">ID do administrador que executa a ação.</param>
        Task AcceptMembershipRequestTeam(Guid teamId, Guid requestId, string adminId);

        /// <summary>
        /// Rejeita um pedido de adesão (Join Request) por parte de um Administrador de Equipa.
        /// </summary>
        /// <remarks>
        /// **Transação:** Remove o pedido da base de dados e notifica o jogador.
        /// </remarks>
        /// <param name="teamId">ID da equipa que rejeita.</param>
        /// <param name="requestId">ID do pedido a rejeitar.</param>
        /// <param name="userId">ID do utilizador (Admin) que executa a ação.</param>
        Task RejectMembershipRequestTeam(Guid teamId, Guid requestId, string userId);

        /// <summary>
        /// Aceita um convite de recrutamento (Recruitment Invite) enviado por uma Equipa.
        /// </summary>
        /// <remarks>
        /// **Transação:** O jogador é afiliado à equipa e todos os seus outros pedidos/convites pendentes são eliminados.
        /// </remarks>
        /// <param name="playerId">ID do jogador que aceita.</param>
        /// <param name="requestId">ID do convite a ser aceite.</param>
        /// <returns>Um DTO [MemberShipRequestDto] do pedido aceite (para confirmação e notificação).</returns>
        Task<MemberShipRequestDto> AcceptMembershipRequestAsyncPlayer(string playerId, Guid requestId);

        /// <summary>
        /// Rejeita um convite de recrutamento (Recruitment Invite) enviado por uma Equipa.
        /// </summary>
        /// <param name="playerId">ID do jogador que rejeita.</param>
        /// <param name="requestId">ID do convite a ser rejeitado.</param>
        /// <returns>Um DTO [MemberShipRequestDto] do pedido rejeitado (para confirmação e notificação).</returns>
        Task<MemberShipRequestDto> RejectMembershipRequestAsyncPlayer(string playerId, Guid requestId);

        /// <summary>
        /// Obtém a lista de pedidos de adesão (Join Requests) recebidos por uma Equipa.
        /// </summary>
        /// <remarks>
        /// **Permissões:** Deve ser chamado por um administrador da equipa.
        /// </remarks>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="playerId">ID do administrador que está a consultar.</param>
        /// <returns>Lista de DTOs dos pedidos pendentes.</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId, string playerId);

        /// <summary>
        /// Obtém a lista de pedidos de adesão recebidos por uma Equipa, aplicando filtros.
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="filters">Filtros de pesquisa.</param>
        /// <param name="playerId">ID do administrador que está a consultar.</param>
        /// <returns>Lista filtrada de DTOs dos pedidos.</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters, string playerId);

        /// <summary>
        /// Obtém a lista de Convites de Recrutamento recebidos por um Jogador (sem filtros).
        /// </summary>
        /// <param name="playerId">ID do jogador.</param>
        /// <returns>Lista de DTOs dos convites recebidos.</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayer(string playerId);

        /// <summary>
        /// Obtém a lista de Convites de Recrutamento recebidos por um Jogador, aplicando filtros.
        /// </summary>
        /// <param name="playerId">ID do jogador.</param>
        /// <param name="filters">Filtros de pesquisa.</param>
        /// <returns>Lista filtrada de DTOs dos convites.</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsyncPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters);
    }
}
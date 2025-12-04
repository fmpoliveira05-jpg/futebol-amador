using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [MembershipRequest] (Pedido/Convite de Adesão).
    /// 
    /// Define os métodos de consulta e persistência para gerir o ciclo de vida dos pedidos de afiliação,
    /// que são bidirecionais (Jogador -> Equipa ou Equipa -> Jogador).
    /// </summary>
    public interface IMembershipRequestRepository
    {
        /// <summary>
        /// Adiciona um novo pedido de adesão/convite à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="request">A entidade [MembershipRequest] a ser persistida.</param>
        public Task AddMembershipRequest(MembershipRequest request);

        /// <summary>
        /// Marca um pedido de adesão/convite existente para ser removido da base de dados.
        /// </summary>
        /// <param name="request">A entidade [MembershipRequest] a ser removida (ex: após aceitação ou rejeição).</param>
        void RemoveMembershipRequest(MembershipRequest request);

        /// <summary>
        /// Obtém um pedido de adesão/convite pelo seu identificador único (ID).
        /// </summary>
        /// <param name="id">O ID (GUID) do pedido.</param>
        /// <returns>A entidade [MembershipRequest] ou null se não for encontrada.</returns>
        Task<MembershipRequest?> GetMembershipRequestById(Guid id);

        /// <summary>
        /// Verifica se já existe um pedido de adesão ou convite pendente entre um par de Jogador/Equipa.
        /// </summary>
        /// <param name="playerId">O ID do jogador.</param>
        /// <param name="teamId">O ID da equipa.</param>
        /// <returns><c>true</c> se o pedido existir, <c>false</c> caso contrário.</returns>
        Task<bool> ExistsRequestBetweenPlayerAndTeam(string playerId, Guid teamId);

        /// <summary>
        /// Obtém um pedido de adesão/convite pendente com base nos IDs do Jogador e da Equipa.
        /// </summary>
        /// <remarks>
        /// Utilizado para evitar pedidos duplicados e para carregar a entidade antes da aceitação/rejeição.
        /// </remarks>
        /// <param name="playerId">O ID do jogador.</param>
        /// <param name="teamId">O ID da equipa.</param>
        /// <returns>A entidade [MembershipRequest] correspondente ou null.</returns>
        Task<MembershipRequest?> GetMembershipRequestByPlayerAndTeam(string playerId, Guid teamId);

        /// <summary>
        /// Obtém a lista de pedidos de adesão (Join Requests) recebidos por uma Equipa específica.
        /// </summary>
        /// <remarks>
        /// Filtra por `IdTeam` e `IsPlayerSender == true`. Projeta o resultado no DTO.
        /// </remarks>
        /// <param name="teamId">O ID da equipa que recebeu os pedidos.</param>
        /// <returns>Uma lista de [MemberShipRequestDto] com os pedidos recebidos.</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId);

        /// <summary>
        /// Obtém a lista de pedidos de adesão recebidos por uma Equipa, aplicando filtros de pesquisa.
        /// </summary>
        /// <param name="teamId">O ID da equipa que recebeu os pedidos.</param>
        /// <param name="filters">O DTO contendo os critérios de filtragem (Nome do Jogador Remetente, Intervalo de Datas).</param>
        /// <returns>Uma lista filtrada de [MemberShipRequestDto].</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters);

        /// <summary>
        /// Obtém a lista de convites de recrutamento (Recruitment Invites) recebidos por um Jogador.
        /// </summary>
        /// <remarks>
        /// Filtra por `IdPlayer` e `IsPlayerSender == false`.
        /// </remarks>
        /// <param name="playerId">O ID do jogador que recebeu os convites.</param>
        /// <returns>Uma lista de [MemberShipRequestDto] com os convites de recrutamento.</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayer(string playerId);

        /// <summary>
        /// Obtém a lista de convites de recrutamento recebidos por um Jogador, aplicando filtros.
        /// </summary>
        /// <param name="playerId">O ID do jogador recetor.</param>
        /// <param name="filters">O DTO contendo os critérios de filtragem (Nome da Equipa Remetente, Datas).</param>
        /// <returns>Uma lista filtrada de [MemberShipRequestDto].</returns>
        Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters);

        /// <summary>
        /// Remove todos os pedidos de adesão/convites associados a um jogador específico.
        /// </summary>
        /// <remarks>
        /// Utilizado no fluxo de aceitação de um pedido ou na eliminação de conta, para garantir que o jogador não tem pedidos pendentes.
        /// </remarks>
        /// <param name="playerId">O ID do jogador alvo.</param>
        Task RemoveAllMemberShipRequestsOfPlayer(string playerId);

        /// <summary>
        /// Remove todos os pedidos de adesão/convites associados a uma equipa específica.
        /// </summary>
        /// <remarks>
        /// Utilizado no fluxo de eliminação de equipa.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa alvo.</param>
        Task RemoveAllMemberShipRequestsOfTeam(Guid idTeam);
    }
}
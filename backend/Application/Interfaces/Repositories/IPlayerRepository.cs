using Application.DTOs.Filters;
using Application.DTOs.Player;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [Player] (Jogador).
    /// 
    /// Define os métodos de consulta e persistência (CRUD) para perfis de jogadores.
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Obtém uma lista de jogadores com base numa lista de IDs fornecida.
        /// </summary>
        /// <param name="playerIdList">A lista de IDs (Strings) a procurar.</param>
        /// <returns>Uma lista de entidades [Player].</returns>
        Task<List<Player>> GetPlayersListByIdListAsync(List<string> playerIdList);

        /// <summary>
        /// Obtém um jogador pelo seu identificador único (ID/UID do Firebase).
        /// </summary>
        /// <remarks>
        /// O método deve, idealmente, carregar a afiliação à equipa (Team) associada ao jogador (Eager Loading).
        /// </remarks>
        /// <param name="id">O ID (string) do jogador.</param>
        /// <returns>A entidade [Player] ou null se não for encontrada.</returns>
        Task<Player?> GetPlayerByIdAsync(string id);

        /// <summary>
        /// Obtém um jogador pelo seu endereço de e-mail de forma assíncrona.
        /// </summary>
        /// <param name="email">O endereço de e-mail do jogador.</param>
        /// <returns>A entidade [Player] ou null.</returns>
        Task<Player?> GetPlayerByEmailAsync(string email);

        /// <summary>
        /// Obtém um jogador pelo seu número de telefone de forma assíncrona.
        /// </summary>
        /// <param name="phoneNumber">O número de telefone completo do jogador.</param>
        /// <returns>A entidade [Player] ou null.</returns>
        Task<Player?> GetPlayerByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// Obtém um pedido de adesão específico (Membership Request) associado a um jogador, carregando as relações necessárias.
        /// </summary>
        /// <remarks>
        /// Consulta a tabela [MembershipRequest] e carrega a afiliação à equipa.
        /// </remarks>
        /// <param name="playerId">O ID do jogador alvo.</param>
        /// <returns>A entidade [MembershipRequest] correspondente, ou null.</returns>
        Task<MembershipRequest?> GetPlayerByIdWithRequestsAsync(string playerId);

        /// <summary>
        /// Obtém uma lista de jogadores para pesquisa de mercado, aplicando filtros dinâmicos.
        /// </summary>
        /// <remarks>
        /// A consulta deve calcular a idade do jogador no lado do servidor e projetar o resultado no DTO [InfoPlayerDto].
        /// </remarks>
        /// <param name="filters">O DTO com os critérios de filtragem (Nome, Idade, Altura, Posição).</param>
        /// <returns>Uma lista de objetos [InfoPlayerDto] para o frontend.</returns>
        Task<List<InfoPlayerDto?>> GetPlayersList(FilterTeamDto? filters);

        /// <summary>
        /// Adiciona um novo jogador à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="player">A entidade [Player] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        Task AddAsync(Player player);

        /// <summary>
        /// Marca uma entidade [Player] existente para ser atualizada na base de dados.
        /// </summary>
        /// <param name="updatedPlayer">A entidade [Player] com os novos valores.</param>
        void UpdatePlayer(Player updatedPlayer);

        /// <summary>
        /// Marca um jogador existente para ser removido da base de dados (deleção).
        /// </summary>
        /// <param name="playerToRemove">A entidade [Player] a ser removida.</param>
        void DeletePlayer(Player playerToRemove);

        Task<List<string?>> GetDeviceTokensMembersTeam(Guid idTeam, bool? isAdmin);
    }
}
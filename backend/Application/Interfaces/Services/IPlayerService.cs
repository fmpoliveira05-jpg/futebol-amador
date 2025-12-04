using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão de Perfis de Jogador.
    /// 
    /// Esta interface define os métodos de lógica de negócio para o ciclo de vida de um jogador,
    /// desde o registo inicial até à afiliação e saída de equipas.
    /// </summary>
    public interface IPlayerService
    {

        /// <summary>
        /// Cria e regista um novo perfil de jogador na plataforma.
        /// </summary>
        /// <remarks>
        /// Esta operação envolve a sincronização da criação do utilizador no fornecedor de identidade (Firebase) e a persistência do perfil na base de dados relacional.
        /// </remarks>
        /// <param name="playerDTO">DTO com os dados do jogador a criar.</param>
        /// <returns>Uma tarefa assíncrona que retorna o ID (string) do novo jogador.</returns>
        Task<string> CreatePlayerAsync(CreatePlayerDto playerDTO);

        /// <summary>
        /// Obtém os dados detalhados do perfil de um jogador pelo ID.
        /// </summary>
        /// <param name="playerId">ID do jogador.</param>
        /// <returns>O DTO [PlayerDetailsDto] com os detalhes do jogador.</returns>
        Task<PlayerDetailsDto> GetPlayerByIdAsync(string playerId);

        /// <summary>
        /// Atualiza as informações básicas de um jogador.
        /// </summary>
        /// <remarks>
        /// A atualização deve sincronizar as alterações de Email ou Telefone com o fornecedor de identidade (Firebase) se necessário.
        /// </remarks>
        /// <param name="playerId">ID do jogador a atualizar.</param>
        /// <param name="dto">DTO com os novos dados.</param>
        /// <returns>O DTO [UpdatePlayerDto] atualizado.</returns>
        Task<UpdatePlayerDto> UpdatePlayerAsync(string playerId, UpdatePlayerDto dto);

        /// <summary>
        /// Elimina um perfil de jogador permanentemente.
        /// </summary>
        /// <remarks>
        /// Esta operação deve ser transacional, eliminando o jogador da base de dados e do fornecedor de identidade.
        /// </remarks>
        /// <param name="playerId">ID do jogador a eliminar.</param>
        Task DeletePlayerAsync(string playerId);

        /// <summary>
        /// Remove a afiliação de um jogador da sua equipa atual.
        /// </summary>
        /// <remarks>
        /// Se o jogador for um Administrador, a lógica interna deve gerir a transferência de privilégios ou a eliminação da equipa se for o último membro.
        /// </remarks>
        /// <param name="playerId">ID do jogador que está a sair.</param>
        /// <returns>DTO [InfoPlayerDto] do jogador agora como agente livre.</returns>
        Task<InfoPlayerDto> LeaveTeam(string playerId);

        /// <summary>
        /// Lista todos os jogadores, aplicando filtros de pesquisa.
        /// </summary>
        /// <remarks>
        /// Utilizado para a funcionalidade de "Mercado de Jogadores".
        /// </remarks>
        /// <param name="filter">Filtros de pesquisa (Nome, Posição, Idade, Altura, etc.).</param>
        /// <returns>Lista de [InfoPlayerDto] (visão resumida).</returns>
        Task<List<InfoPlayerDto?>> ListPlayers(FilterTeamDto? filter);

        /// <summary>
        /// Obtém a lista completa de equipas elegíveis (com vagas) para que o jogador possa interagir (desafiar ou candidatar-se).
        /// </summary>
        /// <returns>Lista de [InfoTeamsDto] com estatísticas resumidas das equipas.</returns>
        Task<List<InfoTeamsDto>> GetListTeams();

        /// <summary>
        /// Obtém a lista de equipas elegíveis, aplicando filtros dinâmicos de pesquisa.
        /// </summary>
        /// <param name="filter">Filtros de pesquisa (Nome, Rank, Pontos, etc.).</param>
        /// <returns>Lista de [InfoTeamsDto] filtrados.</returns>
        Task<List<InfoTeamsDto>> GetTeamListWithFilters(FilterListTeamDto filter);
    }
}
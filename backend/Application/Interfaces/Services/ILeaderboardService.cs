using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão e consulta da Tabela de Classificação (Leaderboard).
    /// 
    /// Esta interface define a operação central para obter o ranking das equipas.
    /// </summary>
    public interface ILeaderboardService
    {
        /// <summary>
        /// Obtém a tabela de classificação global das equipas, ordenada por pontuação.
        /// </summary>
        /// <returns>Uma tarefa assíncrona que retorna uma lista de [TeamLeaderboardDto] com os dados do ranking e a posição (calculada na camada de serviço).</returns>
        Task<List<TeamLeaderboardDto>> GetLeaderboardAsync();
    }
}
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela gestão da Tabela de Classificação (Leaderboard) global.
    /// 
    /// Esta classe é responsável por buscar os dados do ranking, calcular a posição final de cada equipa
    /// e formatar o resultado para a camada de apresentação.
    /// </summary>
    public class LeaderboardService : ILeaderboardService
    {
        /// <summary>
        /// Repositório de Equipas, utilizado para obter os dados do ranking ordenados por pontuação.
        /// </summary>
        private readonly ITeamRepository teamRepository;

        /// <summary>
        /// Construtor do LeaderboardService.
        /// </summary>
        /// <param name="teamRepository">O Repositório de Equipas injetado via DI.</param>
        public LeaderboardService(ITeamRepository teamRepository)
        {
            this.teamRepository = teamRepository;
        }

        /// <summary>
        /// Obtém a tabela de classificação global (Leaderboard) das equipas.
        /// </summary>
        /// <remarks>
        /// 1. Solicita o Top 100 de equipas ordenadas por pontuação ao repositório.
        /// 2. Utiliza LINQ e o método <c>Select</c> com o índice (index) para calcular a posição final
        ///    da equipa (Posição = índice + 1).
        /// </remarks>
        /// <returns>Uma lista de [TeamLeaderboardDto] contendo a posição, nome, pontos e rank de cada equipa.</returns>
        public async Task<List<TeamLeaderboardDto>> GetLeaderboardAsync()
        {
            const int top = 100;
            var teams = await teamRepository.GetTopTeamsAsync(top);

            return teams
                .Select((t, index) => new TeamLeaderboardDto
                {
                    Position = index + 1,
                    TeamName = t.TeamName,
                    CurrentPoints = t.CurrentPoints,
                    RankName = t.RankName
                })
                .ToList();
        }
    }
}
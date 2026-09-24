using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência de dados e consulta da entidade [TeamStatistics].
    /// 
    /// Esta classe implementa o contrato [ITeamStatisticsRepository] e utiliza o [AmateurFootballContext]
    /// para gerir o registo de estatísticas de equipas por partida (ex: número de golos).
    /// </summary>
    public class TeamStatisticsRepository : ITeamStatisticsRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder à tabela de estatísticas.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [TeamStatisticsRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public TeamStatisticsRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona um novo registo de estatísticas de equipa à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="teamStatistics">A entidade [TeamStatistics] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddTeamStatistics(TeamStatistics teamStatistics)
        {
            await context.TeamStatistics.AddAsync(teamStatistics);
        }

        /// <summary>
        /// Obtém o registo de estatísticas de uma equipa numa partida específica.
        /// </summary>
        /// <remarks>
        /// Utiliza Eager Loading (`.Include`) para carregar a entidade de navegação [Team] associada.
        /// </remarks>
        /// <param name="idMatch">O ID (GUID) da partida.</param>
        /// <param name="idTeam">O ID (GUID) da equipa cujas estatísticas se pretende.</param>
        /// <returns>A entidade [TeamStatistics] correspondente à combinação da partida e da equipa, ou null.</returns>
        public async Task<TeamStatistics> GetTeamByIdAndMatch(Guid idMatch, Guid idTeam)
        {
            var query = await context.TeamStatistics
                .Include(T => T.Team)
                .FirstOrDefaultAsync(ts => ts.MatchesId == idMatch
                                    && ts.IdTeam == idTeam);
            return query;
        }
    }
}
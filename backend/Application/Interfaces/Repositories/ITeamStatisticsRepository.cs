using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [TeamStatistics] (Estatísticas por Equipa).
    /// 
    /// Define os métodos de consulta e persistência para gerir o registo de desempenho e resultados de uma equipa numa partida.
    /// </summary>
    public interface ITeamStatisticsRepository
    {
        /// <summary>
        /// Adiciona um novo registo de estatísticas de equipa à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="teamStatistics">A entidade [TeamStatistics] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        Task AddTeamStatistics(TeamStatistics teamStatistics);

        /// <summary>
        /// Obtém o registo de estatísticas de uma equipa numa partida específica.
        /// </summary>
        /// <remarks>
        /// Esta consulta é utilizada para obter os golos, o resultado e o contexto da equipa em relação a um jogo em particular.
        /// </remarks>
        /// <param name="idMatch">O ID (GUID) da partida.</param>
        /// <param name="idTeam">O ID (GUID) da equipa cujas estatísticas se pretende.</param>
        /// <returns>A entidade [TeamStatistics] correspondente à combinação da partida e da equipa, ou null.</returns>
        Task<TeamStatistics> GetTeamByIdAndMatch(Guid idMatch, Guid idTeam);
    }
}
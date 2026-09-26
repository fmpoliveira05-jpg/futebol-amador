using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>Acesso a ligas, épocas, inscrições e títulos.</summary>
    public interface ICompetitionRepository
    {
        Task<List<League>> GetLeaguesAsync();
        Task<Dictionary<Guid, int>> CountTeamsByLeagueAsync();
        Task<League?> GetLeagueAsync(Guid id);
        Task<League?> GetTopLeagueAsync();
        Task<bool> LevelExistsAsync(int level);

        /// <summary>Liga imediatamente acima (nível menor) ou abaixo (nível maior).</summary>
        Task<League?> GetAdjacentLeagueAsync(int level, bool above);

        Task AddLeagueAsync(League league);
        Task AddSeasonAsync(Season season);
        Task AddSeasonTeamAsync(SeasonTeam seasonTeam);

        /// <summary>Época com a liga e as equipas inscritas (com o campo de cada uma).</summary>
        Task<Season?> GetSeasonAsync(Guid id);

        /// <summary>Épocas de uma liga, da mais recente para a mais antiga.</summary>
        Task<List<Season>> GetSeasonsOfLeagueAsync(Guid leagueId);

        /// <summary>Época por terminar (inscrições ou a decorrer) de uma liga.</summary>
        Task<Season?> GetOpenSeasonAsync(Guid leagueId);

        Task<List<Season>> GetSeasonsToStartAsync(DateTime until);
        Task<List<Season>> GetSeasonsToCloseAsync(DateTime now);

        /// <summary>Jogos da época, com as estatísticas e as equipas.</summary>
        Task<List<Matches>> GetSeasonMatchesAsync(Guid seasonId);

        Task<List<Team>> GetTeamsOfLeagueAsync(Guid leagueId);
        Task<Team?> GetTeamWithPitchAsync(Guid teamId);

        /// <summary>Inscrição da equipa numa época ainda não terminada (de qualquer liga).</summary>
        Task<List<SeasonTeam>> GetOpenRegistrationsOfTeamAsync(Guid teamId);

        void RemoveSeasonTeam(SeasonTeam seasonTeam);
        Task AddTitleAsync(TeamTitle title);
        Task<List<TeamTitle>> GetTitlesAsync(Guid teamId);
        Task<bool> IsSuperAdminAsync(string? userId);
        Task AddMatchesAsync(IEnumerable<Matches> matches);
    }
}

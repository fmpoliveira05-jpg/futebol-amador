using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>Acesso aos onzes e aos eventos dos jogos.</summary>
    public interface IMatchDetailsRepository
    {
        /// <summary>Jogo com as estatísticas, as equipas (com os membros), o campo e a época.</summary>
        Task<Matches?> GetMatchWithTeamsAsync(Guid matchId);

        Task<MatchLineup?> GetLineupAsync(Guid matchId, Guid teamId);
        Task<List<MatchLineup>> GetLineupsOfMatchAsync(Guid matchId);

        /// <summary>Último onze da equipa num jogo anterior a <paramref name="before"/>.</summary>
        Task<MatchLineup?> GetLastLineupAsync(Guid teamId, DateTime before);

        Task AddLineupAsync(MatchLineup lineup);
        void RemoveSlots(IEnumerable<LineupSlot> slots);

        /// <summary>Jogos marcados entre as duas datas (inclusive), com as estatísticas das equipas.</summary>
        Task<List<Matches>> GetScheduledMatchesBetweenAsync(DateTime from, DateTime until);

        Task<List<MatchEvent>> GetEventsOfMatchAsync(Guid matchId);
        Task AddEventsAsync(IEnumerable<MatchEvent> events);
        void RemoveEvents(IEnumerable<MatchEvent> events);

        /// <summary>
        /// Dados para as estatísticas de um jogador: jogos terminados em que esteve no onze ou nos eventos,
        /// os eventos desses jogos e as entradas do jogador nos onzes.
        /// </summary>
        Task<(List<Matches> Matches, List<MatchEvent> Events, List<LineupSlot> Slots)> GetPlayerMatchDataAsync(string playerId);

        Task<Dictionary<string, string>> GetPlayerNamesAsync(IEnumerable<string> ids);
    }
}

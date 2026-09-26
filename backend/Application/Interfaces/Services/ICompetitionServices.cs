using Application.DTOs.Competition;

namespace Application.Interfaces.Services
{
    /// <summary>Ligas, épocas, classificação, sorteio e fecho das épocas.</summary>
    public interface ILeagueService
    {
        Task<List<LeagueDto>> GetLeaguesAsync();
        Task<StandingsDto?> GetStandingsAsync(Guid? leagueId, Guid? seasonId);
        Task<List<FixtureRoundDto>> GetFixturesAsync(Guid seasonId);
        Task<List<TeamTitleDto>> GetTitlesAsync(Guid teamId);
        Task<LeagueDto> CreateLeagueAsync(string? userId, CreateLeagueDto dto);
        Task<SeasonDto> CreateSeasonAsync(string? userId, Guid leagueId, CreateSeasonDto dto);
        Task<SeasonDto> RegisterTeamAsync(string? userId, Guid leagueId, Guid teamId);
        Task<SeasonDto> StartSeasonAsync(string? userId, Guid seasonId, StartSeasonDto dto);
        Task<SeasonDto> CloseSeasonAsync(string? userId, Guid seasonId);

        /// <summary>Tarefa periódica: sorteia as épocas que começam em breve e fecha as que acabaram.</summary>
        Task<int> RunScheduledTasksAsync(CancellationToken ct = default);
    }

    /// <summary>Mercado, listagens e propostas de transferência.</summary>
    public interface ITransferService
    {
        Task<List<MarketPlayerDto>> GetMarketAsync(string? userId, Guid teamId, MarketFilterDto filter);
        Task ListPlayerAsync(string? userId, Guid teamId, string playerId);
        Task UnlistPlayerAsync(string? userId, Guid teamId, string playerId);
        Task<TransferOfferDto> CreateOfferAsync(string? userId, CreateTransferOfferDto dto);
        Task<TeamTransferOffersDto> GetTeamOffersAsync(string? userId, Guid teamId);
        Task<List<TransferOfferDto>> GetPlayerOffersAsync(string? userId);
        Task<TransferOfferDto> AcceptOfferAsync(string? userId, Guid offerId);
        Task<TransferOfferDto> RejectOfferAsync(string? userId, Guid offerId);
    }

    /// <summary>Onze inicial, preenchimento automático, eventos e relatório do jogo.</summary>
    public interface IMatchDetailsService
    {
        List<FormationDto> GetFormations();
        Task<LineupDto> GetLineupAsync(string? userId, Guid matchId, Guid teamId);
        Task<LineupDto> SaveLineupAsync(string? userId, Guid matchId, Guid teamId, SaveLineupDto dto);

        /// <summary>Preenche os onzes em falta dos jogos cujo prazo já passou. Devolve quantos preencheu.</summary>
        Task<int> AutoFillMissingLineupsAsync(CancellationToken ct = default);

        /// <summary>Valida os eventos que um administrador submete para a sua equipa (lança exceção se forem inválidos).</summary>
        Task ValidateEventsAsync(Guid matchId, Guid teamId, int teamGoals, MatchEventsDto? events);

        /// <summary>
        /// Prepara a gravação dos eventos das equipas quando o resultado fica confirmado (substitui os anteriores
        /// dessas equipas). Não grava: as alterações entram na próxima gravação da unidade de trabalho.
        /// </summary>
        Task ApplyEventsAsync(Guid matchId, IReadOnlyDictionary<Guid, MatchEventsDto?> eventsByTeam);

        Task<MatchReportDto> GetReportAsync(Guid matchId);
    }

    /// <summary>Perfil completo do jogador (estatísticas, histórico e transferências).</summary>
    public interface IPlayerProfileService
    {
        Task<PlayerProfileDto> GetProfileAsync(string playerId);
    }
}

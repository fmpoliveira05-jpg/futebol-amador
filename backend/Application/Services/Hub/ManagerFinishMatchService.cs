using Application.DTOs.Match;
using Application.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Services.Hub
{
    /// <summary>
    /// Serviço de gestão de estado para o Hub de Finalização de Partida ([FinishMatchHub]).
    /// 
    /// Responsável por:
    /// 1. Coordenar a submissão de resultados finais pelos administradores das equipas.
    /// 2. Manter o estado temporário dos resultados submetidos em memória (Cache).
    /// 3. Verificar se os resultados submetidos por ambas as equipas coincidem.
    /// 4. Se coincidirem, finalizar a partida, definir o vencedor e atualizar os pontos/ranks das equipas.
    /// </summary>
    public class ManagerFinishMatchService : IManagerFinishMatchService
    {
        #region Initialization
        private readonly IMatchRepository matchRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IFinishMatchValidator validator;
        private readonly IGeralHubValidator geralValidator;
        private readonly IMemoryCache cache;
        private readonly ILogger<ManagerFinishMatchService> _logger; 

        /// <summary>
        /// Construtor do ManagerFinishMatchService.
        /// </summary>
        /// <param name="matchRepository">Repositório de partidas (para carregar e atualizar o jogo).</param>
        /// <param name="unityOfWork">Unidade de trabalho para persistir o resultado final.</param>
        /// <param name="validator">Validador de regras de negócio específicas de finalização.</param>
        /// <param name="geralValidator">Validador genérico de Hubs.</param>
        /// <param name="cache">Cache em memória para armazenar os resultados pendentes de validação.</param>
        public ManagerFinishMatchService(IMatchRepository matchRepository, IUnityOfWork unityOfWork, IFinishMatchValidator validator, 
            IGeralHubValidator geralValidator, IMemoryCache cache, ILogger<ManagerFinishMatchService> logger)
        {
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
            this.validator = validator;
            this.geralValidator = geralValidator;
            this.cache = cache;
            this._logger = logger;
        }
        #endregion

        #region public Methods

        /// <summary>
        /// Regista a submissão de um resultado por uma equipa no Hub de Finalização.
        /// </summary>
        /// <remarks>
        /// **Fluxo de Execução:**
        /// 1. Valida a entrada (IDs, Golos não negativos, Admin correto).
        /// 2. Obtém ou cria o estado do lobby no Cache.
        /// 3. Adiciona o resultado da equipa ao lobby.
        /// 4. Se a outra equipa já submeteu o resultado, verifica se coincidem ([FinalizeMatchIfResultsMatch]).
        /// 5. Retorna o estado atual (se o jogo terminou ou se é necessário esperar).
        /// </remarks>
        /// <param name="matchId">ID da partida.</param>
        /// <param name="finishMatch">DTO com os golos submetidos.</param>
        /// <param name="userId">ID do administrador que submete.</param>
        /// <param name="connectionId">ID da conexão SignalR.</param>
        /// <returns>Objeto [JoinFinishMatch] com o estado da finalização.</returns>
        public async Task<JoinFinishMatch> JoinHubAsync(Guid matchId, ResultMatchDto finishMatch, string userId, string connectionId)
        {
            _logger.LogInformation($"[JoinHubAsync] User {userId} entrou. Match: {matchId}. Goals: {finishMatch.NumGoalsTeam}-{finishMatch.NumGoalsOpponent}");

            validator.ValidateVariableJoinMatch(matchId, finishMatch, userId, connectionId);

            EntryHubFinishMatch entry;
            var match = await matchRepository.GetMatchForFinishMatch(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts =>
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin)
            );

            var teamId = teamMatchAdmin.IdTeam;
            var hubCacheKey = GetHubCacheKey(matchId);

            _logger.LogInformation($"[JoinHubAsync] TeamID identificada: {teamId}");

            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub))
            {
                _logger.LogInformation("[JoinHubAsync] Cache vazia. Criando novo dicionário.");
                hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            }
            else
            {
                _logger.LogInformation($"[JoinHubAsync] Cache encontrada. Jogadores no lobby: {hub.Count}");
            }

            validator.ValidateJoinMatch(teamMatchAdmin, teamId, hub);

            var result = new JoinFinishMatch
            {
                IdTeam = teamId,
                ResultMatch = finishMatch,
            };

            if (hub.Count == 0)
            {
                _logger.LogInformation("[JoinHubAsync] É o primeiro Admin a entrar.");
                result.IsFirstAdmin = true;
                result.MatchFinish = false;
            }
            else
            {
                _logger.LogInformation("[JoinHubAsync] Segundo Admin entrou. Tentando finalizar...");
                result.IsFirstAdmin = false;
                result.MatchFinish = true;

                result.FirstAdminConnectionId = hub.First().Value.ConnectionId;
                await FinalizeMatchIfResultsMatch(hub, match, teamMatchAdmin, result, hubCacheKey, finishMatch);
            }

            //Só guarda uma team em cache se o resultado das partidas não coincidirem
            if (!result.IsCoincides.GetValueOrDefault(false))
            {
                _logger.LogInformation("[JoinHubAsync] Resultados não coincidem ou aguardando oponente. Guardando em cache.");
                entry = new EntryHubFinishMatch
                {
                    ConnectionId = connectionId,
                    Result = result
                };

                //Guardo em cache o id da equipa e a sua entrada
                hub.TryAdd(teamId, entry);
                cache.Set(hubCacheKey, hub, GetCacheOptions());
            }
            else
            {
                _logger.LogInformation("[JoinHubAsync] Jogo finalizado com sucesso! Cache será limpa.");
            }

            return result;
        }

        /// <summary>
        /// Atualiza um resultado previamente submetido (correção de erro).
        /// </summary>
        /// <remarks>
        /// Permite que um administrador altere o número de golos se a validação inicial tiver falhado (resultados divergentes).
        /// Após a atualização, tenta novamente finalizar a partida se os resultados agora coincidirem.
        /// </remarks>
        /// <param name="matchId">ID da partida.</param>
        /// <param name="finishMatch">Novo DTO com os golos corrigidos.</param>
        /// <param name="userId">ID do administrador.</param>
        /// <param name="connectionId">ID da conexão.</param>
        /// <returns>Objeto [JoinFinishMatch] atualizado.</returns>
        public async Task<JoinFinishMatch> UpdateResult(Guid matchId, ResultMatchDto finishMatch, string userId, string connectionId)
        {
            validator.ValidateVariableJoinMatch(matchId, finishMatch, userId, connectionId);

            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts =>
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin)
            );

            var teamId = teamMatchAdmin.IdTeam;

            var hubCacheKey = GetHubCacheKey(matchId);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub))
            {
                hub = new ConcurrentDictionary<Guid, EntryHubFinishMatch>();
            }

            validator.ValidateUpdateResult(teamMatchAdmin, teamId, hub);

            var result = new JoinFinishMatch
            {
                IdTeam = teamId,
                ResultMatch = finishMatch,
            };

            //Atualizar o resultado do hub
            hub[teamId].Result.ResultMatch = finishMatch;
            cache.Set(hubCacheKey, hub);

            await FinalizeMatchIfResultsMatch(hub, match, teamMatchAdmin, result, hubCacheKey, finishMatch);

            return result;
        }

        /// <summary>
        /// Remove um administrador do lobby de finalização (Cancelamento/Saída).
        /// </summary>
        /// <returns><c>true</c> se a remoção foi bem-sucedida.</returns>
        public async Task<bool> LeaveHubAsync(Guid matchId, Guid teamId, string connectionId)
        {
            geralValidator.ValidateIdMatchLeaveMatch(matchId, teamId);
            var hubCacheKey = GetHubCacheKey(matchId);

            if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub))
            {
                if (hub.TryRemove(teamId, out _))
                {
                    if (hub.IsEmpty)
                    {
                        cache.Remove(hubCacheKey);
                    }
                    else
                    {
                        cache.Set(hubCacheKey, hub, GetCacheOptions());
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Trata a desconexão abrupta de um cliente.
        /// </summary>
        public async Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId)
        {
            if (!maybeMatchId.HasValue || !maybeTeamId.HasValue)
            {
                return false;
            }

            return await LeaveHubAsync(maybeMatchId.Value, maybeTeamId.Value, connectionId);
        }
        #endregion

        #region private Methods

        /// <summary>
        /// Gera a chave única de cache (Key) para o lobby de finalização de uma partida.
        /// </summary>
        /// <remarks>
        /// Esta chave é usada para armazenar o estado de submissão de resultados ([ConcurrentDictionary]) 
        /// na cache em memória ([IMemoryCache]).
        /// </remarks>
        /// <param name="matchId">O ID da partida (GUID).</param>
        /// <returns>Uma string única no formato "PrefixHubCache-{MatchId}".</returns>
        private static string GetHubCacheKey(Guid matchId)
        {
            return ModelConstants.FinishMatchHubConst.PrefixHubCache + matchId;
        }

        /// <summary>
        /// Configura as opções de expiração e políticas de entrada da cache em memória.
        /// </summary>
        /// <remarks>
        /// Define a expiração absoluta do item na cache para **10 minutos**, garantindo que os dados
        /// pendentes (resultados divergentes) não fiquem na memória por tempo indefinido.
        /// </remarks>
        /// <returns>Um objeto [MemoryCacheEntryOptions] configurado.</returns>
        private static MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        }

        /// <summary>
        /// Compara dois DTOs de resultado para verificar se são consistentes.
        /// </summary>
        /// <remarks>
        /// Verifica se: Golo(Equipa A) segundo A == Golo(Equipa A) segundo B, e vice-versa.
        /// </remarks>
        /// <returns><c>true</c> se os resultados coincidirem.</returns>
        private bool CoincideResults(ResultMatchDto firstResult, ResultMatchDto secondResult)
        {
            _logger.LogInformation($"[CoincideResults] Comparando: \n" +
                                   $"Res1 (Cache): Team {firstResult.NumGoalsTeam} - Opp {firstResult.NumGoalsOpponent} (TeamID: {firstResult.IdTeam})\n" +
                                   $"Res2 (Atual): Team {secondResult.NumGoalsTeam} - Opp {secondResult.NumGoalsOpponent} (TeamID: {secondResult.IdTeam})");

            var coincide = false;
            validator.ValidateMatchResultTwoTeams(firstResult, secondResult);

            if (firstResult.NumGoalsTeam == secondResult.NumGoalsOpponent &&
                firstResult.NumGoalsOpponent == secondResult.NumGoalsTeam)
            {
                coincide = true;
            }

            _logger.LogInformation($"[CoincideResults] Resultado: {coincide}");
            return coincide;
        }

        /// <summary>
        /// Lógica central de finalização: Verifica coincidência e persiste o resultado.
        /// </summary>
        /// <remarks>
        /// 1. Obtém o resultado da equipa adversária do cache.
        /// 2. Compara com o resultado atual ([CoincideResults]).
        /// 3. Se coincidirem:
        ///     - Atualiza os golos nas estatísticas.
        ///     - Determina o vencedor ([DefineWinnerMatch]).
        ///     - Marca o jogo como [DONE].
        ///     - Persiste na BD e limpa o cache.
        /// </remarks>
        private async Task FinalizeMatchIfResultsMatch(ConcurrentDictionary<Guid, EntryHubFinishMatch>? hub,
            Matches match, TeamStatistics team, JoinFinishMatch result, string hubCacheKey, ResultMatchDto finishMatch)
        {
            var opponententry = hub?.FirstOrDefault(kvp => kvp.Key != team.IdTeam).Value;

            if (opponententry == null)
            {
                _logger.LogWarning($"[FinalizeMatch] AVISO CRÍTICO: Não foi encontrada a entrada do oponente na cache! (Hub Count: {hub?.Count}, Meu TeamId: {team.IdTeam})");
                result.IsCoincides = false;
                return;
            }

            var opponentResult = opponententry.Result.ResultMatch;

            if (opponentResult == null)
            {
                _logger.LogWarning("[FinalizeMatch] Oponente encontrado mas sem ResultMatchDto.");
                result.IsCoincides = false;
                return;
            }

            var coincideResult = CoincideResults(opponentResult, finishMatch);

            if (coincideResult)
            {
                var opponent = match.Teams.FirstOrDefault(ts => ts.IdTeam == finishMatch.IdOpponent);
                validator.ValidateOpponentTeam(opponent);

                //Atualização do numero de golos das equipas
                team.NumGoals = finishMatch.NumGoalsTeam;
                opponent.NumGoals = finishMatch.NumGoalsOpponent;

                DefineWinnerMatch(team, opponent);
                match.MatchStatus = MatchStatus.DONE;

                await unityOfWork.SaveChangesAsync();
                result.IsCoincides = true;

                cache.Remove(hubCacheKey); 
                _logger.LogInformation("[FinalizeMatch] BD atualizada e Cache limpa.");
            }
        }

        /// <summary>
        /// Define o resultado da partida (Vitória/Derrota/Empate) e atualiza os pontos.
        /// </summary>
        private static void DefineWinnerMatch(TeamStatistics team, TeamStatistics opponent)
        {
            var numGoalsTeam = team.NumGoals;
            var numGoalsOpponent = opponent.NumGoals;

            if (numGoalsTeam > numGoalsOpponent)
            {
                team.MatchResult = MatchResult.WIN;
                opponent.MatchResult = MatchResult.LOSE;
            }
            else if (numGoalsTeam < numGoalsOpponent)
            {
                team.MatchResult = MatchResult.LOSE;
                opponent.MatchResult = MatchResult.WIN;
            }
            else
            {
                team.MatchResult = MatchResult.DRAW;
                opponent.MatchResult = MatchResult.DRAW;
            }

            updatePointsTeams(team);
            updatePointsTeams(opponent);
        }

        /// <summary>
        /// Atualiza os pontos da equipa com base no resultado e verifica Promoção/Despromoção.
        /// </summary>
        private static void updatePointsTeams(TeamStatistics teamStatistic)
        {
            var team = teamStatistic.Team;
            var rank = team.Rank;

            switch (teamStatistic.MatchResult)
            {
                case MatchResult.WIN:
                    {
                        team.CurrentPoints += rank.WinPoints;
                        break;
                    }
                case MatchResult.DRAW:
                    {
                        team.CurrentPoints += rank.DrawPoints;
                        break;
                    }
                case MatchResult.LOSE:
                    {
                        team.CurrentPoints += rank.LosePoints;
                        break;
                    }
            }

            ValidatePromotionOrDepromotionTeam(team);
        }

        /// <summary>
        /// Verifica se a equipa deve subir ou descer de Rank com base nos novos pontos.
        /// </summary>
        private static void ValidatePromotionOrDepromotionTeam(Team team)
        {
            var nextRank = team.Rank.NextRank;
            var previousRank = team.Rank.PreviousRank;

            if (nextRank != null && team.CurrentPoints >= team.Rank.PointsToPromotion)
            {
                team.Rank = nextRank;
            }
            else if (previousRank != null && team.CurrentPoints < previousRank.PointsToPromotion)
            {
                team.Rank = previousRank;
            }
        }

        #endregion
    }
}
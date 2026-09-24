using Application.DTOs.Rank;
using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Application.Services.Hub
{
    /// <summary>
    /// Serviço de gestão de estado para o Hub de Matchmaker Ranqueado ([RankMatchMakerHub]).
    /// 
    /// Responsável por:
    /// 1. Processar a entrada de equipas no lobby de procura.
    /// 2. Calcular métricas avançadas (Idade média, Cidade, Rank potencial).
    /// 3. Invocar o algoritmo de matchmaking para tentar encontrar par imediato.
    /// 4. Manter o estado dos lobbies ativos em memória (Cache) para processamento em background.
    /// </summary>
    public class ManagerRankMatchMakerService: IManagerRankMatchMakerService
    {
        private readonly IMatchMakerService serviceMatchMaker;
        private readonly ITeamRepository teamRepository;
        private readonly IMatchRepository matchRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IRankMatchMakerValidator validator;
        private readonly IMemoryCache cache;

        /// <summary>
        /// Construtor do ManagerRankMatchMakerService.
        /// </summary>
        /// <param name="serviceMatchMaker">Serviço de domínio puro com a lógica algorítmica de emparelhamento.</param>
        /// <param name="teamRepository">Repositório de equipas.</param>
        /// <param name="matchRepository">Repositório de partidas.</param>
        /// <param name="unityOfWork">Unidade de trabalho para persistência.</param>
        /// <param name="validator">Validador de regras de negócio para entrada no matchmaking.</param>
        /// <param name="cache">Cache em memória para gestão de estado do lobby.</param>
        public ManagerRankMatchMakerService(IMatchMakerService serviceMatchMaker, ITeamRepository teamRepository,
            IMatchRepository matchRepository, IUnityOfWork unityOfWork, IRankMatchMakerValidator validator, 
            IMemoryCache cache)
        {
            this.serviceMatchMaker = serviceMatchMaker;
            this.teamRepository = teamRepository;
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
            this.validator = validator;
            this.cache = cache;
        }

        /// <summary>
        /// Regista a entrada de uma equipa no sistema de Matchmaking Ranqueado.
        /// </summary>
        /// <remarks>
        /// **Fluxo de Execução:**
        /// 1. Valida os parâmetros de entrada e verifica conflitos de horário (12h).
        /// 2. Carrega e valida a equipa (membros suficientes, admin presente).
        /// 3. Calcula as métricas de emparelhamento: Idade Média, Cidade e Rank (incluindo Rank Adjacente).
        /// 4. Tenta encontrar um adversário imediato no cache ([serviceMatchMaker.LogicMatchMakerJoinHub]).
        /// 5. Se encontrar: Cria a partida imediatamente.
        /// 6. Se não encontrar: Adiciona a equipa ao lobby em cache para ser processada pelo Background Service.
        /// </remarks>
        /// <param name="idPlayer">ID do administrador que inicia a procura.</param>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="hoursGame">Horário preferencial do jogo.</param>
        /// <param name="connectionId">ID da conexão SignalR.</param>
        /// <returns>Objeto [EntryRankMatchMakerHub] com os dados da entrada no lobby.</returns>
        public async Task<EntryRankMatchMakerHub> JoinRankMatchMaker(string idPlayer, 
            Guid idTeam, TimeOnly hoursGame, string connectionId)
        {
            validator.ValidateVariableJoinRankMatchMaker(idPlayer, idTeam, hoursGame, connectionId);
            
            Team? team;
            string hubCacheKey = "";
            bool? findUser = false;
            float averageAge = 0;
            InfoTeamRankMatchMakerDto InfoTeam;
            EntryRankMatchMakerHub entry;
            string city = "";
            string previousOrNextRank = "";
            bool haveChangeDifferenceRank = false;
            DateOnly atualDate = DateOnly.FromDateTime(GetNextSunday(DateTime.UtcNow));
            DateTime gameDate = new DateTime(atualDate, hoursGame);
            // Horas que faltam para o jogo (antes a subtração estava invertida e dava sempre um valor
            // negativo, por isso o validador recusava todas as entradas no matchmaking).
            double differenteHoursNowAndGame = (gameDate - DateTime.UtcNow).TotalHours;

            var teamHaveMatchInThisDay = await matchRepository.GetMatchProxim12HoursMatchs(idTeam, gameDate);
            validator.ValidateHoursToMatch(differenteHoursNowAndGame, teamHaveMatchInThisDay);

            team = await teamRepository.GetTeamWitchMemberRankAndPitchAsync(idTeam);
            validator.ValidateTeamJoinRankMatchMaker(team);

            findUser = team?.Members.Any(m => m.Id == idPlayer && m.IsAdmin);
            hubCacheKey = GetHubCacheKey(idTeam);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
            {
                hub = new ConcurrentDictionary<Guid, EntryRankMatchMakerHub>();
            }

            averageAge = CalculateAverageAge(team);
            city = GetCityTeam(team.Pitch.Address);
            previousOrNextRank = GetPreviousOrNextRankTeam(team.CurrentPoints, team.Rank);

            if (!string.IsNullOrEmpty(previousOrNextRank))
            {
                haveChangeDifferenceRank = true;
            }
            
            validator.ValidateJoinRankMatchMaker(team, averageAge, city, findUser, hub);

            InfoTeam = new InfoTeamRankMatchMakerDto
            {
                IdTeam = idTeam,
                Name = team.Name,
                Rank = new InfoRankDto
                {
                    IdRank = team.IdRank,
                    Name = team.Rank.Name,
                },
                NumberPointsTeam = team.CurrentPoints,
                AverageAge = averageAge,
                City = city,
                GameDate = gameDate,
                NextOrPreviousRank = previousOrNextRank,
                isNearToChangeRank = haveChangeDifferenceRank
            };

            entry = new EntryRankMatchMakerHub
            {
                ConnectionId = connectionId,
                Team = InfoTeam
            };
            
            var teamsInCache = GetAllTeamsInCache();
            
            //Não retornar um id mas sim a team (para corrigir o erro, se calhar)
            var teamMatchId = serviceMatchMaker.LogicMatchMakerJoinHub(InfoTeam, teamsInCache, gameDate);

            if (teamMatchId == null || teamMatchId == Guid.Empty)
            {
                hub?.TryAdd(idTeam, entry);
                cache.Set(hubCacheKey, hub, GetCacheOptions());

                if (!cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? globalKeys))
                {
                    globalKeys = new HashSet<string>();
                }

                globalKeys?.Add(hubCacheKey);
                cache.Set(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, globalKeys, GetCacheOptions());
            }
            else
            {
                await CreateMatch(teamMatchId.Value, team, gameDate);

                // A equipa encontrada sai da fila; a que acabou de entrar nunca chegou a entrar.
                RemoverDaFila(teamMatchId.Value);
            }

            return entry;
        }

        /// <summary>
        /// Executa o processo de Matchmaking em lote para todas as equipas em espera.
        /// </summary>
        /// <remarks>
        /// Este método é invocado periodicamente pelo Background Service.
        /// Obtém todas as entradas do cache, executa o algoritmo de emparelhamento ([LogicMatchMaker])
        /// e cria as partidas para os pares encontrados.
        /// </remarks>
        /// <param name="criteria">Critérios de tolerância atuais (Idade/Pontos).</param>
        /// <returns>Dicionário com os pares formados (Host vs Adversário).</returns>
        public async Task<Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>> MatchMaker(CriteriaMatchMaker criteria)
        {
            var teamsInCache = GetAllEntryInCache();
            var result = serviceMatchMaker.LogicMatchMaker(teamsInCache, criteria);

            if (result?.Count > 0)
            {
                await CreateMatchMaking(result);
            }

            return result;
        }

        /// <summary>
        /// Remove uma equipa do lobby de Matchmaking (Cancelamento ou Saída).
        /// </summary>
        /// <param name="teamId">ID da equipa.</param>
        /// <param name="connectionId">ID da conexão.</param>
        /// <returns><c>true</c> se a remoção for bem-sucedida.</returns>
        public async Task<bool> LeaveRankMatchMakerAsync(Guid teamId, string connectionId)
        {
            validator.ValidateLeaveRankMatchMaker(teamId);
            var hubCacheKey = GetHubCacheKey(teamId);

            if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
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

                    if (cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? globalKeys))
                    {
                        globalKeys?.Remove(hubCacheKey);
                        cache.Set(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, globalKeys, GetCacheOptions());
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Trata a desconexão abrupta de um cliente, removendo a equipa do lobby.
        /// </summary>
        public async Task<bool> HandleDisconnectAsync(Guid? maybeTeamId, string connectionId)
        {
            if (!maybeTeamId.HasValue)
            {
                return false;
            }

            return await LeaveRankMatchMakerAsync(maybeTeamId.Value, connectionId);
        }


        #region Private Methods
        
        /// <summary>
        /// Itera sobre os resultados do algoritmo de matchmaking e cria as partidas na base de dados.
        /// </summary>
        private async Task CreateMatchMaking(Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub> matchMaker)
        {
            foreach (var pair in matchMaker)
            {
                var team1 = pair.Key.Team;
                var team2 = pair.Value.Team;

                await CreateMatch(team1.IdTeam, team2.IdTeam, team2.GameDate);

                // Sem isto, as duas equipas continuavam na fila e podiam ser emparelhadas outra vez.
                RemoverDaFila(team1.IdTeam);
                RemoverDaFila(team2.IdTeam);
            }
        }

        /// <summary>Tira uma equipa da fila de matchmaking (cache).</summary>
        private void RemoverDaFila(Guid idTeam)
        {
            var hubCacheKey = GetHubCacheKey(idTeam);
            cache.Remove(hubCacheKey);

            // O conjunto de chaves é o mesmo objeto que está na cache, por isso basta alterá-lo.
            if (cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? globalKeys) && globalKeys != null)
            {
                globalKeys.Remove(hubCacheKey);
            }
        }

        /// <summary>
        /// Recupera todas as entradas (EntryRankMatchMakerHub) de todos os hubs ativos no cache global.
        /// </summary>
        /// <returns>Lista plana de todas as equipas à espera.</returns>
        private IEnumerable<EntryRankMatchMakerHub> GetAllEntryInCache()
        {
            var allTeams = new List<EntryRankMatchMakerHub>();

            if (cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? hubKeys))
            {
                foreach (var hubCacheKey in hubKeys)
                {
                    if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
                    {
                        if (hub.Count == 1)
                        {
                            var entry = hub.Values.First();
                            allTeams.Add(entry);
                        }
                    }
                }
            }

            return allTeams;
        }

        /// <summary>
        /// Recupera apenas os DTOs de informação das equipas em cache (para o algoritmo de join imediato).
        /// </summary>
        private IEnumerable<InfoTeamRankMatchMakerDto> GetAllTeamsInCache()
        {
            var allTeams = new List<InfoTeamRankMatchMakerDto>();

            if (cache.TryGetValue(ModelConstants.ManagerRankMatchMakerServiceConst.GlobalHubKeysCacheKey, out HashSet<string>? hubKeys))
            {
                foreach (var hubCacheKey in hubKeys)
                {
                    if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, EntryRankMatchMakerHub>? hub))
                    {
                        if (hub.Count == 1)
                        {
                            var entry = hub.Values.First();
                            allTeams.Add(entry.Team);
                        }
                    }
                }
            }
            return allTeams;
        }

        /// <summary>
        /// Configurações de cache (expiração de 30 minutos).
        /// </summary>
        private static MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        }

        /// <summary>
        /// Gera a chave de cache única para o lobby de uma equipa.
        /// </summary>
        private static string GetHubCacheKey(Guid idTeam)
        {
            return ModelConstants.ManagerRankMatchMakerServiceConst.PrefixHubCache + idTeam;
        }

        /// <summary>
        /// Calcula a data do próximo Domingo a partir da data atual.
        /// </summary>
        private static DateTime GetNextSunday(DateTime startDate)
        {
            int currentDayOfWeek = (int)startDate.DayOfWeek;
            int targetDayOfWeek = (int)DayOfWeek.Sunday;

            int daysToAdd = targetDayOfWeek - currentDayOfWeek;
            if (daysToAdd <= 0)
            {
                daysToAdd += 7;
            }

            return startDate.Date.AddDays(daysToAdd);
        }

        /// <summary>
        /// Calcula a idade média dos membros da equipa.
        /// </summary>
        private static float CalculateAverageAge(Team team)
        {
            var dateOnly = DateOnly.FromDateTime(DateTime.UtcNow);
            double averageDays = team.Members.Average(m => (dateOnly.DayNumber - m.DateOfBirth.DayNumber));
            return (float)(averageDays / 365.2425);
        }

        /// <summary>
        /// Extrai o nome da cidade a partir da string de endereço da equipa (usando Regex).
        /// </summary>
        private static string GetCityTeam(string addressTeam)
        {
            var city = "";
            var pattern = new Regex(@",\s(?<city>.+)$", RegexOptions.Compiled);
            var validateAddress = pattern.Match(addressTeam);

            city = validateAddress.Groups["city"].Value;       
            
            return city;
        }

        /// <summary>
        /// Determina se a equipa está perto de subir ou descer de Rank.
        /// </summary>
        /// <remarks>
        /// Verifica se a diferença de pontos para a promoção ou despromoção está dentro da margem de tolerância.
        /// </remarks>
        /// <returns>O nome do Rank adjacente (se aplicável) ou string vazia.</returns>
        private static string GetPreviousOrNextRankTeam(int numberPointsTeam, Rank rankTeam)
        {
            const int differencePoints = ModelConstants.DeafultCriteriaMatchMaker.differencePoint;

            // Perto de subir: faltam poucos pontos para a promoção (se houver divisão acima).
            if (rankTeam.NextRank != null && rankTeam.PointsToPromotion - numberPointsTeam <= differencePoints)
            {
                return rankTeam.NextRank.Name;
            }

            // Perto de descer: tem poucos pontos acima do mínimo desta divisão, que é o valor de
            // promoção da divisão anterior. (Antes lia-se PreviousRank.PreviousRank, que é nulo
            // nas duas divisões mais baixas e fazia rebentar o pedido.)
            var previousRank = rankTeam.PreviousRank;
            if (previousRank != null && numberPointsTeam - previousRank.PointsToPromotion <= differencePoints)
            {
                return previousRank.Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// Cria uma partida na base de dados (chamado pelo Join Imediato).
        /// </summary>
        private async Task CreateMatch(Guid teamMatchId, Team team, DateTime gameDate)
        {
            var teamMatchFind = await teamRepository.GetTeamByIdAsync(teamMatchId)
                ?? throw new InvalidOperationException("A equipa adversária já não existe.");

            await GuardarJogo(teamMatchFind, team, gameDate);
        }

        private async Task CreateMatch(Guid teamMatchId, Guid idTeam2, DateTime gameDate)
        {
            var teamMatchFind = await teamRepository.GetTeamByIdAsync(teamMatchId)
                ?? throw new InvalidOperationException("A equipa já não existe.");
            var team = await teamRepository.GetTeamByIdAsync(idTeam2)
                ?? throw new InvalidOperationException("A equipa adversária já não existe.");

            // Antes as estatísticas nunca eram juntas ao jogo e o jogo nunca era guardado.
            await GuardarJogo(teamMatchFind, team, gameDate);
        }

        /// <summary>Cria o jogo competitivo no campo da primeira equipa.</summary>
        private async Task GuardarJogo(Team equipaDaCasa, Team visitante, DateTime gameDate)
        {
            var teamStatistics = new List<TeamStatistics>
            {
                new TeamStatistics(equipaDaCasa),
                new TeamStatistics(visitante),
            };

            var match = new Matches(gameDate, true, equipaDaCasa.IdPitch, teamStatistics);

            await matchRepository.AddMatch(match);
            await unityOfWork.SaveChangesAsync();
        }

        #endregion
    }
}
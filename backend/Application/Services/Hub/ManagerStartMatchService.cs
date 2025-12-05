using Application.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators.Hub;
using Domain.Constants;
using Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Application.Services.Hub
{
    /// <summary>
    /// Serviço de gestão de estado para o Hub de Início de Partida ([StartMatchHub]).
    /// 
    /// Responsável por manter o estado temporário dos lobbies de início de jogo (quem está à espera)
    /// utilizando [IMemoryCache]. Coordena a entrada de administradores e a transição do estado da partida
    /// para [IN_PROGRESS] quando ambos os administradores estão presentes.
    /// </summary>
    public class ManagerStartMatchService : IManagerStartMatchService
    {
        private readonly IMatchRepository matchRepository;
        private readonly IUnityOfWork unityOfWork;
        private readonly IStartMatchHubValidator validator;
        private readonly IGeralHubValidator geralValidator;
        private readonly IMemoryCache cache;

        /// <summary>
        /// Construtor do ManagerStartMatchService.
        /// </summary>
        /// <param name="matchRepository">Repositório de partidas.</param>
        /// <param name="unityOfWork">Unidade de trabalho para persistência.</param>
        /// <param name="validator">Validador de regras de negócio do Hub.</param>
        /// <param name="geralValidator">Validador genérico de Hubs.</param>
        /// <param name="cache">Cache em memória para armazenar o estado dos lobbies.</param>
        public ManagerStartMatchService(IMatchRepository matchRepository, IUnityOfWork unityOfWork,
            IStartMatchHubValidator validator, IGeralHubValidator geralValidator, IMemoryCache cache)
        {
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
            this.validator = validator;
            this.geralValidator = geralValidator;
            this.cache = cache;
        }

        /// <summary>
        /// Regista a entrada de um administrador no lobby de início de partida.
        /// </summary>
        /// <remarks>
        /// **Lógica de Sincronização:**
        /// 1. Verifica se já existe um lobby ativo para esta partida no Cache.
        /// 2. Se não existir (Cache Miss), cria um novo e adiciona o primeiro admin.
        /// 3. Se já existir (Cache Hit) e tiver 1 admin, adiciona o segundo.
        /// 4. Quando o segundo admin entra, o estado da partida é atualizado na BD para [IN_PROGRESS] e o lobby é limpo da cache.
        /// </remarks>
        /// <param name="matchId">ID da partida.</param>
        /// <param name="userId">ID do utilizador (admin).</param>
        /// <param name="idTeam">ID da equipa do admin.</param>
        /// <param name="connectionId">ID da conexão SignalR.</param>
        /// <returns>Objeto [JoinStartMatchResult] com o estado da operação (Se é o primeiro, se o jogo começou, etc.).</returns>
        public async Task<JoinStartMatchResult> JoinHubAsync(Guid matchId, string userId, Guid idTeam, string connectionId)
        {
            validator.ValidateVariableJoinMatch(matchId, userId, connectionId);

            var match = await matchRepository.GetMatchWithListPlayerById(matchId);
            validator.ValidateMatchJoinMatch(match);

            var teamMatchAdmin = match.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam &&
                ts.Team.Members.Any(p => p.Id == userId && p.IsAdmin)
            );

            var hubCacheKey = GetHubCacheKey(matchId);
            if (!cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, string>? hub))
            {
                hub = new ConcurrentDictionary<Guid, string>();
            }

            validator.ValidateJoinMatch(teamMatchAdmin, idTeam, hub);

            var result = new JoinStartMatchResult
            {
                TeamId = idTeam,
                Match = match
            };

            if (hub.Count == 0)
            {
                //Adicionar admin ao hub
                hub.TryAdd(idTeam, connectionId);
                cache.Set(hubCacheKey, hub, GetCacheOptions());
                result.IsFirstAdmin = true;
                result.MatchStarted = false;
            }
            else
            {
                //Adicionar 2º admin e fechar hub
                var first = hub.First();
                var firstTeamId = first.Key;
                var firstConnectionId = first.Value;

                match.MatchStatus = MatchStatus.IN_PROGRESS;
                match.TimeStart = DateTime.UtcNow;
                await unityOfWork.SaveChangesAsync();

                cache.Remove(hubCacheKey);

                result.IsFirstAdmin = false;
                result.MatchStarted = true;
                result.FirstAdminConnectionId = firstConnectionId;
            }

            return result;
        }

        /// <summary>
        /// Remove um administrador do lobby de início de partida (cancelamento ou desconexão).
        /// </summary>
        /// <remarks>
        /// Se o lobby ficar vazio após a remoção, a entrada de cache é eliminada para libertar memória.
        /// </remarks>
        /// <param name="matchId">ID da partida.</param>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="connectionId">ID da conexão (para validação, embora não usado explicitamente na remoção por chave).</param>
        /// <returns><c>true</c> se a remoção foi bem-sucedida; <c>false</c> se o lobby não existia ou o utilizador não estava lá.</returns>
        public async Task<bool> LeaveHubAsync(Guid matchId, Guid idTeam, string connectionId)
        {
            geralValidator.ValidateIdMatchLeaveMatch(matchId, idTeam);
            var hubCacheKey = GetHubCacheKey(matchId);

            if (cache.TryGetValue(hubCacheKey, out ConcurrentDictionary<Guid, string>? hub))
            {
                if (hub.TryRemove(idTeam, out _))
                {
                    // se esvaziou, remove cache; senão atualiza
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
        /// Lida com a desconexão abrupta de um cliente (OnDisconnected).
        /// </summary>
        /// <param name="maybeMatchId">ID da partida (pode ser nulo).</param>
        /// <param name="maybeTeamId">ID da equipa (pode ser nulo).</param>
        /// <param name="connectionId">ID da conexão.</param>
        /// <returns><c>true</c> se a limpeza foi realizada.</returns>
        public async Task<bool> HandleDisconnectAsync(Guid? maybeMatchId, Guid? maybeTeamId, string connectionId)
        {
            if (!maybeMatchId.HasValue || !maybeTeamId.HasValue)
            {
                return false;
            }

            return await LeaveHubAsync(maybeMatchId.Value, maybeTeamId.Value, connectionId);
        }

        #region private methods
        /// <summary>
        /// Gera a chave única de cache para o lobby de uma partida.
        /// </summary>
        private static string GetHubCacheKey(Guid matchId)
        {
            return ModelConstants.StartMatchHubConst.PrefixHubCache + matchId;
        }

        /// <summary>
        /// Configura as opções de expiração da cache (10 minutos).
        /// </summary>
        private static MemoryCacheEntryOptions GetCacheOptions()
        {
            return new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        }

        #endregion
    }
}
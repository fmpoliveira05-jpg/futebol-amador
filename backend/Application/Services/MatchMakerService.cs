using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services;
using Domain.Constants;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio puro responsável pela lógica algorítmica de Matchmaking (Emparelhamento).
    /// 
    /// Esta classe não acede à base de dados; recebe listas de equipas em memória e aplica
    /// heurísticas e regras de negócio para determinar quais as equipas que devem jogar entre si.
    /// </summary>
    public class MatchMakerService : IMatchMakerService
    {
        #region MatchMaker Methods

        /// <summary>
        /// Tenta encontrar um adversário instantâneo para uma equipa que acabou de entrar no Lobby.
        /// </summary>
        /// <remarks>
        /// <b>Lógica de Emparelhamento:</b>
        /// <list type="number">
        ///     <item><b>Filtragem de Rank:</b> Seleciona apenas equipas do mesmo Rank (ou Rank adjacente, se especificado em [NextOrPreviousRank]).</item>
        ///     <item><b>Ordenação:</b> Prioriza as equipas que entraram mais recentemente (LIFO) ou há mais tempo (FIFO), dependendo do valor de [timeEntry].</item>
        ///     <item><b>Critérios Rigorosos:</b> O adversário tem de coincidir exatamente na <b>Cidade</b> e na <b>Data do Jogo</b>.</item>
        ///     <item><b>Critérios de Tolerância:</b> A diferença de Idade Média e Pontos deve estar dentro dos limites padrão definidos em [ModelConstants].</item>
        /// </list>
        /// </remarks>
        /// <param name="finder">O DTO da equipa que acabou de iniciar a procura.</param>
        /// <param name="teamsInSearch">A lista de todas as outras equipas atualmente à espera no Lobby.</param>
        /// <param name="gameDate">A data do jogo pretendida (Nota: O método usa `finder.GameDate`, este parâmetro pode ser redundante).</param>
        /// <returns>O [Guid] da equipa adversária encontrada, ou <c>null</c> se não houver compatibilidade imediata.</returns>
        public Guid? LogicMatchMakerJoinHub(InfoTeamRankMatchMakerDto finder, 
            IEnumerable<InfoTeamRankMatchMakerDto> teamsInSearch, DateTime gameDate)
        {
            IEnumerable<InfoTeamRankMatchMakerDto> orderTeams;
            var rank = finder.Rank.Name;
            InfoTeamRankMatchMakerDto? teamFind;

            if (string.IsNullOrEmpty(finder.NextOrPreviousRank))
            {
                orderTeams = teamsInSearch.Where(t => t.Rank.Name == rank)
                                          .OrderByDescending(t => t.timeEntry);
            } 
            else
            {
                var otherRank = finder.NextOrPreviousRank;
                orderTeams = teamsInSearch.Where(t => t.Rank.Name == rank ||
                                                 t.Rank.Name == otherRank)
                                           .OrderByDescending(t => t.timeEntry);
            }

            teamFind = orderTeams.FirstOrDefault(t => t.City == finder.City &&
            t.GameDate == finder.GameDate &&
            Math.Abs(finder.AverageAge - t.AverageAge) <= ModelConstants.DeafultCriteriaMatchMaker.differenceAverageAge &&
            Math.Abs(finder.NumberPointsTeam - t.NumberPointsTeam) <= ModelConstants.DeafultCriteriaMatchMaker.differencePoint);

            if (teamFind == null)
            {
                return null;
            }

            return teamFind.IdTeam;
        }

        /// <summary>
        /// Executa o algoritmo de emparelhamento em lote (Batch Processing) para todas as equipas em espera.
        /// </summary>
        /// <remarks>
        /// Este método utiliza uma abordagem de <b>Sliding Window (Janela Deslizante)</b> para otimizar a performance:
        /// <list type="number">
        ///     <item>Agrupa as equipas por <b>Data de Jogo</b>.</item>
        ///     <item>Dentro de cada data, <b>ordena</b> as equipas por Pontuação.</item>
        ///     <item>Itera sobre a lista ordenada. Para cada equipa, verifica apenas os vizinhos cuja diferença de pontos esteja dentro do limite ([diffPoints]).</item>
        ///     <item>A verificação para assim que a diferença de pontos excede o limite (graças à ordenação), evitando uma complexidade O(N²).</item>
        /// </list>
        /// 
        /// <b>Critérios de Match:</b>
        /// <list type="bullet">
        ///     <item>Cidade: Deve ser idêntica.</item>
        ///     <item>Idade Média: Diferença deve ser menor ou igual a [criteria.diffAverageAge].</item>
        ///     <item>Pontos: Diferença deve ser menor ou igual a [criteria.differencPoints].</item>
        /// </list>
        /// </remarks>
        /// <param name="teamsInSearch">A lista completa de equipas no Lobby.</param>
        /// <param name="criteria">Os critérios de tolerância dinâmicos (Idade e Pontos) que podem ser relaxados com o tempo.</param>
        /// <returns>Um Dicionário onde a <b>Chave</b> é a equipa "Host" e o <b>Valor</b> é a equipa "Adversária".</returns>
        public Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>? LogicMatchMaker(IEnumerable<EntryRankMatchMakerHub> teamsInSearch, CriteriaMatchMaker criteria)
        {
            var result = new Dictionary<EntryRankMatchMakerHub, EntryRankMatchMakerHub>();
            var matched = new HashSet<Guid?>(); // tornar O(1) ver se uma team já tem uma match
            var diffPoints = criteria.differencPoints;
            var diffAge = criteria.diffAverageAge;
            var agrupListTeams = GetDicitonaryTeamGroupByMatchDate(teamsInSearch);
            
            //Utilizar sliding window
            foreach (var dateGame in agrupListTeams)
            {
                var teamsList = dateGame.Value; //lista de equipas
                var countTeams = teamsList.Count;

                if (countTeams < 2)
                {
                    continue;
                }

                //Ordernar a lista pelo o numero de pontos
                teamsList.Sort((a, b) => a.Team.NumberPointsTeam.CompareTo(b.Team.NumberPointsTeam));

                for (int left = 0; left < countTeams; left++)
                {
                    var entry = teamsList[left];
                    var team = entry.Team;
                    var teamId = team.IdTeam;
                    //Ver se a team já está numa match
                    
                    if (matched.Contains(teamId))
                    {
                        continue;
                    }

                    var findTeam = false;
                    var pointerRight = left + 1;
                    while (pointerRight < countTeams && !findTeam && Math.Abs(team.NumberPointsTeam - teamsList[pointerRight].Team.NumberPointsTeam) <= diffPoints)
                    {
                        var criteriaPass = 0;
                        var entryRight = teamsList[pointerRight];
                        var teamRight = entryRight.Team;

                        if (team.City == teamRight.City)
                        {
                            criteriaPass++;
                        }

                        if (Math.Abs(team.AverageAge - teamRight.AverageAge) <= diffAge)
                        {
                            criteriaPass++;
                        }

                        if (criteriaPass == 2)
                        {
                            if (!result.ContainsKey(entry))
                            {
                                result[entry] = entryRight;
                            }
                            matched.Add(teamId);
                            matched.Add(teamRight.IdTeam);
                            findTeam = true;
                        } 
                        else 
                        {
                            pointerRight++;
                        }

                    }
                }

            }

            return result;
        }
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Agrupa uma lista plana de entradas do Lobby em um dicionário baseado na Data do Jogo.
        /// </summary>
        /// <param name="teamsInSearch">Lista de todas as equipas.</param>
        /// <returns>Dicionário onde a Chave é a [DateTime] do jogo e o Valor é a lista de equipas para esse dia.</returns>
        private static Dictionary<DateTime, List<EntryRankMatchMakerHub>> GetDicitonaryTeamGroupByMatchDate(IEnumerable<EntryRankMatchMakerHub> teamsInSearch) 
        {
            var dicionary = teamsInSearch
                .GroupBy(t => t.Team.GameDate)
                .ToDictionary(dic => dic.Key, teams => teams.ToList());

            return dicionary;
        }
        #endregion
    }
}
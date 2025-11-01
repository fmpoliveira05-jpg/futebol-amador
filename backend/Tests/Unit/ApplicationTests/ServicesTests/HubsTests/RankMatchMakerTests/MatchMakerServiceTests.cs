using Application.DTOs.Rank;
using Application.DTOs.RankMatchMaker;
using Application.Services;
using NUnit.Framework;

namespace Tests.Unit.ApplicationTests.ServicesTests.HubsTests.RankMatchMakerTests
{
    [TestFixture]
    public class MatchMakerServiceTests
    {
        #region Variables
        private MatchMakerService service;
        private readonly DateTime gameDate = DateTime.Now.AddDays(7).Date.AddHours(10);
        private readonly DateTime gameDateAfternoon = DateTime.Now.AddDays(7).Date.AddHours(16); 
        private readonly string city = "Lisbon";
        private readonly string rankGold = "Gold";
        private readonly string rankSilver = "Silver";
        private readonly string rankPlatinum = "Platinum";
        #endregion

        #region SetUp
        [SetUp]
        public void Setup()
        {
            service = new MatchMakerService();
        }
        #endregion

        #region Support Method

        private InfoTeamRankMatchMakerDto CreateTestTeam(
            Guid id, string rank, string city, float age, int points,
            DateTime gameDate, DateTime timeEntry, string nextOrPreviousRank = "")
        {
            return new InfoTeamRankMatchMakerDto
            {
                IdTeam = id,
                Rank = new InfoRankDto { Name = rank },
                City = city,
                AverageAge = age,
                NumberPointsTeam = points,
                GameDate = gameDate,
                timeEntry = timeEntry,
                NextOrPreviousRank = nextOrPreviousRank
            };
        }

        private List<InfoTeamRankMatchMakerDto> GetFifteenMockTeams(Guid perfectMatchId, Guid oldestAdjacentId)
        {
            var teams = new List<InfoTeamRankMatchMakerDto>
            {
                // 1. Match Perfeito (O ÚNICO VÁLIDO DE "GOLD")
                // CORREÇÃO: Dados idênticos ao finder (25, 500) para garantir que a diferença é 0.
                CreateTestTeam(perfectMatchId, rankGold, city, 25, 500, gameDate, DateTime.Now.AddMinutes(-20)), // O mais antigo
                
                // 2. Outro "Gold", mas falha idade (age diff 6 > 5)
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 31, 480, gameDate, DateTime.Now.AddMinutes(-5)), 

                // 3. Critério Falha: Cidade
                CreateTestTeam(Guid.NewGuid(), rankGold, "Porto", 25, 500, gameDate, DateTime.Now.AddMinutes(-8)),

                // 4. Critério Falha: Data Jogo
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 500, gameDate.AddDays(1), DateTime.Now.AddMinutes(-7)),

                // 5. Outro "Gold", mas falha pontos (points diff 400 > 100)
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 20, 900, gameDate, DateTime.Now.AddMinutes(-6)), 

                // 6. Outro "Gold", mas falha pontos (points diff 200 > 100)
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 300, gameDate, DateTime.Now.AddMinutes(-12)), 

                // 7. Match Adjacente (Silver) - O ÚNICO VÁLIDO
                // CORREÇÃO: Dados idênticos ao finder (25, 500) para garantir que a diferença é 0.
                CreateTestTeam(oldestAdjacentId, rankSilver, city, 25, 500, gameDate, DateTime.Now.AddMinutes(-22)), // O mais antigo adjacente

                // 8. Outro "Platinum", mas falha pontos (points diff 150 > 100)
                CreateTestTeam(Guid.NewGuid(), rankPlatinum, city, 23, 650, gameDate, DateTime.Now.AddMinutes(-18)),

                // 9. Critério Falha: Rank (Bronze) - Não adjacente
                CreateTestTeam(Guid.NewGuid(), "Bronze", city, 25, 500, gameDate, DateTime.Now.AddMinutes(-3)),

                // 10. Outro "Gold", mas falha idade (age diff 15 > 5)
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 10, 500, gameDate, DateTime.Now.AddMinutes(-1)), // O mais recente

                // 11. Outro "Gold", mas falha idade (age diff 3, mas points diff 101 > 100)
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 28, 601, gameDate, DateTime.Now.AddMinutes(-11)),

                // 12. "Silver" adjacente, mas falha cidade
                CreateTestTeam(Guid.NewGuid(), rankSilver, "Porto", 25, 500, gameDate, DateTime.Now.AddMinutes(-30)),

                // 13. Outro "Gold", mas falha idade (age diff 15 > 5)
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 40, 500, gameDate, DateTime.Now.AddMinutes(-2)),

                // 14. "Platinum" adjacente, mas falha pontos (points diff 400 > 100)
                CreateTestTeam(Guid.NewGuid(), rankPlatinum, city, 25, 900, gameDate, DateTime.Now.AddMinutes(-25)),

                // 15. Outro "Gold", mas falha pontos (points diff 160 > 100)
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 24, 340, gameDate, DateTime.Now.AddMinutes(-13)),
            };
            return teams;
        }

        private EntryRankMatchMakerHub CreateTestEntry(Guid id, string rank, string city, float age, int points, DateTime gameDate)
        {
            return new EntryRankMatchMakerHub
            {
                ConnectionId = "conn-" + id.ToString(),
                Team = CreateTestTeam(id, rank, city, age, points, gameDate, DateTime.Now)
            };
        }

        private List<EntryRankMatchMakerHub> GetTwentyMockEntries(DateTime date1, DateTime date2)
        {
            var teams = new List<EntryRankMatchMakerHub>();

            // --- Equipas da Data 1 (Manhã) ---
            // Par 1 (Tight): T1 (500) e T2 (520)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 500, date1));
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 26, 520, date1)); 
            // Par 2 (Tight): T3 (580) e T4 (590)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 30, 580, date1));
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 31, 590, date1)); 
            // Par 3 (Porto): T5 (550) e T6 (560)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Porto", 22, 550, date1)); 
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Porto", 23, 560, date1)); 
            // Par 4 (Loose): T7 (700) e T8 (750)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 700, date1)); 
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 35, 750, date1)); // (Age diff 10, Pts diff 50)
            // Equipa Solitária (Falha Ponto Window)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankPlatinum, "Lisbon", 40, 900, date1));
            // Equipa Solitária (Falha City)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankPlatinum, "Faro", 41, 910, date1));
            // Equipa Solitária (Falha Age)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 800, date1));
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 50, 810, date1)); // (Age diff 25)
            // Equipa Solitária (Sem ninguém perto)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankSilver, "Coimbra", 25, 1500, date1)); 
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankSilver, "Coimbra", 25, 2000, date1)); 
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankSilver, "Coimbra", 25, 2500, date1)); 
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankSilver, "Coimbra", 25, 3000, date1)); 

            // --- Equipas da Data 2 (Tarde) ---
            // Par 1 (Tight): T17 (500) e T18 (520)
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 500, date2)); 
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 26, 520, date2)); 
            // Solitárias
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankGold, "Porto", 25, 500, date2)); 
            teams.Add(CreateTestEntry(Guid.NewGuid(), rankSilver, "Faro", 30, 800, date2)); 

            return teams;
        }

        #endregion

        #region Tests

        #region Tests Logic MatchMaker Join Hub

        [Test(Description = "Testa se o serviço encontra o 'match' perfeito (único válido).")]
        public void LogicMatchMakerJoinHub_WhenPerfectMatchExists_ReturnsOldestTeamId()
        {
            var finder = CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 500, gameDate, DateTime.Now);
            var perfectMatchId = Guid.NewGuid();
            var teamsInSearch = GetFifteenMockTeams(perfectMatchId, Guid.NewGuid());

            var result = service.LogicMatchMakerJoinHub(finder, teamsInSearch, gameDate);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(perfectMatchId));
        }

        [Test(Description = "Testa se, sem 'match' de rank igual, encontra o 'match' adjacente (único válido).")]
        public void LogicMatchMakerJoinHub_WhenNoSameRankMatchButAdjacentRankExists_ReturnsAdjacentTeamId()
        {
            var finder = CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 500, gameDate, DateTime.Now, nextOrPreviousRank: rankSilver);
            var oldestAdjacentId = Guid.NewGuid();
            var teamsInSearch = GetFifteenMockTeams(Guid.NewGuid(), oldestAdjacentId);

            var adjacentOnlyTeams = teamsInSearch
                .Where(t => t.Rank.Name != rankGold)
                .ToList();

            var result = service.LogicMatchMakerJoinHub(finder, adjacentOnlyTeams, gameDate);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(oldestAdjacentId));
        }

        [Test(Description = "Testa se retorna 'null' quando o 'finder' não cumpre critérios (ex: pontos muito altos).")]
        public void LogicMatchMakerJoinHub_WhenNoMatchMeetsCriteria_ReturnsNull()
        {
            var finder = CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 9000, gameDate, DateTime.Now);
            var teamsInSearch = GetFifteenMockTeams(Guid.NewGuid(), Guid.NewGuid());

            var result = service.LogicMatchMakerJoinHub(finder, teamsInSearch, gameDate);

            Assert.That(result, Is.Null);
        }

        [Test(Description = "Testa se o serviço retorna 'null' quando a lista de equipas em espera está vazia.")]
        public void LogicMatchMakerJoinHub_WhenSearchListIsEmpty_ReturnsNull()
        {
            var finder = CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 500, gameDate, DateTime.Now);
            var teamsInSearch = new List<InfoTeamRankMatchMakerDto>();
            var result = service.LogicMatchMakerJoinHub(finder, teamsInSearch, gameDate);

            Assert.That(result, Is.Null);
        }

        [Test(Description = "Testa se o serviço retorna 'null' quando uma equipa cumpre todos os critérios, exceto a data/hora do jogo.")]
        public void LogicMatchMakerJoinHub_WhenMatchExistsButDifferentGameDate_ReturnsNull()
        {
            var finder = CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 500, gameDate, DateTime.Now);

            var wrongDateTeams = new List<InfoTeamRankMatchMakerDto>
            {
                CreateTestTeam(Guid.NewGuid(), rankGold, city, 26, 550, gameDate.AddDays(7), DateTime.Now.AddMinutes(-10))
            };

            var result = service.LogicMatchMakerJoinHub(finder, wrongDateTeams, gameDate);

            Assert.That(result, Is.Null);
        }

        #endregion

        #region Tests LogicMatchMaker (BackgroundService)

        [Test(Description = "Testa se, com critérios apertados, o serviço forma apenas pares óbvios e agrupa por data.")]
        public void LogicMatchMaker_WithTightCriteria_FindsOnlyCloseMatches()
        {
            var criteria = new CriteriaMatchMaker
            {
                differencPoints = 50,
                diffAverageAge = 5   
            };
            var teamsInSearch = GetTwentyMockEntries(gameDate, gameDateAfternoon);

            var result = service.LogicMatchMaker(teamsInSearch, criteria);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(4));
        }

        [Test(Description = "Testa se, com critérios largos, o serviço forma os pares óbvios E os pares que antes falhavam.")]
        public void LogicMatchMaker_WithLooseCriteria_FindsAdditionalMatches()
        {
            var finder = CreateTestTeam(Guid.NewGuid(), rankGold, city, 25, 500, gameDate, DateTime.Now, nextOrPreviousRank: rankSilver);
            var oldestAdjacentId = Guid.NewGuid();
            var teamsInSearch = GetFifteenMockTeams(Guid.NewGuid(), oldestAdjacentId);

            var adjacentOnlyTeams = teamsInSearch
                .Where(t => t.Rank.Name != rankGold)
                .ToList();

            var result = service.LogicMatchMakerJoinHub(finder, adjacentOnlyTeams, gameDate);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(oldestAdjacentId));
        }

        [Test(Description = "Testa se equipas que já têm 'match' (como 'left') não são reutilizadas (como 'right').")]
        public void LogicMatchMaker_DoesNotReuseMatchedTeams()
        {
            var criteria = new CriteriaMatchMaker { differencPoints = 100, diffAverageAge = 10 };
            var teamsInSearch = new List<EntryRankMatchMakerHub>
            {
                CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 500, gameDate), 
                CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 26, 510, gameDate),
                CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 27, 520, gameDate) 
            };

            var result = service.LogicMatchMaker(teamsInSearch, criteria);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test(Description = "Testa se equipas com datas de jogo diferentes nunca dão 'match', mesmo sendo pares perfeitos.")]
        public void LogicMatchMaker_DoesNotMatchTeamsWithDifferentGameDates()
        {
            var criteria = new CriteriaMatchMaker { differencPoints = 100, diffAverageAge = 10 };
            var teamsInSearch = new List<EntryRankMatchMakerHub>
            {
                CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 500, gameDate),
                CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 500, gameDateAfternoon) 
            };

            var result = service.LogicMatchMaker(teamsInSearch, criteria);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test(Description = "Testa se uma lista vazia ou com menos de 2 equipas retorna um dicionário vazio.")]
        public void LogicMatchMaker_WithEmptyOrSingleTeamList_ReturnsEmptyDictionary()
        {
            var criteria = new CriteriaMatchMaker { differencPoints = 100, diffAverageAge = 10 };
            var emptyList = new List<EntryRankMatchMakerHub>();
            var singleList = new List<EntryRankMatchMakerHub>
            {
                CreateTestEntry(Guid.NewGuid(), rankGold, "Lisbon", 25, 500, gameDate)
            };

            var resultEmpty = service.LogicMatchMaker(emptyList, criteria);
            var resultSingle = service.LogicMatchMaker(singleList, criteria);

            Assert.That(resultEmpty.Count, Is.EqualTo(0));
            Assert.That(resultSingle.Count, Is.EqualTo(0));
        }

        #endregion

        #endregion
    }
}
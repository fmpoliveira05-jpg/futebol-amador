using Domain.Enums;
using System.ComponentModel.DataAnnotations;

/*
 * Classe que representa um Partida 
 */
namespace Domain.Entities
{
    public class Matches
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public MatchStatus MatchStatus { get; set; }

        [Range(0, 2, ErrorMessage = "O número de equipas que compõem o match deve estar entre 0 e 2")]
        public int TeamsCount { get; set; }

        public ICollection<TeamStatistics> Teams { get; set; }

        public DateTime MatchDate { get; set; }

        public DateTime TimeStart { get; set; }

        public Boolean Iscompetive { get; set; } //Se o match é a valer para o rank ou é so amigável

        public Pitch Pitch { get; set; }

        public Guid idPitch { get; set; } //FK

        protected Matches() { }

        public Matches(MatchStatus matchStatus, int teamsCount, DateTime matchDate, DateTime timeStart, bool iscompetive, Pitch pitch)
        {
            MatchStatus = matchStatus;
            TeamsCount = teamsCount;
            MatchDate = matchDate;
            TimeStart = timeStart;
            this.Iscompetive = iscompetive;
            Pitch = pitch;
            Teams = new List<TeamStatistics>();
        }

        /***
         * Método auxiliar para encontrar uma equipa nas estatísticas do match
         * 
         * Retorna a equipa ou null se não encontrar
         */
        private TeamStatistics findTeam() { 
            return null;
        }

        /***
         * 
         */
        public TeamStatistics AddTeam(TeamStatistics team)
        {
            return null;
        }

        /***
         * Metodo que consulta as estatísticas de uma equipa num determinado match
         */
        public TeamStatistics ShowTeamStatistics(Guid idTeam)
        {
            return null;
        }

        public override string ToString()
        {
            return $"Match [Id={Id}, MatchStatus={MatchStatus}, TeamsCount={TeamsCount}, MatchDate={MatchDate}, TimeStart={TimeStart}, iscompetive={Iscompetive}, Pitch={Pitch}]";
        }
    }
}

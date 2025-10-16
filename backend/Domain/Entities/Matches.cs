using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public ICollection<TeamStatistics> Teams { get; set; }

        public DateTime MatchDate { get; set; }

        public DateTime TimeStart { get; set; }

        public bool IsCompetive { get; set; } //Se o match é a valer para o rank ou é so amigável

        public Pitch Pitch { get; set; }

        [ForeignKey("Pitch")]
        public Guid idPitch { get; set; } //FK

        public Chat Chat { get; set; }

        [ForeignKey("Chat")]
        public Guid IdChat { get; set; }

        protected Matches() { }

        public Matches(MatchStatus matchStatus, int teamsCount, DateTime matchDate, DateTime timeStart, bool iscompetive, Pitch pitch)
        {
            MatchStatus = matchStatus;
            MatchDate = matchDate;
            TimeStart = timeStart;
            IsCompetive = iscompetive;
            Pitch = pitch;
            Teams = new List<TeamStatistics>();
            Chat = new Chat();
            IdChat = Chat.Id;
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
            return $"Match [Id={Id}, MatchStatus={MatchStatus}, MatchDate={MatchDate}, TimeStart={TimeStart}, iscompetive={IsCompetive}, Pitch={Pitch}]";
        }
    }
}

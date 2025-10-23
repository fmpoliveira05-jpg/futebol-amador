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

        public MatchStatus MatchStatus { get; set; } = MatchStatus.SCHEDULED;

        public ICollection<TeamStatistics> Teams { get; set; }

        public DateTime MatchDate { get; set; }

        //public DateTime PostPoneDate { get; set; }

        public DateTime? TimeStart { get; set; } = null;

        public bool IsCompetive { get; set; } //Se o match é a valer para o rank ou é so amigável

        public Pitch Pitch { get; set; }

        [ForeignKey("Pitch")]
        public Guid idPitch { get; set; } //FK

        public Chat Chat { get; set; }

        [ForeignKey("Chat")]
        public Guid IdChat { get; set; }

        protected Matches() { }

        public Matches(DateTime matchDate, bool isCompetive, Pitch pitch)
        {
            this.MatchDate = matchDate;
            //this.PostPoneDate = matchDate
            this.IsCompetive = isCompetive;
            this.Pitch = pitch;
            this.Teams = new List<TeamStatistics>();
            this.Chat = new Chat();
            this.IdChat = Chat.Id;
        }

        public Matches(DateTime matchDate, bool isCompetive, Pitch pitch, List<TeamStatistics> teamStatistics, Chat chat)
        {
            this.MatchDate = matchDate;
            this.IsCompetive = isCompetive;
            this.Pitch = pitch;
            this.Teams = teamStatistics;

            if (chat != null)
            {
                this.Chat = chat;
                this.IdChat = Chat.Id;
            } else
            {
                this.Chat = new Chat();
                this.IdChat = this.Chat.Id;
            }
        }

        public Matches(DateTime matchDate, bool isCompetive, Guid idPitch, List<TeamStatistics> teamStatistics, Guid? idChat)
        {
            this.MatchDate = matchDate;
            this.IsCompetive = isCompetive;
            this.idPitch = idPitch;
            this.Teams = teamStatistics;

            if (idChat != Guid.Empty)
            {
                this.IdChat = idPitch;
            }
            else
            {
                this.Chat = new Chat();
                this.IdChat = this.Chat.Id;
            }
        }

        public override string ToString()
        {
            return $"Match [Id={Id}, MatchStatus={MatchStatus}, MatchDate={MatchDate}, TimeStart={TimeStart}, iscompetive={IsCompetive}, Pitch={Pitch}]";
        }
    }
}

using System.ComponentModel.DataAnnotations;

/**
 * Entidade que representa um calendário de partidas de uma equipa
 */
namespace Domain.Entities
{
    public class Calendar
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public ICollection<Matches> Matches { get; set; } = new List<Matches>(); //FK

        //EF
        public Calendar()
        {
        }

        public Calendar(ICollection<Matches> matches)
        {
            Matches = matches;
        }

        public override string ToString()
        {
            return $"Calendar: {Id}, Matches: {Matches}";
        }
    }
}

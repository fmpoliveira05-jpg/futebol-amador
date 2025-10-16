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

        /***
         * Metodo que permite adicionar uma partida ao calendário
         * 
         * match: partida a adicionar
         * 
         * Retorna a partida adicionada ou null se não for possível adicionar
         */
        public Matches AddMatch(Matches match)
        {
            return null;
        }

        /***
         * Metodo que permite remover uma partida ao calendário
         * 
         * match: partida a remover
         * 
         * Retorna a partida removida ou null se não for possível remover
         */
        public Matches RemoveMatch(Matches match)
        {
            return null;
        }

        /***
         * Metodo que permite obter uma partida do calendário pelo seu id
         * 
         * idMatch: id da partida a obter
         * 
         * Retorna a partida ou null se não for possível encontrar
         */
        public Matches GetMatchById(Guid idMatch)
        {
            return null;
        }

        public override string ToString()
        {
            return $"Calendar: {Id}, Matches: {Matches}";
        }
    }
}

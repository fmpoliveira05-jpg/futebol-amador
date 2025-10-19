using Domain.Enums;
using Domain.Exceptions;
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
        public Matches ScheduledMatch(Matches match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("A partida a adicionar no calendario não pode ser nula");
            }

            if (Matches.Contains(match)) {
                throw new ArgumentException("Este match já existe na lista.", nameof(match));
            }

            Matches.Add(match);
            return match;
        }

        public Matches PostPoneMatch(Guid idMatch, DateTime newDate)
        {
            if (idMatch == Guid.Empty)
            {
                throw new ArgumentNullException("O id da match está vazio!");
            }

            Matches? matchFind = Matches.FirstOrDefault(m => m.Id == idMatch);
            
            if (matchFind == null) {
                throw new NotFindException("A equipa não possui esse jogo");
            }

            matchFind.MatchDate = newDate;
            matchFind.MatchStatus = MatchStatus.POST_PONED;

            return matchFind;
        }

        public Matches AcceptPostPoneMatch(Guid idMatch)
        {
            if (idMatch == Guid.Empty)
            {
                throw new ArgumentNullException("O id da match está vazio!");
            }

            Matches? matchFind = Matches.FirstOrDefault(m => m.Id == idMatch);

            if (matchFind == null)
            {
                throw new NotFindException("A equipa não possui esse jogo");
            }

            matchFind.MatchStatus = MatchStatus.SCHEDULED;

            return matchFind;
        }

        public void CancelMatch(Matches match)
        {
            if (match == null)
            {
                throw new NullReferenceException("O match não pode ser nullo");
            }

            Matches.Remove(match);
        }

        public override string ToString()
        {
            return $"Calendar: {Id}, Matches: {Matches}";
        }
    }
}

using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>Uma época de uma liga: equipas inscritas, calendário e classificação.</summary>
    public class Season
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(League))]
        public Guid IdLeague { get; set; }

        public League League { get; set; } = null!;

        /// <summary>Nome da época, por exemplo "2026/27".</summary>
        [Required, MaxLength(20)]
        public string Name { get; set; } = null!;

        public SeasonStatus Status { get; set; } = SeasonStatus.REGISTRATION;

        public DateTime StartDate { get; set; }

        /// <summary>Data de fim: início + duração da liga. Nessa data a época fecha-se sozinha.</summary>
        public DateTime EndDate { get; set; }

        public ICollection<SeasonTeam> Teams { get; set; } = new List<SeasonTeam>();

        public ICollection<Matches> Matches { get; set; } = new List<Matches>();
    }

    /// <summary>Inscrição de uma equipa numa época.</summary>
    public class SeasonTeam
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Season))]
        public Guid IdSeason { get; set; }

        public Season Season { get; set; } = null!;

        [ForeignKey(nameof(Team))]
        public Guid IdTeam { get; set; }

        public Team Team { get; set; } = null!;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Título ganho por uma equipa. Guarda cópias dos nomes para o palmarés não mudar se a liga for renomeada.
    /// </summary>
    public class TeamTitle
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Team))]
        public Guid IdTeam { get; set; }

        public Team Team { get; set; } = null!;

        public Guid IdSeason { get; set; }

        public Guid IdLeague { get; set; }

        [Required, MaxLength(80)]
        public string TrophyName { get; set; } = null!;

        [Required, MaxLength(60)]
        public string LeagueName { get; set; } = null!;

        [Required, MaxLength(20)]
        public string SeasonName { get; set; } = null!;

        public DateTime WonAt { get; set; } = DateTime.UtcNow;
    }
}

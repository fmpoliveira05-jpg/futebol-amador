using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Classe que representa um Match
 * 
 */
namespace Domain.Entities
{
    internal class Matches
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; }

        public MatchStatus MatchStatus { get; set; }

        [Range(0, 2, MsgError = "O número de equipas que compõem o match deve estar entre 0 e 2")]
        public count TeamsCount { get; set; }

        public ICollection<TeamStatistics> Teams { get; set; }

        public DateTime MatchDate { get; set; }

        public DateTime TimeStart { get; set; }

        public Boolean iscompetive { get; set; } //Se o match é a valer para o rank ou é so amigável

        public Pitch Pitch { get; set; }

        public string idPitch { get; set; } //FK
    }
}

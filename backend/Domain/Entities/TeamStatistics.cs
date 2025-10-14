using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
/**
 * Entidade que representa uma equipa no sistema.
 * Nota: Ver se está tudo
 *
 */
namespace Domain.Entities
{
    internal class TeamStatistics
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; } //PK

        public Teams Team { get; set; } //FK

        public string IdTeam { get; set; } //FK

        public static int max_goals = 100;

        [Range(0, max_goals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int num_goals;

        public MatchResult MatchResult { get; set; } 
    }
}

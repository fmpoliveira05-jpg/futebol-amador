using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Hubs
{
    public class JoinStartMatchResult
    {
        [Required]
        public bool IsFirstAdmin { get; set; }

        [Required]
        public bool MatchStarted { get; set; } // A true quer dizer que já temos os dois admins e a match vai ser iniciada

        [Required]
        public Guid TeamId { get; set; }

        public string? FirstAdminConnectionId { get; set; } //Guardar Connection string do 1º admin

        public Matches? Match { get; set; }

        public bool? IsCoincides { get; set; }
    }
}

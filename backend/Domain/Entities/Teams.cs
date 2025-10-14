using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

/**
 * Entidade que representa uma equipa no sistema.
 * Nota: Ver se está tudo
 *
 */
namespace Domain.Entities
{
    internal class Teams
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; }

        [MaxLength = 50]
        public string Name { get; set; }

        [MaxLength = 250]
        public string? Description { get; set; }

        public byte[]? Icon { get; set; }

        public Pitch Pitch { get; set; }

        public string IdPitch { get; set; } //FK

        public DateTime DataFoundation { get; set; }

        public static int MaxPlayers { get; set; } = 32; //Validar se é mesmo 32

        [Range(1, MaxPlayers, ErrorMessage = "O número minimo de players é 1 de máximo 32")]
        public int MemberCount { get; set; }

        public ICollection<Player> Members { get; set; } = new List<Player>();

        public static int MaxAdmins { get; set; } = 4;
        
        [Range(1, MaxAdmins, ErrorMessage = "O número minimo de admins é 1 de máximo 4")]
        public int AdminCount { get; set; }

        public ICollection<Player> Admins { get; set; } = new List<Player>();

        [Range(18, 70, ErrorMessage = "A idade média deve estar entre os 18 e 70 anos")]
        public float AverageAge { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        public Rank? Rank { get; set; }

        public String IdRank { get; set; } //FK

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou pedidos de adesão")]
        public int CountMemvberShipsRequests { get; set; }

        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais convites de partida")]
        public int CountMatchesInvites { get; set; }

        public ICollection<MatchInvite> MatchInvites { get; set; }  = new List<MatchInvite>();

        public Calendar Calendar { get; set; }

        public string IdCalendar { get; set; } //FK
    }
}

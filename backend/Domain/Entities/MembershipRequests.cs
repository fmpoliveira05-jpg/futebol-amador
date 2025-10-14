
using System.ComponentModel.DataAnnotations;

/**
 * Entidade que representa os pedidos de adesão a uma equipa
 * 
 * Depois ao Criar a BD, temos de definir a PK composta, o ORM por default não o faz
 * Mas fazemos isso com Fluent API, ou seja, no DbContext
 */
namespace Domain.Entities
{
    public class MembershipRequests
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Player Player { get; set; }
    
        public Guid idPlayer { get; set; } //FK

        public Teams Team { get; set; }

        public Guid idTeam { get; set; } //FK

        public DateTime inviteDate { get; set; }

        public Boolean sender { get; set; } // true - Player, false - Team
    }
}

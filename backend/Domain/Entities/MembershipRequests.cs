using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * Entidade que representa os pedidos de adesão a uma equipa
 * 
 * Depois ao Criar a BD, temos de definir a PK composta, o ORM por default não o faz
 * Mas fazemos isso com Fluent API, ou seja, no DbContext
 */
namespace Domain.Entities
{
    internal class MembershipRequests
    {
        public Player Player { get; set; }
    
        public string idPlayer { get; set; } //FK

        public Teams Team { get; set; }

        public string idTeam { get; set; } //FK

        public DateTime inviteDate { get; set; }

        public Boolean sender { get; set; } // true - Player, false - Team
    }
}

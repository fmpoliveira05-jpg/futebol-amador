using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

/*
 Parei aqui validar se extend está bem, acho que não
 */
namespace Domain.Entities
{
    internal class MatchInvite
    {
        public string IdSender { get; set; } //FK e PK
        public string IdReceiver { get; set; } //FK e PK

        public Teams Sender { get; set; }
        public Teams Receiver { get; set; }

        public DateTime GameDate { get; set; }

        public Pitch Pitch { get; set; }

        public string IdPitch { get; set; } //FK

        public Chat Chat { get; set; } //FK

        public string IdChat { get; set; } //FK
    }
}

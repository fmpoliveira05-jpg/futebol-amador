/*
 Parei aqui validar se extend está bem, acho que não
 */
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class MatchInvite
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdSender { get; set; } //FK e PK
        public Guid IdReceiver { get; set; } //FK e PK

        public Teams Sender { get; set; }
        public Teams Receiver { get; set; }

        public DateTime GameDate { get; set; }

        public Pitch Pitch { get; set; }

        public Guid IdPitch { get; set; } //FK

        public Chat Chat { get; set; } //FK

        public Guid IdChat { get; set; } //FK
    }
}

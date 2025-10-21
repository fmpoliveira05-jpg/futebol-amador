using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/**
 * Entidade que representa um convite de partida entre duas equipas
 */
namespace Domain.Entities
{
    public class MatchInvite
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Teams Sender { get; set; }
        public Teams Receiver { get; set; }

        [ForeignKey("Sender")]
        public Guid IdSender { get; set; } //FK e
        
        [ForeignKey("Receiver")]
        public Guid IdReceiver { get; set; } //FK 
        public DateTime GameDate { get; set; }

        public Pitch Pitch { get; set; }

        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        public Chat Chat { get; set; } //FK

        [ForeignKey("Chat")]
        public Guid IdChat { get; set; } //FK

        //FK
        protected MatchInvite() { }

        public MatchInvite(Teams sender, Teams receiver, DateTime gameDate, Pitch pitch)
        {
            Sender = sender;
            IdSender = sender.Id;
            Receiver = receiver;
            IdReceiver = receiver.Id;
            GameDate = gameDate;
            Pitch = pitch;
            IdPitch = pitch.Id;
            Chat = new Chat();
            IdChat = Chat.Id;
        }

        public bool NegociateMatchInvite(DateTime gameDate, Pitch pitch)
        {
            bool hasChanged = false;

            if (this.GameDate != gameDate)
            {
                this.GameDate = gameDate;
                hasChanged = true;
            }

            if (this.Pitch != pitch)
            {
                this.IdPitch = pitch.Id;
                this.Pitch = pitch;
                hasChanged = true;
            }

            if (hasChanged)
            {
                var idSender = this.IdSender;

                this.IdSender = this.IdReceiver;
                this.IdReceiver = idSender;
            }

            return hasChanged;
        }

        public override string ToString()
        {
            return $"MatchInvite [Id={Id}, IdSender={IdSender}, IdReceiver={IdReceiver}, GameDate={GameDate}, IdPitch={IdPitch}, IdChat={IdChat}]";
        }
    }
}

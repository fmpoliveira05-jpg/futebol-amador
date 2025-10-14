using System.ComponentModel.DataAnnotations;

/**
 * Entidade que representa um convite de partida entre duas equipas
 */
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

        //FK
        protected MatchInvite() { }

        public MatchInvite(Teams sender, Teams receiver, DateTime gameDate, Pitch pitch, Chat chat)
        {
            Sender = sender;
            IdSender = sender.Id;
            Receiver = receiver;
            IdReceiver = receiver.Id;
            GameDate = gameDate;
            Pitch = pitch;
            IdPitch = pitch.Id;
            Chat = chat;
            IdChat = chat.Id;
        }


        public override string ToString()
        {
            return $"MatchInvite [Id={Id}, IdSender={IdSender}, IdReceiver={IdReceiver}, GameDate={GameDate}, IdPitch={IdPitch}, IdChat={IdChat}]";
        }
    }
}

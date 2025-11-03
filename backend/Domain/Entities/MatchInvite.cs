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

        public Team Sender { get; set; }

        [Required]
        [ForeignKey("Sender")]
        public Guid IdSender { get; set; } //FK
        
        public Team Receiver { get; set; }

        [Required]
        [ForeignKey("Receiver")]
        public Guid IdReceiver { get; set; } //FK 

        [Required]
        public DateTime GameDate { get; set; }

        public Pitch Pitch { get; set; }

        [Required]
        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        public Chat Chat { get; set; } //FK

        [Required]
        [ForeignKey("Chat")]
        public Guid IdChat { get; set; } //FK

        //FK
        protected MatchInvite() { }

        public MatchInvite(Team sender, Team receiver, DateTime gameDate, Pitch pitch)
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

        public override string ToString()
        {
            return $"MatchInvite [Id={Id}, IdSender={IdSender}, IdReceiver={IdReceiver}, GameDate={GameDate}, IdPitch={IdPitch}, IdChat={IdChat}]";
        }
    }
}

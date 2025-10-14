using System.ComponentModel.DataAnnotations;

/***
 * Entidade que representa uma mensagem no sistema.
 */
namespace Domain.Entities
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public Users actor { get; set; }

        public Guid idUser { get; set; } //FK

        [MaxLength(250)]
        public string MessageText { get; set; }

        public DateTime timeStamp { get; set; }

        protected Message() { }

        public Message(Users actor, string messageText)
        {
            this.actor = actor;
            this.idUser = actor.Id;
            this.MessageText = messageText;
            this.timeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{timeStamp}] {actor.Name}: {MessageText}";
        }
    }
}
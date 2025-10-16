using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public Users Actor { get; set; }

        [ForeignKey("Actor")]
        public Guid IdUser { get; set; } //FK

        [MaxLength(250)]
        public string MessageText { get; set; }

        public DateTime timeStamp { get; set; }

        protected Message() { }

        public Message(Users actor, string messageText)
        {
            this.Actor = actor;
            this.IdUser = actor.Id;
            this.MessageText = messageText;
            this.timeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{timeStamp}] {Actor.Name}: {MessageText}";
        }
    }
}
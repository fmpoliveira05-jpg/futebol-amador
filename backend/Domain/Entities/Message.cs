using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/***
 * Deprecated
 */
namespace Domain.Entities
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Users Actor { get; set; }

        [Required]
        [ForeignKey("Actor")]
        [MaxLength(ModelConstants.UserConst.MaxIdLength)]
        public string IdUser { get; set; } //FK

        [Required]
        [StringLength(ModelConstants.MessageConst.MaxMessageLength, MinimumLength = ModelConstants.MessageConst.MinMessageLength)]
        public string MessageText { get; set; }

        [Required]
        public DateTime TimeStamp { get; set; }

        protected Message() { }

        public Message(Users actor, string messageText)
        {
            this.Actor = actor;
            this.IdUser = actor.Id;
            this.MessageText = messageText;
            this.TimeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{TimeStamp}] {Actor.Name}: {MessageText}";
        }
    }
}
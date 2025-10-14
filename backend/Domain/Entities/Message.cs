using System.ComponentModel.DataAnnotations;

/**
 * Entidade que representam uma mensagem do chat
 * 
 * Ver se é preciso meter o MatchId, como o diagrama de classes 
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
    }
}
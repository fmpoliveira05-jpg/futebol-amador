using System.ComponentModel.DataAnnotations;

/*
 * Entiudade que representa um chat da partida
 */
namespace Domain.Entities
{
    public class Chat
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Range(0, int.MaxValue, ErrorMessage = "o número mínimo de mensagens de um chat é 0")]
        public int countMessages { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}

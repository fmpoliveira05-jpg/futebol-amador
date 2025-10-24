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

        public ICollection<Message> Messages { get; set; } = new List<Message>();

        //Construtor para criar um chat novo
        public Chat()
        {
        }
        public override string ToString()
        {
            return $"Chat [Id={Id}, Messages={Messages}]";
        }
    }
}

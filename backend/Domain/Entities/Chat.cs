using System.ComponentModel.DataAnnotations;

/// <summary>
/// Entidade que representa uma sala de chat associada a uma partida ou grupo de utilizadores.
/// Esta entidade funciona como um contentor para todas as mensagens trocadas nessa conversa.
/// </summary>
namespace Domain.Entities
{
    public class Chat
    {
        /// <summary>
        /// O identificador único (GUID) da sala de Chat.
        /// Serve como chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Coleção de mensagens associadas a esta sala de chat.
        /// Representa a chave estrangeira (FK) de uma relação One-to-Many.
        /// </summary>
        public ICollection<Message> Messages { get; set; } = new List<Message>();

        /// <summary>
        /// Construtor padrão da entidade Chat.
        /// É o construtor primário utilizado pelo Entity Framework (EF) e inicializa a coleção de mensagens.
        /// </summary>
        public Chat()
        {
        }
    }
}

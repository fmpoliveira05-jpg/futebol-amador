using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/***
 * Deprecated
 */
namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa uma mensagem individual enviada num chat.
    /// 
    /// **AVISO:** Esta entidade está marcada como Obsoleta. O armazenamento de mensagens
    /// é tipicamente gerido diretamente pelo Firestore (NoSQL) para suportar a escalabilidade
    /// e as funcionalidades em tempo real (Real-Time Messaging).
    /// </summary>
    public class Message
    {
        /// <summary>
        /// O identificador único (GUID) da Mensagem.
        /// Serve como a chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Entidade de navegação para o Utilizador que enviou a mensagem (Actor).
        /// </summary>
        public User Actor { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para o Utilizador que enviou a mensagem.
        /// O comprimento máximo da string é definido em [ModelConstants.UserConst.MaxIdLength].
        /// </summary>
        [Required]
        [ForeignKey("Actor")]
        [MaxLength(ModelConstants.UserConst.MaxIdLength)]
        public string IdUser { get; set; } //FK

        /// <summary>
        /// O conteúdo textual da mensagem.
        /// </summary>
        /// <value>A string deve ter um comprimento entre o [MinMessageLength] e [MaxMessageLength] definidos nas constantes do modelo.</value>
        [Required]
        [StringLength(ModelConstants.MessageConst.MaxMessageLength, MinimumLength = ModelConstants.MessageConst.MinMessageLength)]
        public string MessageText { get; set; }

        /// <summary>
        /// O carimbo de data e hora em que a mensagem foi criada/enviada.
        /// </summary>
        [Required]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Construtor padrão protegido exigido pelo Entity Framework (EF).
        /// </summary>
        protected Message() { }

        /// <summary>
        /// Construtor utilizado para criar uma nova mensagem.
        /// </summary>
        /// <param name="actor">O utilizador que envia a mensagem.</param>
        /// <param name="messageText">O conteúdo da mensagem.</param>
        public Message(User actor, string messageText)
        {
            this.Actor = actor;
            this.IdUser = actor.Id;
            this.MessageText = messageText;
            this.TimeStamp = DateTime.Now;
        }
    }
}
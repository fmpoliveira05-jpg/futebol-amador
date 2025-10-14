using System;

/**
 * Entidade que representam uma mensagem do chat
 * 
 * Ver se é preciso meter o MatchId, como o diagrama de classes 
 */
namespace Domain.Entities
{
    internal class Message
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; }

        [MaxLength = 50]
        public User actor { get; set; }

        public string idUser { get; set; } //FK

        [MaxLength(250)]
        public string MessageText { get; set; }

        public DateTime timeStamp { get; set; }
    }
}
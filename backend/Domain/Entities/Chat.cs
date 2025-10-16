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

        /**
         * Método auxiliar para encontrar uma mensagem no chat
         */
        private Message findMessage() { 
            return null;
        }

        /***
         * Metodo que permite adicionar uma mensagem ao chat
         * 
         * message: mensagem a adicionar
         * 
         * Retorna a mensagem adicionada ou null se não for possível adicionar
         */
        public Message AddMessage(Message message) {
            return null;
        }

        /***
         * Metodo que permite remover uma mensagem ao chat
         * 
         * message: mensagem a remover
         * 
         * Retorna a mensagem removida ou null se não for possível remover
         */
        public Message RemoveMessage(Message message) {             
            return null;
        }

        /***
         * Metodo que permite obter uma mensagem do chat pelo seu id
         * 
         * idMessage: id da mensagem a obter
         * 
         * Retorna a mensagem ou null se não for possível encontrar
         */
        public Message GetMessageById(Guid idMessage) { 
            return null;
        }

        public override string ToString()
        {
            return $"Chat [Id={Id}, Messages={Messages}]";
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entidade que representa um convite formal de partida (desafio) entre duas equipas.
/// 
/// Esta entidade armazena os termos propostos do jogo (data, local) e rastreia o remetente e o recetor.
/// </summary>
namespace Domain.Entities
{
    public class MatchInvite
    {
        /// <summary>
        /// O identificador único (GUID) do convite de partida.
        /// Serve como a chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Entidade de navegação para a Equipa que enviou o convite (Desafiante).
        /// </summary>
        public Team Sender { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Equipa Remetente.
        /// </summary>
        [Required]
        [ForeignKey("Sender")]
        public Guid IdSender { get; set; }

        /// <summary>
        /// Entidade de navegação para a Equipa que recebeu o convite (Desafiada).
        /// </summary>
        public Team Receiver { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Equipa Recetora.
        /// </summary>
        [Required]
        [ForeignKey("Receiver")]
        public Guid IdReceiver { get; set; } //FK 

        /// <summary>
        /// A data e hora proposta para a realização da partida.
        /// </summary>
        [Required]
        public DateTime GameDate { get; set; }

        /// <summary>
        /// Entidade de navegação para o local onde a partida será disputada.
        /// </summary>
        public Pitch Pitch { get; set; }

        /// <summary>
        /// Chave Estrangeira (FK) para o Local ([Pitch]) da partida.
        /// </summary>
        [Required]
        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        /// <summary>
        /// Entidade de navegação para a Sala de Chat associada a este convite.
        /// </summary>
        public Chat Chat { get; set; } //FK

        /// <summary>
        /// Chave Estrangeira (FK) para a Sala de Chat ([Chat]) criada para este convite.
        /// </summary>
        [Required]
        [ForeignKey("Chat")]
        public Guid IdChat { get; set; } //FK
        
        /// <summary>
        /// Construtor protegido exigido pelo Entity Framework (EF).
        /// </summary>
        protected MatchInvite() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo convite de partida.
        /// </summary>
        /// <param name="sender">A equipa que envia o convite.</param>
        /// <param name="receiver">A equipa que recebe o convite.</param>
        /// <param name="gameDate">A data/hora proposta para o jogo.</param>
        /// <param name="pitch">O local proposto para o jogo.</param>
        public MatchInvite(Team sender, Team receiver, DateTime gameDate, Pitch pitch)
        {
            Sender = sender;
            IdSender = sender.Id;
            Receiver = receiver;
            IdReceiver = receiver.Id;
            GameDate = gameDate;
            Pitch = pitch;
            IdPitch = pitch.Id;
            Chat = new Chat();
            IdChat = Chat.Id;
        }
    }
}

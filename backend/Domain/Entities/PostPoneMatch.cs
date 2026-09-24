using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa o registo de um pedido de adiamento (remarcação) de uma partida.
    /// 
    /// Esta entidade armazena a nova data proposta e a equipa que iniciou o pedido de adiamento.
    /// </summary>
    public class PostPoneMatch
    {
        /// <summary>
        /// O identificador único (GUID) do pedido de adiamento.
        /// Serve como a chave primária da tabela.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = new Guid();

        /// <summary>
        /// Entidade de navegação para a Equipa que solicitou o adiamento.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Equipa que iniciou o pedido de adiamento.
        /// </summary>
        [Required]
        [ForeignKey("Team")]
        public Guid IdTeamPostPone { get; set; }

        /// <summary>
        /// Entidade de navegação para a Partida que está a ser solicitada para ser adiada.
        /// </summary>
        public Matches Match { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Partida ([Matches]) alvo do pedido.
        /// </summary>
        [Required]
        [ForeignKey("Match")]
        public Guid IdMatch { get; set; }

        /// <summary>
        /// A nova data e hora proposta para a realização da partida.
        /// </summary>
        [Required]
        public DateTime PostPoneDate { get; set; }

        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public PostPoneMatch() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo pedido de adiamento.
        /// </summary>
        /// <param name="team">A equipa que envia o pedido de adiamento.</param>
        /// <param name="match">A partida que está a ser adiada.</param>
        /// <param name="postPoneDate">A nova data proposta para o jogo.</param>
        public PostPoneMatch(Team team, Matches match, DateTime postPoneDate)
        {
            this.Team = team;
            this.IdTeamPostPone = team.Id;
            this.Match = match;
            this.IdMatch = match.Id;
            this.PostPoneDate = postPoneDate;
        }
    }
}

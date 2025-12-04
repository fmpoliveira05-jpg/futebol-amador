using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa o registo de um jogo que foi cancelado.
    /// 
    /// Esta entidade armazena o histórico de cancelamento, incluindo a equipa responsável e o motivo.
    /// </summary>
    public class CancelledMatch
    {
        /// <summary>
        /// O identificador único (GUID) do registo de cancelamento.
        /// Serve como a chave primária da tabela.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = new Guid();

        /// <summary>
        /// A entidade de navegação da Equipa que efetuou o cancelamento.
        /// </summary>
        [Required]
        public Team Team { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Equipa que cancelou a partida.
        /// </summary>
        [Required]
        [ForeignKey("Team")]
        public Guid IdTeam { get; set; }

        /// <summary>
        /// A entidade de navegação da Partida que foi cancelada.
        /// </summary>
        public Matches Match { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Partida (Matches) que foi cancelada.
        /// </summary>
        [ForeignKey("Match")]
        public Guid IdMatch { get; set; }

        /// <summary>
        /// A descrição detalhada do motivo do cancelamento.
        /// </summary>
        /// <value>A string deve ter um comprimento entre o [MinDescriptionLength] e [MaxDescriptionLength] definidos nas constantes do modelo.</value>
        [Required]
        [StringLength(ModelConstants.CancelledMatchConst.MaxDescriptionLength, MinimumLength = ModelConstants.CancelledMatchConst.MinDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// O carimbo de data e hora em que o registo de cancelamento foi criado.
        /// Inicializado com o valor UTC atual no momento da criação.
        /// </summary>
        [Required]
        public DateTime TimeCancellation { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public CancelledMatch() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo registo de cancelamento.
        /// </summary>
        /// <param name="team">A equipa responsável pelo cancelamento.</param>
        /// <param name="match">A partida que está a ser cancelada.</param>
        /// <param name="description">O motivo detalhado do cancelamento.</param>
        public CancelledMatch(Team team, Matches match, string description)
        {
            this.Team = team;
            this.IdTeam = team.Id;
            this.Match = match;
            this.IdMatch = match.Id;
            this.Description = description;
            this.TimeCancellation = DateTime.UtcNow;
        }
    }
}

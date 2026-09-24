using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa as estatísticas e o resultado de uma equipa durante uma partida específica.
    /// 
    /// Esta entidade armazena o número de golos e o resultado final (Vitória, Derrota, Empate)
    /// em relação ao jogo em que participou.
    /// </summary>
    public class TeamStatistics
    {
        /// <summary>
        /// O identificador único (GUID) do registo de estatísticas.
        /// Serve como a chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Entidade de navegação para a Equipa à qual estas estatísticas pertencem.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para a Equipa.
        /// </summary>
        [Required]
        [ForeignKey("Team")]
        public Guid IdTeam { get; set; }

        /// <summary>
        /// O número de golos marcados pela equipa nesta partida.
        /// </summary>
        /// <value>O valor deve estar dentro do intervalo permitido (0 a 100).</value>
        [Required]
        [Range(0, ModelConstants.GeneralConst.MaxGoals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int NumGoals { get; set; } = 0;

        /// <summary>
        /// A Chave Estrangeira (FK) para a Partida ([Matches]) à qual estas estatísticas pertencem.
        /// </summary>
        [Required]
        [ForeignKey("Match")]
        public Guid MatchesId { get; set; }

        /// <summary>
        /// Entidade de navegação para a Partida ([Matches]) associada.
        /// </summary>
        public Matches Match { get; set; }

        /// <summary>
        /// O resultado final da equipa nesta partida (Vitória, Derrota, Empate ou Não Jogado).
        /// Padrão: [MatchResult.UNPLAYED].
        /// </summary>
        [Required]
        public MatchResult MatchResult { get; set; } = MatchResult.UNPLAYED;
        
        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public TeamStatistics() { }

        /// <summary>
        /// Construtor utilizado para inicializar a entidade com a Equipa associada.
        /// </summary>
        /// <param name="team">A equipa para a qual estas estatísticas serão registadas.</param>
        public TeamStatistics(Team team)
        {
            this.Team = team;
            this.IdTeam = team.Id;
        }
    }
}
using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa um nível de classificação (Rank) no sistema (ex: Ouro, Prata, Bronze).
    /// 
    /// Define os critérios de pontuação (pontos ganhos/perdidos) e o limite necessário para a promoção.
    /// Esta entidade é hierárquica, referenciando o rank anterior e o próximo.
    /// </summary>
    public class Rank
    {
        /// <summary>
        /// O identificador único (GUID) do Rank.
        /// Serve como chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// O nome do nível de classificação (ex: "Ouro", "Prata").
        /// </summary>
        /// <value>A string deve cumprir os limites de comprimento definidos em [ModelConstants.RankConts.MaxNameLength].</value>
        [Required]
        [StringLength(ModelConstants.RankConts.MaxNameLength, MinimumLength = ModelConstants.RankConts.MinNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// O número de pontos que uma equipa ganha por cada vitória.
        /// </summary>
        /// <value>O valor deve ser maior ou igual a 1.</value>
        [Required]
        [Range(ModelConstants.RankConts.MinPointsWin, ModelConstants.RankConts.MaxPointsWin, ErrorMessage = "O número de pontos ganhos por vitoria deve ser superior ou igual a 1")]
        public int WinPoints { get; set; }

        /// <summary>
        /// O número de pontos que uma equipa ganha por cada empate.
        /// </summary>
        [Required]
        public int DrawPoints { get; set; }

        /// <summary>
        /// O número de pontos que uma equipa perde por cada derrota.
        /// Este valor pode ser zero ou negativo.
        /// </summary>
        /// <value>O valor deve ser menor ou igual a 0.</value>
        [Required]
        [Range(ModelConstants.RankConts.MinPointLose, ModelConstants.RankConts.MaxPointLose, ErrorMessage = "O número de pontos perdidos deve ser igual ou inferior a 0")]
        public int LosePoints { get; set; }

        /// <summary>
        /// O número total de pontos que uma equipa deve ter para ser promovida para este nível.
        /// </summary>
        /// <value>O valor deve ser maior ou igual a 0.</value>
        [Required]
        [Range(ModelConstants.RankConts.MinPointToPromotion, ModelConstants.RankConts.MaxPointToPromotion, ErrorMessage = "O número de pontos para estar num rank deve ser maior ou igaul a 0")]
        public int PointsToPromotion { get; set; }

        /// <summary>
        /// Entidade de navegação para o próximo Rank na hierarquia (para promoção).
        /// É nulo se este for o Rank mais alto.
        /// </summary>
        public Rank? NextRank { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para o próximo Rank.
        /// É nula se este for o Rank mais alto.
        /// </summary>
        [ForeignKey("NextRank")]
        public Guid? IdNextRank { get; set; } 

        /// <summary>
        /// Entidade de navegação para o Rank anterior na hierarquia (para despromoção).
        /// É nulo se este for o Rank mais baixo.
        /// </summary>
        public Rank? PreviousRank { get; set; }

        /// <summary>
        /// A Chave Estrangeira (FK) para o Rank anterior.
        /// É nula se este for o Rank mais baixo.
        /// </summary>
        [ForeignKey("PreviousRank")]
        public Guid? IdPreviousRank { get; set; } 

        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public Rank() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo nível de Rank.
        /// </summary>
        /// <param name="name">O nome do Rank.</param>
        /// <param name="winPoints">Pontos atribuídos por vitória.</param>
        /// <param name="drawPoints">Pontos atribuídos por empate.</param>
        /// <param name="losePoints">Pontos perdidos por derrota.</param>
        /// <param name="pointsToPromotion">Pontos necessários para entrar neste Rank.</param>
        /// <param name="nextRank">A entidade do próximo Rank.</param>
        /// <param name="previousRank">A entidade do Rank anterior.</param>
        public Rank(string name, int winPoints, int drawPoints, int losePoints, int pointsToPromotion, Rank nextRank, Rank previousRank)
        {
            this.Name = name;
            this.WinPoints = winPoints;
            this.DrawPoints = drawPoints;
            this.LosePoints = losePoints;
            this.PointsToPromotion = pointsToPromotion;
            this.PreviousRank = previousRank;
            this.IdPreviousRank = previousRank?.Id;
            this.NextRank = nextRank;
            this.IdNextRank = nextRank?.Id;
        }
    }
}

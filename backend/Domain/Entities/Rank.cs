using System.ComponentModel.DataAnnotations;

/***
 * Entidade que representa um rank no sistema.
 */
namespace Domain.Entities
{
    public class Rank
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O número de pontos ganhos por vitoria deve ser superior ou igual a 1")]
        public int WinPoints { get; set; }

        public int DrawPoints { get; set; }

        [Range(int.MinValue, 0, ErrorMessage = "O número de pontos perdidos deve ser igual ou inferior a 0")]
        public int LosePoints { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "O número de pontos para estar num rank deve ser maior ou igaul a 0")]
        public int PointsToPromotion { get; set; }

        public Rank? NextRank { get; set; }

        public Guid? IdNextRank { get; set; } //FK

        public Rank? PreviousRank { get; set; } 

        public Guid? IdPreviousRank { get; set; } //FK

        // EF
        protected Rank() { }

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

        public override string ToString()
        {
            return $"Rank: {Name}, Win Points: {WinPoints}, Draw Points: {DrawPoints}, Lose Points: {LosePoints}, Points To Promotion: {PointsToPromotion}";
        }
    }
}

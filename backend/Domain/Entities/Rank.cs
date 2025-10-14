using System;

/**
 * Entidade que representa um rank no sistema.
 * 
 * Falta meter o NextRank e PreviousRank como Null e meter regra que so pode estar null 
 * se o outro não estiver
 */
namespace Domain.Entities
{
    internal class Rank
    {
        [Key]
        [MinLength = 32, MaxLength = 50]
        public string Id { get; set; }

        [MaxLength = 50]
        public string Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O número de pontos ganhos por vitoria deve ser superior ou igual a 1")]
        public int winPoints { get; set; }

        public int drawPoints { get; set; }

        [Range(int.MinValue, 0, ErrorMessage = "O número de pontos perdidos deve ser igual ou inferior a 0")]
        public int losePoints { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "O número de pontos para estar num rank deve ser maior ou igaul a 0")]
        public int PointsToPromotion { get; set; }

        public Rank? NextRank { get; set; } //Falta meter restrição

        public string? idNextRank { get; set; } //FK

        public Rank? PreviousRank { get; set; } //Falta meter restrição

        public string? idPreviousRank { get; set; } //FK
    }
}

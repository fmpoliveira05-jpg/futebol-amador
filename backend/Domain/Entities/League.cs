using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Liga (um escalão). O nível 1 é o mais alto; as equipas sobem para o nível anterior e descem para o seguinte.
    /// </summary>
    public class League
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(60)]
        public string Name { get; set; } = null!;

        /// <summary>Escalão: 1 é o mais alto. É único.</summary>
        [Range(1, 50)]
        public int Level { get; set; }

        /// <summary>Quantas equipas sobem no fim da época (as primeiras).</summary>
        [Range(0, 10)]
        public int PromotionSpots { get; set; }

        /// <summary>Quantas equipas descem no fim da época (as últimas).</summary>
        [Range(0, 10)]
        public int RelegationSpots { get; set; }

        /// <summary>Duração de uma época, em dias ("a liga termina ao fim de X dias").</summary>
        [Range(7, 366)]
        public int SeasonDurationDays { get; set; }

        /// <summary>Nome do troféu entregue ao campeão.</summary>
        [Required, MaxLength(80)]
        public string TrophyName { get; set; } = null!;

        public ICollection<Team> Teams { get; set; } = new List<Team>();

        public ICollection<Season> Seasons { get; set; } = new List<Season>();
    }
}

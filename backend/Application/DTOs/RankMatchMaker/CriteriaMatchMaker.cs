using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class CriteriaMatchMaker
    {
        [Required]
        public int differencPoints { get; set; }

        [Required]
        public float diffAverageAge { get; set; }
    }
}

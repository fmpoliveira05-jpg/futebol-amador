using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class CriteriaMatchMaker
    {
        [Required]
        [Range(ModelConstants.DeafultCriteriaMatchMaker.differencePoint, ModelConstants.DeafultCriteriaMatchMaker.maxDifferencePoint)]
        public int differencPoints { get; set; }

        [Required]
        [Range(ModelConstants.DeafultCriteriaMatchMaker.differenceAverageAge, ModelConstants.DeafultCriteriaMatchMaker.maxDifferenceAverageAge)]
        public float diffAverageAge { get; set; }
    }
}

using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Match
{
    public class ResultMatchDto
    {
        [Required]
        public Guid IdMatch { get; set; }

        [Required]
        public Guid IdTeam { get; set; }

        [Required]
        [Range(ModelConstants.GeneralConst.MinGoals, ModelConstants.GeneralConst.MaxGoals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int NumGoalsTeam { get; set; }

        [Required]
        public Guid IdOpponent { get; set; }

        [Required]
        [Range(ModelConstants.GeneralConst.MinGoals, ModelConstants.GeneralConst.MaxGoals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int NumGoalsOpponent { get; set; }
    }
}

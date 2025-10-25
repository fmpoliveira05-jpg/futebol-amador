using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class FinishMatchDTO
    {
        public Guid IdTeam { get; set; }

        [Range(0, ModelConstants.GeneralConst.MaxGoals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int NumGoalsTeam { get; set; }
        public Guid IdOpponent { get; set; }

        [Range(0, ModelConstants.GeneralConst.MaxGoals, ErrorMessage = "O número de golos deve estar entre 0 e 100")]
        public int NumGoalsOpponent { get; set; }
    }
}
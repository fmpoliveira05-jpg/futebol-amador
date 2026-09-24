using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class TeamStatisticsDto
    {
        [Required]
        public Guid IdTeam { get; set; }

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string Name { get; set; } = null!;

        [Range(ModelConstants.GeneralConst.MinGoals, ModelConstants.GeneralConst.MaxGoals)]
        public int NumGoals { get; set; }
    }
}

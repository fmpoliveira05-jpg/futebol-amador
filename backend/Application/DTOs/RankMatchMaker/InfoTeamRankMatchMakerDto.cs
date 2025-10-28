
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class InfoTeamRankMatchMakerDto
    {
        [Required]
        public Guid IdTeam { get; set; }

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [Range(ModelConstants.TeamConst.MinAverageAge, ModelConstants.TeamConst.MaxAverageAge)]
        public float AverageAge { get; set; }

        [Required]
        public InfoRankMatchMakerDto Rank { get; set; }

        [Required]
        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int NumberPointsTeam { get; set; }

        public string cidade { get; set; }
    }
}

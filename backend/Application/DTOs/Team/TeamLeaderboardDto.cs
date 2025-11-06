using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class TeamLeaderboardDto
    {
        [Required]
        [Range(ModelConstants.TeamLeaderBoardConst.FirstPosition, ModelConstants.TeamLeaderBoardConst.LastPosition)]
        public int Position { get; set; }

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string TeamName { get; set; } = null!;

        [Required]
        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints)]
        public int CurrentPoints { get; set; }

        [Required]
        [MinLength(ModelConstants.RankConts.MinNameLength), MaxLength(ModelConstants.RankConts.MaxNameLength)]
        public string RankName { get; set; } = null!;
    }
}

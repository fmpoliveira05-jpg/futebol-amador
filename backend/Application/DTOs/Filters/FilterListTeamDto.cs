using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    public class FilterListTeamDto
    {
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string? NameTeam { get; set; }

        [MinLength(ModelConstants.RankConts.MinNameLength), MaxLength(ModelConstants.RankConts.MaxNameLength)]
        public string? NameRank { get; set; }

        [MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string? City { get; set; }
        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints)]
        public int? MinNumberPoints { get; set; }

        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints)]
        public int? MaxNumberPoints { get; set; }

        [Range(ModelConstants.TeamConst.MinAverageAge, ModelConstants.TeamConst.MaxAverageAge)]
        public float? MinAge { get; set; }

        [Range(ModelConstants.TeamConst.MinAverageAge, ModelConstants.TeamConst.MaxAverageAge)]
        public float? MaxAge { get; set; }

        [Range(ModelConstants.TeamConst.MinMembers, ModelConstants.TeamConst.MaxMembers)]
        public int? MinNumberPlayers { get; set; }

        [Range(ModelConstants.TeamConst.MinMembers, ModelConstants.TeamConst.MaxMembers)]
        public int? MaxNumberPlayers { get; set; }
    }
}

using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    public class FilterPlayersWithoutTeamDto
    {
        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string? PlayerName { get; set; }

        [MinLength(ModelConstants.GeneralConst.MinCityLength), MaxLength(ModelConstants.GeneralConst.MaxCityLength)]
        public string? City { get; set; }

        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int? MinAge { get; set; }

        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int? MaxAge { get; set; }

        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int? MinHeight { get; set; }

        [Range(ModelConstants.PlayerConst.MinHeight, ModelConstants.PlayerConst.MaxHeight)]
        public int? MaxHeight { get; set; }

        public Position? Position { get; set; }
    }
}

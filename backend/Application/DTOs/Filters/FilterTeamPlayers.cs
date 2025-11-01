using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    public class FilterTeamPlayers
    {
        public bool? IsAdmin { get; set; }

        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string? Name { get; set; }

        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int? MinAge { get; set; }

        [Range(ModelConstants.UserConst.MinAge, ModelConstants.UserConst.MaxAge)]
        public int? MaxAge { get; set; }

        public Position? Position { get; set; }
    }
}
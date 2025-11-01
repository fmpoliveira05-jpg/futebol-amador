using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    public class FilterPostPoneMatchDto
    {
        [MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string? NameOpponent { get; set; }
        public bool? IsHome { get; set; }
        public DateOnly? MinDateGame { get; set; }
        public DateOnly? MaxDateGame { get; set; }
        public DateOnly? MinDatePostPoneGame { get; set; }
        public DateOnly? MaxDatePostPoneGame { get; set; }
    }
}

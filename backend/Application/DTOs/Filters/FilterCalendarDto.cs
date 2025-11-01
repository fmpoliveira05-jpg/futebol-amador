using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    public class FilterCalendarDto
    {
        public bool? IsRealized { get; set; }
        public bool? IsRanqued { get; set; }
        public bool? IsHome { get; set; }
        public DateOnly? MinDate { get; set; }
        public DateOnly? MaxDate { get; set; }

        [MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string? NameOpponent { get; set; }
    }
}

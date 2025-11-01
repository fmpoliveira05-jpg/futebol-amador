using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    public class FilterMatchInvitesDto
    {
        [MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string? SenderName { get; set; }
        public DateOnly? MinDate { get; set; }
        public DateOnly? MaxDate { get; set; }
    }
}

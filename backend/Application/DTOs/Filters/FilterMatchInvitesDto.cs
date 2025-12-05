using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Filters
{
    public class FilterMatchInvitesDto
    {
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string? SenderName { get; set; }
        public string? SenderId { get; set; }
        public string? MatchInviteId { get; set; }
        public DateOnly? MinDate { get; set; }
        public DateOnly? MaxDate { get; set; }
    }
}

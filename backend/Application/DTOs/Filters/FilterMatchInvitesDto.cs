namespace Application.DTOs.Filters
{
    public class FilterMatchInvitesDto
    {
        public string? SenderName { get; set; }
        public DateOnly? MinDate { get; set; }
        public DateOnly? MaxDate { get; set; }
    }
}

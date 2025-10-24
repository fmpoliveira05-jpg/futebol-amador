namespace Application.DTOs.Filters
{
    public class FilterCalendar
    {
        public bool? IsRealized { get; set; }
        public bool? IsRanqued { get; set; }
        public bool? IsHome { get; set; }
        public DateOnly? MinDate { get; set; }
        public DateOnly? MaxDate { get; set; }
        public string? NameOpponent { get; set; }
    }
}

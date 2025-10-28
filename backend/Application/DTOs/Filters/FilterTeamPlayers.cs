using Domain.Enums;

namespace Application.DTOs.Filters
{
    public class FilterTeamPlayers
    {
        public bool? IsAdmin { get; set; }

        public string? Name { get; set; }

        public int? MinAge { get; set; }

        public int? MaxAge { get; set; }

        public Position? Position { get; set; }
    }
}
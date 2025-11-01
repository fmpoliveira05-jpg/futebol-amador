using Application.DTOs.Pitch;
using Application.DTOs.Team;
using Domain.Enums;

namespace Application.DTOs.Match
{
    public class InfoMatchCalendar
    {
        public Guid IdMatch { get; set; }
        public MatchStatus MatchStatus { get; set; }
        public DateTime GameDate { get; set; }
        public string? Result { get; set; }
        public MatchResult MatchResult { get; set; }
        public TeamDto Team { get; set; }
        public TeamDto Opponent { get; set; }
        public PitchDto pitchGame { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Match
{
    public class MatchDto
    {
        public Guid MatchId { get; set; }
        public DateTime MatchDate { get; set; }

        public string PitchName { get; set; }
        public string Location { get; set; }
        public Guid HomeTeamId { get; set; }
        public string HomeTeamName { get; set; }
        
        public Guid AwayTeamId { get; set; }

        public string AwayTeamName { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }

    }
}

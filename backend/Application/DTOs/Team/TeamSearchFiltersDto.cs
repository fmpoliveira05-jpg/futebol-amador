using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Team
{
    public class TeamSearchFiltersDto
    {
        public string Name { get; set; }

        public string RankName { get; set; }

        public int MinAvgAge { get; set; }

        public int MaxAvgAge { get; set; }

        public string PitchAddress { get; set; }
    }
}

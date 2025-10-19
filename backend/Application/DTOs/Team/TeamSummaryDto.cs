using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Team
{
    internal class TeamSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RankName { get; set; }
        public int PlayerCount { get; set; }
    }
}

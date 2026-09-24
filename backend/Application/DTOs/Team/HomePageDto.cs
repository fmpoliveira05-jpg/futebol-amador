using Application.DTOs.Match;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Team
{
    public class HomePageDto
    {
        [Required]
        public TeamDto Team { get; set; } = null!;

        public List<InfoMatch>? NextsMatchs { get; set; }

        public List<VitorySequenceTeam>? HistoricPreviousGames { get; set; }
    }
}

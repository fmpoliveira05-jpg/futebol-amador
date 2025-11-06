using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class StartSearchDto
    {
        [Required]
        public Guid IdTeam { get; set; }

        [Required]
        public TimeOnly HoursGame { get; set; }
    }
}

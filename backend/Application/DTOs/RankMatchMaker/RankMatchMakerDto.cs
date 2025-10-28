using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class RankMatchMakerDto
    {
        [Required]
        public Guid idPlayer;
        
        [Required]
        public Guid idTeam;

        [Required]
        public InfoTeamRankMatchMakerDto Team { get; set; }
    }
}

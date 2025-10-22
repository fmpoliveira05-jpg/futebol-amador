using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.PlayerDTOs;

namespace Application.DTOs.Team
{
    public class TeamDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public string? Description { get; set; } 
        public DateTime FoundationDate { get; set; } 
        public int TotalPoints { get; set; }

        //Alterar para o enum dos nomes dos ranks
        public string? RankName { get; set; }

        public string PitchDto { get; set; }

        public List<PlayerDetailsDTO> Players { get; set; }
    }


}

using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class InfoRankMatchMakerDto
    {
        [Required]
        public Guid IdRank { get; set; }

        [Required]
        [MinLength(ModelConstants.RankConts.MinNameLength), MaxLength(ModelConstants.RankConts.MaxNameLength)]
        public string Name { get; set; }
    }
}

using Application.DTOs.Rank;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.RankMatchMaker
{
    public class InfoTeamRankMatchMakerDto
    {
        [Required]
        public Guid IdTeam { get; set; }

        [Required]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [Range(ModelConstants.TeamConst.MinAverageAge, ModelConstants.TeamConst.MaxAverageAge)]
        public float AverageAge { get; set; }

        [Required]
        public InfoRankDto Rank { get; set; }

        [Required]
        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int NumberPointsTeam { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public DateTime timeEntry = DateTime.UtcNow;

        [Required]
        public DateTime GameDate { get; set; }
        /**
         True --> pode subir ou descer
         False --> vai jogar apenas com teams do mesmo rank
         */
        [Required]
        public bool isNearToChangeRank { get; set; }

        [Required]
        public string NextOrPreviousRank { get; set; } = "";
    }
}

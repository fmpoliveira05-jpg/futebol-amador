using Application.DTOs.Team;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MatchInvites
{
    public class InfoMatchInviteDto
    {
        [Required(ErrorMessage = "O id da matchInvite é obrigatorio")]
        public Guid Id { get; set; }

        [Required]
        public TeamDto Sender { get; set; } = null!;

        [Required]
        public TeamDto Receiver { get; set; } = null!;

        [Required(ErrorMessage = "A data do jogo é obrigatoria")]
        public DateTime GameDate { get; set; }

        [Required(ErrorMessage = "O nome do campo é obrigatório")]
        [MinLength(ModelConstants.PitchConst.MinNameLength), MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string NamePitch { get; set; } = null!;

        public bool? isHome { get; set; }
    }
}

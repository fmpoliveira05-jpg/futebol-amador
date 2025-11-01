using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MatchInvites
{
    public class InfoMatchInviteDto
    {
        [Required(ErrorMessage = "O id da matchInvite é obrigatorio")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O id do emissor é obrigatório")]
        public Guid IdSender { get; set; }

        [Required(ErrorMessage = "O nome do emissor é obrigatorio")]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string NameSender { get; set; } = null!;

        [Required(ErrorMessage = "O id do recetor é obrigatório")]
        public Guid IdReceiver { get; set; }

        [Required(ErrorMessage = "O nome do recetor é obrigatorio")]
        [MinLength(ModelConstants.TeamConst.MinNameLength), MaxLength(ModelConstants.TeamConst.MaxNameLength)]
        public string NameReceiver { get; set; } = null!;

        [Required(ErrorMessage = "A data do jogo é obrigatoria")]
        public DateTime GameDate { get; set; }

        [Required(ErrorMessage = "O nome do campo é obrigatório")]
        [MinLength(ModelConstants.PitchConst.MinNameLength), MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string NamePitch { get; set; } = null!;
    }
}

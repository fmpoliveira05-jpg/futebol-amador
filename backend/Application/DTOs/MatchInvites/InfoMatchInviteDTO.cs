using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MatchInvites
{
    public class InfoMatchInviteDTO
    {
        [Required(ErrorMessage = "O id da matchInvite é obrigatorio")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O id do emissor é obrigatório")]
        public Guid IdSender { get; set; }

        [Required(ErrorMessage = "O nome do emissor é obrigatorio")]
        public string NameSender { get; set; }

        [Required(ErrorMessage = "O id do recetor é obrigatório")]
        public Guid IdReceiver { get; set; }

        [Required(ErrorMessage = "O nome do recetor é obrigatorio")]
        public string NameReceiver { get; set; }

        [Required(ErrorMessage = "A data do jogo é obrigatoria")]
        public DateTime GameDate { get; set; }

        [Required(ErrorMessage = "O nome do campo é obrigatório")]
        public string NamePitch { get; set; }
    }
}

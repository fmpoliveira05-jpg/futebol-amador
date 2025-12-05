using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MatchInvites
{
    public class SendMatchInviteDto
    {
        [Required(ErrorMessage = "O Id da equipa que enviou é obrigatório!")]
        public Guid IdSender { get; set; }

        [Required(ErrorMessage = "O Id da equipa que recebeu o convite é obrigatório!")]
        public Guid IdReceiver { get; set; }

        [Required(ErrorMessage = "É obrigatório especificar a data do jogo")]
        public DateTime GameDate { get; set; }

        [Required(ErrorMessage = "É obrigatório especificar o id do Campo")]
        public bool homePitch { get; set; } 
    }
}
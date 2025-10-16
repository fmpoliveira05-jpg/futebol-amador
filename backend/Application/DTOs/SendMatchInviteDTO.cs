using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class SendMatchInviteDTO
    {
        [Required(ErrorMessage = "O Id da equipa que enviou é obrigatório!")]
        public Guid IdSender { get; set; }

        [Required(ErrorMessage = "O Id da equipa que recebeu o convite é obrigatório!")]
        public Guid IdReceiver { get; set; }

        [Required(ErrorMessage = "É obrigatório especificar a equipa que enviou o convite!")]
        public Teams Sender { get; set; }

        [Required(ErrorMessage = "É obrigatório especificar a equipa que recebeu o convite!")]
        public Teams Receiver { get; set; }

        [Required(ErrorMessage = "É obrigatório especificar a data do jogo")]
        public DateTime GameDate { get; set; }

        [Required(ErrorMessage = "É obrigatório especificar o campo do jogo")]
        public Pitch Pitch { get; set; }

        [Required(ErrorMessage = "É obrigatório especificar o id do Campo")]
        public Guid IdPitch { get; set; }
    }
}
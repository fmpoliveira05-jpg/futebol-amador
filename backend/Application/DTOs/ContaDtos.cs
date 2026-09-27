using Application.Validators.Atributos;
using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>Pedido de reenvio da confirmação do e-mail (<c>POST /api/User/resend-verification</c>).</summary>
    public class ReenviarVerificacaoDto
    {
        [Required]
        [MaxLength(ModelConstants.UserConst.MaxEmailLength)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(ModelConstants.PasswordConst.MaxLength)]
        public string Password { get; set; } = null!;

        /// <summary>Campo-armadilha (honeypot) do formulário web: tem de vir vazio.</summary>
        [CampoArmadilha]
        public string? Website { get; set; }
    }

    /// <summary>Pedido de recuperação da palavra-passe (<c>POST /api/User/forgot-password</c>).</summary>
    public class RecuperarPalavraPasseDto
    {
        [Required]
        [MaxLength(ModelConstants.UserConst.MaxEmailLength)]
        public string Email { get; set; } = null!;

        /// <summary>Campo-armadilha (honeypot) do formulário web: tem de vir vazio.</summary>
        [CampoArmadilha]
        public string? Website { get; set; }
    }

    /// <summary>Resposta genérica dos pedidos que não podem revelar se uma conta existe.</summary>
    public class MensagemDto
    {
        public string Mensagem { get; set; } = null!;
    }

    /// <summary>
    /// Resposta do registo quando é preciso confirmar o e-mail antes de entrar (não traz sessão).
    /// </summary>
    public class RegistoPendenteDto
    {
        public string PlayerId { get; set; } = null!;

        /// <summary>Sempre <c>true</c>: o cliente deve pedir ao utilizador que confirme o e-mail.</summary>
        public bool VerificacaoEmailPendente { get; set; } = true;

        public string Mensagem { get; set; } = null!;
    }
}

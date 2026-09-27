using System.ComponentModel.DataAnnotations;

namespace Application.Validators.Atributos
{
    /// <summary>
    /// Campo-armadilha (honeypot): o formulário web esconde-o das pessoas, por isso só um robô que
    /// preenche todos os campos lhe dá valor. Um pedido com o campo preenchido é recusado.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class CampoArmadilhaAttribute : ValidationAttribute
    {
        public CampoArmadilhaAttribute()
            : base("Pedido inválido.")
        {
        }

        public override bool IsValid(object? value) => string.IsNullOrEmpty(value as string);
    }
}

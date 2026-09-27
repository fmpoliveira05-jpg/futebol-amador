using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.Validators.Atributos
{
    /// <summary>
    /// Política de palavras-passe: entre <see cref="ModelConstants.PasswordConst.MinLength"/> e
    /// <see cref="ModelConstants.PasswordConst.MaxLength"/> caracteres, com pelo menos uma
    /// minúscula, uma maiúscula, um algarismo e um símbolo.
    /// </summary>
    /// <remarks>
    /// Os clientes (web e Android) mostram a mesma regra, mas é aqui que ela é garantida.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class PalavraPasseSeguraAttribute : ValidationAttribute
    {
        /// <summary>Mensagem única para qualquer falha (não diz qual das regras falhou).</summary>
        public const string Mensagem =
            "A palavra-passe tem de ter entre 10 e 128 caracteres, com maiúsculas, minúsculas, algarismos e símbolos.";

        public PalavraPasseSeguraAttribute()
            : base(Mensagem)
        {
        }

        /// <summary>Aplica a política a um valor (também usado fora dos DTOs).</summary>
        public static bool Cumpre(string? valor)
        {
            if (string.IsNullOrEmpty(valor) ||
                valor.Length < ModelConstants.PasswordConst.MinLength ||
                valor.Length > ModelConstants.PasswordConst.MaxLength)
            {
                return false;
            }

            return valor.Any(char.IsLower) &&
                   valor.Any(char.IsUpper) &&
                   valor.Any(char.IsDigit) &&
                   valor.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        }

        public override bool IsValid(object? value) => Cumpre(value as string);
    }
}

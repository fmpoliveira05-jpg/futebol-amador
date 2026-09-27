using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Application.Validators.Atributos
{
    /// <summary>
    /// Valida o emblema de uma equipa. Aceita:
    /// <list type="bullet">
    /// <item>nada (a equipa fica com o emblema predefinido);</item>
    /// <item>um URL HTTPS de um anfitrião autorizado (<see cref="ModelConstants.TeamConst.AllowedIconHosts"/>,
    /// o Cloudinary usado pela app Android), com até <see cref="ModelConstants.TeamConst.MaxIconUrlLength"/> caracteres;</item>
    /// <item>uma imagem embebida PNG, JPEG ou WebP em base64 (<c>data:image/...;base64,</c>), como a
    /// que a web gera, com até <see cref="ModelConstants.TeamConst.MaxIconDataUrlLength"/> caracteres.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Recusa outros esquemas (<c>http:</c>, <c>javascript:</c>, <c>data:image/svg+xml</c>, ...) e
    /// anfitriões, para o emblema não servir para rastrear quem vê a equipa nem para injetar conteúdo.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed partial class EmblemaPermitidoAttribute : ValidationAttribute
    {
        public const string Mensagem =
            "O emblema tem de ser uma imagem PNG, JPEG ou WebP enviada pela aplicação (URL do Cloudinary ou imagem até 150 KB).";

        [GeneratedRegex(@"^data:image/(png|jpeg|webp);base64,[A-Za-z0-9+/]+={0,2}$")]
        private static partial Regex DataUrlImagem();

        public EmblemaPermitidoAttribute()
            : base(Mensagem)
        {
        }

        /// <summary>Aplica a regra a um valor.</summary>
        public static bool Valido(string? valor)
        {
            if (string.IsNullOrEmpty(valor))
            {
                return true;
            }

            if (valor.StartsWith("data:", StringComparison.Ordinal))
            {
                return valor.Length <= ModelConstants.TeamConst.MaxIconDataUrlLength && DataUrlImagem().IsMatch(valor);
            }

            if (valor.Length > ModelConstants.TeamConst.MaxIconUrlLength ||
                !Uri.TryCreate(valor, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttps &&
                   uri.IsDefaultPort &&
                   string.IsNullOrEmpty(uri.UserInfo) &&
                   ModelConstants.TeamConst.AllowedIconHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase);
        }

        public override bool IsValid(object? value) => Valido(value as string);
    }
}

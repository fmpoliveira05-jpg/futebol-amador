using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Api.Seguranca
{
    /// <summary>Parâmetros de um upload assinado para o Cloudinary.</summary>
    /// <param name="UploadUrl">Endpoint de upload (<c>https://api.cloudinary.com/v1_1/{cloud}/image/upload</c>).</param>
    /// <param name="ApiKey">Chave pública da conta (não é segredo).</param>
    /// <param name="Timestamp">Segundos Unix; o Cloudinary recusa assinaturas com mais de 1 hora.</param>
    /// <param name="Signature">SHA-1 dos parâmetros assinados com o <c>api_secret</c>.</param>
    /// <param name="Folder">Pasta do utilizador (<c>equipas/{uid}</c>).</param>
    /// <param name="UploadPreset">Preset assinado (com as restrições do lado do Cloudinary).</param>
    /// <param name="AllowedFormats">Formatos aceites (<c>jpg,png,webp</c>).</param>
    /// <param name="MaxBytes">Tamanho máximo que o cliente deve enviar.</param>
    public sealed record AssinaturaUploadDto(
        string UploadUrl, string ApiKey, long Timestamp, string Signature,
        string Folder, string UploadPreset, string AllowedFormats, long MaxBytes);

    /// <summary>
    /// Assina uploads para o Cloudinary no servidor, em vez de a app usar um preset não assinado
    /// (que permitia a qualquer pessoa com o nome da conta enviar ficheiros).
    /// </summary>
    public interface IAssinaturaCloudinary
    {
        /// <summary>Há <c>Cloudinary:CloudName</c>, <c>Cloudinary:ApiKey</c> e <c>Cloudinary:ApiSecret</c> configurados.</summary>
        bool Configurado { get; }

        /// <summary>Gera a assinatura para um upload do utilizador indicado.</summary>
        AssinaturaUploadDto Assinar(string uid);
    }

    /// <inheritdoc />
    public sealed class AssinaturaCloudinary : IAssinaturaCloudinary
    {
        public const string FormatosPermitidos = "jpg,png,webp";
        public const long TamanhoMaximo = 2 * 1024 * 1024;

        private readonly string? cloudName;
        private readonly string? apiKey;
        private readonly string? apiSecret;
        private readonly string preset;
        private readonly TimeProvider relogio;

        public AssinaturaCloudinary(IConfiguration configuration, TimeProvider relogio)
        {
            cloudName = configuration["Cloudinary:CloudName"];
            apiKey = configuration["Cloudinary:ApiKey"];
            // Aceita também a variável de ambiente CLOUDINARY_API_SECRET.
            apiSecret = configuration["Cloudinary:ApiSecret"] ?? configuration["CLOUDINARY_API_SECRET"];
            preset = configuration["Cloudinary:UploadPreset"] ?? "equipas_assinado";
            this.relogio = relogio;
        }

        public bool Configurado =>
            !string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret);

        public AssinaturaUploadDto Assinar(string uid)
        {
            if (!Configurado)
            {
                throw new InvalidOperationException("O Cloudinary não está configurado.");
            }

            // O uid do Firebase só tem letras e algarismos; qualquer outro carácter é removido.
            var pasta = "equipas/" + new string(uid.Where(char.IsLetterOrDigit).ToArray());
            var timestamp = relogio.GetUtcNow().ToUnixTimeSeconds();

            var parametros = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["allowed_formats"] = FormatosPermitidos,
                ["folder"] = pasta,
                ["timestamp"] = timestamp.ToString(CultureInfo.InvariantCulture),
                ["upload_preset"] = preset,
            };

            return new AssinaturaUploadDto(
                $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload",
                apiKey!,
                timestamp,
                Assinatura(parametros, apiSecret!),
                pasta,
                preset,
                FormatosPermitidos,
                TamanhoMaximo);
        }

        /// <summary>
        /// Assinatura do Cloudinary: parâmetros por ordem alfabética em <c>chave=valor</c> unidos por
        /// <c>&amp;</c>, seguidos do <c>api_secret</c>, em SHA-1 hexadecimal.
        /// </summary>
        internal static string Assinatura(IEnumerable<KeyValuePair<string, string>> parametros, string segredo)
        {
            var texto = string.Join("&", parametros.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => $"{p.Key}={p.Value}")) + segredo;
            return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(texto))).ToLowerInvariant();
        }
    }
}

using Application.Interfaces.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Api.Operacao
{
    /// <summary>
    /// Apaga as imagens de um utilizador no Cloudinary (pasta <c>equipas/{uid}</c>, onde a API
    /// assina os uploads) quando a conta é eliminada. As que ainda são o emblema de uma equipa ficam.
    /// </summary>
    /// <remarks>
    /// Usa a Admin API (<c>GET/DELETE /resources/image/upload</c>) com a chave e o segredo da API.
    /// Sem o Cloudinary configurado não faz nada.
    /// </remarks>
    public sealed class ImagensCloudinary : IImagensUtilizador
    {
        private readonly HttpClient http;
        private readonly ILogger<ImagensCloudinary> logger;
        private readonly string? cloudName;
        private readonly string? apiKey;
        private readonly string? apiSecret;

        public ImagensCloudinary(HttpClient http, IConfiguration configuracao, ILogger<ImagensCloudinary> logger)
        {
            this.http = http;
            this.logger = logger;
            cloudName = configuracao["Cloudinary:CloudName"];
            apiKey = configuracao["Cloudinary:ApiKey"];
            apiSecret = configuracao["Cloudinary:ApiSecret"] ?? configuracao["CLOUDINARY_API_SECRET"];
        }

        private bool Configurado =>
            !string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret);

        public async Task ApagarDoUtilizadorAsync(string uid, IReadOnlyCollection<string> emUso, CancellationToken cancelamento = default)
        {
            if (!Configurado)
            {
                return;
            }

            var pasta = "equipas/" + new string(uid.Where(char.IsLetterOrDigit).ToArray()) + "/";
            var baseUrl = $"https://api.cloudinary.com/v1_1/{cloudName}/resources/image/upload";
            var autenticacao = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}")));

            using var listar = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?prefix={Uri.EscapeDataString(pasta)}&max_results=500");
            listar.Headers.Authorization = autenticacao;
            using var resposta = await http.SendAsync(listar, cancelamento);
            resposta.EnsureSuccessStatusCode();

            using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(cancelamento));
            var apagar = json.RootElement.GetProperty("resources").EnumerateArray()
                .Where(r => !emUso.Contains(r.GetProperty("secure_url").GetString() ?? ""))
                .Select(r => r.GetProperty("public_id").GetString()!)
                .ToList();

            foreach (var bloco in apagar.Chunk(100))
            {
                var parametros = string.Join("&", bloco.Select(id => "public_ids[]=" + Uri.EscapeDataString(id)));
                using var remover = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}?{parametros}");
                remover.Headers.Authorization = autenticacao;
                using var r = await http.SendAsync(remover, cancelamento);
                r.EnsureSuccessStatusCode();
            }

            logger.LogInformation("Eliminação da conta: {Apagadas} imagens apagadas do Cloudinary.", apagar.Count);
        }
    }
}

using System.Text.Json;

namespace Api.IntegrationTests.Helpers
{
    /// <summary>
    /// Extensões úteis para HttpClient nos testes de integração.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Lê a resposta HTTP e faz deserialize do JSON para o tipo T.
        /// </summary>
        public static async Task<T?> ReadResponseAsync<T>(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
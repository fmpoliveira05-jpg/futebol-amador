namespace Application.DTOs.Player
{
    /// <summary>
    /// Reduz uma morada à zona (localidade), para as listas de jogadores não exporem a morada completa.
    /// </summary>
    /// <remarks>
    /// As moradas são guardadas como <c>"Rua ..., Localidade"</c> (é assim que o filtro por cidade as
    /// procura): a zona é o último troço depois da vírgula. Sem vírgula, só se devolve o texto se não
    /// tiver algarismos (uma localidade, não uma rua com número).
    /// </remarks>
    public static class Zona
    {
        public static string DaMorada(string? morada)
        {
            if (string.IsNullOrWhiteSpace(morada))
            {
                return string.Empty;
            }

            var indice = morada.LastIndexOf(',');
            var zona = (indice >= 0 ? morada[(indice + 1)..] : morada).Trim();

            return zona.Any(char.IsDigit) ? string.Empty : zona;
        }
    }
}

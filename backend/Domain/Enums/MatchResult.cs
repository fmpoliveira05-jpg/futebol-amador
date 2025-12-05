namespace Domain.Enums
{
    /// <summary>
    /// Define os possíveis resultados de uma equipa numa partida.
    /// Utilizado para registar o resultado final (Vitória, Derrota, Empate) nas estatísticas.
    /// </summary>
    public enum MatchResult
    {
        /// <summary>
        /// A equipa venceu a partida (Resultado positivo).
        /// </summary>
        WIN = 0,

        /// <summary>
        /// A equipa perdeu a partida (Resultado negativo).
        /// </summary>
        LOSE = 1,

        /// <summary>
        /// A partida terminou empatada (Resultado neutro).
        /// </summary>
        DRAW = 2,

        /// <summary>
        /// A partida ainda não foi jogada ou o resultado final ainda não foi reportado.
        /// (Valor padrão para partidas agendadas).
        /// </summary>
        UNPLAYED = 3
    }
}
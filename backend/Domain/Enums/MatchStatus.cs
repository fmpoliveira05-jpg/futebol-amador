namespace Domain.Enums
{
    /// <summary>
    /// Define os possíveis estados do ciclo de vida de uma partida ou jogo.
    /// Utilizado para rastrear o status do jogo desde o agendamento até à conclusão ou cancelamento.
    /// </summary>
    public enum MatchStatus
    {
        /// <summary>
        /// A partida foi agendada/confirmada e aguarda a data de início.
        /// </summary>
        SCHEDULED = 0,

        /// <summary>
        /// A partida está a ser jogada neste momento (Live).
        /// </summary>
        IN_PROGRESS = 1,

        /// <summary>
        /// A partida foi concluída e o resultado final foi reportado.
        /// </summary>
        DONE = 2,

        /// <summary>
        /// A partida foi reagendada para uma data futura (adiada).
        /// </summary>
        POST_PONED = 3,

        /// <summary>
        /// A partida foi permanentemente cancelada e não será jogada.
        /// </summary>
        CANCELED = 4
    }
}
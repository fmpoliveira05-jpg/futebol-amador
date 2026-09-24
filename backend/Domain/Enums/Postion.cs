namespace Domain.Enums
{
    /// <summary>
    /// Define as posições de um jogador.
    /// Utilizada para tipar e categorizar o papel de cadajogador.
    /// </summary>
    public enum Position
    {
        /// <summary>
        /// Posição ofensiva (Avançado/Atacante).
        /// </summary>
        FORWARD = 0,

        /// <summary>
        /// Posição central, ligando a defesa ao ataque (Médio/Meio-Campo).
        /// </summary>
        MIDFIELDER = 1,

        /// <summary>
        /// Posição defensiva, responsável por proteger a baliza (Defesa).
        /// </summary>
        DEFENDER = 2,

        /// <summary>
        /// Posição de guarda-redes (Goleiro/Keeper).
        /// </summary>
        GOALKEEPER = 3
    }
}
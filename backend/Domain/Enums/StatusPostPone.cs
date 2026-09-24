namespace Domain.Enums
{
    /// <summary>
    /// Define os possíveis estados de decisão para um pedido de adiamento (Postpone) de partida.
    /// Utilizado para registar se a equipa adversária aceitou ou rejeitou a data proposta.
    /// </summary>
    public enum StatusPostPone
    {
        /// <summary>
        /// O pedido de adiamento foi aceite pela equipa recetora.
        /// </summary>
        ACCEPT = 0,

        /// <summary>
        /// O pedido de adiamento foi rejeitado pela equipa recetora.
        /// </summary>
        REJECT = 1
    }
}

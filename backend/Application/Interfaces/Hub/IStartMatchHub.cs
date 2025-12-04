namespace Application.Interfaces.Hub
{
    /// <summary>
    /// Define o contrato de métodos que o servidor SignalR pode invocar em clientes conectados ao StartMatchHub,
    /// bem como os métodos que o cliente pode invocar no servidor.
    /// 
    /// Esta interface garante que a comunicação entre o frontend e o backend é tipada e bem definida.
    /// </summary>
    public interface IStartMatchHub
    {
        /// <summary>
        /// Método invocado pelo servidor para notificar o cliente de que a partida foi sincronizada e começou.
        /// </summary>
        /// <param name="msg">A mensagem de confirmação de início (ex: "O jogo começou!").</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        Task ReceiveStartMatch(string msg);

        /// <summary>
        /// Método invocado pelo cliente para se juntar ao lobby de início de partida.
        /// </summary>
        /// <remarks>
        /// É utilizado por um administrador para sinalizar que está pronto para o "handshake" de início do jogo.
        /// </remarks>
        /// <param name="idMatch">O ID (GUID) da partida a ser iniciada.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task JoinStartMatch(Guid idMatch);

        /// <summary>
        /// Método invocado pelo cliente para sair do lobby de início de partida (cancelar espera ou sair).
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task LeaveStartMatch();
    }
}
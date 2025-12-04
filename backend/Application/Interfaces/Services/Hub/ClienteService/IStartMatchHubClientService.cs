namespace Application.Interfaces.Services.Hub.ClienteService
{
    /// <summary>
    /// Contrato de serviço para clientes do Hub de Início de Partida ([StartMatchHub]).
    /// 
    /// Esta interface define os métodos de alto nível que a camada de Aplicação deve usar
    /// para gerir a conexão com o servidor e as operações de sincronização de início de jogo.
    /// </summary>
    public interface IStartMatchHubClientService
    {
        /// <summary>
        /// Inicializa o objeto de conexão SignalR e configura a gestão de reconexão.
        /// </summary>
        /// <remarks>
        /// Deve ser chamado no arranque do serviço ou antes de qualquer tentativa de conexão.
        /// </remarks>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task InitializeAsync();

        /// <summary>
        /// Envia o pedido para entrar no Lobby de Início de Partida.
        /// </summary>
        /// <remarks>
        /// Invoca o método "JoinStartMatch" no Hub, sinalizando ao servidor que o cliente está pronto para começar o jogo.
        /// </remarks>
        /// <param name="idMatch">O ID (GUID) da partida.</param>
        /// <param name="idTeam">O ID (GUID) da equipa do administrador.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task JoinStartMatchAsync(Guid idMatch, Guid idTeam);

        /// <summary>
        /// Sinaliza ao Hub que o cliente está a sair do processo de início de partida (cancelar espera).
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task LeaveStartMatchAsync();
    }
}
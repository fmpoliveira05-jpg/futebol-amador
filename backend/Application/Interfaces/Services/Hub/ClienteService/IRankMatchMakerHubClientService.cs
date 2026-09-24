using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Services.Hub.ClienteService
{
    /// <summary>
    /// Contrato de serviço para clientes do Hub de Matchmaking Ranqueado ([RankMatchMakerHub]).
    /// 
    /// Esta interface define os métodos de alto nível que a camada de Aplicação deve usar
    /// para gerir a conexão e iniciar/parar a procura de adversários em tempo real.
    /// </summary>
    public interface IRankMatchMakerHubClientService
    {
        /// <summary>
        /// Inicializa a conexão com o Hub.
        /// </summary>
        /// <remarks>
        /// Este método deve construir o objeto de conexão SignalR e configurar a gestão de reconexão.
        /// Deve ser chamado antes de qualquer operação de conexão.
        /// </remarks>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task InitializeAsync();

        /// <summary>
        /// Envia o pedido para entrar no Lobby de Matchmaking Ranqueado.
        /// </summary>
        /// <remarks>
        /// Invoca o método "JoinRankMatchMaker" no servidor, iniciando o algoritmo de procura de adversário.
        /// </remarks>
        /// <param name="startSearch">DTO [StartSearchDto] com os critérios de procura (Equipa, Horário, etc.).</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task JoinRankMatchMakerAsync(StartSearchDto startSearch);

        /// <summary>
        /// Sinaliza ao Hub que o cliente está a sair do Lobby de Matchmaking (cancelar a procura).
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task LeaveRankMatchMakerAsync();
    }
}
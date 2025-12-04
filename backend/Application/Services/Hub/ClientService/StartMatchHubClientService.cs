using Application.Interfaces.Services.Hub.ClienteService;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Services.Hub.ClientService
{
    /// <summary>
    /// Serviço Cliente SignalR dedicado à interação com o Hub de Início de Partida ([StartMatchHub]).
    /// 
    /// Esta classe encapsula a gestão da conexão ([HubConnection]) e os métodos de invocação
    /// remota (InvokeAsync) que são expostos pelo Hub. É utilizada por serviços do lado do servidor
    /// para simular a comunicação do cliente com o Hub.
    /// </summary>
    public class StartMatchHubClientService : IStartMatchHubClientService
    {
        /// <summary>
        /// A conexão SignalR subjacente, tipada e com gestão de reconexão automática.
        /// </summary>
        private HubConnection connection = null!;

        /// <summary>
        /// Construtor padrão do serviço cliente.
        /// </summary>
        public StartMatchHubClientService()
        {
        }

        /// <summary>
        /// Inicializa a conexão com o Hub de Início de Partida.
        /// </summary>
        /// <remarks>
        /// O método constrói a URL completa do Hub ([RouteHubConst.StartRoute]/StartMatch) e configura
        /// a gestão automática de reconexão. Deve ser chamado antes de [ConnectAsync].
        /// </remarks>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task InitializeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            connection = new HubConnectionBuilder()
                .WithUrl($"{ModelConstants.RouteHubConst.StartRoute}/StartMatch")
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();
        }

        /// <summary>
        /// Estabelece a conexão física com o Hub.
        /// </summary>
        /// <remarks>
        /// Verifica se a conexão está no estado [HubConnectionState.Disconnected] antes de chamar [StartAsync].
        /// </remarks>
        /// <exception cref="InvalidOperationException">Lançada se a conexão não foi inicializada ([InitializeAsync]).</exception>
        public async Task ConnectAsync()
        {
            if (connection == null)
            {
                throw new InvalidOperationException("Connection not initialized. Call InitializeAsync(idTeam) first.");
            }

            if (connection.State == HubConnectionState.Disconnected)
                await connection.StartAsync();
        }

        /// <summary>
        /// Regista os métodos de callback que o cliente pode receber do Hub.
        /// </summary>
        /// <remarks>
        /// Este método subscreve o cliente ao método "ReceiveStartMatch" (notificação de jogo iniciado).
        /// </remarks>
        private void RegisterHandlers()
        {
            connection.On<string>("ReceiveStartMatch", (msg) =>
            {
                Console.WriteLine("Mensagem recebida do Hub: " + msg);
            });
        }

        /// <summary>
        /// Envia o pedido para entrar no Lobby de Início de Partida.
        /// </summary>
        /// <remarks>
        /// Invoca o método "JoinStartMatch" no servidor, sinalizando que o cliente está pronto para começar o jogo.
        /// </remarks>
        /// <param name="idMatch">O ID da partida.</param>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task JoinStartMatchAsync(Guid idMatch, Guid idTeam)
        {
            await ConnectAsync();
            await connection.InvokeAsync("JoinStartMatch", idMatch, idTeam);
        }

        /// <summary>
        /// Sinaliza ao Hub que o cliente está a sair do processo de início de partida (Cancelamento da espera).
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task LeaveStartMatchAsync()
        {
            await ConnectAsync();
            await connection.InvokeAsync("LeaveStartMatch");
        }
    }
}

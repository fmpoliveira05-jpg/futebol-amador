using Application.DTOs.RankMatchMaker;
using Application.Interfaces.Services.Hub.ClienteService;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Services.Hub.ClientService
{
    /// <summary>
    /// Serviço Cliente SignalR dedicado à interação com o Hub de Matchmaking Ranqueado ([RankMatchMakerHub]).
    /// 
    /// Esta classe é utilizada por serviços de background ou lógica de negócio para entrar na fila de procura
    /// de jogo e receber notificações em tempo real sobre o estado do emparelhamento (match found).
    /// </summary>
    public class RankMatchMakerHubClientService : IRankMatchMakerHubClientService
    {
        /// <summary>
        /// A conexão SignalR subjacente, tipada e com gestão de reconexão automática.
        /// </summary>
        private HubConnection connection = null!;

        /// <summary>
        /// Construtor padrão do serviço cliente.
        /// </summary>
        public RankMatchMakerHubClientService()
        {
        }

        /// <summary>
        /// Inicializa a conexão com o Hub de Matchmaker.
        /// </summary>
        /// <remarks>
        /// O método constrói a URL completa do Hub ([RouteHubConst.StartRoute]/MatchMaker) e configura
        /// a gestão automática de reconexão.
        /// </remarks>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task InitializeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            connection = new HubConnectionBuilder()
                .WithUrl($"{ModelConstants.RouteHubConst.StartRoute}/MatchMaker")
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();
        }

        /// <summary>
        /// Estabelece a conexão física com o Hub de Matchmaker.
        /// </summary>
        /// <remarks>
        /// Verifica se a conexão está no estado [HubConnectionState.Disconnected] antes de chamar [StartAsync].
        /// </remarks>
        /// <exception cref="InvalidOperationException">Lançada se [InitializeAsync] não foi chamado primeiro.</exception>
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
        /// Este método é onde o cliente subscreve as notificações do Hub, como "Match Found" ou "Group Closed".
        /// </remarks>
        private void RegisterHandlers()
        {
            connection.On<string>("ReceiveStartMatch", (msg) =>
            {
                Console.WriteLine("Mensagem recebida do Hub: " + msg);
            });
        }

        /// <summary>
        /// Envia o pedido para entrar no Lobby de Matchmaking.
        /// </summary>
        /// <remarks>
        /// Invoca o método "JoinRankMatchMaker" no servidor, enviando os critérios de procura.
        /// </remarks>
        /// <param name="startSearch">DTO com os critérios de procura (Equipa, Horário, etc.).</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task JoinRankMatchMakerAsync(StartSearchDto startSearch)
        {
            await ConnectAsync();
            await connection.InvokeAsync("JoinRankMatchMaker", startSearch);
        }

        /// <summary>
        /// Sinaliza ao Hub que o cliente está a sair do Lobby de Matchmaking.
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task LeaveRankMatchMakerAsync()
        {
            await ConnectAsync();
            await connection.InvokeAsync("LeaveRankMatchMaker");
        }
    }
}

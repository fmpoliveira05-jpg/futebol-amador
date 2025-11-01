using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Services.Hub.ClientService
{
    public class RankMatchMakerHubClientService: IRankMatchMakerHubClientService
    {
        private HubConnection connection = null!;

        public RankMatchMakerHubClientService()
        {
        }

        public async Task InitializeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            connection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:5218/MatchMaker")
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();
        }

        public async Task ConnectAsync()
        {
            if (connection == null)
            {
                throw new InvalidOperationException("Connection not initialized. Call InitializeAsync(idTeam) first.");
            }

            if (connection.State == HubConnectionState.Disconnected)
                await connection.StartAsync();
        }

        /**
         * Metodo do cliente que permitirá ele receber uma mensagem 
         */
        private void RegisterHandlers()
        {
            connection.On<string>("ReceiveStartMatch", (msg) =>
            {
                Console.WriteLine("Mensagem recebida do Hub: " + msg);
            });
        }

        /**
         * Permite o cliente conectar-se ao Hub
         */
        public async Task JoinRankMatchMakerAsync(Guid idPlayer, Guid idTeam, TimeOnly hoursGame)
        {
            await ConnectAsync();
            await connection.InvokeAsync("JoinRankMatchMaker", idPlayer, idTeam, hoursGame);
        }

        /**
         Permite o cliente sair do Hub
         */
        public async Task LeaveRankMatchMakerAsync()
        {
            await ConnectAsync();
            await connection.InvokeAsync("LeaveRankMatchMaker");
        }
    }
}

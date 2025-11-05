using Application.Interfaces.Services.Hub.ClienteService;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Services.Hub.ClientService
{
    public class StartMatchHubClientService : IStartMatchHubClientService
    {
        private HubConnection connection = null!;

        public StartMatchHubClientService()
        {
        }

        public async Task InitializeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            connection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:5218/StartMatch")
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();
        }


        /**
         * Permite validar se o hub está ativo e se sim fazer a conexão
         */
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
        public async Task JoinStartMatchAsync(Guid idMatch, Guid idTeam)
        {
            await ConnectAsync();
            await connection.InvokeAsync("JoinStartMatch", idMatch, idTeam);
        }

        /**  
         * Permite o cliete sair do Hub
         */
        public async Task LeaveStartMatchAsync()
        {
            await ConnectAsync();
            await connection.InvokeAsync("LeaveStartMatch");
        }
    }
}

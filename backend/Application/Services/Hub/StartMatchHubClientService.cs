using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Services.Hub
{
    public class StartMatchHubClientService: IStartMatchHubClientService
    {
        private HubConnection connection;

        public StartMatchHubClientService()
        {
        }

        public async Task InitializeAsync(Guid idTeam)
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            connection = new HubConnectionBuilder()
                .WithUrl($"https://localhost:7003/api/{idTeam}/StartMatch")
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
        public async Task JoinStartMatchAsync(Guid idMatch)
        {
            await ConnectAsync();
            await connection.InvokeAsync("JoinStartMatch", idMatch);
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

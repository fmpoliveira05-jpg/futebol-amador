using Application.DTOs;
using Application.Interfaces.Services.Hub;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Services.Hub
{
    internal class FinishMatchHubClientService: IFinishMatchHubClientService
    {
        private HubConnection connection;

        public FinishMatchHubClientService()
        {
        }

        public async Task InitializeAsync(Guid idTeam)
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            connection = new HubConnectionBuilder()
                .WithUrl($"https://localhost:7003/api/{idTeam}/FinishMatch")
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
        public async Task JoinFinishMatchAsync(ResultMatchDto result)
        {
            await ConnectAsync();
            await connection.InvokeAsync("JoinFinishMatch", result);
        }

        /**
         Permite a clientes que já estão no Hub editar o resultado da match
         */
        public async Task EditResultMatchAsync(ResultMatchDto result)
        {
            await ConnectAsync(); 
            await connection.InvokeAsync("EditResult", result);
        }


        /** * Permite o cliete sair do Hub
         */
        public async Task LeaveFinishMatchAsync()
        {
            await ConnectAsync(); 
            await connection.InvokeAsync("LeaveFinishMatch");
        }
    }
}

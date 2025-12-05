using Application.DTOs.Match;
using Application.Interfaces.Services.Hub.ClienteService;
using Domain.Constants;
using Microsoft.AspNetCore.SignalR.Client;

namespace Application.Services.Hub.ClientService
{
    /// <summary>
    /// Serviço Cliente SignalR dedicado à interação com o Hub de Finalização de Partida ([FinishMatchHub]).
    /// 
    /// Esta classe encapsula a gestão da conexão ([HubConnection]) e os métodos de invocação
    /// remota (InvokeAsync) que são expostos pelo Hub, permitindo que a lógica de negócio os utilize de forma limpa.
    /// </summary>
    public class FinishMatchHubClientService : IFinishMatchHubClientService
    {
        /// <summary>
        /// A conexão SignalR subjacente, tipada para o Cliente e com gestão de reconexão automática.
        /// </summary>
        private HubConnection connection = null!;

        /// <summary>
        /// Construtor padrão do serviço cliente.
        /// </summary>
        public FinishMatchHubClientService()
        {
        }

        /// <summary>
        /// Inicializa a conexão com o Hub SignalR.
        /// </summary>
        /// <remarks>
        /// Este método constrói a URL completa do Hub ([RouteHubConst.StartRoute]/FinishMatch) e configura
        /// a gestão automática de reconexão em caso de interrupção de rede.
        /// Requer ser chamado antes de qualquer operação [ConnectAsync] ou [InvokeAsync].
        /// </remarks>
        public async Task InitializeAsync()
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            connection = new HubConnectionBuilder()
                .WithUrl($"{ModelConstants.RouteHubConst.StartRoute}/FinishMatch")
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();
        }

        /// <summary>
        /// Estabelece a conexão física com o Hub de Finalização de Partida.
        /// </summary>
        /// <remarks>
        /// Verifica se a conexão já foi iniciada ([connection.State]) antes de tentar iniciar o processo de conexão.
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
        /// Aqui, o cliente regista-se para o método "ReceiveStartMatch" (embora seja o FinishMatchHub, o método pode ser reutilizado).
        /// </remarks>
        private void RegisterHandlers()
        {
            connection.On<string>("ReceiveStartMatch", (msg) =>
            {
                Console.WriteLine("Mensagem recebida do Hub: " + msg);
            });
        }

        /// <summary>
        /// Envia o resultado final de uma equipa para o Hub (Entrar no lobby de finalização).
        /// </summary>
        /// <param name="result">O DTO de resultado [ResultMatchDto] contendo os golos e IDs.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task JoinFinishMatchAsync(ResultMatchDto result)
        {
            await ConnectAsync();
            await connection.InvokeAsync("JoinFinishMatch", result);
        }

        /// <summary>
        /// Permite a clientes que já estão no Hub editar um resultado submetido anteriormente.
        /// </summary>
        /// <param name="result">O DTO de resultado [ResultMatchDto] com os valores corrigidos.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task EditResultMatchAsync(ResultMatchDto result)
        {
            await ConnectAsync();
            await connection.InvokeAsync("EditResult", result);
        }

        /// <summary>
        /// Sinaliza ao Hub que o cliente está a sair do processo de finalização (Leave Hub).
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task LeaveFinishMatchAsync()
        {
            await ConnectAsync();
            await connection.InvokeAsync("LeaveFinishMatch");
        }
    }
}

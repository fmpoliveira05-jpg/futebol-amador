using Application.DTOs.Match;

namespace Application.Interfaces.Services.Hub.ClienteService
{
    /// <summary>
    /// Contrato de serviço para clientes de Finalização de Partida ([FinishMatchHub]).
    /// 
    /// Esta interface define os métodos de alto nível que a camada de Aplicação deve usar
    /// para gerir a conexão e enviar/receber dados do Hub SignalR de finalização de jogos.
    /// </summary>
    public interface IFinishMatchHubClientService
    {
        /// <summary>
        /// Inicializa a conexão com o Hub.
        /// </summary>
        /// <remarks>
        /// Este método deve construir a URL do Hub e configurar a gestão de reconexão.
        /// Deve ser chamado antes de qualquer operação de conexão.
        /// </remarks>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task InitializeAsync();

        /// <summary>
        /// Envia o resultado final de uma equipa para o Hub.
        /// </summary>
        /// <remarks>
        /// Este método invoca o servidor para registar o resultado da partida, entrando no lobby de finalização.
        /// </remarks>
        /// <param name="result">O DTO [ResultMatchDto] contendo os golos submetidos.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task JoinFinishMatchAsync(ResultMatchDto result);

        /// <summary>
        /// Envia uma correção ou atualização de um resultado previamente submetido.
        /// </summary>
        /// <remarks>
        /// Utilizado quando os administradores de duas equipas não concordam com os golos e um deles precisa de corrigir o valor para sincronização.
        /// </remarks>
        /// <param name="result">O DTO [ResultMatchDto] com os valores corrigidos.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task EditResultMatchAsync(ResultMatchDto result);

        /// <summary>
        /// Sinaliza ao Hub que o cliente está a sair do processo de finalização (Leave Hub).
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task LeaveFinishMatchAsync();
    }
}

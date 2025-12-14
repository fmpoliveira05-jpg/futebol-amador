using Application.DTOs.Match;

namespace Application.Interfaces.Hub
{
    /// <summary>
    /// Define o contrato de métodos que o cliente pode invocar no servidor SignalR do Hub de Finalização de Partida.
    /// 
    /// Esta interface é utilizada por administradores das equipas para submeter e sincronizar o resultado final de um jogo.
    /// </summary>
    public interface IFinishMatchHub
    {
        /// <summary>
        /// Método invocado pelo cliente para entrar no lobby de finalização e submeter o seu resultado da partida.
        /// </summary>
        /// <remarks>
        /// Este é o ponto de entrada no processo de sincronização de resultados. O servidor irá armazenar este resultado
        /// e verificar se o resultado do adversário já foi submetido e se coincide.
        /// </remarks>
        /// <param name="finishMatch">O DTO [ResultMatchDto] contendo o número de golos da equipa e do adversário.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        Task JoinFinishMatch(ResultMatchDto finishMatch);

        /// <summary>
        /// Método invocado pelo cliente para corrigir ou atualizar um resultado que já foi submetido.
        /// </summary>
        /// <remarks>
        /// É utilizado quando os resultados não coincidem (divergência) e um administrador precisa de corrigir o valor.
        /// </remarks>
        /// <param name="finishMatch">O DTO [ResultMatchDto] com os novos golos corrigidos.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        Task EditResult(ResultMatchDto finishMatch);

        Task ReceiveFinishMatch(bool success);

        /// <summary>
        /// Método invocado pelo cliente para sair do lobby de finalização (cancelar espera).
        /// </summary>
        /// <remarks>
        /// Sinaliza ao servidor que o cliente não está mais ativo neste processo, permitindo a limpeza do estado temporário.
        /// </remarks>
        /// <returns>Uma tarefa assíncrona.</returns>
        Task LeaveFinishMatch();
    }
}

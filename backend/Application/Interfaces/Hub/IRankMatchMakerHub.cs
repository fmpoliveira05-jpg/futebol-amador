using Application.DTOs.RankMatchMaker;

namespace Application.Interfaces.Hub
{
    /// <summary>
    /// Define o contrato de métodos que o servidor SignalR pode invocar em clientes conectados ao RankMatchMakerHub.
    /// 
    /// Estes métodos são utilizados para atualizar o estado do lobby de matchmaking e notificar os clientes
    /// sobre eventos importantes, como o encerramento do grupo ou a atualização da fila de espera.
    /// </summary>
    public interface IRankMatchMakerHub
    {
        /// <summary>
        /// Método invocado pelo cliente para entrar na fila de matchmaking.
        /// </summary>
        /// <param name="idPlayer">ID do jogador (Admin) que inicia a procura.</param>
        /// <param name="idTeam">ID da equipa que procura adversário.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task JoinRankMatchMaker(Guid idPlayer, Guid idTeam);

        /// <summary>
        /// Método invocado pelo cliente para sair da fila de matchmaking (cancelar a procura).
        /// </summary>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task LeaveRankMatchMaker();

        /// <summary>
        /// Método invocado pelo servidor para enviar uma lista atualizada das equipas em espera no lobby.
        /// </summary>
        /// <remarks>
        /// Utilizado para atualizar o ecrã de Matchmaker ou notificar sobre novos candidatos.
        /// </remarks>
        /// <param name="teamsToNotify">Lista de entradas de equipas que necessitam de ser notificadas.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task NotifyAllTeams(List<EntryRankMatchMakerHub> teamsToNotify);

        /// <summary>
        /// Método invocado pelo servidor para sinalizar que o processo de matchmaking para este grupo terminou.
        /// </summary>
        /// <remarks>
        /// É disparado quando um adversário é encontrado e a partida é criada, ou quando o limite de tempo do lobby expira.
        /// </remarks>
        /// <param name="groupName">O nome do grupo (canal) que está a ser fechado.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public Task OnGroupClosed(string groupName);
    }
}
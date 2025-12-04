using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a Gestão de Partidas e Calendário.
    /// 
    /// Esta interface define a lógica de negócio para a criação, consulta, cancelamento e gestão de pedidos de remarcação de jogos.
    /// </summary>
    public interface IMatchService
    {
        /// <summary>
        /// Obtém o calendário completo de jogos de uma equipa (Agendados e Finalizados).
        /// </summary>
        /// <param name="idTeam">O ID da equipa cujo calendário se pretende.</param>
        /// <returns>Uma tarefa assíncrona que retorna uma lista de [InfoMatchCalendar].</returns>
        public Task<List<InfoMatchCalendar>> GetCalendar(Guid idTeam);

        /// <summary>
        /// Obtém o calendário de jogos de uma equipa, aplicando filtros de pesquisa.
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <param name="filter">O DTO contendo os critérios de filtragem (Datas, Adversário, Local).</param>
        /// <returns>Uma tarefa assíncrona que retorna uma lista filtrada de [InfoMatchCalendar].</returns>
        public Task<List<InfoMatchCalendar>> GetCalendarWithFilters(Guid idTeam, FilterCalendarDto filter);

        /// <summary>
        /// Obtém os detalhes de uma partida específica para visualização.
        /// </summary>
        /// <remarks>
        /// O serviço deve validar se a equipa solicitante faz parte do jogo e identificar o adversário.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que está a consultar.</param>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>Uma tarefa assíncrona que retorna o DTO [InfoMatch] com os detalhes do jogo.</returns>
        public Task<InfoMatch> GetMatchById(Guid idTeam, Guid idMatch);

        /// <summary>
        /// Inicia um pedido de adiamento (remarcação) de uma partida.
        /// </summary>
        /// <remarks>
        /// Cria a entidade [PostPoneMatch] e atualiza o status da partida para POST_PONED.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que solicita o adiamento.</param>
        /// <param name="dto">DTO com a nova data proposta.</param>
        /// <returns>Uma tarefa assíncrona que retorna o DTO [InfoPostPoneMatch] do pedido criado.</returns>
        public Task<InfoPostPoneMatch> PostPoneMatch(Guid idTeam, PostPoneMatchDto dto);

        /// <summary>
        /// Aceita um pedido de adiamento de partida.
        /// </summary>
        /// <remarks>
        /// **Transação:** Remove o registo de adiamento, atualiza a data do jogo ([Matches.MatchDate]) para a nova data proposta e restaura o status para SCHEDULED.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que aceita (Recetora).</param>
        /// <param name="dto">DTO com o status de aceitação.</param>
        /// <returns>Uma tarefa assíncrona que retorna o DTO [MatchDto] da partida atualizada.</returns>
        public Task<MatchDto> AcceptPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto);

        /// <summary>
        /// Rejeita um pedido de adiamento de partida.
        /// </summary>
        /// <remarks>
        /// **Transação:** Remove o registo de adiamento e marca a partida como CANCELED (Cancelada), pois não houve acordo.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que rejeita.</param>
        /// <param name="dto">DTO com o status de rejeição.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>).</returns>
        public Task RejectPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto);

        /// <summary>
        /// Obtém a lista de pedidos de adiamento pendentes recebidos por uma equipa (sem filtros).
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <returns>Uma tarefa assíncrona que retorna uma lista de [InfoPostPoneMatch].</returns>
        public Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(Guid idTeam);

        /// <summary>
        /// Obtém a lista de pedidos de adiamento pendentes com filtros aplicados.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="filter">Filtros de data e adversário.</param>
        /// <returns>Uma tarefa assíncrona que retorna uma lista filtrada de [InfoPostPoneMatch].</returns>
        public Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeamWithFilters(Guid idTeam, FilterPostPoneMatchDto filter);

        /// <summary>
        /// Cancela uma partida agendada.
        /// </summary>
        /// <remarks>
        /// **Transação:** Cria um registo em [CancelledMatch] para histórico e atualiza o status da partida para CANCELED.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que cancela.</param>
        /// <param name="idMatch">ID da partida.</param>
        /// <param name="description">Motivo do cancelamento.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>).</returns>
        public Task CancelMatch(Guid idTeam, Guid idMatch, string description);
    }
}
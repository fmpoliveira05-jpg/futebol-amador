using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [Matches] (Partidas).
    /// 
    /// Define os métodos de consulta e persistência para gerir o ciclo de vida dos jogos (agendamento, status, adiamentos).
    /// </summary>
    public interface IMatchRepository
    {
        /// <summary>
        /// Adiciona um novo registo de partida à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="match">A entidade [Matches] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public Task AddMatch(Matches match);

        /// <summary>
        /// Obtém uma partida pelo seu identificador único (ID).
        /// </summary>
        /// <param name="idMatch">O ID (GUID) da partida.</param>
        /// <returns>A entidade [Matches] ou null se não for encontrada.</returns>
        public Task<Matches?> GetMatchById(Guid idMatch);

        /// <summary>
        /// Obtém uma partida apenas se o seu estado atual for SCHEDULED (Agendada).
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] ou null se o estado for diferente de Agendada.</returns>
        public Task<Matches?> GetScheduledMatchById(Guid idMatch);

        /// <summary>
        /// Obtém uma partida apenas se o seu estado atual for IN_PROGRESS (A Decorrer).
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] ou null.</returns>
        public Task<Matches?> GetMatchInProgressByIdAsync(Guid idMatch);

        /// <summary>
        /// Obtém uma partida que pode ser cancelada (estado SCHEDULED ou POST_PONED).
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] ou null se o estado não for elegível para cancelamento.</returns>
        public Task<Matches?> GetMatchToCancelById(Guid idMatch);

        /// <summary>
        /// Obtém uma partida, garantindo o carregamento da entidade [Pitch] (Local de Jogo).
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] com o Pitch carregado, ou null.</returns>
        public Task<Matches?> GetMatchWitchPitchById(Guid idMatch);

        /// <summary>
        /// Obtém uma partida, carregando as estatísticas e todos os membros das equipas participantes.
        /// </summary>
        /// <param name="idMatch">O ID da partida.</param>
        /// <returns>A entidade [Matches] com membros da equipa carregados, ou null.</returns>
        public Task<Matches?> GetMatchWithListPlayerById(Guid idMatch);

        Task<Matches?> GetMatchForFinishMatch(Guid idMatch);

        /// <summary>
        /// Verifica se uma equipa tem alguma partida agendada (SCHEDULED ou POST_PONED) dentro de um intervalo de 12 horas da [gameDate] fornecida.
        /// </summary>
        /// <param name="idReceiver">O ID da equipa a verificar.</param>
        /// <param name="gameDate">A data e hora de referência para a comparação.</param>
        /// <returns>A partida [Matches] de conflito encontrada ou null.</returns>
        public Task<Matches?> GetMatchProxim12HoursMatchs(Guid idReceiver, DateTime gameDate);

        /// <summary>
        /// Obtém a lista de todas as partidas agendadas e finalizadas que ocorrem numa data específica.
        /// </summary>
        /// <param name="date">A data de referência.</param>
        /// <returns>Uma lista de objetos [InfoMatchCalendar] para o frontend.</returns>
        public Task<List<InfoMatchCalendar>> GetMatchesByDateAsync(DateTime date);

        /// <summary>
        /// Obtém o calendário completo de jogos de uma equipa (Agendados e Finalizados), projetado no DTO.
        /// </summary>
        /// <param name="idTeam">O ID da equipa cujo calendário se pretende.</param>
        /// <returns>Uma lista de objetos [InfoMatchCalendar] para o frontend.</returns>
        public Task<List<InfoMatchCalendar>> GetAllMatchesTeam(Guid idTeam);

        /// <summary>
        /// Obtém o calendário de jogos de uma equipa, aplicando filtros complexos (ex: Status, Tipo de Jogo, Local).
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <param name="filter">O DTO contendo os critérios de filtragem.</param>
        /// <returns>Uma lista de objetos [InfoMatchCalendar] filtrados.</returns>
        public Task<List<InfoMatchCalendar>> GetAllMatchesTeamWithFilters(Guid idTeam, FilterCalendarDto filter);

        /// <summary>
        /// Obtém a lista de pedidos de adiamento recebidos por uma equipa.
        /// </summary>
        /// <param name="idReceiver">O ID da equipa que recebeu o pedido.</param>
        /// <returns>Uma lista de [InfoPostPoneMatch] com os detalhes dos pedidos pendentes.</returns>
        public Task<List<InfoPostPoneMatch>> GetAllMatchPostPoneReceiverById(Guid idReceiver);

        /// <summary>
        /// Obtém a lista de pedidos de adiamento recebidos por uma equipa, aplicando filtros.
        /// </summary>
        /// <param name="idReceiver">O ID da equipa que recebeu o pedido.</param>
        /// <param name="filter">O DTO com os critérios de filtragem (Datas de Jogo Original e Proposta).</param>
        /// <returns>Uma lista de [InfoPostPoneMatch] filtrada.</returns>
        Task<List<InfoPostPoneMatch>> GetAllMatchPostPoneReceiverByIdWithFilters(Guid idReceiver, FilterPostPoneMatchDto filter);
    }
}
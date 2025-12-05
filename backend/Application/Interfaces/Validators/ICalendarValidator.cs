using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio e Controlo de Integridade para o ciclo de vida do Calendário e Partidas ([Matches]).
    /// 
    /// Esta interface define as regras de validação síncrona que garantem a validade dos dados e a
    /// aplicação de regras de agendamento, cancelamento, adiamento e submissão de resultados.
    /// </summary>
    public interface ICalendarValidator
    {
        /// <summary>
        /// Valida se a entidade [Matches] foi encontrada (não é nula).
        /// </summary>
        /// <param name="match">A partida.</param>
        public void ExistsMatch(Matches match);

        /// <summary>
        /// Valida se a entidade de estatísticas da equipa ([TeamStatistics]) foi encontrada (não é nula).
        /// </summary>
        /// <param name="team">As estatísticas da equipa.</param>
        public void ExistsTeamStatistics(TeamStatistics team);

        /// <summary>
        /// Valida se o ID da equipa (contexto) não é Guid.Empty.
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        public void ValidateTeamCalendar(Guid idTeam);
        
        /// <summary>
        /// Valida a consistência e o intervalo dos filtros de data do calendário.
        /// </summary>
        /// <remarks>
        /// Regra verificada: Data Mínima não pode ser posterior à Data Máxima.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa (contexto).</param>
        /// <param name="filter">O DTO com os filtros de data.</param>
        public void ValidateFilterCalendar(Guid idTeam, FilterCalendarDto filter);

        /// <summary>
        /// Valida a consistência do intervalo de datas no filtro de pedidos de adiamento (MinDatePostPoneGame vs MaxDatePostPoneGame).
        /// </summary>
        /// <param name="filter">O DTO com os filtros de pedidos de adiamento.</param>
        public void ValidateFilterPostPoneMatch(FilterPostPoneMatchDto filter);

        /// <summary>
        /// Valida os IDs essenciais para o DTO de pedido de adiamento.
        /// </summary>
        /// <param name="idTeam">O ID da equipa que solicita o adiamento.</param>
        /// <param name="dto">O DTO do pedido de adiamento.</param>
        public void ValidatePostPoneMatchDto(Guid idTeam, PostPoneMatchDto dto);

        /// <summary>
        /// Valida as regras de negócio para a criação de um pedido de adiamento (Datas, Horário, Status).
        /// </summary>
        /// <param name="match">A partida a ser adiada.</param>
        /// <param name="newDate">A nova data proposta.</param>
        /// <param name="team">Estatísticas da equipa solicitante.</param>
        /// <param name="idTeam">ID da equipa solicitante.</param>
        /// <param name="opponetTeam">Estatísticas do adversário.</param>
        /// <param name="idOpponnent">ID do adversário.</param>
        public void ValidatorPostPoneMatch(Matches match, DateTime newDate, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);

        /// <summary>
        /// Valida se o DTO recebido é uma aceitação de pedido de adiamento.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="dto">DTO com o status de aceitação.</param>
        public void ValidateAcceptPostPoneMatchDto(Guid idTeam, AcceptRefusePostPoneDto dto);

        /// <summary>
        /// Valida as regras de negócio antes de aceitar um pedido de adiamento (ex: sem conflitos de horário).
        /// </summary>
        /// <param name="postPoneMatch">O pedido de adiamento.</param>
        /// <param name="match">A partida alvo.</param>
        /// <param name="team">Estatísticas da equipa recetora.</param>
        /// <param name="idTeam">ID da equipa recetora.</param>
        /// <param name="opponetTeam">Estatísticas do adversário.</param>
        /// <param name="idOpponnent">ID do adversário.</param>
        /// <param name="matchFind">Partida de conflito (deve ser nula).</param>
        public void ValidatorAcceptPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent, Matches matchFind);

        /// <summary>
        /// Valida se o DTO recebido é uma rejeição de pedido de adiamento.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="dto">DTO com o status de rejeição.</param>
        public void ValidateRejectPostPoneMatchDTO(Guid idTeam, AcceptRefusePostPoneDto dto);

        /// <summary>
        /// Valida as regras de negócio antes de rejeitar um pedido de adiamento.
        /// </summary>
        /// <param name="postPoneMatch">O pedido de adiamento.</param>
        /// <param name="match">A partida alvo.</param>
        /// <param name="team">Estatísticas da equipa recetora.</param>
        /// <param name="idTeam">ID da equipa recetora.</param>
        /// <param name="opponetTeam">Estatísticas do adversário.</param>
        /// <param name="idOpponnent">ID do adversário.</param>
        public void ValidatorRejectPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent);

        /// <summary>
        /// Valida os IDs de entrada e a descrição para o cancelamento de uma partida.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="idMatch">ID da partida.</param>
        /// <param name="description">Motivo do cancelamento.</param>
        public void ValidateVariabelCancelMatch(Guid idTeam, Guid idMatch, string description);

        /// <summary>
        /// Valida as regras de negócio para o cancelamento de uma partida (ex: status SCHEDULED, antecedência mínima).
        /// </summary>
        /// <param name="match">A partida a cancelar.</param>
        /// <param name="team">Estatísticas da equipa que cancela.</param>
        /// <param name="idTeam">ID da equipa que cancela.</param>
        /// <param name="opponent">Estatísticas do adversário.</param>
        /// <param name="idOpponent">ID do adversário.</param>
        public void ValidateCancelMatch(Matches match, TeamStatistics team, Guid idTeam, TeamStatistics opponent, Guid idOpponent);

        /// <summary>
        /// Valida os IDs e a consistência do resultado submetido para a finalização.
        /// </summary>
        /// <param name="idTeam">ID da equipa que submete o resultado.</param>
        /// <param name="result">O DTO de resultado.</param>
        public void validateResultMatch(Guid idTeam, ResultMatchDto result);

        /// <summary>
        /// Valida as regras de negócio para a finalização de uma partida (Estado, Existência das entidades).
        /// </summary>
        /// <param name="match">A partida.</param>
        /// <param name="team">Estatísticas da equipa submetida.</param>
        /// <param name="idTeam">ID da equipa submetida.</param>
        /// <param name="opponent">Estatísticas do adversário.</param>
        /// <param name="idOponnent">ID do adversário.</param>
        /// <param name="result">O DTO de resultado submetido.</param>
        public void ValidateFinishMatch(Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponent, Guid idOponnent, ResultMatchDto result);

        /// <summary>
        /// Valida se o utilizador pode sair do lobby de finalização.
        /// </summary>
        /// <param name="match">A partida.</param>
        /// <param name="team">A equipa.</param>
        public void ValidateCancelFinishMatch(Matches match, Team team);
    }
}
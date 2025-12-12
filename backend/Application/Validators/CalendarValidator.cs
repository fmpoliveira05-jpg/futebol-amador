using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Validators;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Validators
{
    /// <summary>
    /// Validador de Regras de Negócio para o ciclo de vida do Calendário e Partidas.
    /// 
    /// Esta classe é responsável por verificar a validade de agendamentos, adiamentos, cancelamentos e
    /// submissões de resultados, garantindo que as regras de domínio (ex: limite de cancelamento, conflito de horário) são respeitadas.
    /// </summary>
    public class CalendarValidator : ICalendarValidator
    {
        /// <summary>
        /// Valida se a entidade [Matches] existe (não é nula).
        /// </summary>
        /// <param name="match">A partida.</param>
        /// <exception cref="ArgumentException">Lançada se a partida for nula.</exception>
        public void ExistsMatch(Matches match)
        {
            if (match == null)
            {
                throw new ArgumentException("A partida não foi encontrada");
            }
        }
        
        /// <summary>
        /// Valida se a entidade de estatísticas da equipa existe (não é nula).
        /// </summary>
        /// <param name="team">As estatísticas da equipa (TeamStatistics).</param>
        /// <exception cref="ArgumentException">Lançada se a estatística da equipa for nula.</exception>
        public void ExistsTeamStatistics(TeamStatistics team)
        {
            if (team == null)
            {
                throw new ArgumentException("A equipa não foi encontrada");
            }
        }

        /// <summary>
        /// Valida se o ID da equipa não é Guid.Empty.
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <exception cref="InvalidOperationException">Lançada se o ID da equipa for vazio.</exception>
        public void ValidateTeamCalendar(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new InvalidOperationException("O id da equipa não pode estar vazio");
            }
        }

        /// <summary>
        /// Valida os filtros de data do calendário.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: ID da equipa válido e Data Mínima não pode ser superior à Data Máxima.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa (contexto).</param>
        /// <param name="filter">O DTO com os filtros de data.</param>
        /// <exception cref="InvalidOperationException">Se o intervalo de datas for inválido.</exception>
        public void ValidateFilterCalendar(Guid idTeam, FilterCalendarDto filter)
        {
            var dateMin = filter.MinDate;
            var dateMax = filter.MaxDate;

            ValidateTeamCalendar(idTeam);

            if (dateMin.HasValue && dateMax.HasValue)
            {
                if (dateMin.Value > dateMax.Value)
                {
                    throw new InvalidOperationException("A data minima tem de ser inferior ou igual à data maxima");
                }
            }
        }

        #region Validações de adiamento

        /// <summary>
        /// Valida os IDs essenciais para o DTO de pedido de adiamento.
        /// </summary>
        /// <param name="idTeam">O ID da equipa que solicita o adiamento.</param>
        /// <param name="dto">O DTO do pedido de adiamento.</param>
        /// <exception cref="ArgumentException">Se qualquer um dos IDs (Match, Team, Opponent) for Guid.Empty.</exception>
        public void ValidatePostPoneMatchDto(Guid idTeam, PostPoneMatchDto dto)
        {
            if (dto.IdMatch == Guid.Empty)
            {
                throw new ArgumentException("O id da partida a adiar está vazio");
            }

            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }

            if (dto.IdOpponent == Guid.Empty)
            {
                throw new ArgumentException("O id do opponent está vazio");
            }
        }

        /// <summary>
        /// Valida as regras de negócio para a criação de um pedido de adiamento.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>A data de adiamento não pode ser a mesma que a data original.</item>
        ///     <item>A nova data deve estar pelo menos 12 horas no futuro.</item>
        ///     <item>A partida deve estar em estado [SCHEDULED] ou [POST_PONED].</item>
        /// </list>
        /// </remarks>
        /// <param name="match">A partida a ser adiada.</param>
        /// <param name="newDate">A nova data proposta.</param>
        /// <param name="team">As estatísticas da equipa.</param>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="opponetTeam">As estatísticas do adversário.</param>
        /// <param name="idOpponnent">ID do adversário.</param>
        /// <exception cref="BusinessRuleException">Se as regras de horário ou status forem violadas.</exception>
        public void ValidatorPostPoneMatch(Matches match, DateTime newDate, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent)
        {
            if (match == null)
            {
                throw new ArgumentException("A match não pode estar nula");
            }

            if (match.MatchDate == newDate)
            {
                throw new BusinessRuleException("A data de adiamento não pode ser a mesma da data já marcada");
            }

            if ((newDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }

            if (match.MatchStatus != MatchStatus.SCHEDULED || match.MatchStatus != MatchStatus.POST_PONED)
            {
                throw new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento");
            }

            ValidateTeam(idTeam, team);

            ValidateTeam(idOpponnent, opponetTeam);
        }

        /// <summary>
        /// Valida se o DTO recebido é uma aceitação de pedido de adiamento.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="dto">DTO com o status de aceitação/rejeição.</param>
        /// <exception cref="InvalidOperationException">Se o status não for [StatusPostPone.ACCEPT].</exception>
        public void ValidateAcceptPostPoneMatchDto(Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            ValidateAcceptOrRefusePostPoneMatch(idTeam, dto);

            if (dto.StatusPostPone != StatusPostPone.ACCEPT)
            {
                throw new InvalidOperationException("Não está a acitar o pedido de adiamento");
            }
        }

        /// <summary>
        /// Valida as regras de negócio antes de aceitar um pedido de adiamento.
        /// </summary>
        /// <param name="postPoneMatch">O pedido de adiamento.</param>
        /// <param name="match">A partida alvo.</param>
        /// <param name="team">Estatísticas da equipa recetora.</param>
        /// <param name="idTeam">ID da equipa recetora.</param>
        /// <param name="opponetTeam">Estatísticas do adversário.</param>
        /// <param name="idOpponnent">ID do adversário.</param>
        /// <param name="matcheFind">Partida de conflito (deve ser nula).</param>
        /// <exception cref="ValidationException">Se houver conflito de horário.</exception>
        public void ValidatorAcceptPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent, Matches matcheFind)
        {
            ValidateStatusPostPoneMatch(match);

            ValidateTeam(idTeam, team);

            ValidateTeam(idOpponnent, opponetTeam);

            ValidatePostPoneMatch(postPoneMatch, idTeam);

            /*
            if (matcheFind != null)
            {
                throw new ValidationException("A equipa já tem um jogo marcado 12 horas, antes desse adiamento");
            }
            */
        }

        /// <summary>
        /// Valida se o DTO recebido é uma rejeição de pedido de adiamento.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="dto">DTO com o status de aceitação/rejeição.</param>
        /// <exception cref="InvalidOperationException">Se o status não for [StatusPostPone.REJECT].</exception>
        public void ValidateRejectPostPoneMatchDTO(Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            ValidateAcceptOrRefusePostPoneMatch(idTeam, dto);

            if (dto.StatusPostPone != StatusPostPone.REJECT)
            {
                throw new InvalidOperationException("Não está a rejeitar o pedido de adiamento");
            }
        }

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
            TeamStatistics opponetTeam, Guid idOpponnent)
        {
            ValidateStatusPostPoneMatch(match);

            ValidateTeam(idTeam, team);

            ValidateTeam(idOpponnent, opponetTeam);

            ValidatePostPoneMatch(postPoneMatch, idTeam);
        }

        /// <summary>
        /// Valida os filtros do pedido de adiamento.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: As datas mínimas (Jogo Original e Proposta) não podem ser no passado, e os intervalos (Mínima vs Máxima) devem ser válidos.
        /// </remarks>
        /// <param name="filter">O DTO de filtros.</param>
        /// <exception cref="InvalidOperationException">Se as datas forem inconsistentes ou no passado.</exception>
        public void ValidateFilterPostPoneMatch(FilterPostPoneMatchDto filter)
        {
            if (filter.MinDatePostPoneGame.HasValue && filter.MinDatePostPoneGame.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidOperationException("A data mínima de adiamento do jogo, não pode ser menor que agora");
            }

            if (filter.MinDatePostPoneGame.HasValue && filter.MaxDatePostPoneGame.HasValue)
            {
                if (filter.MinDatePostPoneGame.Value > filter.MaxDatePostPoneGame.Value)
                {
                    throw new InvalidOperationException("A data minima de adiamento não pode superior há data máxima");
                }
            }

            if (filter.MinDateGame.HasValue && filter.MinDateGame.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidOperationException("A data mínima do jogo, não pode ser menor que hoje");
            }

            if (filter.MinDateGame.HasValue && filter.MaxDateGame.HasValue)
            {
                if (filter.MinDateGame.Value > filter.MaxDateGame.Value)
                {
                    throw new InvalidOperationException("A data minima de jogo não pode superior há data máxima");
                }
            }

        }
        #endregion

        #region Cancelamento

        /// <summary>
        /// Valida os IDs de entrada e a descrição para o cancelamento de uma partida.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="idMatch">ID da partida.</param>
        /// <param name="description">Motivo do cancelamento.</param>
        /// <exception cref="ArgumentException">Se os IDs forem vazios ou a descrição estiver inválida/fora dos limites.</exception>
        public void ValidateVariabelCancelMatch(Guid idTeam, Guid idMatch, string description)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa não pode estar vazio");
            }

            if (idMatch == Guid.Empty)
            {
                throw new ArgumentException("O id da partida está vazio");
            }

            if (!string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("A descrição tem de estar preenchida");
            }

            var lengthDescription = description.Length;
            if (lengthDescription < ModelConstants.CancelledMatchConst.MinDescriptionLength || lengthDescription > ModelConstants.CancelledMatchConst.MaxDescriptionLength)
            {
                throw new ArgumentException("A descrição não tem o tamanho correto");
            }

        }

        /// <summary>
        /// Valida as regras de negócio para o cancelamento de uma partida.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>A partida deve ser [SCHEDULED] (Agendada).</item>
        ///     <item>O cancelamento deve ser feito com pelo menos 2 dias de antecedência.</item>
        ///     <item>As estatísticas da equipa devem ser válidas.</item>
        /// </list>
        /// </remarks>
        /// <param name="match">A partida a cancelar.</param>
        /// <param name="team">Estatísticas da equipa que cancela.</param>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="opponent">Estatísticas do adversário.</param>
        /// <param name="idOpponent">ID do adversário.</param>
        /// <exception cref="ArgumentException">Se a partida for nula ou o status for inválido.</exception>
        /// <exception cref="BusinessRuleException">Se o cancelamento for feito com menos de 2 dias de antecedência.</exception>
        public void ValidateCancelMatch(Matches match, TeamStatistics team, Guid idTeam, TeamStatistics opponent, Guid idOpponent)
        {
            if (match == null)
            {
                throw new ArgumentException("A match a cancelar não existe ou já não pode ser cancelada.");
            }

            if (match.MatchStatus != MatchStatus.SCHEDULED)
            {
                throw new ArgumentException($"ERRO! O estado é {match.MatchStatus}, mas devia ser SCHEDULED.");
            }

            var diffDaysToCancel = (match.MatchDate - DateTime.UtcNow).TotalDays;
            const int numDays = 2;

            if (diffDaysToCancel < numDays)
            {
                throw new BusinessRuleException("Uma partida só pode ser cancelada " + numDays + " dias antes da data do jogo");
            }

            ValidateTeam(idTeam, team);
            ValidateTeam(idOpponent, opponent);
        }
        #endregion

        #region Validation Finish Match

        /// <summary>
        /// Valida os IDs e a consistência do resultado submetido.
        /// </summary>
        /// <param name="idTeam">ID da equipa que submete o resultado.</param>
        /// <param name="result">O DTO de resultado.</param>
        /// <exception cref="ArgumentException">Se o ID da equipa não coincidir com o DTO.</exception>
        public void validateResultMatch(Guid idTeam, ResultMatchDto result)
        {
            if (idTeam != result.IdTeam)
            {
                throw new ArgumentException("A team que está a tentar finalizar não faz parte do jogo");
            }

            ValidateNumGoals(result.NumGoalsTeam);

            ValidateNumGoals(result.NumGoalsOpponent);
        }

        /// <summary>
        /// Valida as regras de negócio para a finalização de uma partida.
        /// </summary>
        /// <param name="match">A partida.</param>
        /// <param name="team">Estatísticas da equipa submetida.</param>
        /// <param name="idTeam">ID da equipa submetida.</param>
        /// <param name="opponent">Estatísticas do adversário.</param>
        /// <param name="idOponnent">ID do adversário.</param>
        /// <param name="result">O DTO de resultado submetido.</param>
        /// <exception cref="ArgumentException">Se a partida não existir ou o contexto for inválido.</exception>
        public void ValidateFinishMatch(Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponent, Guid idOponnent, ResultMatchDto result)
        {
            if (match == null)
            {
                throw new ArgumentException("A match não existe ou então não está em progresso");
            }

            if (team == null)
            {
                throw new ArgumentException("A team que quer finalizar a partida não foi encontrada ou não existe");
            }

            if (idTeam != result.IdTeam)
            {
                throw new ArgumentException("A team que está a tentar finalizar não faz parte do jogo");
            }

            if (opponent == null)
            {
                throw new ArgumentException("A team que quer finalizar a partida não foi encontrada ou não existe");
            }

            if (opponent.IdTeam == result.IdOpponent)
            {
                throw new ArgumentException("A team que quer finalizar a partida não foi encontrada ou não existe");
            }
        }

        /// <summary>
        /// Valida a intenção de cancelar a finalização da partida (sair do lobby de finalização).
        /// </summary>
        /// <param name="match">A partida.</param>
        /// <param name="team">A equipa.</param>
        public void ValidateCancelFinishMatch(Matches match, Team team)
        {
            if (match == null)
            {
                throw new ArgumentException("A match não foi encontrada");
            }

            if (team == null)
            {
                throw new ArgumentException("A equipa que está a tentar sair do match não faz parte do mesmo");
            }
        }
        #endregion

        #region Private Validations

        /// <summary>
        /// Verifica se a equipa está realmente envolvida na partida e se os dados foram carregados corretamente.
        /// </summary>
        private static void ValidateTeam(Guid idTeam, TeamStatistics? teamStatistics)
        {
            if (teamStatistics == null)
            {
                throw new ArgumentException("A equipa não existe neste jogo");
            }

            if (teamStatistics.Team == null)
            {
                throw new ArgumentException("Não foram carregados os dados da equipa");
            }

            if (teamStatistics.IdTeam != idTeam)
            {
                throw new BusinessRuleException("A equipa não pertence ao jogo");
            }
        }

        /// <summary>
        /// Valida se o pedido de adiamento é válido para a equipa que está a aceitar/rejeitar.
        /// </summary>
        /// <remarks>
        /// Regra principal: Garante que a equipa que executa a ação NÃO é a equipa que solicitou o adiamento.
        /// </remarks>
        private static void ValidatePostPoneMatch(PostPoneMatch postPoneMatch, Guid idTeam)
        {
            if (postPoneMatch == null)
            {
                throw new ArgumentException("O adiamento da partida está a null");
            }

            if (postPoneMatch.IdTeamPostPone == idTeam)
            {
                throw new BusinessRuleException($"Apenas a equipa que recebeu o convite pode aceita-lo ou rejeita-lo");
            }
        }

        /// <summary>
        /// Valida os IDs essenciais para aceitar ou rejeitar um pedido de adiamento.
        /// </summary>
        private static void ValidateAcceptOrRefusePostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da Team está vazio");
            }

            if (dto.IdMatch == Guid.Empty)
            {
                throw new ArgumentException("O id da match não pode estar vazio");
            }

            if (dto.IdTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa não pode estar vazio");
            }

            if (dto.IdOpponent == Guid.Empty)
            {
                throw new ArgumentException("O id do opponete não pode estar vazio");
            }
        }

        /// <summary>
        /// Valida se o status da partida é [POST_PONED].
        /// </summary>
        private static void ValidateStatusPostPoneMatch(Matches? match)
        {
            if (match == null)
            {
                throw new ArgumentException("A match não pode estar nula");
            }

            if (match.MatchStatus != MatchStatus.POST_PONED)
            {
                throw new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento");
            }
        }

        /// <summary>
        /// Valida se o número de golos está correto (não pode ser negativo).
        /// </summary>
        private static void ValidateNumGoals(int numGoals)
        {
            if (numGoals < 0)
            {
                throw new InvalidOperationException("O número de golos de uma equipa não pode ser menor que 0");
            }
        }
        #endregion
    }
}
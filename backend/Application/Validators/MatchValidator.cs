using Application.DTOs;
using Application.DTOs.Filters;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Validators
{
    public class MatchValidator: IMatchValidator
    {
        public void ValidateFilterCalendar(Guid idTeam, FilterCalendar filter)
        {
            var dateMin = filter.MinDate;
            var dateMax = filter.MaxDate;

            if (idTeam == Guid.Empty)
            {
                throw new InvalidOperationException("O id da equipa não pode estar nulo");
            }

            if (dateMin.HasValue && dateMax.HasValue)
            {
                if (dateMin.Value > dateMax.Value)
                {
                    throw new InvalidOperationException("A data minima tem de ser inferior ou igual à data maxima");
                }
            }
        }
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

            if (match.MatchStatus != MatchStatus.SCHEDULED && match.MatchStatus != MatchStatus.POST_PONED)
            {
                throw new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento");
            }

            ValidateTeam(idTeam, team);

            ValidateTeam(idOpponnent, opponetTeam);
        }

        public void ValidatorAcceptPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent)
        {
            ValidateStatusPostPoneMatch(match);

            ValidateTeam(idTeam, team);

            ValidateTeam(idOpponnent, opponetTeam);

            ValidatePostPoneMatch(postPoneMatch, idTeam);
        }

        public void ValidatorRejectPostPoneMatch(PostPoneMatch postPoneMatch, Matches match, TeamStatistics team, Guid idTeam,
            TeamStatistics opponetTeam, Guid idOpponnent)
        {
            ValidateStatusPostPoneMatch(match);

            ValidateTeam(idTeam, team);

            ValidateTeam(idOpponnent, opponetTeam);

            ValidatePostPoneMatch(postPoneMatch, idTeam);
        }


        public void ValidatorGetListPostPoneMatchTeam(List<InfoPostPoneMatch> listPostPone)
        {
            if (listPostPone.Count == 0)
            {
                throw new EmptyCollectionException("A lista de adiamentos da equipa está vazia");
            }
        }

        public void ValidateCancelMatch(Matches match, TeamStatistics team, Guid idTeam, TeamStatistics opponent, Guid idOpponent) {
            if (match == null)
            {
                throw new ArgumentException("A match a cancelar não existe ou já não pode ser cancelada.");
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

        public void validateResultMatch(Guid idTeam, ResultMatchDto result)
        {
            if (idTeam != result.IdTeam)
            {
                throw new ArgumentException("A team que está a tentar finalizar não faz parte do jogo");
            }

            ValidateNumGoals(result.MyTeamGoals);

            ValidateNumGoals(result.OpponentGoals);
        }

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

        public void ValidateCancelFinishMatch(Matches match, Teams team)
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

        private void ValidateTeam(Guid idTeam, TeamStatistics? teamStatistics)
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

        private void ValidatePostPoneMatch(PostPoneMatch postPoneMatch, Guid idTeam)
        {
            if (postPoneMatch == null)
            {
                throw new ArgumentException("O adiamento da partida está a null");
            }

            if (postPoneMatch.IdTeamPostPone == idTeam)
            {
                throw new BusinessRuleException("Apenas a equipa que recebeu o convite pode aceita-lo ou rejeita-lo");
            }
        }

        private void ValidateStatusPostPoneMatch(Matches? match)
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

        private void ValidateNumGoals(int numGoals) {
            if (numGoals < 0)
            {
                throw new InvalidOperationException("O número de golos de uma equipa não pode ser menor que 0");
            }
        }
    }
}

using Application.DTOs.PostPoneGame;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Validators
{
    public class MatchValidator: IMatchValidator
    {
        public void ValidatorPostPoneMatch(Matches match, DateTime newDate, TeamStatistics team, Guid idTeam, 
            TeamStatistics opponetTeam, Guid idOpponnent)
        {
            if (match == null)
            {
                throw new ArgumentNullException("A match não pode estar nula");
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
                throw new ArgumentNullException("A match a cancelar não existe ou já não pode ser cancelada.");
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

        private void ValidateTeam(Guid idTeam, TeamStatistics? teamStatistics)
        {
            if (teamStatistics == null)
            {
                throw new ArgumentNullException("A equipa não existe neste jogo");
            }

            if (teamStatistics.Team == null)
            {
                throw new ArgumentNullException("Não foram carregados os dados da equipa");
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
                throw new ArgumentNullException("O adiamento da partida está a null");
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
                throw new ArgumentNullException("A match não pode estar nula");
            }

            if (match.MatchStatus != MatchStatus.POST_PONED)
            {
                throw new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento");
            }
        }
    }
}

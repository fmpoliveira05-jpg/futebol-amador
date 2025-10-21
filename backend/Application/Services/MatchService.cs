using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class MatchService: IMatchService
    {
        IMatchRepository matchRepository;
        IUnityOfWork unityOfWork;

        public MatchService(IMatchRepository matchRepository, IUnityOfWork unityOfWork)
        {
            this.matchRepository = matchRepository;
            this.unityOfWork = unityOfWork;
        }

        /***
         * Deve validar se o user não atualizou para mais ou menos 5 minutos.
         * Ou seja só dá para atualizar para um tempo significativo
         */
        public async Task<Matches> PostPoneMatch(PostponeMatchDTO dto)
        {
            var idMatch = dto.IdMatch;
            var match = await matchRepository.GetMatchById(idMatch);

            if (match == null)
            {
                throw new ArgumentNullException("A match não pode estar nula", nameof(match));
            }

            var newDate = dto.MatchDate;
            
            if (match.MatchDate == newDate) {
                throw new BusinessRuleException("A data de adiamento não pode ser a mesma da data já marcada");
            }

            if (match.MatchStatus != MatchStatus.SCHEDULED && match.MatchStatus != MatchStatus.POST_PONED)
            {
                throw new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento");
            }
            
            var teamStatistic = match.ShowTeamStatistics(dto.IdTeam);

            if (teamStatistic.Id != dto.IdTeam) {
                throw new BusinessRuleException("O id da equipa deve ser igual ao do url");
            }

            var opponentStatistics = match.ShowTeamStatistics(dto.IdOpponent);

            if (teamStatistic.Id != dto.IdOpponent)
            {
                throw new BusinessRuleException("O id da equipa deve ser igual ao do url");
            }

            //Adiar a partida
            match.MatchDate = dto.MatchDate;
            match.MatchStatus = MatchStatus.POST_PONED;

            try 
            {
                opponentStatistics.Team.Calendar.PostPoneMatch(idMatch, newDate);
                teamStatistic.Team.Calendar.PostPoneMatch(idMatch, newDate);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
            catch (ArgumentException ex) 
            {
                throw new ArgumentException(ex.Message);
            }

            await unityOfWork.SaveChangesAsync();
            
            return match;
        }

        private string validateStatusPostPoneMatch(Matches? match)
        {
            if (match == null)
            {
                return "A match não pode estar nula";
            }

            if (match.MatchStatus != MatchStatus.POST_PONED)
            {
                return "Só podem ser adiadas partidas marcadas ou em estado de adiamento";
            }

            return "";
        }

        public async Task<Matches> AcceptPostPoneMatch(AcceptRefusePostPoneDTO dto)
        {
            //Talvez meter isto no controller
            if(dto.StatusPostPone != StatusPostPone.ACCEPT)
            {
                throw new BusinessRuleException("Para poder aceitar um convite ele precisa de estar aceite");
            }

            var idMatch = dto.IdMatch;
            var match = await matchRepository.GetMatchById(idMatch);
            var validatePostPone = validateStatusPostPoneMatch(match);

            if (validatePostPone != "")
            {
                throw new MatchException(validatePostPone);
            }

            var teamStatistic = match.ShowTeamStatistics(dto.IdTeam);

            if (teamStatistic.Id != dto.IdTeam) 
            {
                throw new BusinessRuleException("O id da equipa deve ser igual ao do url");
            }

            var opponentStatistics = match.ShowTeamStatistics(dto.IdOpponent);

            if (teamStatistic.Id != dto.IdOpponent) 
            {
                throw new BusinessRuleException("O id da equipa deve ser igual ao do url");
            }

            //Adiar a partida
            match.MatchStatus = MatchStatus.SCHEDULED;

            try
            {
                opponentStatistics.Team.Calendar.AcceptPostPoneMatch(idMatch);
                teamStatistic.Team.Calendar.AcceptPostPoneMatch(idMatch);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
            catch (NotFindException ex)
            {
                throw new NotFindException(ex.Message);
            }

            await unityOfWork.SaveChangesAsync();

            return match;
        }

        /**
         * O jogo fica cancelado, chama o cancelMatch
         */
        public async Task<Matches> RejectPostPoneMatch(AcceptRefusePostPoneDTO dto)
        {
            //Talvez meter no controller
            if (dto.StatusPostPone != StatusPostPone.REJECT)
            {
                throw new BusinessRuleException("Para poder aceitar um convite ele precisa de estar aceite");
            }

            var idMatch = dto.IdMatch;
            var match = await matchRepository.GetMatchById(idMatch);
            var validatePostPone = validateStatusPostPoneMatch(match);

            if (validatePostPone != "")
            {
                throw new MatchException(validatePostPone);
            }

            var teamStatistic = match.ShowTeamStatistics(dto.IdTeam);

            if (teamStatistic.Id != dto.IdTeam)
            {
                throw new BusinessRuleException("O id da equipa deve ser igual ao do url");
            }

            var opponentStatistics = match.ShowTeamStatistics(dto.IdOpponent);

            if (teamStatistic.Id != dto.IdOpponent)
            {
                throw new BusinessRuleException("O id da equipa deve ser igual ao do url");
            }

            match.MatchStatus = MatchStatus.CANCELED;

            try
            {
                opponentStatistics.Team.Calendar.CancelMatch(match);
                teamStatistic.Team.Calendar.CancelMatch(match);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }

            await unityOfWork.SaveChangesAsync();

            return match;
        }
    }
}
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class MatchService: IMatchService
    {
        IMatchRepository matchRepository;
        ITeamStatisticsRepository teamStatisticsRepository;
        ITeamPostPoneGameRepository teamPostPoneGameRepository;
        ICancelledMatchRepository cancelledMatchRepository;
        IUnityOfWork unityOfWork;

        public MatchService(IMatchRepository matchRepository, ITeamStatisticsRepository teamStatisticsRepository, 
            ITeamPostPoneGameRepository teamPostPoneGameRepository, ICancelledMatchRepository cancelledMatchRepository,
            IUnityOfWork unityOfWork)
        {
            this.matchRepository = matchRepository;
            this.teamStatisticsRepository = teamStatisticsRepository;
            this.teamPostPoneGameRepository = teamPostPoneGameRepository;
            this.cancelledMatchRepository = cancelledMatchRepository;
            this.unityOfWork = unityOfWork;
        }

        /***
         * Trocar os dois if do opponeten e temam por private metodo
         * Deve validar se o user não atualizou para mais ou menos 5 minutos.
         * Ou seja só dá para atualizar para um tempo significativo
         */

        private string validateTeam(Guid idTeam, TeamStatistics? teamStatistics)
        {
            if (teamStatistics == null)
            {
                return "A equipa não existe neste jogo";
            }

            if (teamStatistics.IdTeam != idTeam)
            {
                return "A equipa não pertence ao jogo";
            }

            return "";
        }

        //Testar
        public async Task<InfoPostPoneMatch> PostPoneMatch(PostponeMatchDTO dto)
        {
            var idMatch = dto.IdMatch;
            var match = await matchRepository.GetMatchById(idMatch);

            if (match == null)
            {
                throw new ArgumentNullException("A match não pode estar nula", nameof(match));
            }

            var newDate = dto.PostPoneDate;
            
            if (match.MatchDate == newDate) {
                throw new BusinessRuleException("A data de adiamento não pode ser a mesma da data já marcada");
            }

            if (match.MatchStatus != MatchStatus.SCHEDULED && match.MatchStatus != MatchStatus.POST_PONED)
            {
                throw new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento");
            }

            var teamStatistic = await teamStatisticsRepository.GetTeamByIdAndMatch(dto.IdMatch, dto.IdTeam);

            string validateT = validateTeam(dto.IdTeam, teamStatistic);
            if (validateT != "")
            {
                throw new BusinessRuleException(validateT);
            }

            var opponentStatistics = await teamStatisticsRepository.GetTeamByIdAndMatch(dto.IdMatch, dto.IdOpponent);

            string validateO = validateTeam(dto.IdOpponent, opponentStatistics);
            if (validateO != "")
            {
                throw new BusinessRuleException(validateO);
            }

            var team = teamStatistic.Team;
            if (team == null)
            {
                throw new ArgumentNullException("A equipa que fez o adiamento está nula");
            }
 
            var postPoneDate = new PostPoneMatch(team, match, newDate);
            await teamPostPoneGameRepository.AddTeamPostPoneMatch(postPoneDate);


            match.MatchStatus = MatchStatus.POST_PONED;

            var postPoneMatch = new InfoPostPoneMatch
            {
                IdMatch = match.Id,
                PostPoneDate = match.MatchDate,
                IdTeam = teamStatistic.IdTeam,
                nameTeam = teamStatistic.Team.Name,
                IdOpponent = opponentStatistics.IdTeam,
                nameOpponent = opponentStatistics.Team.Name
            };
            await unityOfWork.SaveChangesAsync();

            return postPoneMatch;
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

        public async Task<MatchDto> AcceptPostPoneMatch(Guid idTeamUrl, AcceptRefusePostPoneDTO dto)
        {
            var idMatch = dto.IdMatch;
            var match = await matchRepository.GetMatchWitchPitchById(idMatch);
            string validatePostPone = validateStatusPostPoneMatch(match);

            if (validatePostPone != "")
            {
                throw new MatchException(validatePostPone);
            }

            var teamStatistic = await teamStatisticsRepository.GetTeamByIdAndMatch(idMatch, dto.IdTeam);

            string validateT = validateTeam(dto.IdTeam, teamStatistic);
            if (validateT != "")
            {
                throw new BusinessRuleException(validateT);
            }
            

            var opponentStatistics = await teamStatisticsRepository.GetTeamByIdAndMatch(idMatch, dto.IdOpponent);

            string validateO = validateTeam(dto.IdOpponent, opponentStatistics);
            if (validateO != "")
            {
                throw new BusinessRuleException(validateO);
            }

            var postPoneMatch = await teamPostPoneGameRepository.GetTeamPostPoneMatch(dto.IdOpponent, dto.IdMatch);

            if (postPoneMatch == null)
            {
                throw new ArgumentNullException("O adiamento da partida está a null");
            }

            if (postPoneMatch.IdTeamPostPone == teamStatistic.IdTeam)
            {
                throw new BusinessRuleException("Apenas a equipa que recebeu o convite pode aceita-lo");
            }

            //Adiar a partida
            teamPostPoneGameRepository.RemoveTeamPostPoneMatch(postPoneMatch);
            match.MatchStatus = MatchStatus.SCHEDULED;

            //O match não inclui o pitch
            var matchDTO = new MatchDto
            {
                IdMatch = dto.IdMatch,
                GameDate = match.MatchDate,
                NameTeam = teamStatistic.Team.Name,
                NameOpponent = opponentStatistics.Team.Name,
                NamePitch = match.Pitch.Name
            };

            await unityOfWork.SaveChangesAsync();

            return matchDTO;
        }

        /**
         * O jogo fica cancelado, chama o cancelMatch
         */
        public async Task RejectPostPoneMatch(Guid idTeamUrl, AcceptRefusePostPoneDTO dto)
        {
            var idMatch = dto.IdMatch;
            var match = await matchRepository.GetMatchById(idMatch);
            string validatePostPone = validateStatusPostPoneMatch(match);

            if (validatePostPone != "")
            {
                throw new MatchException(validatePostPone);
            }

            var teamStatistic = await teamStatisticsRepository.GetTeamByIdAndMatch(dto.IdMatch, dto.IdTeam);

            string validateT = validateTeam(dto.IdTeam, teamStatistic);
            if (validateT != "")
            {
                throw new BusinessRuleException(validateT);
            }

            var opponentStatistics = await teamStatisticsRepository.GetTeamByIdAndMatch(dto.IdMatch, dto.IdOpponent); ;

            string validateO = validateTeam(dto.IdOpponent, opponentStatistics);
            if (validateO != "")
            {
                throw new BusinessRuleException(validateO);
            }

            var postPoneMatch = await teamPostPoneGameRepository.GetTeamPostPoneMatch(dto.IdOpponent, dto.IdMatch);

            if (postPoneMatch == null)
            {
                throw new ArgumentNullException("O adiamento da partida está a null");
            }

            if (postPoneMatch.IdTeamPostPone == teamStatistic.IdTeam)
            {
                throw new BusinessRuleException("Apenas a equipa que recebeu o convite pode rejetia-lo");
            }

            teamPostPoneGameRepository.RemoveTeamPostPoneMatch(postPoneMatch);
            match.MatchStatus = MatchStatus.CANCELED;

            await unityOfWork.SaveChangesAsync();
        }

        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(Guid idTeam)
        {
            var listPostPone = await matchRepository.GetAllMatchPostPoneReceiverById(idTeam);
            
            if (listPostPone.Count == 0)
            {
                throw new EmptyCollectionException("A lista de adiamentos da equipa está vazia");
            }

            return listPostPone;
        }

        public async Task CancelMatch(Guid idTeam, Guid idMatch)
        {
            var match = await matchRepository.GetMatchValideToCancelById(idMatch);

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

            var teamsMatch = match.Teams;
            var teamStatistics = teamsMatch.FirstOrDefault(ts => ts.IdTeam == idTeam);

            string validateO = validateTeam(idTeam, teamStatistics);
            if (validateO != "")
            {
                throw new BusinessRuleException(validateO);
            }

            var opponentStatistic = teamsMatch.FirstOrDefault(ts => ts.IdTeam != idTeam);
            if (opponentStatistic == null)
            {
                throw new ArgumentNullException("O opponente da equipa para este jogo não foi encotnrado");
            }



            //Falta o resto!!! 
        }
    }
}
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class MatchService: IMatchService
    {
        IMatchRepository MatchRepository;
        ITeamStatisticsRepository TeamStatisticsRepository;
        ITeamPostPoneGameRepository TeamPostPoneGameRepository;
        ICancelledMatchRepository CancelledMatchRepository;
        IUnityOfWork UnityOfWork;
        IMatchValidator ValidatorMatch;

        public MatchService(IMatchRepository matchRepository, ITeamStatisticsRepository teamStatisticsRepository, 
            ITeamPostPoneGameRepository teamPostPoneGameRepository, ICancelledMatchRepository cancelledMatchRepository,
            IUnityOfWork unityOfWork, IMatchValidator validatorMatch)
        {
            this.MatchRepository = matchRepository;
            this.TeamStatisticsRepository = teamStatisticsRepository;
            this.TeamPostPoneGameRepository = teamPostPoneGameRepository;
            this.CancelledMatchRepository = cancelledMatchRepository;
            this.UnityOfWork = unityOfWork;
            this.ValidatorMatch = validatorMatch;

        }

        //Testar (Já fiz otimizações nas pesquisas)
        public async Task<InfoPostPoneMatch> PostPoneMatch(PostponeMatchDTO dto)
        {
            var idMatch = dto.IdMatch;
            var newDate = dto.PostPoneDate;
            var idTeam = dto.IdTeam;
            var idOpponnent = dto.IdOpponent;
            PostPoneMatch postPoneDate;
            Teams team;

            var match = await MatchRepository.GetMatchById(idMatch);
            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);

            //Valida os dados carregados
            ValidatorMatch.ValidatorPostPoneMatch(match, newDate, teamStatistic, idTeam, opponentStatistics, idOpponnent);

            //Criação do adiamento e atualização do estado da equipa
            team = teamStatistic.Team;
            postPoneDate = new PostPoneMatch(team, match, newDate);
            await TeamPostPoneGameRepository.AddTeamPostPoneMatch(postPoneDate);

            match.MatchStatus = MatchStatus.POST_PONED;

            var postPoneMatch = new InfoPostPoneMatch
            {
                IdMatch = idMatch,
                PostPoneDate = newDate,
                IdTeam = idTeam,
                nameTeam = team.Name,
                IdOpponent = idOpponnent,
                nameOpponent = opponentStatistics.Team.Name
            };

            await UnityOfWork.SaveChangesAsync();

            return postPoneMatch;
        }

        //Vou ter que implementar aquele find na database para ver se quem adiou tem um jogo já marcado a pelo menos 12 horas
        public async Task<MatchDto> AcceptPostPoneMatch(Guid idTeamUrl, AcceptRefusePostPoneDTO dto)
        {
            var idTeam = dto.IdTeam;
            var idOpponnent = dto.IdOpponent;
            var idMatch = dto.IdMatch;
            DateTime newDate;

            var postPoneMatch = await TeamPostPoneGameRepository.GetTeamPostPoneMatchWithPitch(idOpponnent, idMatch);
            var match = postPoneMatch?.Match;
            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);

            //Validator
            ValidatorMatch.ValidatorAcceptPostPoneMatch(postPoneMatch, match, teamStatistic, idTeam, opponentStatistics, idOpponnent);

            //Adiamento da partida
            TeamPostPoneGameRepository.RemoveTeamPostPoneMatch(postPoneMatch);
            newDate = postPoneMatch.PostPoneDate;
            match.MatchDate = newDate;
            match.MatchStatus = MatchStatus.SCHEDULED;

            var matchDTO = new MatchDto
            {
                IdMatch = idMatch,
                GameDate = newDate,
                NameTeam = teamStatistic.Team.Name,
                NameOpponent = opponentStatistics.Team.Name,
                NamePitch = match.Pitch.Name
            };

            await UnityOfWork.SaveChangesAsync();
            return matchDTO;
        }

        /**
         * O jogo fica cancelado, chama o cancelMatch
         */
        public async Task RejectPostPoneMatch(Guid idTeamUrl, AcceptRefusePostPoneDTO dto)
        {
            var idMatch = dto.IdMatch;
            var idTeam = dto.IdTeam;
            var idOpponnent = dto.IdOpponent;

            var postPoneMatch = await TeamPostPoneGameRepository.GetTeamPostPoneMatch(idOpponnent, idMatch);
            var match = postPoneMatch?.Match;
            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);

            ValidatorMatch.ValidatorRejectPostPoneMatch(postPoneMatch, match, teamStatistic, idTeam, opponentStatistics, idOpponnent);

            //Cancelamento do match
            TeamPostPoneGameRepository.RemoveTeamPostPoneMatch(postPoneMatch);
            match.MatchStatus = MatchStatus.CANCELED;

            await UnityOfWork.SaveChangesAsync();
        }

        //Testar
        public async Task CancelMatch(Guid idTeam, Guid idMatch, string description)
        {
            var match = await MatchRepository.GetMatchToCancelById(idMatch);
            var teamsStatistics = match?.Teams;
            var team = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponent = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam != idTeam);

            ValidatorMatch.ValidateCancelMatch(match, team, idTeam, opponent, opponent.IdTeam);
            //Cancelamento do jogo
            var cancelledMatch = new CancelledMatch(team.Team, match, description);
            await CancelledMatchRepository.AddCancelledMatch(cancelledMatch);
            match.MatchStatus = MatchStatus.CANCELED;
            
            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(Guid idTeam)
        {
            var listPostPone = await MatchRepository.GetAllMatchPostPoneReceiverById(idTeam);

            ValidatorMatch.ValidatorGetListPostPoneMatchTeam(listPostPone);

            return listPostPone;
        }
    }
}
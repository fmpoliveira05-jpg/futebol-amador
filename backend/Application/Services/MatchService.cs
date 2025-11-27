using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class MatchService: IMatchService
    {
        #region Inicializer
        private readonly IMatchRepository MatchRepository;
        private readonly ITeamPostPoneGameRepository TeamPostPoneGameRepository;
        private readonly ICancelledMatchRepository CancelledMatchRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly ICalendarValidator MatchValidator;
        private readonly IPlayerAuthorizationService AuthorizationService;
        private readonly IPlayerAuthorizationValidator AuthorizationValidator;
        private IMatchRepository object1;
        private ITeamPostPoneGameRepository object2;
        private ICancelledMatchRepository object3;
        private IUnityOfWork object4;
        private CalendarValidator validator;
        private IPlayerAuthorizationService object5;

        public MatchService(IMatchRepository matchRepository, ITeamPostPoneGameRepository teamPostPoneGameRepository, 
            ICancelledMatchRepository cancelledMatchRepository, IUnityOfWork unityOfWork, 
            ICalendarValidator MatchValidator,
            IPlayerAuthorizationService AuthorizationService)
        {
            this.MatchRepository = matchRepository;
            this.TeamPostPoneGameRepository = teamPostPoneGameRepository;
            this.CancelledMatchRepository = cancelledMatchRepository;
            this.UnityOfWork = unityOfWork;
            this.MatchValidator = MatchValidator;
            this.AuthorizationService = AuthorizationService;
        }
        #endregion

        #region Calendar
        public async Task<List<InfoMatchCalendar>> GetCalendar(Guid idTeam)
        {
            MatchValidator.ValidateTeamCalendar(idTeam);
            return await MatchRepository.GetAllMatchesTeam(idTeam);
        }

        public async Task<List<InfoMatchCalendar>> GetCalendarWithFilters(Guid idTeam, FilterCalendarDto filter)
        {
            MatchValidator.ValidateFilterCalendar(idTeam, filter);
            return await MatchRepository.GetAllMatchesTeamWithFilters(idTeam, filter);
        }

        public async Task<InfoMatch> GetMatchById(Guid idTeam, Guid idMatch)
        {
            var match = await MatchRepository.GetMatchById(idMatch);

            if (match == null)
            {
                throw new ArgumentException("A match não foi encontrada");
            }

            var team = match.Teams.FirstOrDefault(ts => ts.Team.Id == idTeam);

            if (team == null)
            {
                throw new ArgumentException("A equipa solicitante não faz parte desta partida.");
            }

            var opponent = match.Teams.FirstOrDefault(ts => ts.Team.Id != idTeam);

            if (opponent == null)
            {
                throw new Exception("Oponente não encontrado.");
            }

            return new InfoMatch
            {
                IdMatch = match.Id,
                Team = new TeamDto
                {
                    IdTeam = team.Id,
                    Name = team.Team.Name,
                },
                Opponent = new TeamDto
                {
                    IdTeam = opponent.Id,
                    Name = opponent.Team.Name,
                },
                GameDate = match.MatchDate,
                IsCompetitive = match.IsCompetive,
                IsHome = match.idPitch == team.Team.IdPitch
            };
        }

        #endregion

        #region PostPoneMatch
        public async Task<InfoPostPoneMatch> PostPoneMatch(Guid idTeam, PostPoneMatchDto dto)
        {
            var idMatch = dto.IdMatch;
            var match = await MatchRepository.GetMatchById(idMatch);
            if (match == null)
            {
                throw new BusinessRuleException("A partida não foi encontrada.");
            }

            MatchValidator.ValidatePostPoneMatchDto(idTeam, dto);

            var newDate = dto.PostPoneDate;
            var idOpponnent = dto.IdOpponent;
            PostPoneMatch postPoneDate;
            Team team;

            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);

            if (teamStatistic == null || opponentStatistics == null)
            {
                throw new BusinessRuleException("A partida não possui equipas válidas.");
            }

            if (match.MatchStatus != MatchStatus.SCHEDULED && match.MatchStatus != MatchStatus.POST_PONED)
            {
                throw new BusinessRuleException("Só podem ser adiadas partidas marcadas ou em estado de adiamento.");
            }

            if (newDate <= DateTime.UtcNow)
            {
                throw new BusinessRuleException("A nova data não pode ser igual ou antes da data atual.");
            }

            if (newDate == match.MatchDate)
            {
                throw new BusinessRuleException("A data de adiamento não pode ser a mesma da data já marcada.");
            }

            team = teamStatistic.Team;
            postPoneDate = new PostPoneMatch(team, match, newDate);
            await TeamPostPoneGameRepository.AddTeamPostPoneMatch(postPoneDate);

            match.MatchStatus = MatchStatus.POST_PONED;

            var postPoneMatch = new InfoPostPoneMatch
            {
                IdMatch = idMatch,
                PostPoneDate = newDate,
                Team = new TeamDto
                {
                    IdTeam = team.Id,
                    Name = team.Name,
                },
                Opponent =
                {
                    IdTeam = idOpponnent,
                    Name = opponentStatistics.Team.Name
                }
            };

            await UnityOfWork.SaveChangesAsync();

            return postPoneMatch;
        }

        //Vou ter que implementar aquele find na database para ver se quem adiou tem um jogo já marcado a pelo menos 12 horas
        public async Task<MatchDto> AcceptPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            MatchValidator.ValidateAcceptPostPoneMatchDto(idTeam, dto);
            var idOpponnent = dto.IdOpponent;
            var idMatch = dto.IdMatch;
            DateTime newDate;
            var postPoneMatch = await TeamPostPoneGameRepository.GetTeamPostPoneMatchWithPitch(idOpponnent, idMatch);
            var match = postPoneMatch?.Match;
            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);
            var validateMatch = await MatchRepository.GetMatchProxim12HoursMatchs(idTeam, match.MatchDate);
            
            //Validator
            MatchValidator.ValidatorAcceptPostPoneMatch(postPoneMatch, match, teamStatistic, idTeam, opponentStatistics, idOpponnent, validateMatch);

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
        public async Task RejectPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            MatchValidator.ValidateRejectPostPoneMatchDTO(idTeam, dto);
            
            var idMatch = dto.IdMatch;
            var idOpponnent = dto.IdOpponent;

            var postPoneMatch = await TeamPostPoneGameRepository.GetTeamPostPoneMatch(idOpponnent, idMatch);
            var match = postPoneMatch?.Match;
            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);

            MatchValidator.ValidatorRejectPostPoneMatch(postPoneMatch, match, teamStatistic, idTeam, opponentStatistics, idOpponnent);

            TeamPostPoneGameRepository.RemoveTeamPostPoneMatch(postPoneMatch);
            match.MatchStatus = MatchStatus.CANCELED;

            await UnityOfWork.SaveChangesAsync();
        }

        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(Guid idTeam)
        {
            MatchValidator.ValidateTeamCalendar(idTeam);
            var listPostPone = await MatchRepository.GetAllMatchPostPoneReceiverById(idTeam);

            return listPostPone;
        }

        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeamWithFilters(Guid idTeam, FilterPostPoneMatchDto filter)
        {
            MatchValidator.ValidateTeamCalendar(idTeam);
            MatchValidator.ValidateFilterPostPoneMatch(filter);
            var listPostPone = await MatchRepository.GetAllMatchPostPoneReceiverByIdWithFilters(idTeam, filter);

            return listPostPone;
        }

        #endregion

        #region CancelMatch
        public async Task CancelMatch(Guid idTeam, Guid idMatch, string description)
        {
            var match = await MatchRepository.GetMatchToCancelById(idMatch);
            MatchValidator.ExistsMatch(match);

            var teamsStatistics = match?.Teams;
            var team = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponent = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam != idTeam);

            MatchValidator.ValidateCancelMatch(match, team, idTeam, opponent, opponent.IdTeam);

            var cancelledMatch = new CancelledMatch(team.Team, match, description);

            await CancelledMatchRepository.AddCancelledMatch(cancelledMatch);

            match.MatchStatus = MatchStatus.CANCELED;

            await UnityOfWork.SaveChangesAsync();
        }
        #endregion
    }
}
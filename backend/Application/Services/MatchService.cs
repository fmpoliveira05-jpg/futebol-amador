using Application.DTOs.Filters;
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
        #region Inicializer
        private readonly IMatchRepository MatchRepository;
        private readonly ITeamPostPoneGameRepository TeamPostPoneGameRepository;
        private readonly ICancelledMatchRepository CancelledMatchRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly ICalendarValidator MatchValidator;
        private readonly IPlayerRepository PlayerRepository;
        private readonly IAuthorizationService AuthorizationService;

        public MatchService(IMatchRepository matchRepository, ITeamPostPoneGameRepository teamPostPoneGameRepository, 
            ICancelledMatchRepository cancelledMatchRepository, IUnityOfWork unityOfWork, 
            ICalendarValidator MatchValidator, IPlayerRepository playerRepository,
            IAuthorizationService AuthorizationService)
        {
            this.MatchRepository = matchRepository;
            this.TeamPostPoneGameRepository = teamPostPoneGameRepository;
            this.CancelledMatchRepository = cancelledMatchRepository;
            this.UnityOfWork = unityOfWork;
            this.MatchValidator = MatchValidator;
            this.PlayerRepository = playerRepository;
            this.AuthorizationService = AuthorizationService;
        }
        #endregion

        #region Calendar
        public async Task<List<InfoMatchCalendar>> GetCalendar(string userId, Guid idTeam)
        {
            await AuthorizationService.UserAuthorizationIsMemberTeamById(userId, idTeam);
            MatchValidator.ValidateTeamCalendar(idTeam);
            return await MatchRepository.GetAllMatchesTeam(idTeam);
        }

        public async Task<List<InfoMatchCalendar>> GetCalendarWithFilters(string userId, Guid idTeam, FilterCalendarDto filter)
        {
            await AuthorizationService.UserAuthorizationIsMemberTeamById(userId, idTeam);
            MatchValidator.ValidateFilterCalendar(idTeam, filter);
            return await MatchRepository.GetAllMatchesTeamWithFilters(idTeam, filter);
        }

        #endregion

        #region PostPoneMatch
        public async Task<InfoPostPoneMatch> PostPoneMatch(string userId, Guid idTeam, PostPoneMatchDto dto)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(userId, idTeam);

            MatchValidator.ValidatePostPoneMatchDto(idTeam, dto);

            var idMatch = dto.IdMatch;
            var newDate = dto.PostPoneDate;
            var idOpponnent = dto.IdOpponent;
            PostPoneMatch postPoneDate;
            Team team;

            var match = await MatchRepository.GetMatchById(idMatch);
            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);

            //Valida os dados carregados
            MatchValidator.ValidatorPostPoneMatch(match, newDate, teamStatistic, idTeam, opponentStatistics, idOpponnent);

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
        public async Task<MatchDto> AcceptPostPoneMatch(string userId, Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(userId, idTeam);

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
        public async Task RejectPostPoneMatch(string userId, Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(userId, idTeam);
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

        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(string userId, Guid idTeam)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(userId, idTeam);
            MatchValidator.ValidateTeamCalendar(idTeam);
            var listPostPone = await MatchRepository.GetAllMatchPostPoneReceiverById(idTeam);

            return listPostPone;
        }

        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeamWithFilters(string userId, Guid idTeam, FilterPostPoneMatchDto filter)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(userId, idTeam);
            MatchValidator.ValidateTeamCalendar(idTeam);
            MatchValidator.ValidateFilterPostPoneMatch(filter);
            var listPostPone = await MatchRepository.GetAllMatchPostPoneReceiverByIdWithFilters(idTeam, filter);

            return listPostPone;
        }

        #endregion

        #region CancelMatch
        public async Task CancelMatch(string userId, Guid idTeam, Guid idMatch, string description)
        {
            await AuthorizationService.UserAuthorizationIsAdminTeamById(userId, idTeam);

            var match = await MatchRepository.GetMatchToCancelById(idMatch);
            MatchValidator.ExistsMatch(match);

            var teamsStatistics = match?.Teams;
            var team = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponent = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam != idTeam);

            MatchValidator.ExistsTeamStatistics(opponent);

            MatchValidator.ValidateCancelMatch(match, team, idTeam, opponent, opponent.IdTeam);
            
            var cancelledMatch = new CancelledMatch(team.Team, match, description);
            await CancelledMatchRepository.AddCancelledMatch(cancelledMatch);
            
            match.MatchStatus = MatchStatus.CANCELED;
            
            await UnityOfWork.SaveChangesAsync();
        }
        #endregion
    }
}
using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

/*
 Fazer breves testes para ver se está tudo a dar com estas alterações
 */
namespace Application.Services
{
    public class MatchService: IMatchService
    {
        private readonly IMatchRepository MatchRepository;
        private readonly ITeamPostPoneGameRepository TeamPostPoneGameRepository;
        private readonly ICancelledMatchRepository CancelledMatchRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly ICalendarValidator MatchValidator;
        private object @object;

        public MatchService(IMatchRepository matchRepository, ITeamPostPoneGameRepository teamPostPoneGameRepository, 
            ICancelledMatchRepository cancelledMatchRepository, IUnityOfWork unityOfWork, 
            ICalendarValidator MatchValidator, IPlayerRepository @object)
        {
            this.MatchRepository = matchRepository;
            this.TeamPostPoneGameRepository = teamPostPoneGameRepository;
            this.CancelledMatchRepository = cancelledMatchRepository;
            this.UnityOfWork = unityOfWork;
            this.MatchValidator = MatchValidator;
        }

        public MatchService(object @object)
        {
            this.@object = @object;
        }

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

        public async Task<InfoPostPoneMatch> PostPoneMatch(Guid idTeam, PostPoneMatchDto dto)
        {
            MatchValidator.ValidatePostPoneMatchDto(idTeam, dto);

            var idMatch = dto.IdMatch;
            var newDate = dto.PostPoneDate;
            var idOpponnent = dto.IdOpponent;
            PostPoneMatch postPoneDate;
            Team team;

            var match = await MatchRepository.GetMatchById(idMatch);
            var teamStatistic = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponentStatistics = match?.Teams.FirstOrDefault(ts => ts.IdTeam == idOpponnent);

            if (match == null)
            {
                throw new BusinessRuleException("A partida não foi encontrada.");
            }

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
                IdTeam = idTeam,
                nameTeam = team.Name,
                IdOpponent = idOpponnent,
                nameOpponent = opponentStatistics.Team.Name
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

            //Validator
            MatchValidator.ValidatorAcceptPostPoneMatch(postPoneMatch, match, teamStatistic, idTeam, opponentStatistics, idOpponnent);

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
            MatchValidator.ValidatorGetListPostPoneMatchTeam(listPostPone);

            return listPostPone;
        }

        public async Task CancelMatch(Guid idTeam, Guid idMatch, string description)
        {
            var match = await MatchRepository.GetMatchToCancelById(idMatch);
            if (match == null)
            {
                throw new ArgumentException("A match a cancelar não existe ou já não pode ser cancelada.");
            }

            var teamsStatistics = match?.Teams;
            if (teamsStatistics == null || !teamsStatistics.Any())
            {
                throw new ArgumentException("As estatísticas da equipe não foram encontradas.");
            }

            var team = teamsStatistics.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponent = teamsStatistics.FirstOrDefault(ts => ts.IdTeam != idTeam);

            if (team == null)
            {
                throw new ValidationException("A equipa não pertence à partida.");
            }
            if (opponent == null)
            {
                throw new ValidationException("O adversário não foi encontrado na partida.");
            }

            MatchValidator.ValidateCancelMatch(match, team, idTeam, opponent, opponent.IdTeam);

            var cancelledMatch = new CancelledMatch(team.Team, match, description);

            await CancelledMatchRepository.AddCancelledMatch(cancelledMatch);

            match.MatchStatus = MatchStatus.CANCELED;

            await UnityOfWork.SaveChangesAsync();
        }

        Task<List<InfoPostPoneMatch>> IMatchService.GetListPostPoneMatchTeamWithFilters(Guid idTeam, FilterPostPoneMatchDto filter)
        {
            throw new NotImplementedException();
        }
    }
}
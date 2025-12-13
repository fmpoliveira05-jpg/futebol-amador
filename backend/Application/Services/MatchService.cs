using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.PostPoneGame;
using Application.DTOs.Team;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela lógica de negócio e ciclo de vida das Partidas ([Matches]).
    /// 
    /// Gere o calendário de jogos, pedidos de adiamento (Postpone), cancelamentos e visualização de detalhes.
    /// Coordena a interação entre repositórios de jogos, equipas e validadores de regras de negócio.
    /// </summary>
    public class MatchService: IMatchService
    {
        #region Inicializer
        private readonly IMatchRepository MatchRepository;
        private readonly ITeamPostPoneGameRepository TeamPostPoneGameRepository;
        private readonly ICancelledMatchRepository CancelledMatchRepository;
        private readonly IUnityOfWork UnityOfWork;
        private readonly ICalendarValidator MatchValidator;
        private readonly IPlayerRepository PlayerRepository;
        private readonly INotificationFirebaseService notificationFirebaseService;

        /// <summary>
        /// Construtor do MatchService.
        /// </summary>
        /// <param name="matchRepository">Repositório de Partidas.</param>
        /// <param name="teamPostPoneGameRepository">Repositório de Pedidos de Adiamento.</param>
        /// <param name="cancelledMatchRepository">Repositório de Partidas Canceladas.</param>
        /// <param name="unityOfWork">Unidade de Trabalho para transações.</param>
        /// <param name="MatchValidator">Validador de regras de calendário e jogo.</param>
        public MatchService(IMatchRepository matchRepository, ITeamPostPoneGameRepository teamPostPoneGameRepository, 
            ICancelledMatchRepository cancelledMatchRepository, IUnityOfWork unityOfWork, 
            ICalendarValidator MatchValidator, IPlayerRepository PlayerRepository,
            INotificationFirebaseService notificationFirebaseService)
        {
            this.MatchRepository = matchRepository;
            this.TeamPostPoneGameRepository = teamPostPoneGameRepository;
            this.CancelledMatchRepository = cancelledMatchRepository;
            this.UnityOfWork = unityOfWork;
            this.MatchValidator = MatchValidator;
            this.PlayerRepository = PlayerRepository;
            this.notificationFirebaseService = notificationFirebaseService;
        }
        #endregion

        #region Calendar

        /// <summary>
        /// Obtém o calendário completo de jogos de uma equipa.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <returns>Lista de [InfoMatchCalendar] com os jogos agendados e realizados.</returns>
        public async Task<List<InfoMatchCalendar>> GetCalendar(Guid idTeam)
        {
            MatchValidator.ValidateTeamCalendar(idTeam);
            return await MatchRepository.GetAllMatchesTeam(idTeam);
        }

        /// <summary>
        /// Obtém o calendário de jogos de uma equipa, aplicando filtros de pesquisa.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="filter">Filtros (Datas, Adversário, Local).</param>
        /// <returns>Lista filtrada de [InfoMatchCalendar].</returns>
        public async Task<List<InfoMatchCalendar>> GetCalendarWithFilters(Guid idTeam, FilterCalendarDto filter)
        {
            MatchValidator.ValidateFilterCalendar(idTeam, filter);
            return await MatchRepository.GetAllMatchesTeamWithFilters(idTeam, filter);
        }

        /// <summary>
        /// Obtém os detalhes de uma partida específica para visualização.
        /// </summary>
        /// <remarks>
        /// Valida se a equipa solicitante faz parte do jogo e identifica o adversário.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que está a consultar.</param>
        /// <param name="idMatch">ID da partida.</param>
        /// <returns>DTO [InfoMatch] com os detalhes do jogo.</returns>
        /// <exception cref="ArgumentException">Se a partida não existir ou a equipa não participar nela.</exception>
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
                    IdTeam = team.IdTeam,
                    Name = team.Team.Name,
                },
                Opponent = new TeamDto
                {
                    IdTeam = opponent.IdTeam,
                    Name = opponent.Team.Name,
                },
                GameDate = match.MatchDate,
                IsCompetitive = match.IsCompetive,
                IsHome = match.idPitch == team.Team.IdPitch
            };
        }

        #endregion

        #region PostPoneMatch

        /// <summary>
        /// Inicia um pedido de adiamento (remarcação) de uma partida.
        /// </summary>
        /// <remarks>
        /// **Regras:**
        /// <list type="bullet">
        ///     <item>A partida deve existir e ter equipas válidas.</item>
        ///     <item>O status deve ser [SCHEDULED] ou [POST_PONED].</item>
        ///     <item>A nova data deve ser futura e diferente da atual.</item>
        /// </list>
        /// **Transação:** Cria um registo [PostPoneMatch] e atualiza o status da partida para [POST_PONED].
        /// </remarks>
        /// <param name="idTeam">ID da equipa que solicita o adiamento.</param>
        /// <param name="dto">DTO com a nova data proposta.</param>
        /// <returns>DTO [InfoPostPoneMatch] com os detalhes do pedido criado.</returns>
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

            if (DateTime.Compare(newDate, DateTime.UtcNow) <= 0)
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
                Opponent = new TeamDto
                {
                    IdTeam = idOpponnent,
                    Name = opponentStatistics.Team.Name
                }
            };

            await UnityOfWork.SaveChangesAsync();

            return postPoneMatch;
        }

        /// <summary>
        /// Aceita um pedido de adiamento de partida.
        /// </summary>
        /// <remarks>
        /// **Transação:**
        /// 1. Verifica se não há conflito de horário (jogos nas 12h adjacentes).
        /// 2. Remove o registo de pedido de adiamento ([TeamPostPoneGameRepository.RemoveTeamPostPoneMatch]).
        /// 3. Atualiza a data do jogo ([MatchDate]) para a nova data proposta.
        /// 4. Restaura o status do jogo para [SCHEDULED].
        /// </remarks>
        /// <param name="idTeam">ID da equipa que aceita (deve ser o recetor do pedido).</param>
        /// <param name="dto">DTO de aceitação.</param>
        /// <returns>DTO [MatchDto] com a partida atualizada.</returns>
        public async Task<MatchDto> AcceptPostPoneMatch(Guid idTeam, AcceptRefusePostPoneDto dto)
        {
            MatchValidator.ValidateAcceptPostPoneMatchDto(idTeam, dto);
            var idOpponnent = dto.IdOpponent;
            var idMatch = dto.IdMatch;
            DateTime newDate;
            var postPoneMatch = await TeamPostPoneGameRepository.GetTeamPostPoneMatchWithPitch(idTeam, idMatch);
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

            var nameTeam = teamStatistic.Team.Name;
            var opponentName = opponentStatistics.Team.Name;
           

            var matchDTO = new MatchDto
            {
                IdMatch = idMatch,
                GameDate = newDate,
                NameTeam = nameTeam,
                NameOpponent = opponentName,
                NamePitch = match.Pitch.Name
            };

            await UnityOfWork.SaveChangesAsync();

            await notifyAcceptPostPone(idMatch, idTeam, nameTeam, idOpponnent, opponentName, newDate);

            return matchDTO;
        }

        /// <summary>
        /// Rejeita um pedido de adiamento de partida.
        /// </summary>
        /// <remarks>
        /// **Transação:** Remove o pedido de adiamento e marca a partida como [CANCELED], pois não houve acordo.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que rejeita.</param>
        /// <param name="dto">DTO de rejeição.</param>
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

        /// <summary>
        /// Obtém a lista de pedidos de adiamento pendentes recebidos pela equipa.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <returns>Lista de [InfoPostPoneMatch].</returns>
        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeam(Guid idTeam)
        {
            MatchValidator.ValidateTeamCalendar(idTeam);
            var listPostPone = await MatchRepository.GetAllMatchPostPoneReceiverById(idTeam);

            return listPostPone;
        }

        /// <summary>
        /// Obtém a lista de pedidos de adiamento pendentes com filtros.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="filter">Filtros de data e adversário.</param>
        /// <returns>Lista filtrada de [InfoPostPoneMatch].</returns>
        public async Task<List<InfoPostPoneMatch>> GetListPostPoneMatchTeamWithFilters(Guid idTeam, FilterPostPoneMatchDto filter)
        {
            MatchValidator.ValidateTeamCalendar(idTeam);
            MatchValidator.ValidateFilterPostPoneMatch(filter);
            var listPostPone = await MatchRepository.GetAllMatchPostPoneReceiverByIdWithFilters(idTeam, filter);

            return listPostPone;
        }

        #endregion

        #region CancelMatch

        /// <summary>
        /// Cancela uma partida agendada.
        /// </summary>
        /// <remarks>
        /// **Regras:** Só pode cancelar com antecedência mínima (ex: 2 dias).
        /// **Transação:** Cria um registo em [CancelledMatch] para histórico e atualiza o status da partida para [CANCELED].
        /// </remarks>
        /// <param name="idTeam">ID da equipa que cancela.</param>
        /// <param name="idMatch">ID da partida.</param>
        /// <param name="description">Motivo do cancelamento.</param>
        public async Task CancelMatch(Guid idTeam, Guid idMatch, string description)
        {
            var match = await MatchRepository.GetMatchToCancelById(idMatch);
            MatchValidator.ExistsMatch(match);

            var teamsStatistics = match?.Teams;
            var team = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam == idTeam);
            var opponent = teamsStatistics?.FirstOrDefault(ts => ts.IdTeam != idTeam);
            var idOpponentTeam = opponent.IdTeam;

            MatchValidator.ValidateCancelMatch(match, team, idTeam, opponent, idOpponentTeam);

            var cancelledMatch = new CancelledMatch(team.Team, match, description);

            await CancelledMatchRepository.AddCancelledMatch(cancelledMatch);

            match.MatchStatus = MatchStatus.CANCELED;

            await UnityOfWork.SaveChangesAsync();

            await notifyCancelMatch(idMatch, idTeam, team.Team.Name, idOpponentTeam, opponent.Team.Name);
        }


        #endregion

        #region Notifications
        private async Task notifyCancelMatch(Guid matchId, Guid idTeam, string nameTeam, Guid idOpponnent, string opponentName)
        {
            var title = "Jogo Cancelado";
            var textTeam = $"A sua partida com a equipa {opponentName} foi cancelada!";
            var textOpponent = $"A sua partida com a equipa {nameTeam} foi cancelada pelos mesmos.";
            var type = "CANCEL_MATCH";
            var payloadTeam = new Dictionary<string, string>
            {
                { "type", type },
                { "matchId", matchId.ToString() },
                { "title", title },
                { "body", textTeam }
            };
            var payloadOpponent = new Dictionary<string, string>
            {
                { "type", type },
                { "matchId", matchId.ToString() },
                { "title", title },
                { "body", textOpponent }
            };

            await notificationFirebaseService.sendNotificationToTeamsWithDataAsync(idTeam, idOpponnent, payloadTeam, payloadOpponent);
        }

        private async Task notifyAcceptPostPone(Guid matchId, Guid idTeam, string nameTeam, Guid idOpponnent, string opponentName, DateTime newDate)
        {
            var dateUtc = newDate.Kind == DateTimeKind.Utc ? newDate : DateTime.SpecifyKind(newDate, DateTimeKind.Utc);
            long newDateMillis = new DateTimeOffset(dateUtc).ToUnixTimeMilliseconds();

            var title = "Jogo Reagendado";
            var textTeam = $"A sua partida com a equipa {opponentName} foi adiada para o dia {newDate:dd/MM HH:mm}.";
            var textOpponent = $"A sua partida com a equipa {nameTeam} foi adiada para o dia {newDate:dd/MM HH:mm}.";
            var type = "POST_PONE_MATCH";

            var payloadTeam = new Dictionary<string, string>
            {
                { "type", type },
                { "matchId", matchId.ToString() },
                { "newDateMillis", newDateMillis.ToString() },
                { "title", title },
                { "body", textTeam }
            };

            var payloadOpponent = new Dictionary<string, string>
            {
                { "type", type },
                { "matchId", matchId.ToString() },
                { "newDateMillis", newDateMillis.ToString() },
                { "title", title },
                { "body", textOpponent }
            };

            await notificationFirebaseService.sendNotificationToTeamsWithDataAsync(idTeam, idOpponnent, payloadTeam, payloadOpponent);
        }
        #endregion
    }
}
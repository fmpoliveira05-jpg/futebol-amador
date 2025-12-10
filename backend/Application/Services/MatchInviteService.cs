using Application.DTOs.Chat;
using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.DTOs.Team;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Hub;
using Application.Interfaces.Validators;
using Domain.Entities;
using System.ComponentModel;

namespace Application.Services
{
    /// <summary>
    /// Serviço de domínio responsável pela lógica de negócio e ciclo de vida dos Convites de Partida ([MatchInvite]).
    /// 
    /// Esta classe orquestra a criação, aceitação, rejeição e negociação de desafios entre equipas,
    /// garantindo o cumprimento das regras de negócio e a sincronização do estado da base de dados.
    /// </summary>
    public class MatchInviteService: IMatchInviteService
    {
        #region Initialization
        private readonly IMatchInviteRepository MatchInviteRepository;
        private readonly ITeamRepository TeamRepository;
        private readonly IMatchRepository MatchRepository;
        private readonly IPitchRepository PitchRepository;
        private readonly IMatchInviteValidator MatchInviteValidator;
        private readonly IUnityOfWork UnityOfWork;
        private readonly INotificationService notificationService;
        private readonly INotificationFirebaseService notificationFirebaseService;
        private readonly IChatRoomService ChatService;

        /// <summary>
        /// Construtor do MatchInviteService.
        /// </summary>
        public MatchInviteService(
            IMatchInviteRepository matchInviteRepository,
            ITeamRepository teamRepository,
            IMatchRepository matchRepository,
            IPitchRepository pitchRepository,
            IMatchInviteValidator matchInviteValidator,
            IUnityOfWork unityOfWork,
            INotificationService notificationService,
            IChatRoomService chatRoomService,
            INotificationFirebaseService notificationFirebaseService)

        {
            this.MatchInviteRepository = matchInviteRepository;
            this.TeamRepository = teamRepository;
            this.MatchRepository = matchRepository;
            this.PitchRepository = pitchRepository;
            this.MatchInviteValidator = matchInviteValidator;
            this.UnityOfWork = unityOfWork;
            this.notificationService = notificationService;
            this.ChatService = chatRoomService;
            this.notificationFirebaseService = notificationFirebaseService;
        }
        #endregion

        #region Methods MatchInvite

        /// <summary>
        /// Obtém a informação resumida de um convite de partida se este estiver associado à equipa especificada.
        /// </summary>
        /// <remarks>
        /// Verifica se a equipa (`idTeam`) é a remetente ou recetora do convite e projeta os dados no DTO **[InfoMatchInviteDto]**.
        /// </remarks>
        /// <param name="idMatchInvite">O ID (GUID) do convite.</param>
        /// <param name="idTeam">O ID da equipa (Utilizador) que está a aceder ao convite.</param>
        /// <returns>O DTO **[InfoMatchInviteDto]** com os detalhes resumidos, ou <c>null</c> se não for encontrado ou se a equipa não estiver envolvida.</returns>
        public async Task<InfoMatchInviteDto?> GetMatchInvite(Guid idTeam, Guid idMatchInvite)
        {
            var matchInvite = await MatchInviteRepository.GetMatchInviteById(idMatchInvite);
            
            if(matchInvite == null)
            {
                return null;
            }

            var team = matchInvite.Sender;

            if (team.Id != idTeam)
            {
                team = matchInvite.Receiver;
            }

            var infoMatchInvite = new InfoMatchInviteDto
            {
                Id = matchInvite.Id,
                Receiver = new TeamDto
                {
                    IdTeam = matchInvite.Receiver.Id,
                    Name = matchInvite.Receiver.Name
                },
                Sender = new TeamDto
                {
                    IdTeam = matchInvite.Sender.Id,
                    Name = matchInvite.Sender.Name
                },
                GameDate = matchInvite.GameDate,
                NamePitch = matchInvite.Pitch.Name,
                isHome = matchInvite.Pitch == team.Pitch,
            };

            return infoMatchInvite;
        }

        /// <summary>
        /// Envia um novo convite de partida (desafio) de uma equipa para outra.
        /// </summary>
        /// <remarks>
        /// **Transação:**
        /// 1. Valida IDs, horário e duplicidade.
        /// 2. Cria a entidade [MatchInvite] e a persiste.
        /// 3. Cria a sala de chat associada ao convite.
        /// 4. Notifica os administradores da equipa recetora.
        /// </remarks>
        /// <param name="idSender">ID da equipa que envia o desafio.</param>
        /// <param name="dto">DTO com os termos do jogo (Data, Pitch, Recetor).</param>
        /// <returns>DTO [InfoMatchInviteDto] do convite criado.</returns>
        public async Task<InfoMatchInviteDto> SendMatchInvite(Guid idSender, SendMatchInviteDto dto)
        {
            MatchInviteValidator.ValidateSenderMatchInvite(dto, idSender);
            
            var idReceiver = dto.IdReceiver;
            var gameDate = dto.GameDate;
            var isHome = dto.homePitch;
            var receiver = await TeamRepository.GetTeamByIdWithPitchAsync(idReceiver);
            var existingMatchInvite = await MatchInviteRepository.GetMatchInvite(idSender, idReceiver, gameDate);
            var findMatchWith12hours = await MatchRepository.GetMatchProxim12HoursMatchs(idSender, gameDate);
            var sender = await TeamRepository.GetTeamByIdWithPitchAsync(idSender);
            MatchInviteValidator.ValidateSendMatchInvite(receiver, sender, existingMatchInvite, findMatchWith12hours);

            Pitch pitch = GetPitchMatch(isHome, sender.Pitch, receiver.Pitch);

            var matchInvite = new MatchInvite(sender, receiver, gameDate, pitch);

            await MatchInviteRepository.AddMatchInvite(matchInvite);
            
            var sendMatchInviteDto = new InfoMatchInviteDto
            {
                Id = matchInvite.Id,
                Sender = new TeamDto
                {
                    IdTeam = sender.Id,
                    Name = sender.Name
                },
                Receiver = new TeamDto
                {
                    IdTeam = receiver.Id,
                    Name = receiver.Name,
                },             
                GameDate = gameDate,
                NamePitch = pitch.Name,
                isHome = isHome
            };

            var teamAdmins = receiver.Members
                            .Where(p => p.IsAdmin == true)
                            .ToList();

            foreach (var admin in teamAdmins)
            {
                await notificationService.SendUserAsync(receiver.Id.ToString(), "New Match Invite", $"{sender.Name} wants to play a match against your team!");
            }
            var roomname = sender.Name + ".V.S." + receiver.Name + " " + gameDate;
            await ChatService.CreateMatchRoomAsync(new CreateChatRoomRequestDto {RoomName = roomname, TeamIds ={receiver.Id, sender.Id } },idSender.ToString());

            await UnityOfWork.SaveChangesAsync();

            await notificationFirebaseService.sendNotificationToTeamsAsync(idSender, idReceiver, "NEW_MATCH_INVITE", "Novo convite de partida",
                $"Envio do convite para a equipa {receiver.Name}, com sucesso!",
                $"A sua equipa recebeu um novo convite de partida da equipa {sender.Name}.");

            return sendMatchInviteDto;
        }

        /// <summary>
        /// Aceita um convite de partida recebido pela Equipa.
        /// </summary>
        /// <remarks>
        /// **Transação Crítica:**
        /// 1. Valida conflito de horário (12h).
        /// 2. Remove o convite da base de dados.
        /// 3. Cria a entidade [Matches] ([Matches.MatchStatus] = SCHEDULED) e as [TeamStatistics].
        /// 4. Adiciona o novo jogo ao calendário de ambas as equipas.
        /// 5. Notifica ambas as equipas.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que aceita (Recetora).</param>
        /// <param name="idMatchInvite">ID do convite.</param>
        /// <returns>DTO [MatchDto] da partida agendada.</returns>
        public async Task<MatchDto> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite)
        {
            MatchInviteValidator.ValidateAcceptRefuseMatchInvite(idTeam, idMatchInvite);

            DateTime gameDate;
            var receiver = await TeamRepository.GetByIdWithReceivedInvitesAndCalendar(idTeam);
            MatchInviteValidator.ValidateReciever(receiver);

            var receivedInvitesList = receiver?.ReceivedInvites;
            MatchInvite? matchInvite = receivedInvitesList?.FirstOrDefault(i => i.Id == idMatchInvite);

            MatchInviteValidator.ValidateMatchInvite(matchInvite);

            var sender = await TeamRepository.GetByIdWithReceivedInvitesAndCalendar(matchInvite.IdSender);
            var pitch = await PitchRepository.GetPitchById(matchInvite.IdPitch);
            var validateMatch = await MatchRepository.GetMatchProxim12HoursMatchs(receiver.Id, matchInvite.GameDate);

            MatchInviteValidator.ValidateAcceptMatchInvite(sender, validateMatch, matchInvite, pitch);
            
            gameDate = matchInvite.GameDate;
            List<TeamStatistics> teamStatistics = ListTeamsStatistics(sender, receiver);

            var match = new Matches(gameDate, false, pitch.Id, teamStatistics, matchInvite.Chat);
            
            MatchInviteRepository.DeleteMatchInvite(matchInvite);

            receiver.Calendar.Matches.Add(match);
            sender.Calendar.Matches.Add(match);
            await MatchRepository.AddMatch(match);
 
            var nameTeam = receiver.Name;
            var nameOpponent = sender.Name;

            var matchDTO = new MatchDto
            {
                IdMatch = match.Id,
                GameDate = match.MatchDate,
                NameTeam = nameTeam,
                NameOpponent = sender.Name,
                NamePitch = pitch.Name
            };

            await notificationService.SendTeamAsync(receiver.Id.ToString(), "Match Scheduled!", $"Your match against {sender.Name} has been Scheduled to {match.MatchDate}, don't miss it!");
            await notificationService.SendTeamAsync(sender.Id.ToString(), "Match Scheduled!", $"Your match against {receiver.Name} has been Scheduled to {match.MatchDate}, don't miss it!");

            await notificationFirebaseService.sendNotificationToTeamsAsync(idTeam, sender.Id, "ACCEPT_MATCH_INVITE", "Partida marcada",
                $"A sua partida com a equipa {nameOpponent} foi marcada!",
                $"A sua partida com a equipa {nameTeam} foi marcada.");

            await UnityOfWork.SaveChangesAsync();

            return matchDTO;
        }

        /// <summary>
        /// Rejeita um convite de partida recebido pela Equipa.
        /// </summary>
        /// <remarks>
        /// **Transação:** Remove o [MatchInvite] da base de dados e notifica o remetente.
        /// </remarks>
        /// <param name="idTeam">ID da equipa que rejeita.</param>
        /// <param name="idMatchInvite">ID do convite a ser rejeitado.</param>
        public async Task RefuseMatchInvites(Guid idTeam, Guid idMatchInvite)
        {
            MatchInviteValidator.ValidateAcceptRefuseMatchInvite(idTeam, idMatchInvite);

            MatchInvite? matchInvite = null;
            var receiver = await TeamRepository.GetByIdWithReceivedInvites(idTeam);
            
            MatchInviteValidator.ValidateReciever(receiver);

            matchInvite = receiver.ReceivedInvites.FirstOrDefault(i => i.Id == idMatchInvite);
            MatchInviteValidator.ValidateMatchInvite(matchInvite);

            var sender = await TeamRepository.GetTeamByIdAsync(matchInvite.IdSender);

            MatchInviteValidator.ValidateRefuseMatchInvite(sender, matchInvite);

            MatchInviteRepository.DeleteMatchInvite(matchInvite);
            await UnityOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Envia uma contra-proposta para um convite de partida recebido.
        /// </summary>
        /// <remarks>
        /// **Transação:**
        /// 1. Valida se a negociação é válida (não pode haver conflito de horário).
        /// 2. Atualiza os dados do convite ([GameDate], [Pitch]).
        /// 3. **Inverte o Remetente/Recetor** no convite para que a equipa original se torne o recetor (aguardando a próxima resposta).
        /// 4. Notifica o recetor original da contra-proposta.
        /// </remarks>
        /// <param name="idSender">ID da equipa que envia a contra-proposta (Recetor Original).</param>
        /// <param name="dto">DTO com os novos termos propostos.</param>
        /// <returns>DTO [InfoMatchInviteDto] com o convite atualizado.</returns>
        public async Task<InfoMatchInviteDto> NegociateMatchInvite(Guid idSender, SendMatchInviteDto dto)
        {
            MatchInviteValidator.ValidateSenderMatchInvite(dto, idSender);
            
            var gameDate = dto.GameDate;
            var isHome = dto.homePitch;
            var idReceiver = dto.IdReceiver;
            var hasChanged = false;
            var matchInvite = await MatchInviteRepository.GetMatchInviteWithPitchByTeams(idReceiver, idSender);
            var findMatchWith12hour = await MatchRepository.GetMatchProxim12HoursMatchs(idReceiver, gameDate);

            var senderTeam = await TeamRepository.GetTeamByIdAsync(matchInvite.IdSender);
            var receiverTeam = await TeamRepository.GetTeamByIdAsync(matchInvite.IdReceiver);
            var senderPitch = await PitchRepository.GetPitchById(senderTeam.IdPitch);
            var receiverPitch = await PitchRepository.GetPitchById(receiverTeam.IdPitch);

            var pitch = GetPitchMatch(isHome, senderTeam.Pitch, receiverTeam.Pitch);

            MatchInviteValidator.ValidateNegociateMatchInvite(pitch, matchInvite, senderTeam, receiverTeam, findMatchWith12hour);
            
            hasChanged = NegociateMatchInvite(matchInvite, gameDate, pitch); 

            MatchInviteValidator.ValidateHasChangeNegociateMatchInvite(hasChanged);

            var sendMatchInviteDto = new InfoMatchInviteDto
            {
                Id = matchInvite.Id,
                Sender = new TeamDto
                {
                    IdTeam = senderTeam.Id,
                    Name = senderTeam.Name
                },
                Receiver = new TeamDto
                {
                    IdTeam = receiverTeam.Id,
                    Name = receiverTeam.Name
                },
                GameDate = gameDate,
                NamePitch = pitch.Name
            };

            await UnityOfWork.SaveChangesAsync();

            return sendMatchInviteDto;
        }

        /// <summary>
        /// Obtém a lista completa de convites de partida recebidos por uma equipa (sem filtros).
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <returns>Lista de [InfoMatchInviteDto].</returns>
        public async Task<List<InfoMatchInviteDto?>> GetAllMatchInvitesTeam(Guid idTeam)
        {
            MatchInviteValidator.ValidateTeamCalendar(idTeam);

            var listMatchInvites = await MatchInviteRepository.GetAllMatchInviteReceiverById(idTeam);
           
            return listMatchInvites;
        }

        /// <summary>
        /// Obtém a lista de convites de partida recebidos por uma equipa, aplicando filtros.
        /// </summary>
        /// <param name="idTeam">ID da equipa.</param>
        /// <param name="filter">Filtros (Nome do Remetente, Data).</param>
        /// <returns>Lista filtrada de [InfoMatchInviteDto].</returns>
        public async Task<List<InfoMatchInviteDto?>> GetAllMatchInvitesTeamWithFilters(Guid idTeam, FilterMatchInvitesDto filter)
        {
            MatchInviteValidator.ValidateFilterMatchInvite(idTeam, filter);
            var teamSearching = await TeamRepository.GetTeamByIdAsync(idTeam);
            if (teamSearching == null) {
                return null;
            }

            var listMatchInvite = await MatchInviteRepository.GetAllMatchInvitesTeamWithFilters(idTeam, filter);

            return listMatchInvite;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Cria as entidades estatísticas ([TeamStatistics]) para duas equipas (necessário para criar uma [Matches]).
        /// </summary>
        /// <param name="sender">Equipa 1.</param>
        /// <param name="receiver">Equipa 2.</param>
        /// <returns>Lista de [TeamStatistics] inicializadas (0-0).</returns>
        private static List<TeamStatistics> ListTeamsStatistics(Team sender, Team receiver)
        {
            var list = new List<TeamStatistics>();

            TeamStatistics sendTeam = new TeamStatistics(sender);
            TeamStatistics receiverTeam = new TeamStatistics(receiver);
            list.Add(sendTeam);
            list.Add(receiverTeam);

            return list;
        }

        /// <summary>
        /// Lógica central da Negociação: Atualiza a [GameDate] e/ou [Pitch] do convite e **inverte** o remetente/recetor.
        /// </summary>
        /// <param name="matchInvite">A entidade [MatchInvite] a ser atualizada.</param>
        /// <param name="gameDate">A nova data proposta.</param>
        /// <param name="pitch">O novo campo proposto.</param>
        /// <returns>True se houve alguma alteração nos dados do convite.</returns>
        private static bool NegociateMatchInvite(MatchInvite matchInvite, DateTime gameDate, Pitch pitch)
        {
            Guid idPitch = pitch.Id;
            bool hasChanged = false;

            if (matchInvite.GameDate != gameDate)
            {
                matchInvite.GameDate = gameDate;
                hasChanged = true;
            }

            if (matchInvite.IdPitch != idPitch)
            {
                matchInvite.IdPitch = idPitch;
                matchInvite.Pitch = pitch;
                hasChanged = true;
            }

            if (hasChanged)
            {
                var idSender = matchInvite.IdSender;

                matchInvite.IdSender = matchInvite.IdReceiver;
                matchInvite.IdReceiver = idSender;
            }

            return hasChanged;
        }

        /// <summary>
        /// Determina qual o Campo ([Pitch]) a usar para o jogo com base na escolha do remetente ([isHome]).
        /// </summary>
        /// <param name="isHome">Se o jogo será no campo do remetente (true) ou do recetor (false).</param>
        /// <param name="senderPitch">O campo da equipa remetente.</param>
        /// <param name="receiverPitch">O campo da equipa recetora.</param>
        /// <returns>O Pitch selecionado.</returns>
        private Pitch GetPitchMatch(bool isHome, Pitch senderPitch, Pitch receiverPitch)
        {
            Pitch pitchGame = senderPitch;
            if(!isHome)
            {
                pitchGame = receiverPitch;
            }

            return pitchGame;
        }

        #endregion
    }
}
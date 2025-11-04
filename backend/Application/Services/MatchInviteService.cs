using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Validators;
using Domain.Entities;

namespace Application.Services
{
    public class MatchInviteService: IMatchInviteService
    {
        private readonly ITeamRepository TeamRepository;
        
        private readonly IMatchInviteRepository MatchInviteRepository;
        
        private readonly IMatchRepository MatchRepository;
        
        private readonly ITeamStatisticsRepository TeamStatisticsRepository;

        private readonly IPitchRepository PitchRepository;

        private readonly IUnityOfWork UnityOfWork;

        private readonly IMatchInviteValidator MatchInviteValidator;

        public MatchInviteService(ITeamRepository teamRepository, IMatchInviteRepository matchInviteRepository, 
            IMatchRepository matchRepository, ITeamStatisticsRepository teamStatisticsRepository,
            IPitchRepository pitchRepository, IMatchInviteValidator matchInviteValidator,
            IUnityOfWork unityOfWork, ITeamPostPoneGameRepository @object) {
            this.TeamRepository = teamRepository; 
            this.MatchInviteRepository = matchInviteRepository;
            this.MatchRepository = matchRepository;
            this.TeamStatisticsRepository = teamStatisticsRepository;
            this.PitchRepository = pitchRepository;
            this.MatchInviteValidator = matchInviteValidator;
            this.UnityOfWork = unityOfWork;
        }

        public async Task<InfoMatchInviteDto> SendMatchInvite(Guid idSender, SendMatchInviteDto dto)
        {
            MatchInviteValidator.ValidateSenderMatchInvite(dto, idSender);
            
            var idReceiver = dto.IdReceiver;
            var gameDate = dto.GameDate;
            var pitchName = dto.namePitch;
            Pitch? pitch = null;

            var receiver = await TeamRepository.GetTeamByIdWithPitchAsync(idReceiver);
            var existingMatchInvite = await MatchInviteRepository.GetMatchInvite(idSender, idReceiver, gameDate);
            var findMatchWith12hours = await MatchRepository.GetMatchProxim12HoursMatchs(idSender, gameDate);
            var sender = await TeamRepository.GetTeamByIdWithPitchAsync(idSender);
            MatchInviteValidator.ValidateSendMatchInvite(receiver, sender, existingMatchInvite, findMatchWith12hours, pitchName);
            
            if (pitchName == receiver.Pitch.Name)
            {
                pitch = receiver.Pitch;
            } 
            else
            {
                pitch = sender.Pitch;
            }

            var matchInvite = new MatchInvite(sender, receiver, gameDate, pitch);

            await MatchInviteRepository.AddMatchInvite(matchInvite);
            
            var sendMatchInviteDto = new InfoMatchInviteDto
            {
                Id = matchInvite.Id,
                IdSender = matchInvite.IdSender,
                NameSender = sender.Name,
                IdReceiver = matchInvite.IdReceiver,
                NameReceiver = receiver.Name,
                GameDate = gameDate,
                NamePitch = pitchName
            };

            await UnityOfWork.SaveChangesAsync();

            return sendMatchInviteDto;
        }

        public async Task<MatchDto> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite)
        {
            MatchInviteValidator.ValidateAcceptRefuseMatchInvite(idTeam, idMatchInvite);

            DateTime gameDate;
            var receiver = await TeamRepository.GetByIdWithReceivedInvitesAndCalendar(idTeam);
            MatchInviteValidator.ValidateReciever(receiver);

            var receivedInvitesList = receiver?.ReceivedInvites;
            MatchInvite? matchInvite = receivedInvitesList?.FirstOrDefault(i => i.Id == idMatchInvite);

            MatchInviteValidator.ValidateMatchInvite(matchInvite);

            var sender = await TeamRepository.GetTeamByIdAsync(matchInvite.IdSender); ;
            var pitch = await PitchRepository.GetPitchById(matchInvite.IdPitch);
            var validateMatch = await MatchRepository.GetMatchProxim12HoursMatchs(receiver.Id, matchInvite.GameDate);

            MatchInviteValidator.ValidateAcceptMatchInvite(sender, validateMatch, matchInvite, pitch);
            
            gameDate = matchInvite.GameDate;
            List<TeamStatistics> teamStatistics = await ListTeamsStatistics(sender, receiver);

            var match = new Matches(gameDate, false, pitch.Id, teamStatistics, matchInvite.Chat);
            
            MatchInviteRepository.DeleteMatchInvite(matchInvite);

            receiver.Calendar.Matches.Add(match);
            sender.Calendar.Matches.Add(match);
            await MatchRepository.AddMatch(match);


            var matchDTO = new MatchDto
            {
                IdMatch = match.Id,
                GameDate = match.MatchDate,
                NameTeam = receiver.Name,
                NameOpponent = sender.Name,
                NamePitch = pitch.Name
            };

            await UnityOfWork.SaveChangesAsync();

            return matchDTO;
        }

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


        public async Task<InfoMatchInviteDto> NegociateMatchInvite(Guid idSender, SendMatchInviteDto dto)
        {
            MatchInviteValidator.ValidateSenderMatchInvite(dto, idSender);
            
            var gameDate = dto.GameDate;
            var namePitch = dto.namePitch;
            var idReceiver = dto.IdReceiver;
            bool hasChanged = false;
            var matchInvite = await MatchInviteRepository.GetMatchInviteWithPitchByTeams(idSender, idReceiver);
            var findMatchWith12hour = await MatchRepository.GetMatchProxim12HoursMatchs(idReceiver, gameDate);
            var senderTeam = matchInvite?.Sender;
            var receiverTeam = matchInvite?.Receiver;
            var pitch = matchInvite?.Pitch;

            MatchInviteValidator.ValidateNegociateMatchInvite(namePitch, pitch, matchInvite, senderTeam, receiverTeam, findMatchWith12hour);
            
            hasChanged = NegociateMatchInvite(matchInvite, gameDate, pitch); 

            MatchInviteValidator.ValidateHasChangeNegociateMatchInvite(hasChanged);
          
            var sendMatchInviteDto = new InfoMatchInviteDto
            {
                Id = matchInvite.Id,
                IdSender = matchInvite.IdSender,
                NameSender = senderTeam.Name,
                IdReceiver = matchInvite.IdReceiver,
                NameReceiver = receiverTeam.Name,
                GameDate = gameDate,
                NamePitch = namePitch
            };

            await UnityOfWork.SaveChangesAsync();

            return sendMatchInviteDto;
        }

        public async Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeam(Guid idTeam)
        {
            MatchInviteValidator.ValidateTeamCalendar(idTeam);

            var listMatchInvites = await MatchInviteRepository.GetAllMatchInviteReceiverById(idTeam);
           
            return listMatchInvites;
        }

        public async Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeamWithFilters(Guid idTeam, FilterMatchInvitesDto filter)
        {
            MatchInviteValidator.ValidateFilterMatchInvite(idTeam, filter);

            var listMatchInvite = await MatchInviteRepository.GetAllMatchInvitesTeamWithFilters(idTeam, filter);
            return listMatchInvite;
        }

        #region Private Methods
        private static async Task<List<TeamStatistics>> ListTeamsStatistics(Team sender, Team receiver)
        {
            var list = new List<TeamStatistics>();

            TeamStatistics sendTeam = new TeamStatistics(sender);
            TeamStatistics receiverTeam = new TeamStatistics(receiver);
            list.Add(sendTeam);
            list.Add(receiverTeam);

            return list;
        }

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

        #endregion
    }
}
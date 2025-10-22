using Application.DTOs.Match;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;

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

        public MatchInviteService(ITeamRepository teamRepository, IMatchInviteRepository matchInviteRepository, 
            IMatchRepository matchRepository, ITeamStatisticsRepository teamStatisticsRepository,
            IPitchRepository pitchRepository, IUnityOfWork unityOfWork) {
            this.TeamRepository = teamRepository; 
            this.MatchInviteRepository = matchInviteRepository;
            this.MatchRepository = matchRepository;
            this.TeamStatisticsRepository = teamStatisticsRepository;
            this.PitchRepository = pitchRepository;
            this.UnityOfWork = unityOfWork;
        }

        /***
         * Ainda não dá a regra das 12 horas para acietar, enviar e negociar (Ver se´já tem jogo marcado)
         */
        private async Task<string> validateHaveMatchWith12Hours(Guid idTeam, DateTime gameDate)
        {
            var findMatchWith12hours = await MatchRepository.GetMatchProxim12HoursMatchs(idTeam, gameDate);

            if (findMatchWith12hours != null)
            {
                return "Não pode marcar esse jogo a essa hora, porque já tem um a pelo menos 12 horas da data especificada";
            }

            return "";
        }

        /***
        * Ainda não dá a regra das 12 horas para acietar, enviar e negociar (Ver se´já tem jogo marcado)
        * Falta corrigir erro de quando clico em aceitar dar erro 500
        
         */
        public async Task<InfoMatchInviteDTO> SendMatchInvite(SendMatchInviteDTO dto)
        {
            if ((dto.GameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }

            var receiver = await TeamRepository.GetTeamByIdWithPitchAsync(dto.IdReceiver);

            if (receiver == null)
            {
                throw new ArgumentNullException("A equipa que recebeu o convite não foi encontrada");
            }

            var sender = await TeamRepository.GetTeamByIdWithPitchAsync(dto.IdSender);

            if (sender == null)
            {
                throw new ArgumentNullException("A equipa que enviou o convite não foi encontrada");
            }

            MatchInvite? matchInviteFind = await MatchInviteRepository.GetMatchInvite(dto);

            if (matchInviteFind != null)
            {
                throw new BusinessRuleException("A match invite já existe");
            }

            var validateMatch = await validateHaveMatchWith12Hours(dto.IdSender, dto.GameDate);
            if (validateMatch != "") {
                throw new BusinessRuleException(validateMatch);
            }

            if (sender.Pitch.Name != dto.namePitch && receiver.Pitch.Name != dto.namePitch)
            {
                throw new BusinessRuleException("O campo da partida não pertence a nenhuma das equipas");
            }

            //Determinar de quem é o campo
            var pitch = sender.Pitch;

            if (dto.namePitch == receiver.Pitch.Name)
            {
                pitch = receiver.Pitch;
            } 

            var matchInvite = new MatchInvite(sender, receiver, dto.GameDate, pitch);

            await MatchInviteRepository.AddMatchInvite(matchInvite);

            var sendMatchInviteDto = new InfoMatchInviteDTO
            {
                Id = matchInvite.Id,
                IdSender = matchInvite.IdSender,
                NameSender = sender.Name,
                IdReceiver = matchInvite.IdReceiver,
                NameReceiver = receiver.Name,
                GameDate = matchInvite.GameDate,
                NamePitch = pitch.Name
            };
            await UnityOfWork.SaveChangesAsync();

            return sendMatchInviteDto;
        }

        private async Task<List<TeamStatistics>> ListTeamsStatistics(Teams sender, Teams receiver)
        {
            var list = new List<TeamStatistics>();

            TeamStatistics sendTeam = new TeamStatistics(sender);
            TeamStatistics receiverTeam = new TeamStatistics(receiver);
            list.Add(sendTeam);
            list.Add(receiverTeam);

            await TeamStatisticsRepository.AddTeamStatistics(sendTeam);
            await TeamStatisticsRepository.AddTeamStatistics(receiverTeam);

            return list;
        }

        private string ValidateReciever(Teams receiver)
        { 
            if (receiver == null)
            {
                return "A equipa não foi encontrada";
            }

            var numReceivedInvites = receiver.ReceivedInvites.Count();
            if (numReceivedInvites == 0)
            {
                return "Não é possível recusar um convite porque não existem convites recebidos.";
            }

            return "";
        }

        private string ValidateSender(Teams sender, MatchInvite matchInvite)
        {

            if (sender.SentInvites.FirstOrDefault(matchInvite) == null)
            {
                return "O convite a recusar não existe na equipa oponetne";
            }

            return "";
        }

        //Trocar para DTO com dados do Match
        public async Task<MatchDto> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite)
        {
            var receiver = await TeamRepository.GetByIdWithReceivedInvitesAndCalendar(idTeam);
            
            string validateReceiver = ValidateReciever(receiver);
            if (validateReceiver != "")
            {
                throw new ValidatorException(validateReceiver);
            }
            
            var receivedInvitesList = receiver.ReceivedInvites;
            MatchInvite? matchInvite = receivedInvitesList.FirstOrDefault(i => i.Id == idMatchInvite);

            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite a aceitar não existe");
            }

            var validateMatch = await validateHaveMatchWith12Hours(receiver.Id, matchInvite.GameDate);
            if (validateMatch != "")
            {
                throw new BusinessRuleException(validateMatch);
            }

            var sender = await TeamRepository.GetTeamByIdAsync(matchInvite.IdSender);
           
            if (sender == null)
            {
                throw new NullReferenceException("O emissor do convite está a null");
            }

            var validateSender = ValidateSender(sender, matchInvite);
            if (validateSender != "")
            {
                throw new ValidatorException(validateSender);
            }

            var pitch = await PitchRepository.GetPitchById(matchInvite.IdPitch);
            if (pitch == null)
            {
                throw new ArgumentNullException("O campo do convite de partida não pode ser nulo");
            }

            List<TeamStatistics> teamStatistics = await ListTeamsStatistics(sender, receiver);

            var match = new Matches(matchInvite.GameDate, false, pitch, teamStatistics, matchInvite.Chat);

            await MatchInviteRepository.DeleteMatchInvite(matchInvite);
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

            //var acceptMatch
            return matchDTO;
        }

        public async Task RefuseMatchInvites(Guid idTeam, Guid idMatchInvite)
        {
            var receiver = await TeamRepository.GetByIdWithReceivedInvites(idTeam);

            string validateReceiver = ValidateReciever(receiver);
            if (validateReceiver != "")
            {
                throw new ValidatorException(validateReceiver);
            }

            var receivedInvitesList = receiver.ReceivedInvites;
            MatchInvite? matchInvite = receivedInvitesList.FirstOrDefault(i => i.Id == idMatchInvite);

            if (matchInvite == null)
            {
                throw new MatchInviteException("O convite a recusar não existe");
            }

            var validateMatch = await validateHaveMatchWith12Hours(receiver.Id, matchInvite.GameDate);
            if (validateMatch != "")
            {
                throw new BusinessRuleException(validateMatch);
            }

            var sender = await TeamRepository.GetTeamByIdAsync(matchInvite.IdSender);

            var validateSender = ValidateSender(sender, matchInvite);
            if(validateSender != "")
            {
                throw new ValidatorException(validateSender);
            }

            await MatchInviteRepository.DeleteMatchInvite(matchInvite);
 
            await UnityOfWork.SaveChangesAsync();
        }

        //Ver erro da data 12h e error 500 que deixa executar o codigo e depois é lançado
        public async Task<InfoMatchInviteDTO> NegociateMatchInvite(SendMatchInviteDTO dto)
        {
            if ((dto.GameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }

            var pitch = await PitchRepository.GetPitchByName(dto.namePitch);
            
            if (pitch == null)
            {
                throw new NullReferenceException("O campo não pode estar a nulo");
            }
            
            var matchInvite = await MatchInviteRepository.GetMatchInviteByTeams(dto.IdSender, dto.IdReceiver);

            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite de partida a negociar não existe", nameof(dto));
            }

            bool hasChanged = matchInvite.NegociateMatchInvite(dto.GameDate, pitch);

            if (!hasChanged)
            {
                throw new BusinessRuleException("Não é possível lançar uma contra-oferta uma vez que os dados estão iguais");
            }

            var senderTeam = await TeamRepository.GetTeamByIdAsync(dto.IdSender);

            if (senderTeam == null)
            {
                throw new NullReferenceException("A equipa que enviou o convite não foi encontrada");
            }

            var receiverTeam = await TeamRepository.GetTeamByIdAsync(dto.IdReceiver);

            if (receiverTeam == null)
            {
                throw new NullReferenceException("A equipa que recebeu o convite não foi encontrada");
            }


            var sendMatchInviteDto = new InfoMatchInviteDTO
            {
                Id = matchInvite.Id,
                IdSender = matchInvite.IdSender,
                NameSender = senderTeam.Name,
                IdReceiver = matchInvite.IdReceiver,
                NameReceiver = receiverTeam.Name,
                GameDate = matchInvite.GameDate,
                NamePitch = pitch.Name
            };

            await UnityOfWork.SaveChangesAsync();

            return sendMatchInviteDto;
        }

        public async Task<List<InfoMatchInviteDTO>> GetAllMatchInvitesTeam(Guid idTeam)
        {
            var team = await TeamRepository.GetTeamByIdAsync(idTeam);

            if (team == null)
            {
                throw new NullReferenceException("A team a consultar a lista de pedidos não existe");
            }

            List<InfoMatchInviteDTO> listMatchInvites = await MatchInviteRepository.GetAllMatchInviteReceiverById(idTeam);

            if (listMatchInvites == null || !listMatchInvites.Any())
            {
                throw new NullReferenceException("A equipa ainda não recebeu pedidos de partida");
            }

            return listMatchInvites;
        }
    }
}

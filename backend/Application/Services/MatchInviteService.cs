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
        private readonly ITeamRepository teamRepository;
        
        private readonly IMatchInviteRepository matchInviteRepository;
        
        private readonly IMatchRepository matchRepository;
        
        private readonly ITeamStatisticsRepository teamStatisticsRepository;

        private readonly IPitchRepository pitchRepository;

        private readonly IUnityOfWork unityOfWork;

        public MatchInviteService(ITeamRepository teamRepository, IMatchInviteRepository matchInviteRepository, 
            IMatchRepository matchRepository, ITeamStatisticsRepository teamStatisticsRepository,
            IPitchRepository pitchRepository, IUnityOfWork unityOfWork) {
            this.teamRepository = teamRepository; 
            this.matchInviteRepository = matchInviteRepository;
            this.matchRepository = matchRepository;
            this.teamStatisticsRepository = teamStatisticsRepository;
            this.pitchRepository = pitchRepository;
            this.unityOfWork = unityOfWork;
        }

        /***
         * Ainda não dá a regra das 12 horas para acietar, enviar e negociar (Ver se´já tem jogo marcado)
         */
        private async Task<string> validateHaveMatchWith12Hours(Guid idTeam, DateTime gameDate)
        {
            var findMatchWith12hours = await matchRepository.GetMatchProxim12HoursMatchs(idTeam, gameDate);

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

            var receiver = await teamRepository.GetTeamByIdWithPitchAsync(dto.IdReceiver);

            if (receiver == null)
            {
                throw new ArgumentNullException("A equipa que recebeu o convite não foi encontrada");
            }

            var sender = await teamRepository.GetTeamByIdWithPitchAsync(dto.IdSender);

            if (sender == null)
            {
                throw new ArgumentNullException("A equipa que enviou o convite não foi encontrada");
            }

            MatchInvite? matchInviteFind = await matchInviteRepository.GetMatchInvite(dto);

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

            await matchInviteRepository.AddMatchInvite(matchInvite);

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
            await unityOfWork.SaveChangesAsync();

            return sendMatchInviteDto;
        }

        private async Task<List<TeamStatistics>> ListTeamsStatistics(Teams sender, Teams receiver)
        {
            var list = new List<TeamStatistics>();

            TeamStatistics sendTeam = new TeamStatistics(sender);
            TeamStatistics receiverTeam = new TeamStatistics(receiver);
            list.Add(sendTeam);
            list.Add(receiverTeam);

            await teamStatisticsRepository.AddTeamStatistics(sendTeam);
            await teamStatisticsRepository.AddTeamStatistics(receiverTeam);

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
            var receiver = await teamRepository.GetByIdWithReceivedInvitesAndCalendar(idTeam);
            
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

            var sender = await teamRepository.GetTeamByIdAsync(matchInvite.IdSender);
           
            if (sender == null)
            {
                throw new NullReferenceException("O emissor do convite está a null");
            }

            var validateSender = ValidateSender(sender, matchInvite);
            if (validateSender != "")
            {
                throw new ValidatorException(validateSender);
            }

            var pitch = await pitchRepository.GetPitchById(matchInvite.IdPitch);
            if (pitch == null)
            {
                throw new ArgumentNullException("O campo do convite de partida não pode ser nulo");
            }

            List<TeamStatistics> teamStatistics = await ListTeamsStatistics(sender, receiver);

            var match = new Matches(matchInvite.GameDate, false, pitch, teamStatistics, matchInvite.Chat);

            await matchInviteRepository.DeleteMatchInvite(matchInvite);
            await matchRepository.AddMatch(match);

            var matchDTO = new MatchDto
            {
                IdMatch = match.Id,
                GameDate = match.MatchDate,
                NameTeam = receiver.Name,
                NameOpponent = sender.Name,
                NamePitch = pitch.Name
            };

            await unityOfWork.SaveChangesAsync();

            //var acceptMatch
            return matchDTO;
        }

        public async Task RefuseMatchInvites(Guid idTeam, Guid idMatchInvite)
        {
            var receiver = await teamRepository.GetByIdWithReceivedInvites(idTeam);

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

            var sender = await teamRepository.GetTeamByIdAsync(matchInvite.IdSender);

            var validateSender = ValidateSender(sender, matchInvite);
            if(validateSender != "")
            {
                throw new ValidatorException(validateSender);
            }

            await matchInviteRepository.DeleteMatchInvite(matchInvite);
 
            await unityOfWork.SaveChangesAsync();
        }

        //Ver erro da data 12h e error 500 que deixa executar o codigo e depois é lançado
        public async Task<InfoMatchInviteDTO> NegociateMatchInvite(SendMatchInviteDTO dto)
        {
            if ((dto.GameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }

            var pitch = await pitchRepository.GetPitchByName(dto.namePitch);
            
            if (pitch == null)
            {
                throw new NullReferenceException("O campo não pode estar a nulo");
            }
            
            var matchInvite = await matchInviteRepository.GetMatchInviteByTeams(dto.IdSender, dto.IdReceiver);

            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite de partida a negociar não existe", nameof(dto));
            }

            bool hasChanged = matchInvite.NegociateMatchInvite(dto.GameDate, pitch);

            if (!hasChanged)
            {
                throw new BusinessRuleException("Não é possível lançar uma contra-oferta uma vez que os dados estão iguais");
            }

            var senderTeam = await teamRepository.GetTeamByIdAsync(dto.IdSender);

            if (senderTeam == null)
            {
                throw new NullReferenceException("A equipa que enviou o convite não foi encontrada");
            }

            var receiverTeam = await teamRepository.GetTeamByIdAsync(dto.IdReceiver);

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

            await unityOfWork.SaveChangesAsync();

            return sendMatchInviteDto;
        }

        public async Task<List<InfoMatchInviteDTO>> GetAllMatchInvitesTeam(Guid idTeam)
        {
            var team = await teamRepository.GetTeamByIdAsync(idTeam);

            if (team == null)
            {
                throw new NullReferenceException("A team a consultar a lista de pedidos não existe");
            }

            List<InfoMatchInviteDTO> listMatchInvites = await matchInviteRepository.GetAllMatchInviteReceiverById(idTeam);

            if (listMatchInvites == null || !listMatchInvites.Any())
            {
                throw new NullReferenceException("A equipa ainda não recebeu pedidos de partida");
            }

            return listMatchInvites;
        }
    }
}

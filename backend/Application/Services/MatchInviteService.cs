using Application.DTOs;
using Application.Interfaces.Repositorys;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class MatchInviteService
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

        public async Task SendMatchInvite(SendMatchInviteDTO dto)
        {
            if ((dto.GameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }

            var sender = await teamRepository.GetTeamById(dto.IdSender);
            
            if (sender == null)
            {
                throw new ArgumentNullException("A equipa que enviou o convite não foi encontrada");
            }

            var receiver = await teamRepository.GetTeamById(dto.IdReceiver);

            if (receiver == null)
            {
                throw new ArgumentNullException("A equipa que recebeu o convite não foi encontrada");
            }

            if (sender.IdPitch != dto.IdPitch && receiver.IdPitch != dto.IdPitch)
            {
                throw new BusinessRuleException("O campo da partida não pertence a nenhuma das equipas");
            }

            //Determinar de quem é o campo
            var pitch = sender.Pitch;

            if (dto.IdPitch == receiver.IdPitch)
            {
                pitch = receiver.Pitch;
            } 

            var matchInvite = new MatchInvite(sender, receiver, dto.GameDate, pitch);

            await matchInviteRepository.AddMatchInvite(matchInvite);
            
            receiver.AddReceiveMatchInvite(matchInvite);
            sender.AddSendMatchInvite(matchInvite);

            await unityOfWork.SaveChangesAsync();
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

        public async Task<Matches> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite)
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

            var sender = matchInvite.Sender;
            if (sender.SentInvites.FirstOrDefault(matchInvite) == null)
            {
                throw new ArgumentNullException("O convite a recusar não existe na equipa oponetne");
            }

            Pitch pitch = matchInvite.Pitch;
            if (pitch == null)
            {
                throw new ArgumentNullException("O campo do convite de partida não pode ser nulo");
            }

            List<TeamStatistics> teamStatistics = await ListTeamsStatistics(sender, receiver);

            var match = new Matches(matchInvite.GameDate, false, pitch, teamStatistics, matchInvite.Chat);

            await matchInviteRepository.DeleteMatchInvite(matchInvite);
            await matchRepository.AddMatch(match);

            sender.removeSendMatchInvite(matchInvite);
            receiver.removeReceiverMatchInvite(matchInvite);

            sender.Calendar.AddMatch(match);
            receiver.Calendar.AddMatch(match);

            await unityOfWork.SaveChangesAsync();

            return match;
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

            Teams sender = matchInvite.Sender;
            if (sender.SentInvites.FirstOrDefault(matchInvite) == null)
            {
                throw new MatchInviteException("O convite a recusar não existe na equipa oponetne");
            }

            await matchInviteRepository.DeleteMatchInvite(matchInvite);
            sender.removeSendMatchInvite(matchInvite);
            receiver.removeReceiverMatchInvite(matchInvite);

            await unityOfWork.SaveChangesAsync();
        }

        //Validar a data (Falta) e provavelmente mais coisas
        public async Task<MatchInvite> NegociateMatchInvite(SendMatchInviteDTO dto)
        {
            if ((dto.GameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }

            var pitch = await pitchRepository.GetPitchById(dto.IdPitch);
            
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

            var senderTeam = await teamRepository.GetTeamById(dto.IdSender);

            if (senderTeam == null)
            {
                throw new NullReferenceException("A equipa que enviou o convite não foi encontrada");
            }

            var receiverTeam = await teamRepository.GetTeamById(dto.IdReceiver);

            if (receiverTeam == null)
            {
                throw new NullReferenceException("A equipa que recebeu o convite não foi encontrada");
            }

            //Quem enviou o convite passa a ser o recetor
            senderTeam.removeSendMatchInvite(matchInvite);
            senderTeam.AddReceiveMatchInvite(matchInvite);

            //Quem recebou o convite e fez a contra-oferta passa a ser o emissor
            receiverTeam.removeReceiverMatchInvite(matchInvite);
            receiverTeam.AddSendMatchInvite(matchInvite);

            await unityOfWork.SaveChangesAsync();

            return matchInvite;
        }
    }
}

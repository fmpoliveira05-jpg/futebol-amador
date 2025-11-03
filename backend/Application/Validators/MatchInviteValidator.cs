using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators
{
    public class MatchInviteValidator: IMatchInviteValidator
    {
        public void ValidateSenderMatchInvite(SendMatchInviteDto dto, Guid idSender)
        {
            if (dto.IdSender == Guid.Empty)
            {
                throw new ArgumentException("O id que quem enviou o convite é invalido");
            }

            if (dto.IdSender != idSender)
            {
                throw new ArgumentException("O id de quem enviou não é o mesmo do endPoint");
            }

            if (dto.IdReceiver == Guid.Empty)
            {
                throw new ArgumentException("O id do recetor está vazio");
            }

            ValidateHoursGame(dto.GameDate);
        }

        public void ValidateAcceptRefuseMatchInvite(Guid idReceiver, Guid idMatchInvite)
        {
            if (idReceiver == Guid.Empty)
            {
                throw new ArgumentException("O id que quem recebeu o convite é invalido");
            }

            if (idMatchInvite == Guid.Empty)
            {
                throw new ArgumentException("O id da match está invalido");
            }
        }

        public void ValidateTeamCalendar(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }
        }

        //Talvez falte validar se pelo menos o sender já tem um convite igual
        public void ValidateSendMatchInvite(Team receiver, Team sender, MatchInvite matchInviteFind,
            Matches findMatchWith12hours, string namePitch)
        {
            const string msgNullErrorReceiver = "A equipa que receberá o convite não foi encontrada";
            ValidateNullTeam(receiver, msgNullErrorReceiver);

            const string msgNullErrorSender = "A equipa que enviou o convite não foi encontrada";
            ValidateNullTeam(sender, msgNullErrorSender);

            if (matchInviteFind != null)
            {
                throw new BusinessRuleException("A match invite já existe");
            }

            const string msgError = "Não pode marcar esse jogo a essa hora, porque já tem um a pelo menos 12 horas da data especificada";
            ValidateTwentyHoursMatch(findMatchWith12hours, msgError);

            if (sender.Pitch.Name != namePitch && receiver.Pitch.Name != namePitch)
            {
                throw new BusinessRuleException("O campo da partida não pertence a nenhuma das equipas");
            }
        }

        public void ValidateMatchInvite(MatchInvite matchInvite)
        {
            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite a aceitar não existe");
            }
        }

        public void ValidateAcceptMatchInvite(Team sender, Matches twentyhoursMatch, MatchInvite matchInvite, Pitch pitch)
        {
            const string msgError = "O jogo não pode ser aceite a essa hora, por causa que já tem um jogo a pelo menos 12 horas desse";
            ValidateTwentyHoursMatch(twentyhoursMatch, msgError);

            //Não devia de ser o reciever???????
            const string errorMsg = "Não dá para aceitar o convite porque ele não foi enviado, pelo emissor";
            const string msgNullSender = "O recetor do convite não foi encontrado ou não existe";
            ValidateSender(sender, matchInvite, msgNullSender, errorMsg);

            const string msgNullPitch = "O campo do convite de partida não pode ser nulo";
            ValidateNullPitch(pitch, msgNullPitch);
        }

        public void ValidateReciever(Team receiver)
        {
            const string messageNull = "O recetor do convite não foi encontrado ou não exsite";
            ValidateNullTeam(receiver, messageNull);

            var numReceivedInvites = receiver.ReceivedInvites.Count();
            if (numReceivedInvites == 0)
            {
                throw new EmptyCollectionException("O recetor não possui nenhum convite de partida");
            }
        }

        //Crias constante
        public void ValidateRefuseMatchInvite(Team sender, MatchInvite matchInvite)
        {
            const string errorMsg = "Não dá para rejeitar o convite porque ele não existe";
            const string errorMsgNullSender = "O emissor do convite a rejeitar não foi encontrado ou não existe";
            ValidateSender(sender, matchInvite, errorMsgNullSender, errorMsg);
        }

        public void ValidateNegociateMatchInvite(string namePitch, Pitch pitch, MatchInvite matchInvite, Team senderTeam, 
            Team receiverTeam, Matches findMatchWith12hour)
        {
            const string msgNullPitch = "O campo da partida não pode estar a nulo";
            ValidateNullPitch(pitch, msgNullPitch);

            if (pitch.Name != namePitch)
            {
                throw new InvalidOperationException("O nome do campo não bate com o da partida");
            }

            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite de partida a negociar não existe!");
            }

            const string msgError = "Não foi possível negociar o convite para essa data, pois já tem um jogo marcado com uma diferença horaria de 12 horas para a data que inseriou";
            ValidateTwentyHoursMatch(findMatchWith12hour, msgError);
        }

        public void ValidateHasChangeNegociateMatchInvite(bool hasChanged)
        {
            if (!hasChanged)
            {
                throw new BusinessRuleException("Não é possível lançar uma contra-oferta uma vez que os dados estão iguais");
            }
        }

        public void ValidateFilterMatchInvite(Guid idTeam, FilterMatchInvitesDto filter)
        {
            var dateMin = filter.MinDate;
            var dateMax = filter.MaxDate;

            ValidateTeamCalendar(idTeam);

            if (dateMin.HasValue && dateMax.HasValue)
            {
                if (dateMin.Value > dateMax.Value)
                {
                    throw new InvalidOperationException("A data minima tem de ser inferior à data maxima");
                }
            }
        }

        private static void ValidateHoursGame(DateTime gameDate)
        {
            if ((gameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }
        }

        private static void ValidateTwentyHoursMatch(Matches twentyhoursMatch, string msgError)
        {
            if (twentyhoursMatch != null)
            {
                throw new BusinessRuleException(msgError);
            }
        }

        private static void ValidateSender(Team sender, MatchInvite matchInvite, string messageNull, string messageError)
        {
            ValidateNullTeam(sender, messageNull);
            if (sender.SentInvites.FirstOrDefault(matchInvite) == null)
            {
                throw new ValidatorException(messageError);
            }
        }

        //Meter para receber mensagem de error (Talvez apagar par aque cada null tenha uma mensagem diferentes ou adaptar o de cima e este para receber mensagem)
        private static void ValidateNullTeam(Team team, string msgError)
        {
            if (team == null)
            {
                throw new ArgumentNullException(msgError);
            }
        }

        private static void ValidateNullPitch(Pitch pitch, string msgError)
        {
            if (pitch == null)
            {
                throw new ArgumentNullException(msgError);
            }
        }
    }
}

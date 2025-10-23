using Application.DTOs.MatchInvites;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators
{
    public class MatchInviteValidator: IMatchInviteValidator
    {
        public void ValidateHoursGame(DateTime gameDate)
        {
            if ((gameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }
        }

        //Talvez falte validar se pelo menos o sender já tem um convite igual
        public void ValidateSendMatchInvite(Teams receiver, Teams sender, MatchInvite matchInviteFind,
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

        public void ValidateAcceptMatchInvite(Teams sender, Matches twentyhoursMatch, MatchInvite matchInvite, Pitch pitch)
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

        public void ValidateReciever(Teams receiver)
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
        public void ValidateRefuseMatchInvite(Teams sender, MatchInvite matchInvite)
        {
            const string errorMsg = "Não dá para rejeitar o convite porque ele não existe";
            const string errorMsgNullSender = "O emissor do convite a rejeitar não foi encontrado ou não existe";
            ValidateSender(sender, matchInvite, errorMsgNullSender, errorMsg);
        }

        public void ValidateNegociateMatchInvite(Pitch pitch, MatchInvite matchInvite, Teams senderTeam, 
            Teams receiverTeam, Matches findMatchWith12hour)
        {
            const string msgNullPitch = "O campo da partida não pode estar a nulo";
            ValidateNullPitch(pitch, msgNullPitch);

            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite de partida a negociar não existe!");
            }

            //Posso depois torcar para a validação abaixo
            const string msgNullSender = "A equipa que enviou o convite não foi encontrada ou não existe";
            ValidateNullTeam(senderTeam, msgNullSender);

            //string msg = "O emissor do convite não possui o mesmo"
            //ValidateSender(senderTeam, matchInvite, msg);

            const string msgNullReceiver = "A equipa que recebeu o convite não foi encontrada";
            ValidateNullTeam(receiverTeam, msgNullReceiver);

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

        public void ValidateGetAll(Teams team, List<InfoMatchInviteDTO?> listMatchInvites)
        {
            const string msgError = "A team a consultar a lista de pedidos não existe";
            ValidateNullTeam(team, msgError);

            if (listMatchInvites == null || !listMatchInvites.Any())
            {
                throw new NullReferenceException("A equipa ainda não recebeu pedidos de partida");
            }
        }

        private void ValidateTwentyHoursMatch(Matches twentyhoursMatch, string msgError)
        {
            if (twentyhoursMatch != null)
            {
                throw new BusinessRuleException(msgError);
            }
        }

        private void ValidateSender(Teams sender, MatchInvite matchInvite, string messageNull, string messageError)
        {
            ValidateNullTeam(sender, messageNull);
            if (sender.SentInvites.FirstOrDefault(matchInvite) == null)
            {
                throw new ValidatorException(messageError);
            }
        }

        //Meter para receber mensagem de error (Talvez apagar par aque cada null tenha uma mensagem diferentes ou adaptar o de cima e este para receber mensagem)
        private void ValidateNullTeam(Teams team, string msgError)
        {
            if (team == null)
            {
                throw new ArgumentNullException(msgError);
            }
        }

        private void ValidateNullPitch(Pitch pitch, string msgError)
        {
            if (pitch == null)
            {
                throw new ArgumentNullException(msgError);
            }
        }
    }
}

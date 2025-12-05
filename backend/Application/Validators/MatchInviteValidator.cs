using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Application.Interfaces.Validators;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Validators
{
    /// <summary>
    /// Validador de Regras de Negócio e de Entidade para o ciclo de vida dos convites de partida ([MatchInvite]).
    /// 
    /// Esta classe verifica se as entidades existem, se as datas são válidas e se as operações
    /// respeitam as regras de concorrência e agendamento da plataforma.
    /// </summary>
    public class MatchInviteValidator : IMatchInviteValidator
    {
        /// <summary>
        /// Valida os IDs essenciais no DTO de envio de convite.
        /// </summary>
        /// <remarks>
        /// Regras verificadas:
        /// <list type="bullet">
        ///     <item>Os IDs do Remetente e Recetor não podem ser Guid.Empty.</item>
        ///     <item>O ID do Remetente do DTO deve corresponder ao ID do Remetente real que chama o endpoint.</item>
        ///     <item>O Remetente e o Recetor não podem ser a mesma equipa.</item>
        ///     <item>A data do jogo deve ser pelo menos 12 horas após a hora atual.</item>
        /// </list>
        /// </remarks>
        /// <param name="dto">O DTO de envio do convite.</param>
        /// <param name="idSender">O ID do remetente (obtido do contexto do utilizador).</param>
        /// <exception cref="ArgumentException">Se os IDs forem inválidos, inconsistentes, ou Remetente == Recetor.</exception>
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

            if(dto.IdSender == dto.IdReceiver)
            {
                throw new ArgumentException("O seu adversário não pode ser voce");
            }

            ValidateHoursGame(dto.GameDate);
        }

        /// <summary>
        /// Valida os IDs de entrada para as operações de Aceitar ou Rejeitar um convite.
        /// </summary>
        /// <param name="idReceiver">O ID da equipa que está a aceitar/rejeitar.</param>
        /// <param name="idMatchInvite">O ID do convite.</param>
        /// <exception cref="ArgumentException">Se os IDs forem Guid.Empty.</exception>
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

        /// <summary>
        /// Valida se o ID da equipa é válido antes de consultar o calendário.
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        /// <exception cref="ArgumentException">Se o ID da equipa for Guid.Empty.</exception>
        public void ValidateTeamCalendar(Guid idTeam)
        {
            if (idTeam == Guid.Empty)
            {
                throw new ArgumentException("O id da equipa está vazio");
            }
        }

        /// <summary>
        /// Valida as entidades principais e as regras de agendamento antes de enviar um novo convite.
        /// </summary>
        /// <param name="receiver">A equipa recetora (deve existir).</param>
        /// <param name="sender">A equipa remetente (deve existir).</param>
        /// <param name="matchInviteFind">Resultado da procura de um convite duplicado (deve ser nulo).</param>
        /// <param name="findMatchWith12hours">Resultado da procura de um conflito de agendamento nas 12h.</param>
        /// <exception cref="ArgumentNullException">Se as equipas forem nulas.</exception>
        /// <exception cref="BusinessRuleException">Se o convite já existir ou houver conflito de horário.</exception>
        /// <exception cref="InvalidOperationException">Se o remetente for igual ao recetor.</exception>
        public void ValidateSendMatchInvite(Team receiver, Team sender, MatchInvite matchInviteFind,
            Matches findMatchWith12hours)
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

            if(receiver.Id == sender.Id)
            {
                throw new InvalidOperationException("Não pode mandar um convite de partida a si mesmo");
            }
        }

        /// <summary>
        /// Valida se a entidade [MatchInvite] existe.
        /// </summary>
        /// <param name="matchInvite">A entidade [MatchInvite].</param>
        /// <exception cref="ArgumentNullException">Lançada se o convite for nulo.</exception>
        public void ValidateMatchInvite(MatchInvite matchInvite)
        {
            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite a aceitar não existe");
            }
        }

        /// <summary>
        /// Valida as condições para aceitar um convite de partida.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Sem conflito de horário e Pitch e Sender válidos.
        /// </remarks>
        /// <param name="sender">A equipa remetente.</param>
        /// <param name="twentyhoursMatch">Resultado da procura de conflito de horário nas 12h.</param>
        /// <param name="matchInvite">O convite.</param>
        /// <param name="pitch">O campo do jogo.</param>
        /// <exception cref="BusinessRuleException">Se houver conflito de horário.</exception>
        /// <exception cref="ValidatorException">Se o remetente for inválido.</exception>
        /// <exception cref="ArgumentNullException">Se o Pitch for nulo.</exception>
        public void ValidateAcceptMatchInvite(Team sender, Matches twentyhoursMatch, MatchInvite matchInvite, Pitch pitch)
        {
            const string msgError = "O jogo não pode ser aceite a essa hora, por causa que já tem um jogo a pelo menos 12 horas desse";
            ValidateTwentyHoursMatch(twentyhoursMatch, msgError);

            const string errorMsg = "Não dá para aceitar o convite porque ele não foi enviado, pelo emissor";
            const string msgNullSender = "O recetor do convite não foi encontrado ou não existe";
            ValidateSender(sender, matchInvite, msgNullSender, errorMsg);

            const string msgNullPitch = "O campo do convite de partida não pode ser nulo";
            ValidateNullPitch(pitch, msgNullPitch);
        }

        /// <summary>
        /// Valida se a equipa recetora possui convites pendentes.
        /// </summary>
        /// <param name="receiver">A equipa recetora (carregada com convites).</param>
        /// <exception cref="EmptyCollectionException">Lançada se a coleção de convites estiver vazia.</exception>
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

        /// <summary>
        /// Valida as condições para rejeitar um convite de partida.
        /// </summary>
        /// <param name="sender">A equipa remetente.</param>
        /// <param name="matchInvite">O convite a rejeitar.</param>
        /// <exception cref="ValidatorException">Se o convite não existir.</exception>
        /// <exception cref="ArgumentNullException">Se o remetente for nulo.</exception>
        public void ValidateRefuseMatchInvite(Team sender, MatchInvite matchInvite)
        {
            const string errorMsg = "Não dá para rejeitar o convite porque ele não existe";
            const string errorMsgNullSender = "O emissor do convite a rejeitar não foi encontrado ou não existe";
            ValidateSender(sender, matchInvite, errorMsgNullSender, errorMsg);
        }

        /// <summary>
        /// Valida as condições para enviar uma contra-proposta (Negociação).
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Não pode haver conflito de horário e o remetente da negociação não pode ser o mesmo que enviou o convite original (deve ser o recetor).
        /// </remarks>
        /// <param name="pitch">O campo do jogo.</param>
        /// <param name="matchInvite">O convite original.</param>
        /// <param name="senderTeam">A equipa que envia a negociação (deve ser o recetor original).</param>
        /// <param name="receiverTeam">A equipa recetora.</param>
        /// <param name="findMatchWith12hour">Resultado da procura de conflito de horário nas 12h.</param>
        /// <exception cref="ArgumentNullException">Se o Pitch ou o Convite for nulo.</exception>
        /// <exception cref="BusinessRuleException">Se houver conflito de horário.</exception>
        public void ValidateNegociateMatchInvite(Pitch pitch, MatchInvite matchInvite, Team senderTeam,
            Team receiverTeam, Matches findMatchWith12hour)
        {
            const string msgNullPitch = "O campo da partida não pode estar a nulo";
            ValidateNullPitch(pitch, msgNullPitch);
            if (matchInvite == null)
            {
                throw new ArgumentNullException("O convite de partida a negociar não existe!");
            }
            if (matchInvite.IdSender.Equals(senderTeam.Id)) {
                throw new ArgumentNullException("Deve aguardar a resposta da outra equipa antes de negociar novamente!");
            }


            const string msgError = "Não foi possível negociar o convite para essa data, pois já tem um jogo marcado com uma diferença horaria de 12 horas para a data que inseriou";
            ValidateTwentyHoursMatch(findMatchWith12hour, msgError);
        }

        /// <summary>
        /// Valida se houve alguma alteração nos dados do convite para lançar uma contra-proposta.
        /// </summary>
        /// <param name="hasChanged">Booleano que indica se os dados mudaram.</param>
        /// <exception cref="BusinessRuleException">Lançada se os dados estiverem iguais (não houve negociação).</exception>
        public void ValidateHasChangeNegociateMatchInvite(bool hasChanged)
        {
            if (!hasChanged)
            {
                throw new BusinessRuleException("Não é possível lançar uma contra-oferta uma vez que os dados estão iguais");
            }
        }

        /// <summary>
        /// Valida os filtros de data de um convite de partida.
        /// </summary>
        /// <param name="idTeam">O ID da equipa (para validação de contexto).</param>
        /// <param name="filter">Os filtros.</param>
        /// <exception cref="InvalidOperationException">Se a data mínima for posterior à máxima.</exception>
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

        #region private Methods

        /// <summary>
        /// Valida se a data do jogo é pelo menos 12 horas após a hora atual.
        /// </summary>
        /// <param name="gameDate">A data do jogo.</param>
        /// <exception cref="BusinessRuleException">Se o horário for muito próximo.</exception>
        private static void ValidateHoursGame(DateTime gameDate)
        {
            if ((gameDate - DateTime.UtcNow).TotalHours < 12)
            {
                throw new BusinessRuleException("O horario da partida deve ser pelo menos 12 horas apos a hora atual");
            }
        }

        /// <summary>
        /// Verifica se há um jogo agendado num intervalo de 12 horas do jogo proposto.
        /// </summary>
        /// <param name="twentyhoursMatch">A partida de conflito (se existir).</param>
        /// <param name="msgError">Mensagem de erro em caso de conflito.</param>
        /// <exception cref="BusinessRuleException">Lançada se for encontrado um jogo no intervalo de 12 horas.</exception>
        private static void ValidateTwentyHoursMatch(Matches twentyhoursMatch, string msgError)
        {
            if (twentyhoursMatch != null)
            {
                throw new BusinessRuleException(msgError);
            }
        }

        /// <summary>
        /// Valida se a equipa remetente é válida e se o convite pertence à coleção de convites enviados.
        /// </summary>
        /// <param name="sender">A equipa remetente.</param>
        /// <param name="matchInvite">O convite alvo.</param>
        /// <param name="messageNull">Mensagem de erro se o remetente for nulo.</param>
        /// <param name="messageError">Mensagem de erro se o convite não pertencer à coleção.</param>
        /// <exception cref="ArgumentNullException">Se o remetente for nulo.</exception>
        /// <exception cref="ValidatorException">Se o convite não for encontrado na coleção do remetente.</exception>
        private static void ValidateSender(Team sender, MatchInvite matchInvite, string messageNull, string messageError)
        {
            ValidateNullTeam(sender, messageNull);
            if (sender.SentInvites.FirstOrDefault(matchInvite) == null)
            {
                throw new ValidatorException(messageError);
            }
        }

        /// <summary>
        /// Valida se a entidade de equipa não é nula.
        /// </summary>
        /// <param name="team">A equipa.</param>
        /// <param name="msgError">Mensagem de erro em caso de nulo.</param>
        /// <exception cref="ArgumentNullException">Se a equipa for nula.</exception>
        private static void ValidateNullTeam(Team team, string msgError)
        {
            if (team == null)
            {
                throw new ArgumentNullException(msgError);
            }
        }

        /// <summary>
        /// Valida se a entidade do campo de jogo não é nula.
        /// </summary>
        /// <param name="pitch">O campo de jogo.</param>
        /// <param name="msgError">Mensagem de erro em caso de nulo.</param>
        /// <exception cref="ArgumentNullException">Se o campo de jogo for nulo.</exception>
        private static void ValidateNullPitch(Pitch pitch, string msgError)
        {
            if (pitch == null)
            {
                throw new ArgumentNullException(msgError);
            }
        }
        #endregion
    }
}
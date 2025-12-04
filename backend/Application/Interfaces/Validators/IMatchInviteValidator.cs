using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Domain.Entities;

namespace Application.Interfaces.Validators
{
    /// <summary>
    /// Contrato de Validador de Regras de Negócio e Controlo de Integridade para o ciclo de vida dos Convites de Partida ([MatchInvite]).
    /// 
    /// Esta interface define as regras de validação síncrona que garantem a validade da operação (criação, aceitação, rejeição, negociação)
    /// e a conformidade com as regras de agendamento (ex: conflito de horário, duplicação).
    /// </summary>
    public interface IMatchInviteValidator
    {
        /// <summary>
        /// Valida os IDs essenciais no DTO de envio de convite, verificando a consistência entre o remetente (DTO) e o contexto do utilizador.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: IDs válidos (não Guid.Empty) e Remetente != Recetor.
        /// </remarks>
        /// <param name="dto">O DTO de envio do convite.</param>
        /// <param name="idSender">O ID do remetente (obtido do contexto do utilizador autenticado).</param>
        public void ValidateSenderMatchInvite(SendMatchInviteDto dto, Guid idSender);

        /// <summary>
        /// Valida os IDs de entrada para as operações de Aceitar ou Rejeitar um convite.
        /// </summary>
        /// <param name="idReceiver">O ID da equipa que está a aceitar/rejeitar.</param>
        /// <param name="idMatchInvite">O ID do convite alvo.</param>
        public void ValidateAcceptRefuseMatchInvite(Guid idReceiver, Guid idMatchInvite);

        /// <summary>
        /// Valida se o ID da equipa é válido antes de consultar o seu calendário.
        /// </summary>
        /// <param name="idTeam">O ID da equipa.</param>
        public void ValidateTeamCalendar(Guid idTeam);

        /// <summary>
        /// Valida as entidades e as regras de agendamento antes de enviar um novo convite de partida.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Existência das equipas, ausência de convite duplicado e ausência de conflitos de horário em 12 horas.
        /// </remarks>
        /// <param name="receiver">A equipa recetora (deve existir).</param>
        /// <param name="sender">A equipa remetente (deve existir).</param>
        /// <param name="matchInviteFind">Resultado da procura de um convite duplicado (deve ser nulo).</param>
        /// <param name="findMatchWith12hours">Resultado da procura de um conflito de agendamento (deve ser nulo).</param>
        void ValidateSendMatchInvite(Team receiver, Team sender, MatchInvite matchInviteFind, Matches findMatchWith12hours);

        /// <summary>
        /// Valida se a entidade [MatchInvite] existe (não é nula).
        /// </summary>
        /// <param name="matchInvite">A entidade [MatchInvite] a ser verificada.</param>
        void ValidateMatchInvite(MatchInvite matchInvite);

        /// <summary>
        /// Valida as regras de negócio para aceitar um convite de partida.
        /// </summary>
        /// <remarks>
        /// Regras verificadas: Validade do campo e ausência de conflito de horário no agendamento.
        /// </remarks>
        /// <param name="sender">A equipa remetente.</param>
        /// <param name="twentyhoursMatch">Resultado da procura de conflito de horário nas 12h (deve ser nulo).</param>
        /// <param name="matchInvite">O convite.</param>
        /// <param name="pitch">O campo do jogo.</param>
        void ValidateAcceptMatchInvite(Team sender, Matches twentyhoursMatch, MatchInvite matchInvite, Pitch pitch);

        /// <summary>
        /// Valida se a equipa recetora possui convites pendentes e é válida.
        /// </summary>
        /// <param name="receiver">A equipa recetora (carregada com convites).</param>
        void ValidateReciever(Team receiver);

        /// <summary>
        /// Valida as condições para rejeitar um convite de partida (existência do convite e do remetente).
        /// </summary>
        /// <param name="sender">A equipa remetente.</param>
        /// <param name="matchInvite">O convite a rejeitar.</param>
        void ValidateRefuseMatchInvite(Team sender, MatchInvite matchInvite);

        /// <summary>
        /// Valida as condições e regras de negócio para enviar uma contra-proposta (Negociação).
        /// </summary>
        /// <remarks>
        /// Regras verificadas: O negociador deve ser o recetor original; não pode haver conflito de horário com a nova data.
        /// </remarks>
        /// <param name="pitch">O campo do jogo.</param>
        /// <param name="matchInvite">O convite original.</param>
        /// <param name="senderTeam">A equipa que envia a negociação (deve ser o recetor original).</param>
        /// <param name="receiverTeam">A equipa recetora (o remetente original).</param>
        /// <param name="findMatchWith12hour">Resultado da procura de conflito de horário nas 12h (deve ser nulo).</param>
        void ValidateNegociateMatchInvite(Pitch pitch, MatchInvite matchInvite, Team senderTeam, Team receiverTeam, Matches findMatchWith12hour);

        /// <summary>
        /// Valida se os dados do convite foram efetivamente alterados durante o processo de negociação.
        /// </summary>
        /// <param name="hasChanged">Booleano que indica se os dados mudaram.</param>
        void ValidateHasChangeNegociateMatchInvite(bool hasChanged);

        /// <summary>
        /// Valida a consistência e o intervalo dos filtros de data de um convite de partida.
        /// </summary>
        /// <remarks>
        /// Regra verificada: Data Mínima não pode ser posterior à Data Máxima.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa (contexto).</param>
        /// <param name="filter">O DTO com os filtros de data.</param>
        void ValidateFilterMatchInvite(Guid idTeam, FilterMatchInvitesDto filter);
    }
}
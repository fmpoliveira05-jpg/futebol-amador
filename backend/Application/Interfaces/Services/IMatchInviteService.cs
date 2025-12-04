using Application.DTOs.Filters;
using Application.DTOs.Match;
using Application.DTOs.MatchInvites;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de Serviço de Domínio para a gestão do ciclo de vida dos Convites de Partida (Match Invites).
    /// 
    /// Esta interface abstrai a lógica de negócio para a criação de desafios, aceitação/rejeição e negociação de termos,
    /// coordenando as operações entre os repositórios e serviços de notificação.
    /// </summary>
    public interface IMatchInviteService
    {
        /// <summary>
        /// Envia um novo convite de partida de uma equipa para outra.
        /// </summary>
        /// <remarks>
        /// O serviço deve validar a unicidade do pedido, verificar conflitos de horário e notificar a equipa recetora.
        /// </remarks>
        /// <param name="idSender">O ID (GUID) da equipa que envia o desafio (Remetente).</param>
        /// <param name="dto">DTO com os termos do jogo (Data, Pitch, Recetor).</param>
        /// <returns>Uma tarefa assíncrona que retorna o DTO [InfoMatchInviteDto] do convite criado.</returns>
        public Task<InfoMatchInviteDto> SendMatchInvite(Guid idSender, SendMatchInviteDto dto);

        /// <summary>
        /// Aceita um convite de partida recebido.
        /// </summary>
        /// <remarks>
        /// **Transação Crítica:** Se aceite, o convite é removido, e a partida real ([Matches]) é criada e agendada no calendário.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que aceita o convite (Recetora).</param>
        /// <param name="idMatchInvite">O ID (GUID) do convite a ser aceito.</param>
        /// <returns>Uma tarefa assíncrona que retorna o DTO [MatchDto] da nova partida agendada.</returns>
        public Task<MatchDto> AcceptMatchInvite(Guid idTeam, Guid idMatchInvite);

        /// <summary>
        /// Rejeita um convite de partida recebido.
        /// </summary>
        /// <remarks>
        /// **Transação:** O convite é removido da base de dados sem criar uma partida.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa que rejeita o convite.</param>
        /// <param name="idMatchInvite">O ID (GUID) do convite a ser rejeitado.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>).</returns>
        public Task RefuseMatchInvites(Guid idTeam, Guid idMatchInvite);

        /// <summary>
        /// Envia uma contra-proposta para um convite existente (Negociação).
        /// </summary>
        /// <remarks>
        /// O serviço deve atualizar os termos do convite (data/local) e inverter os papéis de Remetente/Recetor.
        /// </remarks>
        /// <param name="idSender">O ID da equipa que está a enviar a contra-proposta (Recetor Original).</param>
        /// <param name="dto">DTO com os novos termos propostos.</param>
        /// <returns>Uma tarefa assíncrona que retorna o DTO [InfoMatchInviteDto] do convite atualizado.</returns>
        public Task<InfoMatchInviteDto> NegociateMatchInvite(Guid idSender, SendMatchInviteDto dto);

        /// <summary>
        /// Obtém a lista completa de convites de partida recebidos por uma equipa.
        /// </summary>
        /// <param name="idTeam">O ID da equipa recetora.</param>
        /// <returns>Uma lista de [InfoMatchInviteDto] com informações resumidas dos convites.</returns>
        public Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeam(Guid idTeam);

        /// <summary>
        /// Obtém a lista de convites de partida recebidos, aplicando filtros de pesquisa.
        /// </summary>
        /// <param name="idTeam">O ID da equipa recetora.</param>
        /// <param name="filter">O DTO contendo os critérios de filtragem (Nome do Remetente, Intervalo de Datas).</param>
        /// <returns>Uma lista filtrada de [InfoMatchInviteDto].</returns>
        public Task<List<InfoMatchInviteDto>> GetAllMatchInvitesTeamWithFilters(Guid idTeam, FilterMatchInvitesDto filter);
    }
}
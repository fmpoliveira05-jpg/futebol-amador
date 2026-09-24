using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato de Repositório para operações de acesso a dados da entidade [MatchInvite] (Convite de Partida).
    /// 
    /// Define os métodos de persistência e consulta necessários para gerir o ciclo de vida dos desafios entre equipas.
    /// </summary>
    public interface IMatchInviteRepository
    {
        /// <summary>
        /// Adiciona um novo registo de convite de partida à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="matchInvite">A entidade [MatchInvite] a ser persistida.</param>
        public Task AddMatchInvite(MatchInvite matchInvite);

        /// <summary>
        /// Marca um convite de partida existente para ser removido da base de dados.
        /// </summary>
        /// <param name="matchInvite">A entidade [MatchInvite] a ser removida (ex: após aceitação ou rejeição).</param>
        public void DeleteMatchInvite(MatchInvite matchInvite);

        /// <summary>
        /// Obtém um convite de partida pelo seu identificador único (ID).
        /// </summary>
        /// <param name="id">O ID (GUID) do convite.</param>
        /// <returns>A entidade [MatchInvite] ou null se não for encontrada.</returns>
        public Task<MatchInvite?> GetMatchInviteById(Guid id);

        /// <summary>
        /// Obtém um convite de partida específico com base no remetente, recetor e data do jogo.
        /// </summary>
        /// <remarks>
        /// Utilizado para verificar a existência de convites duplicados entre o mesmo par de equipas.
        /// </remarks>
        /// <param name="idSender">O ID da equipa remetente.</param>
        /// <param name="idReceiver">O ID da equipa recetora.</param>
        /// <param name="gameDate">A data e hora proposta para o jogo.</param>
        /// <returns>A entidade [MatchInvite] correspondente ou null.</returns>
        public Task<MatchInvite?> GetMatchInvite(Guid idSender, Guid idReceiver, DateTime gameDate);

        /// <summary>
        /// Obtém um convite de partida específico, carregando as entidades de navegação Pitch (Local) e as Equipas.
        /// </summary>
        /// <param name="idSender">O ID da equipa remetente.</param>
        /// <param name="idReceiver">O ID da equipa recetora.</param>
        /// <returns>A entidade [MatchInvite] com entidades de navegação carregadas, ou null.</returns>
        public Task<MatchInvite?> GetMatchInviteWithPitchByTeams(Guid idSender, Guid idReceiver);

        /// <summary>
        /// Obtém uma lista resumida de todos os convites de partida recebidos por uma equipa específica.
        /// </summary>
        /// <remarks>
        /// A consulta projeta o resultado diretamente no DTO [InfoMatchInviteDto].
        /// </remarks>
        /// <param name="idReceiver">O ID da equipa recetora dos convites.</param>
        /// <returns>Uma lista de [InfoMatchInviteDto] com informações resumidas dos convites.</returns>
        public Task<List<InfoMatchInviteDto?>> GetAllMatchInviteReceiverById(Guid idReceiver);

        /// <summary>
        /// Obtém uma lista filtrada de convites de partida recebidos, utilizando critérios avançados.
        /// </summary>
        /// <param name="idReceiver">O ID da equipa que está a receber os convites.</param>
        /// <param name="filter">O DTO contendo os critérios de filtragem (Nome do Remetente, Intervalo de Datas, IDs).</param>
        /// <returns>Uma lista de [InfoMatchInviteDto] que satisfaz os critérios de filtragem.</returns>
        public Task<List<InfoMatchInviteDto?>> GetAllMatchInvitesTeamWithFilters(Guid idReceiver, FilterMatchInvitesDto filter);
    }
}
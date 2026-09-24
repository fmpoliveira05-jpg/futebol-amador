using Application.DTOs.Filters;
using Application.DTOs.MatchInvites;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência de dados e consulta da entidade [MatchInvite].
    /// 
    /// Esta classe utiliza o [AmateurFootballContext] e métodos assíncronos do EF Core para gerir
    /// o ciclo de vida dos convites de partida.
    /// </summary>
    public class MatchInviteRepository : IMatchInviteRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder às tabelas.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [MatchInviteRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados injetado via Dependency Injection.</param>
        public MatchInviteRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona um novo convite de partida à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="matchInvite">A entidade [MatchInvite] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddMatchInvite(MatchInvite matchInvite)
        {
            await context.MatchInvite.AddAsync(matchInvite);
        }

        /// <summary>
        /// Marca um convite de partida existente para ser removido da base de dados.
        /// </summary>
        /// <param name="matchInvite">A entidade [MatchInvite] a ser removida.</param>
        public void DeleteMatchInvite(MatchInvite matchInvite)
        {
            context.MatchInvite.Remove(matchInvite);
        }

        /// <summary>
        /// Obtém um convite de partida pelo seu identificador único (ID).
        /// </summary>
        /// <param name="id">O ID (GUID) do convite.</param>
        /// <returns>A entidade [MatchInvite] ou null se não for encontrada.</returns>
        public async Task<MatchInvite?> GetMatchInviteById(Guid id)
        {
            return await context.MatchInvite
                .Include(mi => mi.Sender)
                .Include(mi => mi.Receiver)
                .Include(mi => mi.Pitch)
                .FirstOrDefaultAsync(mi => mi.Id == id);
        }

        /// <summary>
        /// Obtém um convite de partida específico com base no remetente, recetor e data do jogo.
        /// Utilizado para evitar convites duplicados com as mesmas especificações.
        /// </summary>
        /// <param name="idSender">O ID da equipa remetente.</param>
        /// <param name="idReceiver">O ID da equipa recetora.</param>
        /// <param name="gameDate">A data e hora proposta para o jogo.</param>
        /// <returns>A entidade [MatchInvite] correspondente ou null.</returns>
        public async Task<MatchInvite?> GetMatchInvite(Guid idSender, Guid idReceiver, DateTime gameDate)
        {
            return await context.MatchInvite.FirstOrDefaultAsync(mi => mi.IdSender == idSender
                                                                    && mi.IdReceiver == idReceiver
                                                                    && mi.GameDate == gameDate);
        }

        /// <summary>
        /// Obtém um convite de partida específico, carregando as entidades relacionadas (Equipas e Local).
        /// </summary>
        /// <remarks>
        /// Utiliza `.Include()` para carregar as entidades [Sender], [Receiver] e [Pitch] numa única consulta (Eager Loading).
        /// </remarks>
        /// <param name="idSender">O ID da equipa remetente.</param>
        /// <param name="idReceiver">O ID da equipa recetora.</param>
        /// <returns>A entidade [MatchInvite] completa com entidades de navegação carregadas.</returns>
        public async Task<MatchInvite?> GetMatchInviteWithPitchByTeams(Guid idSender, Guid idReceiver)
        {
            return await context.MatchInvite
                .Include(mi => mi.Sender)
                .Include(mi => mi.Receiver)
                .Include(mi => mi.Pitch)
                .FirstOrDefaultAsync(mi => mi.IdSender == idSender && mi.IdReceiver == idReceiver);
        }

        /// <summary>
        /// Obtém uma lista resumida de todos os convites de partida recebidos por uma equipa específica.
        /// </summary>
        /// <remarks>
        /// A consulta projeta o resultado diretamente no DTO [InfoMatchInviteDto] para otimização
        /// e evitar a sobrecarga de carregar dados desnecessários.
        /// </remarks>
        /// <param name="idReceiver">O ID da equipa recetora dos convites.</param>
        /// <returns>Uma lista de [InfoMatchInviteDto] com informações resumidas dos convites.</returns>
        public async Task<List<InfoMatchInviteDto?>> GetAllMatchInviteReceiverById(Guid idReceiver)
        {
            var query = await context.MatchInvite
                .Where(mi => mi.IdReceiver == idReceiver)
                .Include(mi => mi.Sender)
                .Include(mi => mi.Pitch)
                .Select(mi => new InfoMatchInviteDto
                {
                    Id = mi.Id,

                    Sender = new TeamDto
                    {
                        IdTeam = mi.IdSender,
                        Name = mi.Sender.Name
                    },

                    Receiver = new TeamDto
                    {
                        IdTeam = mi.IdReceiver,
                        Name = mi.Receiver.Name
                    },

                    GameDate = mi.GameDate,
                    NamePitch = mi.Pitch.Name
                }).ToListAsync();

            return query;
        }

        /// <summary>
        /// Obtém uma lista filtrada de convites de partida recebidos, utilizando critérios avançados.
        /// </summary>
        /// <remarks>
        /// Constrói a query de forma dinâmica (`AsQueryable()`) para aplicar filtros condicionais
        /// e projeta o resultado no DTO [InfoMatchInviteDto].
        /// </remarks>
        /// <param name="idReceiver">O ID da equipa que está a receber os convites.</param>
        /// <param name="filter">O DTO contendo os critérios de filtragem (Nome do Remetente, Intervalo de Datas, IDs).</param>
        /// <returns>Uma lista de [InfoMatchInviteDto] que satisfaz os critérios de filtragem.</returns>
        public async Task<List<InfoMatchInviteDto?>> GetAllMatchInvitesTeamWithFilters(Guid idReceiver, FilterMatchInvitesDto filter)
        {
            var senderName = filter.SenderName;
            var minDate = filter.MinDate;
            var maxDate = filter.MaxDate;

            var query = context.MatchInvite.AsQueryable();

            query = query.Where(mi => mi.IdReceiver == idReceiver);

            if (!string.IsNullOrEmpty(senderName))
            {
                query = query.Where(mi => mi.Sender.Name.ToUpper().Contains(senderName.ToUpper()));
            }

            if (minDate != null)
            {
                query = query.Where(mi => DateOnly.FromDateTime(mi.GameDate) >= minDate);
            }

            if (maxDate != null)
            {
                query = query.Where(mi => DateOnly.FromDateTime(mi.GameDate) <= maxDate);
            }

            var list = await query
                .Select(mi => new InfoMatchInviteDto
                {
                    Id = mi.Id,
                    Sender = new TeamDto
                    {
                        IdTeam = mi.IdSender,
                        Name = mi.Sender.Name
                    },
                    Receiver = new TeamDto
                    {
                        IdTeam = mi.IdReceiver,
                        Name = mi.Receiver.Name
                    },
                    GameDate = mi.GameDate,
                    NamePitch = mi.Pitch.Name,
                    isHome = mi.IdPitch == mi.Receiver.IdPitch
                }).ToListAsync();

            return list;
        }
    }
}
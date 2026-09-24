using Application.DTOs.Filters;
using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência e consulta da entidade [MembershipRequest].
    /// 
    /// Gere o ciclo de vida dos pedidos de adesão (Jogador -> Equipa) e convites de recrutamento (Equipa -> Jogador).
    /// </summary>
    public class MembershipRequestRepository : IMembershipRequestRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder às tabelas.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [MembershipRequestRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public MembershipRequestRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona um novo pedido de adesão/convite à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="request">A entidade [MembershipRequest] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddMembershipRequest(MembershipRequest request)
        {
            await context.MembershipRequests.AddAsync(request);
        }

        /// <summary>
        /// Marca um pedido de adesão/convite existente para ser removido da base de dados.
        /// </summary>
        /// <param name="request">A entidade [MembershipRequest] a ser removida (ex: após aceitação ou rejeição).</param>
        public void RemoveMembershipRequest(MembershipRequest request)
        {
            context.MembershipRequests.Remove(request);
        }

        /// <summary>
        /// Obtém um pedido de adesão/convite pelo seu identificador único (ID), carregando as entidades relacionadas.
        /// </summary>
        /// <remarks>
        /// Utiliza `.Include()` para carregar as entidades [Player] e [Team] numa única consulta.
        /// </remarks>
        /// <param name="id">O ID (GUID) do pedido.</param>
        /// <returns>A entidade [MembershipRequest] completa ou null se não for encontrada.</returns>
        public async Task<MembershipRequest?> GetMembershipRequestById(Guid id)
        {
            return await context.MembershipRequests
                .Include(mr => mr.Player)
                .Include(mr => mr.Team)
                .FirstOrDefaultAsync(mr => mr.Id == id);
        }

        /// <summary>
        /// Obtém um pedido de adesão/convite pendente com base nos IDs do Jogador e da Equipa.
        /// </summary>
        /// <remarks>
        /// Utilizado para verificar a existência de pedidos duplicados entre o mesmo par de entidades.
        /// </remarks>
        /// <param name="playerId">O ID do jogador.</param>
        /// <param name="teamId">O ID da equipa.</param>
        /// <returns>A entidade [MembershipRequest] correspondente ou null.</returns>
        public async Task<MembershipRequest?> GetMembershipRequestByPlayerAndTeam(string playerId, Guid teamId)
        {
            return await context.MembershipRequests
                .Include(mr => mr.Player)
                .Include(mr => mr.Team)
                .FirstOrDefaultAsync(mr => mr.IdPlayer == playerId && mr.IdTeam == teamId);
        }

        /// <summary>
        /// Obtém a lista de pedidos de adesão **enviados por Jogadores** (Join Requests) para uma Equipa específica.
        /// </summary>
        /// <remarks>
        /// Filtra por `IdTeam` e `IsPlayerSender == true`. Projeta o resultado no DTO resumido.
        /// </remarks>
        /// <param name="teamId">O ID da equipa que recebeu os pedidos.</param>
        /// <returns>Uma lista de [MemberShipRequestDto] com os pedidos recebidos.</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeam(Guid teamId)
        {
            return await context.MembershipRequests
                .Where(mr => mr.IdTeam == teamId && mr.IsPlayerSender == true)
                .Select(mr => new MemberShipRequestDto
                {
                    RequestId = mr.Id,
                    Player = new PlayerDto
                    {
                        Id = mr.IdPlayer,
                        Name = mr.Player.Name
                    },
                    Team = new TeamDto
                    {
                        IdTeam = mr.IdTeam,
                        Name = mr.Team.Name,
                    },
                    RequestDate = mr.InviteDate,
                    IsPlayerSender = mr.IsPlayerSender
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtém a lista de pedidos de adesão **enviados por Equipas** (Recruitment Invites) para uma Equipa específica, aplicando filtros.
        /// </summary>
        /// <remarks>
        /// Filtra por `IdTeam` e `IsPlayerSender == true` para obter apenas os Join Requests, e aplica filtros condicionais de data/nome.
        /// </remarks>
        /// <param name="teamId">O ID da equipa que recebeu os pedidos.</param>
        /// <param name="filters">O DTO contendo os critérios de filtragem (Nome do Remetente, Intervalo de Datas).</param>
        /// <returns>Uma lista filtrada de [MemberShipRequestDto].</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByTeamWithFilters(Guid teamId, FilterMembershipRequestsTeam filters)
        {
            var query = context.MembershipRequests
                .Where(mr => mr.IdTeam == teamId && mr.IsPlayerSender == true);

            if (filters.MinDate.HasValue)
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) >= filters.MinDate.Value);

            if (filters.MaxDate.HasValue)
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) <= filters.MaxDate.Value);

            if (!string.IsNullOrWhiteSpace(filters.SenderName))
            {
                var upperName = filters.SenderName.ToUpper();
                query = query.Where(mr => mr.Player.Name.ToUpper().Contains(upperName));
            }

            return await query
                .Select(mr => new MemberShipRequestDto
                {
                    RequestId = mr.Id,
                    Player = new PlayerDto
                    {
                        Id = mr.IdPlayer,
                        Name = mr.Player.Name
                    },
                    Team = new TeamDto
                    {
                        IdTeam = mr.IdTeam,
                        Name = mr.Team.Name,
                    },
                    RequestDate = mr.InviteDate,
                    IsPlayerSender = mr.IsPlayerSender
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtém a lista de convites de recrutamento **recebidos por um Jogador** de várias Equipas.
        /// </summary>
        /// <remarks>
        /// Filtra por `IdPlayer` e `IsPlayerSender == false`.
        /// </remarks>
        /// <param name="playerId">O ID do jogador que recebeu os convites.</param>
        /// <returns>Uma lista de [MemberShipRequestDto] com os convites de recrutamento.</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayer(string playerId)
        {
            return await context.MembershipRequests
                .Where(mr => mr.IdPlayer == playerId && mr.IsPlayerSender == false)
                .Select(mr => new MemberShipRequestDto
                {
                    RequestId = mr.Id,
                    Player = new PlayerDto
                    {
                        Id = mr.IdPlayer,
                        Name = mr.Player.Name
                    },
                    Team = new TeamDto
                    {
                        IdTeam = mr.IdTeam,
                        Name = mr.Team.Name,
                    },
                    RequestDate = mr.InviteDate,
                    IsPlayerSender = mr.IsPlayerSender
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtém a lista de convites de recrutamento **recebidos por um Jogador** com filtros aplicados.
        /// </summary>
        /// <remarks>
        /// Filtra por `IdPlayer`, `IsPlayerSender == false`, e aplica filtros condicionais (Nome da Equipa Remetente, Datas).
        /// </remarks>
        /// <param name="playerId">O ID do jogador recetor.</param>
        /// <param name="filters">O DTO contendo os critérios de filtragem.</param>
        /// <returns>Uma lista filtrada de [MemberShipRequestDto].</returns>
        public async Task<List<MemberShipRequestDto>> GetMembershipRequestsByPlayerWithFilters(string playerId, FilterMembershipRequestsPlayer filters)
        {
            var query = context.MembershipRequests
                .Where(mr => mr.IdPlayer == playerId && mr.IsPlayerSender == false);

            if (filters.MinDate.HasValue)
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) >= filters.MinDate.Value);

            if (filters.MaxDate.HasValue)
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) <= filters.MaxDate.Value);

            if (!string.IsNullOrWhiteSpace(filters.SenderName))
            {
                var upperName = filters.SenderName.ToUpper();
                query = query.Where(mr => mr.Team.Name.ToUpper().Contains(upperName));
            }

            return await query
                .Select(mr => new MemberShipRequestDto
                {
                    RequestId = mr.Id,
                    Player = new PlayerDto
                    {
                        Id = mr.IdPlayer,
                        Name = mr.Player.Name
                    },
                    Team = new TeamDto
                    {
                        IdTeam = mr.IdTeam,
                        Name = mr.Team.Name,
                    },
                    RequestDate = mr.InviteDate,
                    IsPlayerSender = mr.IsPlayerSender
                })
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se já existe um pedido de adesão ou convite pendente entre um par de Jogador/Equipa.
        /// </summary>
        /// <param name="playerId">O ID do jogador.</param>
        /// <param name="teamId">O ID da equipa.</param>
        /// <returns><c>true</c> se o pedido existir, <c>false</c> caso contrário.</returns>
        public async Task<bool> ExistsRequestBetweenPlayerAndTeam(string playerId, Guid teamId)
        {
            return await context.MembershipRequests
                .AnyAsync(mr => mr.IdPlayer == playerId && mr.IdTeam == teamId);
        }

        /// <summary>
        /// Remove todos os pedidos de adesão ou convites associados a um jogador específico.
        /// </summary>
        /// <remarks>
        /// Utilizado no cenário de eliminação de conta ou no processo de gestão de conflitos.
        /// </remarks>
        /// <param name="playerId">O ID do jogador alvo.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a remoção.</returns>
        public async Task RemoveAllMemberShipRequestsOfPlayer(string playerId)
        {
            var requests = await context.MembershipRequests
                .Where(mr => mr.IdPlayer == playerId)
                .ToListAsync();

            context.MembershipRequests.RemoveRange(requests);
        }

        /// <summary>
        /// Remove todos os pedidos de adesão ou convites associados a uma equipa específica.
        /// </summary>
        /// <remarks>
        /// Utilizado no cenário de eliminação de equipa.
        /// </remarks>
        /// <param name="idTeam">O ID da equipa alvo.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a remoção.</returns>
        public async Task RemoveAllMemberShipRequestsOfTeam(Guid idTeam)
        {
            var requests = await context.MembershipRequests
                .Where(mr => mr.IdTeam == idTeam)
                .ToListAsync();

            context.MembershipRequests.RemoveRange(requests);
        }
    }
}
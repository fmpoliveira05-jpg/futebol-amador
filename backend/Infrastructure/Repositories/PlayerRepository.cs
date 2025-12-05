using Application.DTOs.Filters;
using Application.DTOs.Player;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório específico para operações de persistência de dados e consulta da entidade [Player].
    /// 
    /// Implementa o contrato [IPlayerRepository] e é a fonte de verdade para perfis de jogadores.
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder às tabelas.
        /// </summary>
        private readonly AmateurFootballContext context;

        /// <summary>
        /// Construtor da classe [PlayerRepository].
        /// </summary>
        /// <param name="context">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public PlayerRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Obtém um jogador pelo seu número de telefone de forma assíncrona.
        /// </summary>
        /// <param name="phoneNumber">O número de telefone completo do jogador.</param>
        /// <returns>A entidade [Player] ou null.</returns>
        public async Task<Player?> GetPlayerByPhoneNumberAsync(string phoneNumber) {
            return await context.Player.FirstOrDefaultAsync(p => p.Phone == phoneNumber);
        }

        /// <summary>
        /// Adiciona um novo jogador à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="player">A entidade [Player] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddAsync(Player player)
        {
            await context.Player.AddAsync(player);
        }

        /// <summary>
        /// Marca um jogador existente para ser removido da base de dados (deleção).
        /// </summary>
        /// <param name="playerToRemove">A entidade [Player] a ser removida.</param>
        public void DeletePlayer(Player playerToRemove)
        {
            context.Player.Remove(playerToRemove);
        }

        /// <summary>
        /// Obtém um jogador pelo seu endereço de e-mail de forma assíncrona.
        /// </summary>
        /// <param name="email">O endereço de e-mail do jogador.</param>
        /// <returns>A entidade [Player] ou null.</returns>
        public async Task<Player?> GetPlayerByEmailAsync(string email)
        {
            return await context.Player.FirstOrDefaultAsync(p => p.Email == email);
        }

        /// <summary>
        /// Obtém um jogador pelo seu identificador único (ID), carregando a afiliação à equipa.
        /// </summary>
        /// <remarks>
        /// Utiliza `.Include(p => p.Team)` para carregar os dados da equipa associada (Eager Loading).
        /// </remarks>
        /// <param name="id">O ID (UID) do jogador.</param>
        /// <returns>A entidade [Player] com os dados da equipa carregados, ou null.</returns>
        public async Task<Player?> GetPlayerByIdAsync(string id)
        {
            return await context.Player
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtém uma lista de jogadores com base numa lista de IDs fornecida.
        /// </summary>
        /// <param name="playerIdList">A lista de IDs (Strings) a procurar.</param>
        /// <returns>Uma lista de entidades [Player].</returns>
        public async Task<List<Player>> GetPlayersListByIdListAsync(List<string> playerIdList)
        {
            return await context.Player
                .Where(p => playerIdList.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Marca uma entidade [Player] para ser atualizada na base de dados.
        /// </summary>
        /// <param name="updatedPlayer">A entidade [Player] com os novos valores.</param>
        public void UpdatePlayer(Player updatedPlayer)
        {
            context.Player.Update(updatedPlayer);
        }

        /// <summary>
        /// Obtém um pedido de adesão específico associado a um jogador, carregando as relações necessárias.
        /// </summary>
        /// <remarks>
        /// Consulta a tabela [MembershipRequests] mas carrega o jogador e a equipa associada através de [ThenInclude].
        /// </remarks>
        /// <param name="playerId">O ID do jogador.</param>
        /// <returns>A entidade [MembershipRequest] correspondente, ou null.</returns>
        public async Task<MembershipRequest?> GetPlayerByIdWithRequestsAsync(string playerId)
        {
            return await context.MembershipRequests
                .Include(m => m.Player)
                .ThenInclude(m => m.Team)
                .FirstOrDefaultAsync(p => p.IdPlayer == playerId);
        }

        /// <summary>
        /// Obtém uma lista de jogadores para pesquisa de mercado, aplicando filtros dinâmicos.
        /// </summary>
        /// <remarks>
        /// Esta consulta complexa calcula a idade do jogador usando [EF.Functions.DateDiffDay] e projeta o resultado
        /// no DTO [InfoPlayerDto]. Também implementa filtragem por nome, cidade e atributos físicos.
        /// </remarks>
        /// <param name="filters">O DTO com os critérios de filtragem (Nome, Idade, Altura, Posição).</param>
        /// <returns>Uma lista de objetos [InfoPlayerDto] para o frontend.</returns>
        public async Task<List<InfoPlayerDto?>> GetPlayersList(FilterTeamDto? filters)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            var query = context.Player.AsQueryable();

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.PlayerName))
                {
                    var upperCase = filters.PlayerName.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(upperCase));
                }

                if (!string.IsNullOrEmpty(filters.City))
                {
                    var fragment = filters.City.ToLower();
                    query = query.Where(p =>
                        EF.Functions.Like(p.Address.ToLower(), "%, %" + fragment + "%")
                        &&
                        !EF.Functions.Like(p.Address.ToLower(), "%, %" + fragment + "%,%")
                    );
                }

                if (filters.MinAge.HasValue)
                {
                    query = query.Where(p => EF.Functions.DateDiffDay(p.DateOfBirth, dateNow) >= filters.MinAge);
                }

                if (filters.MaxAge.HasValue)
                {
                    query = query.Where(p => EF.Functions.DateDiffDay(p.DateOfBirth, dateNow) <= filters.MaxAge);
                }

                if (filters.MinHeight.HasValue)
                {
                    query = query.Where(p => p.Height >= filters.MinHeight);
                }

                if (filters.MaxHeight.HasValue)
                {
                    query = query.Where(p => p.Height <= filters.MaxHeight);
                }

                if (filters.Position.HasValue)
                {
                    query = query.Where(p => p.Position == filters.Position);
                }
            }

            var list = await query
                .Select(p => new InfoPlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Age = EF.Functions.DateDiffDay(p.DateOfBirth, dateNow) / 365,
                    Heigth = p.Height,
                    Position = p.Position,
                    HaveTeam = p.IdTeam != null
                })
                .ToListAsync();

            return list!; 
        }
    }
}
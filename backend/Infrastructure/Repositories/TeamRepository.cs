using Application.DTOs.Filters;
using Application.DTOs.Pitch;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Rank;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Repositório principal para a entidade [Team].
    /// 
    /// Esta classe gere todas as operações de persistência e consultas relacionadas com equipas,
    /// incluindo o Leaderboard e as listas de pesquisa (Matchmaking e Mercado).
    /// </summary>
    public class TeamRepository : ITeamRepository
    {
        /// <summary>
        /// O contexto da base de dados ([AmateurFootballContext]) injetado.
        /// Utilizado para aceder às tabelas.
        /// </summary>
        private readonly AmateurFootballContext DbContext;

        /// <summary>
        /// Construtor da classe [TeamRepository].
        /// </summary>
        /// <param name="DbContext">O contexto da base de dados (Db Context) injetado via Dependency Injection.</param>
        public TeamRepository(AmateurFootballContext DbContext)
        {
            this.DbContext = DbContext;
        }

        /// <summary>
        /// Marca uma equipa para ser removida da base de dados.
        /// </summary>
        /// <param name="teamToRemove">A entidade [Team] a ser removida.</param>
        public void DeleteTeam(Team teamToRemove)
        {
            DbContext.Team.Remove(teamToRemove);
        }

        /// <summary>
        /// Marca uma entidade [Team] existente para ser atualizada na base de dados.
        /// </summary>
        /// <param name="updatedTeam">A entidade [Team] com os novos valores.</param>
        public void UpdateTeam(Team updatedTeam)
        {
            DbContext.Team.Update(updatedTeam);
        }

        /// <summary>
        /// Obtém todas as equipas.
        /// </summary>
        /// <returns>Uma lista de todas as entidades [Team].</returns>
        public async Task<List<Team>?> GetAllTeamsAsync()
        {
            return await DbContext.Team.ToListAsync();
        }

        /// <summary>
        /// Adiciona uma nova equipa à base de dados de forma assíncrona.
        /// </summary>
        /// <param name="team">A entidade [Team] a ser persistida.</param>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que representa a operação de adição.</returns>
        public async Task AddAsync(Team team)
        {
            await DbContext.Team.AddAsync(team);
        }
       
        /// <summary>
        /// Obtém uma equipa pelo seu ID, carregando todas as coleções de convites e o calendário.
        /// </summary>
        /// <remarks>
        /// Utiliza Eager Loading para [Calendar], [SentInvites] e [ReceivedInvites].
        /// </remarks>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] completa ou null.</returns>
        public async Task<Team?> GetTeamByIdAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Calendar)
                .Include(t => t.SentInvites)
                .Include(t => t.ReceivedInvites)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma equipa pelo seu nome.
        /// </summary>
        /// <param name="name">O nome da equipa.</param>
        /// <returns>A entidade [Team] ou null.</returns>
        public async Task<Team?> GetTeamByNameAsync(String name)
        {
            return await DbContext.Team
                .FirstOrDefaultAsync(t => t.Name == name); 
        }

        /// <summary>
        /// Obtém os dados básicos de uma equipa (DTO) pelo seu ID.
        /// </summary>
        /// <remarks>
        /// Utiliza Projeção (Select) para criar um objeto [TeamDto] leve, ideal para ser usado como Opponent ou em cabeçalhos.
        /// </remarks>
        /// <param name="idTeam">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [TeamDto] ou null.</returns>
        public async Task<TeamDto?> GetOpponentTeamById(Guid idTeam)
        {
            return await DbContext.Team
                    .Where(t => t.Id == idTeam)  
                    .Select(t => new TeamDto
                    {
                        IdTeam = t.Id,
                        Name = t.Name
                    })        
                    .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtém uma equipa pelo seu ID, carregando o campo ([Pitch]) associado.
        /// </summary>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] com o Pitch carregado, ou null.</returns>
        public async Task<Team?> GetTeamByIdWithPitchAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma equipa pelo ID, carregando Convites Recebidos e Calendário.
        /// </summary>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] completa ou null.</returns>
        public async Task<Team?> GetByIdWithReceivedInvitesAndCalendar(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.ReceivedInvites)
                .Include(t => t.Calendar)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma equipa pelo ID, carregando apenas os Convites Recebidos.
        /// </summary>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] com os convites recebidos carregados, ou null.</returns>
        public async Task<Team?> GetByIdWithReceivedInvites(Guid id)
        {
            return await DbContext.Team
               .Include(t => t.ReceivedInvites)
               .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém o perfil completo de uma equipa, projetando o resultado diretamente no DTO [TeamDetailsDto].
        /// </summary>
        /// <remarks>
        /// Esta consulta complexa é otimizada para ser executada numa única viagem à base de dados. 
        /// Carrega os dados da Equipa, Rank, Pitch e todos os seus Membros, e calcula a Idade dos jogadores
        /// utilizando a função nativa do EF Core [EF.Functions.DateDiffDay].
        /// </remarks>
        /// <param name="teamId">O ID (GUID) da equipa a ser consultada.</param>
        /// <returns>O DTO [TeamDetailsDto] contendo todas as informações do perfil e a lista de jogadores, ou null.</returns>
        public async Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            return await DbContext.Team
                .Where(t => t.Id == teamId)
                .Include(t => t.Pitch)
                .Select(t => new TeamDetailsDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    FoundationDate = DateOnly.FromDateTime(t.DataFoundation),
                    TotalPoints = t.CurrentPoints,
                    RankName = t.Rank.Name,
                    PitchDto = new PitchDto
                    {
                        Name = t.Pitch.Name,
                        Address = t.Pitch.Address,
                    },
                    Players = t.Members.Select(player => new PlayerDetailsDto
                    {
                        PlayerId = player.Id,
                        Name = player.Name,
                        Email = player.Email,
                        PhoneNumber = player.Phone,
                        Address = player.Address,
                        Age = EF.Functions.DateDiffDay(player.DateOfBirth, dateNow),
                        DateOfBirth = player.DateOfBirth,
                        Team = new TeamDto
                        {
                            IdTeam = t.Id,
                            Name = t.Name,
                        },
                        Height = player.Height,
                        Position = player.Position,
                        IsAdmin = player.IsAdmin
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtém uma equipa pelo ID, carregando pedidos de adesão e membros.
        /// </summary>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] com [MembershipRequests] e [Members] carregados, ou null.</returns>
        public async Task<Team?> GetTeamForMembershipRequestAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.MembershipRequests)
                    .ThenInclude(r => r.Player)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma equipa para o processo de deleção, carregando as dependências complexas (Membros, Calendário e Partidas).
        /// </summary>
        /// <remarks>
        /// Utiliza [AsSplitQuery()] para otimizar o desempenho, quebrando a consulta em múltiplas queries SQL.
        /// Carrega: Team -> Members; Team -> Calendar -> Matches.
        /// </remarks>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] carregada com todas as dependências de deleção.</returns>
        public async Task<Team?> GetTeamForDeletionAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .Include(t => t.Calendar)
                    .ThenInclude(c => c.Matches)
                .AsSplitQuery() // Importante para evitar explosão cartesiana
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma equipa para o formulário de atualização, carregando membros e pitch.
        /// </summary>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] com membros e pitch carregados, ou null.</returns>
        public async Task<Team?> GetTeamForUpdateAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma equipa pelo seu nome, carregando os membros associados.
        /// </summary>
        /// <param name="name">O nome da equipa.</param>
        /// <returns>A entidade [Team] com a coleção de [Members] carregada, ou null.</returns>
        public async Task<Team?> GetTeamByNameWithMembersAsync(string name)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        /// <summary>
        /// Obtém uma equipa pelo ID, carregando os membros para gestão.
        /// </summary>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] com os [Members] carregados, ou null.</returns>
        public async Task<Team?> GetTeamForMemberManagementAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma lista filtrada de jogadores de uma equipa específica.
        /// </summary>
        /// <remarks>
        /// A consulta aplica filtros de gestão (Nome, Idade, Posição, Admin Status) sobre a coleção local de membros da equipa.
        /// </remarks>
        /// <param name="teamId">O ID da equipa cujos jogadores serão listados.</param>
        /// <param name="filter">O DTO contendo os critérios de filtragem.</param>
        /// <returns>Uma lista de [PlayerDetailsDto] com os jogadores filtrados.</returns>
        public async Task<List<PlayerDetailsDto>> GetTeamPlayersDtoAsyncWithFilters(Guid teamId, FilterTeamPlayers filter)
        {
            var team = await DbContext.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                return new List<PlayerDetailsDto>();
            }

            var playersQuery = team.Members.AsQueryable();

            if (filter.IsAdmin.HasValue)
            {
                playersQuery = playersQuery.Where(p => p.IsAdmin == filter.IsAdmin.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var upperName = filter.Name.ToUpper();
                playersQuery = playersQuery.Where(p => p.Name.ToUpper().Contains(upperName));
            }

            if (filter.Position.HasValue)
            {
                playersQuery = playersQuery.Where(p => p.Position == filter.Position.Value);
            }

            if (filter.MinAge.HasValue || filter.MaxAge.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                if (filter.MinAge.HasValue)
                {
                    var maxBirthDate = today.AddYears(-filter.MinAge.Value);
                    playersQuery = playersQuery.Where(p => p.DateOfBirth <= maxBirthDate);
                }

                if (filter.MaxAge.HasValue)
                {
                    var minBirthDate = today.AddYears(-filter.MaxAge.Value);
                    playersQuery = playersQuery.Where(p => p.DateOfBirth >= minBirthDate);
                }
            }

            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            return playersQuery
                .Select(player => new PlayerDetailsDto
                {
                    PlayerId = player.Id,
                    Name = player.Name,
                    DateOfBirth = player.DateOfBirth,
                    Address = player.Address,
                    Position = player.Position,
                    Height = player.Height,
                    Email = player.Email,
                    PhoneNumber = player.Phone,
                    Team = new TeamDto {
                        IdTeam = team.Id,
                        Name = team.Name
                    },
                    Age = EF.Functions.DateDiffDay(player.DateOfBirth, dateNow),
                    IsAdmin = player.IsAdmin
                })
                .ToList();
        }

        /// <summary>
        /// Obtém a tabela de classificação das equipas por pontuação, limitada ao Top N.
        /// </summary>
        /// <param name="top">O número máximo de equipas a retornar (ex: 100).</param>
        /// <returns>Uma lista de [TeamLeaderboardDto] ordenada por pontos.</returns>
        public async Task<List<TeamLeaderboardDto>> GetTopTeamsAsync(int top)
        {
            return await DbContext.Team
                .Include(t => t.Rank)
                .OrderByDescending(t => t.CurrentPoints)
                .Take(top)
                .Select(t => new TeamLeaderboardDto
                {
                    TeamName = t.Name,
                    CurrentPoints = t.CurrentPoints,
                    RankName = t.Rank.Name
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtém uma equipa pelo seu ID, carregando Membros, Rank e Pitch.
        /// </summary>
        /// <param name="id">O ID (GUID) da equipa.</param>
        /// <returns>A entidade [Team] com [Members], [Rank] e [Pitch] carregados, ou null.</returns>
        public async Task<Team?> GetTeamWitchMemberRankAndPitchAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .Include (t => t.Rank)
                .Include (t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Obtém uma lista de todas as equipas elegíveis para recrutar (membros &lt; MaxMembers) ou que podem ser desafiadas.
        /// </summary>
        /// <remarks>
        /// Calcula a idade média dos membros na base de dados (o que pode ser dispendioso).
        /// </remarks>
        /// <returns>Uma lista de [InfoTeamsDto] com estatísticas agregadas.</returns>
        public async Task<List<InfoTeamsDto>> GetListTeamsPlayer()
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = (from t in DbContext.Team
                         join pitch in DbContext.Pitch on t.IdPitch equals pitch.Id
                         join rank in DbContext.Rank on t.IdRank equals rank.Id

                         where t.Members.Count < ModelConstants.TeamConst.MaxMembers
                         let averageAge = t.Members.Any()
                                    ? t.Members.Average(m =>
                                        ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                                    : 0.0

                         select new InfoTeamsDto
                         {
                             Id = t.Id,
                             Name = t.Name,
                             Description = t.Description,
                             Address = pitch.Address,
                             PlayerCount = t.Members.Count,
                             AverageAge = (float)averageAge,
                             Rank = new InfoRankDto
                             {
                                 IdRank = t.IdRank,
                                 Name = rank.Name
                             },
                             CurrentPoints = t.CurrentPoints,
                         }
                )
                .ToListAsync();

            return await query;
        }

        /// <summary>
        /// Obtém uma lista de equipas para Matchmaking/Desafio, excluindo a equipa de origem e aplicando filtros.
        /// </summary>
        /// <remarks>
        /// Carrega os dados necessários e projeta-os no DTO, aplicando filtros dinâmicos (Nome, Rank, Pontos, Idade Média, Localização e Contagem de Jogadores).
        /// </remarks>
        /// <param name="idTeam">O ID (GUID) da equipa que está a fazer a pesquisa.</param>
        /// <param name="filters">O DTO com os critérios de filtragem.</param>
        /// <returns>Uma lista de [InfoTeamsDto] filtrados, excluindo a equipa de origem.</returns>
        public async Task<List<InfoTeamsDto>> GetListTeamsPlayersWithFilters(FilterListTeamDto filters)
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = DbContext.Team
                            .Include(t => t.Pitch)
                            .Include(t => t.Rank)
                            .Where(t => t.Members.Count < ModelConstants.TeamConst.MaxMembers);

            if (!string.IsNullOrEmpty(filters.NameTeam))
            {
                var upperCase = filters.NameTeam.ToUpper();
                query = query.Where(t => t.Name.ToUpper().Contains(upperCase));
            }

            if (!string.IsNullOrEmpty(filters.NameRank))
            {
                var upperCase = filters.NameRank.ToUpper();
                query = query.Where(t => t.Rank.Name.ToUpper().Contains(upperCase));
            }

            if (!string.IsNullOrEmpty(filters.City))
            {
                var fragment = filters.City.ToLower();
                query = query.Where(t =>
                    EF.Functions.Like(t.Pitch.Address.ToLower(), "%, %" + fragment + "%")
                    &&
                    !EF.Functions.Like(t.Pitch.Address.ToLower(), "%, %" + fragment + "%,%")
                );
            }

            if (filters.MinNumberPoints.HasValue)
            {
                query = query.Where(t => t.CurrentPoints >= filters.MinNumberPoints.Value);
            }

            if (filters.MaxNumberPoints.HasValue)
            {
                query = query.Where(t => t.CurrentPoints <= filters.MaxNumberPoints.Value);
            }

            if (filters.MinAge.HasValue)
            {
                query = query.Where(t =>
                     (t.Members.Any()
                         ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                         : 0.0) >= filters.MinAge.Value
                );
            }

            if (filters.MaxAge.HasValue)
            {
                query = query.Where(t =>
                    (t.Members.Any()
                        ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                        : 0.0) <= filters.MaxAge.Value
                );
            }

            if (filters.MinNumberPlayers.HasValue)
            {
                query = query.Where(t => t.Members.Count >= filters.MinNumberPlayers.Value);
            }

            if (filters.MaxNumberPlayers.HasValue)
            {
                query = query.Where(t => t.Members.Count <= filters.MaxNumberPlayers.Value);
            }

            var list = await query
                .Select(t => new
                {
                    Team = t,
                    AverageAge = t.Members.Any()
                        ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                        : 0.0,
                    NumMembers = t.Members.Count,
                    Pitch = t.Pitch,
                    Rank = t.Rank,
                })
                .Select(x => new InfoTeamsDto
                {
                    Id = x.Team.Id,
                    Name = x.Team.Name,
                    Description = x.Team.Description,
                    Address = x.Pitch.Address,
                    AverageAge = (float)x.AverageAge,
                    CurrentPoints = x.Team.CurrentPoints,
                    PlayerCount = x.NumMembers,
                    Rank = new InfoRankDto
                    {
                        IdRank = x.Rank.Id,
                        Name = x.Rank.Name
                    }
                }).ToListAsync();

            return list;
        }

        /// <summary>
        /// Obtém a lista de IDs de membros de uma equipa.
        /// </summary>
        /// <remarks>
        /// Carrega a entidade [Team] e projeta a coleção [Members] para obter apenas os IDs (Strings) dos jogadores.
        /// </remarks>
        /// <param name="teamId">O ID (GUID) da equipa.</param>
        /// <returns>Uma tarefa que retorna uma lista de strings contendo os IDs dos membros.</returns>
        public Task<List<string>> GetMemberIdsByTeamIdAsync(Guid teamId)
        {
            return DbContext.Team
                .Where(t => t.Id == teamId)
                .Include(p => p.Members)
                .Select(p => p.Id.ToString())
                .ToListAsync();
        }

        /// <summary>
        /// Obtém uma lista de equipas para Matchmaking/Desafio.
        /// </summary>
        /// <remarks>
        /// Esta consulta complexa utiliza LINQ para:
        /// 1. Excluir a equipa de origem e filtrar por equipas com membros suficientes (>= 11).
        /// 2. Calcular a idade média dos membros ([AverageAge]) no servidor (durante a projeção).
        /// 3. Projetar o resultado no DTO [InfoTeamsDto] com detalhes de Rank e Pitch.
        /// </remarks>
        /// <param name="idTeam">O ID (GUID) da equipa que está a fazer a pesquisa.</param>
        /// <returns>Uma lista de [InfoTeamsDto] com estatísticas agregadas das equipas elegíveis.</returns>
        public async Task<List<InfoTeamsDto>> GetListTeamsForTeams(Guid idTeam)
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = (from t in DbContext.Team
                         join pitch in DbContext.Pitch on t.IdPitch equals pitch.Id
                         join rank in DbContext.Rank on t.IdRank equals rank.Id

                         where t.Members.Count >= 11
                            && t.Id != idTeam


                         let averageAge = t.Members.Any()
                                    ? t.Members.Average(m =>
                                        ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                                    : 0.0

                         select new InfoTeamsDto
                         {
                             Id = t.Id,
                             Name = t.Name,
                             Description = t.Description,
                             Address = pitch.Address,
                             PlayerCount = t.Members.Count,
                             AverageAge = (float)averageAge,
                             Rank = new InfoRankDto
                             {
                                 IdRank = t.IdRank,
                                 Name = rank.Name
                             },
                             CurrentPoints = t.CurrentPoints,
                         }
                )
                .ToListAsync();

            return await query;
        }

        /// <summary>
        /// Obtém uma lista de equipas para Matchmaking/Desafio, excluindo a equipa de origem e aplicando filtros.
        /// </summary>
        /// <remarks>
        /// Esta consulta dinâmica aplica múltiplos filtros condicionais (Nome, Rank, Pontos, Localização e Estatísticas de Membros)
        /// antes de calcular as estatísticas agregadas e projetar para o DTO [InfoTeamsDto].
        /// </remarks>
        /// <param name="idTeam">O ID (GUID) da equipa que está a fazer a pesquisa (excluída dos resultados).</param>
        /// <param name="filters">O DTO com os critérios de filtragem.</param>
        /// <returns>Uma lista de [InfoTeamsDto] filtrados, excluindo a equipa de origem.</returns>
        public async Task<List<InfoTeamsDto>> GetListTeamsByTeamsWithFilters(Guid idTeam, FilterListTeamDto filters)
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = DbContext.Team
                            .Include(t => t.Pitch)
                            .Include(t => t.Rank)
                            .Where(t => t.Id != idTeam 
                                && t.Members.Count < ModelConstants.TeamConst.MaxMembers);

            if (!string.IsNullOrEmpty(filters.NameTeam))
            {
                var upperCase = filters.NameTeam.ToUpper();
                query = query.Where(t => t.Name.ToUpper().Contains(upperCase));
            }

            if (!string.IsNullOrEmpty(filters.NameRank))
            {
                var upperCase = filters.NameRank.ToUpper();
                query = query.Where(t => t.Rank.Name.ToUpper().Contains(upperCase));
            }

            if (!string.IsNullOrEmpty(filters.City))
            {
                var fragment = filters.City.ToLower();
                query = query.Where(t =>
                    EF.Functions.Like(t.Pitch.Address.ToLower(), "%, %" + fragment + "%") 
                    &&
                    !EF.Functions.Like(t.Pitch.Address.ToLower(), "%, %" + fragment + "%,%")
                );
            }

            if (filters.MinNumberPoints.HasValue)
            {
                query = query.Where(t => t.CurrentPoints >= filters.MinNumberPoints.Value);
            }

            if (filters.MaxNumberPoints.HasValue)
            {
                query = query.Where(t => t.CurrentPoints <= filters.MaxNumberPoints.Value);
            }

            if (filters.MinAge.HasValue)
            {
                query = query.Where(t =>
                     (t.Members.Any()
                         ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                         : 0.0) >= filters.MinAge.Value
                );
            }

            if (filters.MaxAge.HasValue)
            {
                query = query.Where(t =>
                    (t.Members.Any()
                        ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                        : 0.0) <= filters.MaxAge.Value
                );
            }

            if (filters.MinNumberPlayers.HasValue)
            {
                query = query.Where(t => t.Members.Count >= filters.MinNumberPlayers.Value);
            }

            if (filters.MaxNumberPlayers.HasValue)
            {
                query = query.Where(t => t.Members.Count <= filters.MaxNumberPlayers.Value);
            }

            var list = await query
                .Select(t => new
                {
                    Team = t,
                    AverageAge = t.Members.Any()
                        ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                        : 0.0,
                    NumMembers = t.Members.Count,
                    Pitch = t.Pitch,
                    Rank = t.Rank,
                })
                .Select(x => new InfoTeamsDto
                {
                    Id = x.Team.Id,
                    Name = x.Team.Name,
                    Description = x.Team.Description,
                    Address = x.Pitch.Address,
                    AverageAge = (float)x.AverageAge,
                    CurrentPoints = x.Team.CurrentPoints,
                    PlayerCount = x.NumMembers,
                    Rank = new InfoRankDto
                    {
                        IdRank = x.Rank.Id,
                        Name = x.Rank.Name
                    }
                })
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// Obtém uma lista de equipas para pesquisa pública, aplicando filtros complexos.
        /// </summary>
        /// <remarks>
        /// Esta consulta dinâmica aplica múltiplos filtros condicionais (Nome, Rank, Pontos, Localização, Idade Média e Contagem de Jogadores)
        /// sobre todas as equipas. Esta é a consulta principal para o Mercado/Pesquisa Global.
        /// </remarks>
        /// <param name="filters">O DTO com os critérios de filtragem (opcional).</param>
        /// <returns>Uma lista de [InfoTeamsDto] filtrados.</returns>
        public async Task<List<InfoTeamsDto>> GetListTeams(FilterListTeamDto? filters)
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = DbContext.Team
                            .Include(t => t.Pitch)
                            .Include(t => t.Rank)
                            .AsQueryable();

            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.NameTeam))
                {
                    var upperCase = filters.NameTeam.ToUpper();
                    query = query.Where(t => t.Name.ToUpper().Contains(upperCase));
                }

                if (!string.IsNullOrEmpty(filters.NameRank))
                {
                    var upperCase = filters.NameRank.ToUpper();
                    query = query.Where(t => t.Rank.Name.ToUpper().Contains(upperCase));
                }

                if (!string.IsNullOrEmpty(filters.City))
                {
                    var fragment = filters.City.ToLower();
                    query = query.Where(t =>
                        EF.Functions.Like(t.Pitch.Address.ToLower(), "%, %" + fragment + "%")
                        &&
                        !EF.Functions.Like(t.Pitch.Address.ToLower(), "%, %" + fragment + "%,%")
                    );
                }

                if (filters.MinNumberPoints.HasValue)
                {
                    query = query.Where(t => t.CurrentPoints >= filters.MinNumberPoints.Value);
                }

                if (filters.MaxNumberPoints.HasValue)
                {
                    query = query.Where(t => t.CurrentPoints <= filters.MaxNumberPoints.Value);
                }

                if (filters.MinAge.HasValue)
                {
                    query = query.Where(t =>
                         (t.Members.Any()
                             ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                             : 0.0) >= filters.MinAge.Value
                    );
                }

                if (filters.MaxAge.HasValue)
                {
                    query = query.Where(t =>
                        (t.Members.Any()
                            ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                            : 0.0) <= filters.MaxAge.Value
                    );
                }

                if (filters.MinNumberPlayers.HasValue)
                {
                    query = query.Where(t => t.Members.Count >= filters.MinNumberPlayers.Value);
                }

                if (filters.MaxNumberPlayers.HasValue)
                {
                    query = query.Where(t => t.Members.Count <= filters.MaxNumberPlayers.Value);
                }
            }

            var list = await query
                .Select(t => new
                {
                    Team = t,
                    AverageAge = t.Members.Any()
                        ? t.Members.Average(m => ((double)EF.Functions.DateDiffDay(m.DateOfBirth, nowDateOnly) / 365.25))
                        : 0.0,
                    NumMembers = t.Members.Count,
                    Pitch = t.Pitch,
                    Rank = t.Rank,
                })
                .Select(x => new InfoTeamsDto
                {
                    Id = x.Team.Id,
                    Name = x.Team.Name,
                    Description = x.Team.Description,
                    Address = x.Pitch.Address,
                    AverageAge = (float)x.AverageAge,
                    CurrentPoints = x.Team.CurrentPoints,
                    PlayerCount = x.NumMembers,
                    Rank = new InfoRankDto
                    {
                        IdRank = x.Rank.Id,
                        Name = x.Rank.Name
                    }
                })
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// Obtém uma lista de jogadores que não são administradores e não pertencem a nenhuma equipa (Agentes Livres).
        /// </summary>
        /// <remarks>
        /// Consulta a tabela [Player] filtrando por `IsAdmin == false` e `IdTeam == null`.
        /// O resultado é projetado no DTO [PlayerWithoutTeamInfoDto] com cálculo de idade.
        /// </remarks>
        /// <returns>Uma lista de [PlayerWithoutTeamInfoDto] com os agentes livres.</returns>
        public async Task<List<PlayerWithoutTeamInfoDto>> GetListPlayersWithoutTeam(Guid idTeam)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow); 

            var query = await DbContext.Player
                .Where(p => !p.IsAdmin 
                    && p.IdTeam == null
                    && !p.MembershipRequests.Any(ms => ms.IdTeam == idTeam && ms.IsPlayerSender == false))
                .Select(p => new PlayerWithoutTeamInfoDto
                {
                    PlayerId = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Age = (int)(EF.Functions.DateDiffDay(p.DateOfBirth, dateNow) / 365.25),
                    Height = p.Height,
                    Position = p.Position
                })
                .ToListAsync();

            return query;
        }

        /// <summary>
        /// Obtém uma lista de jogadores Agentes Livres (sem equipa) aplicando filtros dinâmicos de pesquisa.
        /// </summary>
        /// <remarks>
        /// Aplica filtros por Nome, Cidade, Idade, Altura e Posição sobre jogadores não afiliados e não-administradores.
        /// </remarks>
        /// <param name="filters">O DTO com os critérios de filtragem.</param>
        /// <returns>Uma lista de [PlayerWithoutTeamInfoDto] filtrada.</returns>
        public async Task<List<PlayerWithoutTeamInfoDto>> GetListPlayersWithoutTeamtWithFilters(Guid idTeam, FilterTeamDto filters)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = DbContext.Player
                .Where(p => !p.IsAdmin 
                    && p.IdTeam == null
                    && !p.MembershipRequests.Any(ms => ms.IdTeam == idTeam && ms.IsPlayerSender == false));

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

            var list = await query.Select(p => new PlayerWithoutTeamInfoDto
            {
                PlayerId = p.Id,
                Name = p.Name,
                Address = p.Address,
                Age = EF.Functions.DateDiffDay(p.DateOfBirth, dateNow),
                Height = p.Height,
                Position = p.Position
            })
                .ToListAsync();

            return list;
        }

        /// <summary>
        /// Obtém a lista de IDs de todos os Administradores de uma equipa específica.
        /// </summary>
        /// <remarks>
        /// Carrega a entidade [Team] e projeta o resultado para obter apenas os IDs (Strings) dos membros que têm a flag [IsAdmin] verdadeira.
        /// </remarks>
        /// <param name="teamId">O ID da equipa.</param>
        /// <returns>Uma lista de strings contendo os IDs dos membros administradores.</returns>
        public Task<List<string>> GetAdminsIdsByTeamIdAsync(Guid teamId)
        {
            return DbContext.Player
                    .Where(p => p.IdTeam == teamId && p.IsAdmin) // Filtra direto pelo ID da FK e flag Admin
                    .Select(p => p.Id.ToString())
                    .ToListAsync();
        }
    }
}
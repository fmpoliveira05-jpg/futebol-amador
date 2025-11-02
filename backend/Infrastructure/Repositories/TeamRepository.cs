using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Rank;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AmateurFootballContext context;

        public TeamRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public Task DeleteTeam(Teams teamToRemove)
        {
            if (teamToRemove == null)
            {
                throw new ArgumentNullException(nameof(teamToRemove));
            }

            context.Team.Remove(teamToRemove);
            return Task.CompletedTask;
        }

        public Task UpdateTeam(Teams updatedTeam)
        {
            if (updatedTeam == null)
            {
                throw new ArgumentNullException(nameof(updatedTeam));
            }

            context.Team.Update(updatedTeam);
            return Task.CompletedTask;
        }

        public async Task<List<Teams>?> GetAllTeamsAsync()
        {
            return await context.Team.ToListAsync();
        }

        //Talvez crie uma variação deste apenas com o send e outro apenas com o receiver
        public async Task<Teams?> GetTeamByIdAsync(Guid id)
        {
            return await context.Team
                .Include(t => t.Calendar)
                .Include(t => t.SentInvites)
                .Include(t => t.ReceivedInvites)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamByNameAsync(string name)
        {
            return await context.Team
                .FirstOrDefaultAsync(t => t.Name == name); 
        }

        public async Task AddAsync(Teams team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            await context.Team.AddAsync(team);
        }

        public async Task<Teams?> GetTeamByIdWithPitchAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return await context.Team
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id)
        {
            if (id == Guid.Empty) {
                return null;
            }

            return await context.Team
                .Include(t => t.ReceivedInvites)
                .Include(t => t.Calendar)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<Teams?> GetByIdWithReceivedInvites(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return await context.Team
               .Include(t => t.ReceivedInvites)
               .FirstOrDefaultAsync(t => t.Id == id);
        }
        
        public async Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId)
        {
            return await context.Team
                .Where(t => t.Id == teamId)
                .Select(t => new TeamDetailsDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    FoundationDate = t.DataFoundation,
                    TotalPoints = t.CurrentPoints,
                    RankName = t.Rank.Name,
                    PitchDto = $"{t.Pitch.Name}, {t.Pitch.Address}",
                    Players = t.Members.Select(player => new PlayerDetailsDto
                    {
                        PlayerId = player.Id,
                        Name = player.Name,
                        Height = player.Height,
                        IdTeam = player.IdTeam,
                        Position = player.Position,
                        IsAdmin = player.IsAdmin
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<MemberShipRequestDto>?> GetMembershipRequestsDtoAsync(Guid teamId)
        {

            return await context.MembershipRequests
                        .Where(mr => mr.IdTeam == teamId && mr.IsPlayerSender == true)
                        .Select(mr => new MemberShipRequestDto
                        {
                            RequestId = mr.Id,
                            PlayerName = mr.Player.Name, 
                            PlayerId = mr.IdPlayer,
                            TeamName = mr.Team.Name,     
                            RequestDate = mr.InviteDate,
                            IsPlayerSender = mr.IsPlayerSender
                        })
                        .ToListAsync();
        }

        public async Task<List<MemberShipRequestDto>?> GetMembershipRequestsDtoAsyncWithFilters(Guid teamId, FilterMembershipRequestsTeam filters)
        {
            var query = context.MembershipRequests
                .Where(mr => mr.IdTeam == teamId && mr.IsPlayerSender == true);

            if (filters.MinDate.HasValue)
            {
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) >= filters.MinDate.Value);
            }

            if (filters.MaxDate.HasValue)
            {
                query = query.Where(mr => DateOnly.FromDateTime(mr.InviteDate) <= filters.MaxDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.SenderName))
            {
                var upperName = filters.SenderName.ToUpper();
                query = query.Where(mr => mr.Player.Name.ToUpper().Contains(upperName));
            }

            var list = await query
                .Select(mr => new MemberShipRequestDto
                {
                    RequestId = mr.Id,
                    PlayerName = mr.Player.Name,
                    PlayerId = mr.IdPlayer,
                    TeamName = mr.Team.Name,
                    RequestDate = mr.InviteDate,
                    IsPlayerSender = mr.IsPlayerSender
                })
                .ToListAsync();

            return list;
        }

        public async Task<Teams?> GetTeamForMembershipRequestAsync(Guid id)
        {
            return await context.Team
                .Include(t => t.MembershipRequests)
                    .ThenInclude(r => r.Player)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamForDeletionAsync(Guid id)
        {
            return await context.Team
                .Include(t => t.Members)
                .Include(t => t.Calendar)
                    .ThenInclude(c => c.Matches)
                .AsSplitQuery() // Importante para evitar explosão cartesiana
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamForUpdateAsync(Guid id)
        {
            return await context.Team
                .Include(t => t.Members)
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamByNameWithMembersAsync(string name)
        {
            return await context.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Teams?> GetTeamForMemberManagementAsync(Guid id)
        {
            return await context.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<PlayerDetailsDto>> GetTeamPlayersDtoAsyncWithFilters(Guid teamId, FilterTeamPlayers filter)
        {
            var team = await context.Team
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

            return playersQuery
                .Select(player => new PlayerDetailsDto
                {
                    PlayerId = player.Id,
                    Name = player.Name,
                    DateOfBirth = player.DateOfBirth,
                    Address = player.Address,
                    Position = player.Position,
                    Height = player.Height,
                    IdTeam = player.IdTeam,
                    IsAdmin = player.IsAdmin
                })
                .ToList();
        }

        public async Task<List<TeamLeaderboardDto>> GetTopTeamsAsync(int top)
        {
            return await context.Team
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

        public async Task<Teams?> GetTeamWitchMemberRankAndPitchAsync(Guid id)
        {
            return await context.Team
                .Include(t => t.Members)
                .Include (t => t.Rank)
                .Include (t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<InfoTeamsDto>> GetListTeamsPlayer()
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = (from t in context.Team
                         join pitch in context.Pitch on t.IdPitch equals pitch.Id
                         join rank in context.Rank on t.IdRank equals rank.Id

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

        public async Task<List<InfoTeamsDto>> GetListTeamsPlayersWithFilters(FilterListTeamDto filters)
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = context.Team
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

            if(!string.IsNullOrEmpty(filters.City))
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
                .Select (t => new
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

        public async Task<List<InfoTeamsDto>> GetListTeamsForTeams(Guid idTeam)
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = (from t in context.Team
                         join pitch in context.Pitch on t.IdPitch equals pitch.Id
                         join rank in context.Rank on t.IdRank equals rank.Id

                         where t.Members.Count > 11
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

        public async Task<List<InfoTeamsDto>> GetListTeamsByTeamsWithFilters(Guid idTeam, FilterListTeamDto filters)
        {
            var nowDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = context.Team
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

        public async Task<List<PlayerWithoutTeamInfoDto>> GetListPlayersWithoutTeam()
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow); 

            var query = await context.Player.Where(p => !p.IsAdmin 
                                                    && p.IdTeam == null)
                .Select(p => new PlayerWithoutTeamInfoDto
                {
                    PlayerId = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Age = EF.Functions.DateDiffDay(p.DateOfBirth, dateNow),
                    Height = p.Height,
                    Position = p.Position
                })
                .ToListAsync();

            return query;
        }

        public async Task<List<PlayerWithoutTeamInfoDto>> GetListPlayersWithoutTeamtWithFilters(FilterPlayersWithoutTeamDto filters)
        {
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);

            var query = context.Player.Where(p => !p.IsAdmin && p.IdTeam == null);
              
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
    }
}
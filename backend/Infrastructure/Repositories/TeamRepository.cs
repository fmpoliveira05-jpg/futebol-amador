using Application.DTOs.Rank;
using Application.DTOs.Filters;
using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
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
        private readonly AmateurFootballContext DbContext;

        public TeamRepository(AmateurFootballContext DbContext)
        {
            this.DbContext = DbContext;
        }

        public void DeleteTeam(Teams teamToRemove)
        {
            DbContext.Team.Remove(teamToRemove);
        }

        public void UpdateTeam(Teams updatedTeam)
        {
            DbContext.Team.Update(updatedTeam);
        }

        public async Task<List<Teams>?> GetAllTeamsAsync()
        {
            return await DbContext.Team.ToListAsync();
        }

        //Talvez crie uma variação deste apenas com o send e outro apenas com o receiver
        public async Task<Teams?> GetTeamByIdAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Calendar)
                .Include(t => t.SentInvites)
                .Include(t => t.ReceivedInvites)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        //verificar se o nome é unico, caso não seja, alterar pra retornar uma lista
        public async Task<Teams?> GetTeamByNameAsync(String name)
        {
            return await DbContext.Team
                .FirstOrDefaultAsync(t => t.Name == name); // Retorna a equipa ou null
        }

        public async Task AddAsync(Teams team)
        {
            await DbContext.Team.AddAsync(team);
        }

        public async Task<Teams?> GetTeamByIdWithPitchAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.ReceivedInvites)
                .Include(t => t.Calendar)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetByIdWithReceivedInvites(Guid id)
        {
            return await DbContext.Team
               .Include(t => t.ReceivedInvites)
               .FirstOrDefaultAsync(t => t.Id == id);
        }
        
        public async Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId)
        {
            return await DbContext.Team
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
                    Players = t.Members.Select(player => new PlayerDetailsDTO
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

            return await DbContext.MembershipRequests
                        .Where(mr => mr.IdTeam == teamId)
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

        public async Task<Teams?> GetTeamForMembershipRequestAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .Include(t => t.MembershipRequests)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamForDeletionAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .Include(t => t.Calendar)
                    .ThenInclude(c => c.Matches)
                .AsSplitQuery() // Importante para evitar explosão cartesiana
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamForUpdateAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamByNameWithMembersAsync(string name)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Teams?> GetTeamForMemberManagementAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamWitchMemberRankAndPitchAsync(Guid id)
        {
            return await DbContext.Team
                .Include(t => t.Members)
                .Include (t => t.Rank)
                .Include (t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

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

            if(!string.IsNullOrEmpty(filters.City))
            {
                var upperCase = filters.City.ToUpper();
                query = query.Where(t => t.Pitch.Address.ToUpper().Contains(upperCase));
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

            var query = (from t in DbContext.Team
                         join pitch in DbContext.Pitch on t.IdPitch equals pitch.Id
                         join rank in DbContext.Rank on t.IdRank equals rank.Id

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
                var upperCase = filters.City.ToUpper();
                query = query.Where(t => t.Pitch.Address.ToUpper().Contains(upperCase));
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
    }
}

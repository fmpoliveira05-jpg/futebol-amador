using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
using Application.DTOs.Filters;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AmateurFootballContext _context;

        public TeamRepository(AmateurFootballContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task DeleteTeam(Teams teamToRemove)
        {
            if (teamToRemove == null)
            {
                throw new ArgumentNullException(nameof(teamToRemove));
            }

            _context.Team.Remove(teamToRemove);
            return Task.CompletedTask;
        }

        public Task UpdateTeam(Teams updatedTeam)
        {
            if (updatedTeam == null)
            {
                throw new ArgumentNullException(nameof(updatedTeam));
            }

            _context.Team.Update(updatedTeam);
            return Task.CompletedTask;
        }

        public async Task<List<Teams>?> GetAllTeamsAsync()
        {
            return await _context.Team.ToListAsync();
        }

        //Talvez crie uma variação deste apenas com o send e outro apenas com o receiver
        public async Task<Teams?> GetTeamByIdAsync(Guid id)
        {
            return await _context.Team
                .Include(t => t.Calendar)
                .Include(t => t.SentInvites)
                .Include(t => t.ReceivedInvites)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamByNameAsync(string name)
        {
            return await _context.Team
                .FirstOrDefaultAsync(t => t.Name == name); // Retorna a equipa ou null
        }

        public async Task AddAsync(Teams team)
        {
            if (team == null)
            {
                throw new ArgumentNullException(nameof(team));
            }

            await _context.Team.AddAsync(team);
        }

        public async Task<Teams?> GetTeamByIdWithPitchAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return await _context.Team
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetByIdWithReceivedInvitesAndCalendar(Guid id)
        {
            if (id == Guid.Empty) {
                return null;
            }

            return await _context.Team
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

            return await _context.Team
               .Include(t => t.ReceivedInvites)
               .FirstOrDefaultAsync(t => t.Id == id);
        }

        public IQueryable<Teams> GetTeamsQueryable()
        {
            return _context.Team.AsQueryable();
        }
        
        public async Task<TeamDetailsDto?> GetTeamDetailsDtoAsync(Guid teamId)
        {
            return await _context.Team
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

            return await _context.MembershipRequests
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
            var query = _context.MembershipRequests
                .Where(mr => mr.IdTeam == teamId && mr.IsPlayerSender == true);

            if (filters.IsPlayerSender.HasValue)
            {
                query = query.Where(mr => mr.IsPlayerSender == filters.IsPlayerSender.Value);
            }

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
            return await _context.Team
                .Include(t => t.MembershipRequests)
                    .ThenInclude(r => r.Player)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamForDeletionAsync(Guid id)
        {
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.Calendar)
                    .ThenInclude(c => c.Matches)
                .AsSplitQuery() // Importante para evitar explosão cartesiana
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamForUpdateAsync(Guid id)
        {
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.Pitch)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamByNameWithMembersAsync(string name)
        {
            return await _context.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Teams?> GetTeamForMemberManagementAsync(Guid id)
        {
            return await _context.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TeamSummaryDto>> GetAllTeamsWithFilters(TeamSearchFiltersDto filters)
        {
            var query = _context.Team.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(t => t.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.RankName))
            {
                query = query.Where(t => t.Rank.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.PitchAddress))
            {
                query = query.Where(t => t.Pitch.Address.Contains(filters.PitchAddress));
            }

            var teams = await query
                .Select(t => new TeamSummaryDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    RankName = t.Rank.Name,
                    PlayerCount = t.Members.Count,
                })
                .ToListAsync();

            return teams;
        }

        public async Task<List<PlayerDetailsDTO>> GetTeamPlayersDtoAsyncWithFilters(Guid teamId, FilterTeamPlayers filter)
        {
            var team = await _context.Team
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null)
                return new List<PlayerDetailsDTO>();
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
                .Select(player => new PlayerDetailsDTO
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
            return await _context.Team
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
    }
}
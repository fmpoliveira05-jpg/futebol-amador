using Application.DTOs.MemberShip;
using Application.DTOs.PlayerDTOs;
using Application.DTOs.Team;
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
            return await DbContext.Team
                .Include(t => t.Calendar)
                .Include(t => t.SentInvites)
                .Include(t => t.ReceivedInvites)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teams?> GetTeamByNameAsync(string name)
        {
            return await DbContext.Team
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
            return DbContext.Team.AsQueryable();
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

        public Task<List<string>> GetMemberIdsByTeamIdAsync(Guid teamId)
        {
            return DbContext.Team
                .Where(t => t.Id == teamId)
                .Include(p => p.Members)
                .Select(p => p.Id.ToString())
                .ToListAsync();
        }

        public Task<List<string>> GetAdminsIdsByTeamIdAsync(Guid teamId)
        {
            return DbContext.Team
                .Where(t => t.Id == teamId)
                .Include(p => p.Members)
                .Where(p => p.Members.Any(m => m.IsAdmin))
                .Select(p => p.Id.ToString())
                .ToListAsync();
        }
        public async Task<List<TeamSummaryDto>> GetAllTeamsWithFilters(TeamSearchFiltersDto filters)
        {
            var query = DbContext.Team.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Name))
            {
                query = query.Where(t => t.Name.Contains(filters.Name));
            }

            if (!string.IsNullOrWhiteSpace(filters.RankName))
            {
                query = query.Where(t => t.Rank.Name.Contains(filters.Name));
            }

            if (filters.MinAvgAge > 0)
            {
                query = query.Where(t => t.AverageAge > filters.MinAvgAge);
            }

            if (filters.MaxAvgAge > 0)
            {
                query = query.Where(t => t.AverageAge < filters.MaxAvgAge);
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
    }
}
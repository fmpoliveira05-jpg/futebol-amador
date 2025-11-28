using Application.DTOs.Filters;
using Application.DTOs.Membership;
using Application.DTOs.MemberShip;
using Application.DTOs.Team;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Repositories
{
    public class MembershipRequestRepository : IMembershipRequestRepository
    {
        private readonly AmateurFootballContext context;

        public MembershipRequestRepository(AmateurFootballContext context)
        {
            this.context = context;
        }

        public async Task AddMembershipRequest(MembershipRequest request)
        {
            await context.MembershipRequests.AddAsync(request);
        }

        public void RemoveMembershipRequest(MembershipRequest request)
        {
            context.MembershipRequests.Remove(request);
        }

        public async Task<MembershipRequest?> GetMembershipRequestById(Guid id)
        {
            return await context.MembershipRequests
                .Include(mr => mr.Player)
                .Include(mr => mr.Team)
                .FirstOrDefaultAsync(mr => mr.Id == id);
        }

        public async Task<MembershipRequest?> GetMembershipRequestByPlayerAndTeam(string playerId, Guid teamId)
        {
            return await context.MembershipRequests
                .Include(mr => mr.Player)
                .Include(mr => mr.Team)
                .FirstOrDefaultAsync(mr => mr.IdPlayer == playerId && mr.IdTeam == teamId);
        }

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

        public async Task<bool> ExistsRequestBetweenPlayerAndTeam(string playerId, Guid teamId)
        {
            return await context.MembershipRequests
                .AnyAsync(mr => mr.IdPlayer == playerId && mr.IdTeam == teamId);
        }

        public async Task RemoveAllMemberShipRequestsOfPlayer(string playerId)
        {
            var requests = await context.MembershipRequests
                .Where(mr => mr.IdPlayer == playerId)
                .ToListAsync();

            context.MembershipRequests.RemoveRange(requests);
        }

        public async Task RemoveAllMemberShipRequestsOfTeam(Guid idTeam)
        {
            var requests = await context.MembershipRequests
                .Where(mr => mr.IdTeam == idTeam)
                .ToListAsync();

            context.MembershipRequests.RemoveRange(requests);
        }
    }
}
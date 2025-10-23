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
    }
}

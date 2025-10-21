using Application.DTOs.Team;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        Task<Guid> CreateTeamAsync(CreateTeamDto teamDto, Guid creatorUserId);

        Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);

        Task<List<TeamSummaryDto>> SearchTeamsAsync(TeamSearchFiltersDto filters);

        Task UpdateTeamInfoAsync(Guid teamId, UpdateTeamDto dto, Guid currentUserId);

        Task DeleteTeamAsync(Guid teamId, Guid currentUserId);

        Task<List<MemberShipRequestDto>> GetMembershipRequestsAsync(Guid teamId, Guid adminUserId);

        Task AcceptMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);

        Task RejectMembershipRequestAsync(Guid teamId, Guid requestId, Guid adminUserId);

        Task RemovePlayerFromTeamAsync(Guid teamId, Guid playerIdToRemove, Guid playerRemovingId);

        //Gestão de admins
        Task PromotePlayerToAdminAsync(Guid teamId, Guid playerIdToPromoteId, Guid playerPromotingId);

        Task DemoteAdminToPlayerAsync(Guid teamId, Guid adminIdToDemote, Guid currentAdminId);

        Task<List<PlayerDto>> GetTeamPlayersAsync(Guid teamId);

        Task<List<MatchDto>> GetTeamScheduleAsync(Guid teamId);
    }
}

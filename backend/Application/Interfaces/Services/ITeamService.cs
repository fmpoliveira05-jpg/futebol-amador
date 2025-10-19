using Application.DTOs.Team;
using Application.DTOs.MemberShip;
using Application.DTOs.Player;
using Application.DTOs.Match;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        public Task<Guid> CreateTeamAsync(CreateTeamDto teamDto);

        public Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);
    }
}

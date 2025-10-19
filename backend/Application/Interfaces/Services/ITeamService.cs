using Application.DTOs.Team;

namespace Application.Interfaces.Services
{
    public interface ITeamService
    {
        public Task<Guid> CreateTeamAsync(CreateTeamDto teamDto);

        public Task<TeamDetailsDto> GetTeamByIdAsync(Guid teamId);
    }
}

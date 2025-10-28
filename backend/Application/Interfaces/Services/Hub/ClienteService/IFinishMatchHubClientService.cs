using Application.DTOs;

namespace Application.Interfaces.Services.Hub.ClienteService
{
    public interface IFinishMatchHubClientService
    {
        public Task InitializeAsync(Guid idTeam);
        public Task JoinFinishMatchAsync(ResultMatchDto result);
        public Task EditResultMatchAsync(ResultMatchDto result);
        public Task LeaveFinishMatchAsync();
    }
}

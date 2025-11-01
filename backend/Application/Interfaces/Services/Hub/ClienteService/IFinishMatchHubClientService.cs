using Application.DTOs.Match;

namespace Application.Interfaces.Services.Hub.ClienteService
{
    public interface IFinishMatchHubClientService
    {
        public Task InitializeAsync();
        public Task JoinFinishMatchAsync(ResultMatchDto result);
        public Task EditResultMatchAsync(ResultMatchDto result);
        public Task LeaveFinishMatchAsync();
    }
}

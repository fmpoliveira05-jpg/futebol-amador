using Application.DTOs;

namespace Application.Interfaces.Services.Hub
{
    public interface IFinishMatchHubClientService
    {
        public Task JoinFinishMatchAsync(ResultMatchDto result);
        public Task EditResultMatchAsync(ResultMatchDto result);
        public Task LeaveFinishMatchAsync();
    }
}

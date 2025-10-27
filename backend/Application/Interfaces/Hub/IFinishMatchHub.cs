using Application.DTOs;

namespace Application.Interfaces.Hub
{
    public interface IFinishMatchHub
    {
        public Task JoinFinishMatch(ResultMatchDto finishMatch);
        public Task EditResult(ResultMatchDto finishMatch);
        public Task LeaveFinishMatch();
    }
}

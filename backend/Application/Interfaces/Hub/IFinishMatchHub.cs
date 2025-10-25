using Application.DTOs;

namespace Application.Interfaces.Hub
{
    public interface IFinishMatchHub
    {
        public Task JoinMatch(Guid idMatch);
        public Task EditResult(Guid idMatch, FinishMatchDTO finishMatch);
        public Task LeaveMatch();
    }
}

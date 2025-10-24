namespace Application.Interfaces.Hub
{
    public interface IFinishMatchHub
    {
        public Task JoinMatch(Guid idMatch);
        public Task LeaveMatch();
    }
}

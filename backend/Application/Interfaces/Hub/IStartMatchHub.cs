namespace Application.Interfaces.Hub
{
    public interface IStartMatchHub
    {
        public Task JoinMatch(Guid idMatch);
        public Task LeaveMatch();
    }
}

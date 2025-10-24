namespace Application.Interfaces.Hub
{
    public interface IStartMatchHub
    {
        public Task JoinStartMatch(Guid idMatch);
        public Task LeaveStartMatch();
    }
}

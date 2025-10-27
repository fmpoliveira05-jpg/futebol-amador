namespace Application.Interfaces.Hub
{
    public interface IStartMatchHub
    {
        Task ReceiveStartMatch(string msg);
        public Task JoinStartMatch(Guid idMatch);
        public Task LeaveStartMatch();
    }
}

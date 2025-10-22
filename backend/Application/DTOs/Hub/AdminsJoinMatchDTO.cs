namespace Application.DTOs.Hub
{
    public class AdminsJoinMatchDTO
    {
        public string ConnectionId { get; set; }
        public Guid idMatch { get; set; }
        public Guid IdAdmin { get; set; }
        public string UserName { get; set; }
        public Guid IdTeam { get; set; }
        public string Team { get; set; }
        public bool IsReady { get; set; }
    }
}

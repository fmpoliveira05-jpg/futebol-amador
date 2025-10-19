namespace Application.DTOs.Membership
{
    public class MembershipRequestDTO
    {
        public Guid Id { get; set; }
        public Guid IdPlayer { get; set; }
        public string? PlayerName { get; set; }
        public Guid IdTeam { get; set; }
        public string? TeamName { get; set; }
        public DateTime InviteDate { get; set; }
        public bool Sender { get; set; }
        public string? Message { get; set; }
    }
}

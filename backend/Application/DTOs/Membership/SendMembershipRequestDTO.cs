namespace Application.DTOs.Membership
{
    public class SendMembershipRequestDTO
    {
        public Guid IdPlayer { get; set; }

        public Guid IdTeam { get; set; }

        public bool? Sender { get; set; }
    }
}

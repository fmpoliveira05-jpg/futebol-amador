namespace Application.DTOs.Match
{
    public class CreateUpdateMatchDTO
    {
        public Guid Id { get; set; }

        public DateTime GameDate { get; set; }
        public string NameOpponent { get; set; }

        public string NamePitch { get; set; }
    }
}

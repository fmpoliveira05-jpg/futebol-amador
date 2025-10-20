using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Exceptions;
/***
 * Entidade que representa uma equipa desportiva.
 */
namespace Domain.Entities
{
    public class Teams
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public byte[]? Icon { get; set; }

        public Pitch Pitch { get; set; }

        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        public DateTime DataFoundation { get; set; }

        public const int MaxPlayers = 32; //Validar se é mesmo 32

        public const int MaxAdmins = 4;

        public ICollection<Player> Members { get; set; } = new List<Player>();

        [Range(18, 70, ErrorMessage = "A idade média deve estar entre os 18 e 70 anos")]
        public float AverageAge { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        public Rank? Rank { get; set; }

        [ForeignKey("Rank")]
        public Guid? IdRank { get; set; } //FK

        public ICollection<MembershipRequests> MembershipRequests { get; set; } = new List<MembershipRequests>();

        [InverseProperty("Sender")]
        public ICollection<MatchInvite> SentInvites { get; set; } = new List<MatchInvite>();

        [InverseProperty("Receiver")]
        public ICollection<MatchInvite> ReceivedInvites { get; set; } = new List<MatchInvite>();

        public Calendar Calendar { get; set; }

        [ForeignKey("Calendar")]
        public Guid IdCalendar { get; set; } //FK

        //EF
        protected Teams() { }

        public Teams(string name, string? description, byte[]? icon, Pitch pitch)
        {
            Name = name;
            Description = description;
            Icon = icon;
            Pitch = pitch;
            IdPitch = pitch.Id;
            DataFoundation = DateTime.Now;
            AverageAge = 18;
            CurrentPoints = 0;
            Calendar = new Calendar();
            IdCalendar = Calendar.Id;
        }

        public override string ToString()
        {
            return $"Team: {Name}, Description: {Description}, Founded: {DataFoundation.ToShortDateString()}, Average Age: {AverageAge}, Current Points: {CurrentPoints}";
        }
    }
}
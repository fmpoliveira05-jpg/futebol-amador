using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

/***
 * Entidade que representa uma equipa desportiva.
 */
namespace Domain.Entities
{
    public class Teams
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(ModelConstants.TeamConst.MaxNameLength, MinimumLength = ModelConstants.TeamConst.MinNameLength)]
        public string Name { get; set; }

        [StringLength(ModelConstants.TeamConst.MaxDescriptionLength)]
        [MaxLength()]
        public string? Description { get; set; }

        public byte[]? Icon { get; set; }

        public Pitch Pitch { get; set; }

        [Required]
        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        [Required]
        public DateTime DataFoundation { get; set; }

        public ICollection<Player> Members { get; set; } = new List<Player>();

        [Required]
        [Range(ModelConstants.TeamConst.MinNumberPoints, ModelConstants.TeamConst.MaxNumberPoints, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        public Rank Rank { get; set; }

        [Required]
        [ForeignKey("Rank")]
        public Guid IdRank { get; set; } //FK

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

        public Teams(string name, string? description, byte[]? icon, Pitch pitch, Rank DefaultRank)
        {
            this.Name = name;
            this.Description = description;
            this.Icon = icon;
            this.Pitch = pitch;
            this.IdPitch = pitch.Id;
            this.DataFoundation = DateTime.Now;
            this.Rank = DefaultRank;
            this.CurrentPoints = 0;
            this.Calendar = new Calendar();
            this.IdCalendar = Calendar.Id;
        }
        //
        public override string ToString()
        {
            return $"Team: {Name}, Description: {Description}, Founded: {DataFoundation.ToShortDateString()}, Current Points: {CurrentPoints}";
        }
    }
}
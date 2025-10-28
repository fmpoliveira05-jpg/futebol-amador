using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    internal class Teams
    {
<<<<<<< Updated upstream
=======
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(ModelConstants.TeamConst.MaxNameLength), Required(ErrorMessage = "O nome do time é obrigatorio.")]
        public string Name { get; set; }

        [MaxLength(ModelConstants.TeamConst.MaxDescriptionLength)]
        public string? Description { get; set; }

        public byte[]? Icon { get; set; }

        public Pitch Pitch { get; set; }

        [ForeignKey("Pitch")]
        public Guid IdPitch { get; set; } //FK

        public DateTime DataFoundation { get; set; }

        public ICollection<Player> Members { get; set; } = new List<Player>();

        [Range(0, int.MaxValue, ErrorMessage = "Um equipa tem 0 ou mais pontos")]
        public int CurrentPoints { get; set; }

        public Rank Rank { get; set; }

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
            Name = name;
            Description = description;
            Icon = icon;
            Pitch = pitch;
            IdPitch = pitch.Id;
            DataFoundation = DateTime.Now;
            this.Rank = DefaultRank;
            CurrentPoints = 0;
            Calendar = new Calendar();
            IdCalendar = Calendar.Id;
        }
        //
        public override string ToString()
        {
            return $"Team: {Name}, Description: {Description}, Founded: {DataFoundation.ToShortDateString()}, Current Points: {CurrentPoints}";
        }
>>>>>>> Stashed changes
    }
}

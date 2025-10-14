using System.ComponentModel.DataAnnotations;

namespace Domain.Entities {
    public class Pitch
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }
    }
}
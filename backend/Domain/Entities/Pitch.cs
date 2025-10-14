using System;

namespace Domain.Entities {
    internal class Pitch
    {
        [Key]
        [MinLength = 32, MaxLength = 36]
        public string Id { get; set; }

        [MaxLength = 50]
        public string Name { get; set; }

        [MaxLength = 250]
        public string Address { get; set; }
    }
}
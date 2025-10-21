using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PlayerDTOs
{
    public class UpdatePlayerDTO
    {
        public string Name { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public Position Position { get; set; }

        public int Height { get; set; }
    }
}

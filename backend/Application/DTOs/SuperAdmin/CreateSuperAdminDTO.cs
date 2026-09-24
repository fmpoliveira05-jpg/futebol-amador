using Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.SuperAdmin
{
    public class CreateSuperAdminDTO
    {
        public string Name { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = null!;

        [Required]
        [MinLength(ModelConstants.UserConst.MinEmailLength), MaxLength(ModelConstants.UserConst.MaxEmailLength)]
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}

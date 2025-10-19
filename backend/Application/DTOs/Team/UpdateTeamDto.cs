using Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Team
{
    internal class UpdateTeamDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [MaxLength(ModelConstants.Pitch.MaxNameLength)]
        public string? PitchName{ get; set; }

        [MaxLength(ModelConstants.General.MaxAddressLength)]
        public string? PitchLocation { get; set; }
    }
}

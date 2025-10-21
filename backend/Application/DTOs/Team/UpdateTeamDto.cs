using Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Team
{
    public class UpdateTeamDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public byte[]? icon { get; set; }

        [MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string? PitchName{ get; set; }

        [MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string? PitchLocation { get; set; }
    }
}

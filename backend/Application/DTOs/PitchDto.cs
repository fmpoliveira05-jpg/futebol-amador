using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Constants;
namespace Application.DTOs
{
    public class PitchDto
    {
        [MaxLength(ModelConstants.Pitch.MaxNameLength), Required(ErrorMessage = "É Obrigatorio o nome do campo.")]
        public string Name { get; set; }

        [MaxLength(ModelConstants.General.MaxAddressLength), Required(ErrorMessage = "É Obrigatorio o Endereço do campo.")]
        public string Address { get; set; }

        public PitchDto()
        {
        }

        public PitchDto(string name, string address)
        {
            Name = name;
            Address = address;
        }
    }
}

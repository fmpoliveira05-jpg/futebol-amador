using System.ComponentModel.DataAnnotations;
using Domain.Constants;
namespace Application.DTOs
{
    public class PitchDto
    {
        [MaxLength(ModelConstants.PitchConst.MaxNameLength), Required(ErrorMessage = "É Obrigatorio o nome do campo.")]
        public string Name { get; set; }

        [MaxLength(ModelConstants.GeneralConst.MaxAddressLength), Required(ErrorMessage = "É Obrigatorio o Endereço do campo.")]
        public string Address { get; set; }

        public PitchDto()
        {
        }
    }
}

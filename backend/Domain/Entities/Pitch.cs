using System.ComponentModel.DataAnnotations;
using Domain.Constants;
/***
 * Entidade que representa um campo de jogo no sistema.
 */
namespace Domain.Entities {
    public class Pitch
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(ModelConstants.PitchConst.MaxNameLength, MinimumLength = ModelConstants.PitchConst.MinNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(ModelConstants.GeneralConst.MaxAddressLength, MinimumLength = ModelConstants.GeneralConst.MinAddressLength)]
        public string Address { get; set; }

        public Pitch() { }

        public Pitch(string name, string address)
        {
            Name = name;
            Address = address;
        }

        public override string ToString()
        {
            return $"Pitch: {Name}, Address: {Address}";
        }
    }
}
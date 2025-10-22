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

        [MaxLength(ModelConstants.PitchConst.MaxNameLength)]
        public string Name { get; set; }

        [MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; }

        protected Pitch() { }

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
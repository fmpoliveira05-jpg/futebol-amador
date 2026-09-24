using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa um campo de jogo (local, estádio ou recinto) disponível no sistema.
    /// 
    /// Utilizada para armazenar as coordenadas geográficas e informações de contacto do local de jogo.
    /// </summary>
    public class Pitch
    {
        /// <summary>
        /// O identificador único (GUID) do Campo.
        /// Serve como a chave primária da entidade.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// O nome pelo qual o campo de jogo é conhecido (ex: "Estádio Municipal de Braga").
        /// </summary>
        /// <value>A string deve cumprir os limites de comprimento definidos em [ModelConstants.PitchConst].</value>
        [Required]
        [StringLength(ModelConstants.PitchConst.MaxNameLength, MinimumLength = ModelConstants.PitchConst.MinNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// A morada física completa ou o endereço da localização do campo.
        /// </summary>
        /// <value>A string deve cumprir os limites de comprimento definidos em [ModelConstants.GeneralConst].</value>
        [Required]
        [StringLength(ModelConstants.GeneralConst.MaxAddressLength, MinimumLength = ModelConstants.GeneralConst.MinAddressLength)]
        public string Address { get; set; }

        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public Pitch() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo campo de jogo.
        /// </summary>
        /// <param name="name">O nome do campo.</param>
        /// <param name="address">A morada ou localização.</param>

        public Pitch(string name, string address)
        {
            Name = name;
            Address = address;
        }
    }
}
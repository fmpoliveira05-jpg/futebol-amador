using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// Classe abstrata base que representa um utilizador genérico no sistema.
    /// 
    /// Esta classe define as propriedades de identidade e contacto comuns a todos os tipos de utilizadores (Jogadores, Administradores, etc.).
    /// É a base para a herança de outras entidades de utilizador (ex: Player, SuperAdmin).
    /// </summary>
    public abstract class User
    {
        /// <summary>
        /// O identificador único (UID) do utilizador.
        /// Este campo é a chave primária e geralmente corresponde ao ID fornecido pelo Firebase Auth.
        /// </summary>
        [Key]
        [MaxLength(ModelConstants.UserConst.MaxIdLength)]
        public string Id { get; set; }

        /// <summary>
        /// O nome completo do utilizador.
        /// </summary>
        [Required]
        [StringLength(ModelConstants.UserConst.MaxNameLength, MinimumLength = ModelConstants.UserConst.MinNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// A data de nascimento do utilizador.
        /// </summary>
        [Required]
        public DateOnly DateOfBirth { get; set; }

        /// <summary>
        /// A morada ou localização primária do utilizador.
        /// </summary>
        [Required]
        [StringLength(ModelConstants.GeneralConst.MaxAddressLength, MinimumLength = ModelConstants.GeneralConst.MinAddressLength)]
        public string Address { get; set; }

        /// <summary>
        /// O endereço de correio eletrónico do utilizador.
        /// </summary>
        [Required]
        [StringLength(ModelConstants.UserConst.MaxEmailLength, MinimumLength = ModelConstants.UserConst.MinEmailLength)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// O número de telefone do utilizador, incluindo o código do país.
        /// </summary>
        [Required]
        [StringLength(ModelConstants.UserConst.SizePhoneNumber, ErrorMessage = "Phone number must have 13 digits")]
        public string Phone { get; set; }

        /// <summary>
        /// O carimbo de data e hora da criação do perfil.
        /// </summary>
        [Required]
        public DateTime CreationDate { get; set; }

        public string? DeviceToken { get; set; }

        /// <summary>
        /// Construtor protegido exigido pelo Entity Framework (EF) para inicialização.
        /// Utilizado apenas pelas classes que herdam de [User].
        /// </summary>
        protected User() { }

        /// <summary>
        /// Construtor principal utilizado para inicializar um novo perfil de utilizador.
        /// </summary>
        /// <param name="Id">O ID único do utilizador (geralmente o UID do Firebase).</param>
        /// <param name="name">O nome completo.</param>
        /// <param name="dateOfBirth">A data de nascimento.</param>
        /// <param name="address">A morada.</param>
        /// <param name="email">O email.</param>
        /// <param name="phone">O número de telefone.</param>
        public User(string Id, string name, DateOnly dateOfBirth, string address, string email, string phone)
        {
            this.Id = Id;
            this.Name = name;
            this.DateOfBirth = dateOfBirth;
            this.Address = address;
            this.Email = email;
            this.Phone = phone;
            this.CreationDate = DateTime.Now;
            this.DeviceToken = null;
        }
    }
}

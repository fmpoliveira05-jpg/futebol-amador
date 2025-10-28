using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Domain.Entities
{
    public abstract class Users
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(ModelConstants.UserConst.MaxNameLength, MinimumLength = ModelConstants.UserConst.MinNameLength)]
        public string Name { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [StringLength(ModelConstants.GeneralConst.MaxAddressLength, MinimumLength = ModelConstants.GeneralConst.MinAddressLength)]
        public string Address { get; set; }

        [Required]
        [StringLength(ModelConstants.UserConst.MaxEmailLength, MinimumLength = ModelConstants.UserConst.MinEmailLength)]
        [EmailAddress(ErrorMessage = "Invalid email format")] 
        public string Email { get; set; }

        [Required]
        [StringLength(ModelConstants.UserConst.MaxPasswordLength, MinimumLength = ModelConstants.UserConst.MinPasswordLength)]
        public string Password { get; set; }

        [Required]
        [StringLength(ModelConstants.UserConst.SizePhoneNumber, ErrorMessage = "Phone number must have 9 digits")]
        public string Phone { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        // Construtor protegido para uso em classes derivadas
        protected Users() { }

        // Construtor público para inicializar todas as propriedades obrigatórias
        public Users(string name, DateOnly dateOfBirth, string address, string email, string password, string phone)
        {
            this.Name = name;
            this.DateOfBirth = dateOfBirth;
            this.Address = address;
            this.Email = email;
            this.Password = password;
            this.Phone = phone;
            this.CreationDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, DateOfBirth: {DateOfBirth}, Address: {Address}, Email: {Email}, PhoneNumber: {Phone}, CreationDate: {CreationDate}";
        }
    }
}

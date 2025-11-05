using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public abstract class User
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }

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
        [StringLength(ModelConstants.UserConst.SizePhoneNumber, ErrorMessage = "Phone number must have 9 digits")]
        public string Phone { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        // Construtor protegido para uso em classes derivadas
        protected User() { }

        // Construtor público para inicializar todas as propriedades obrigatórias
        public User(string Id, string name, DateOnly dateOfBirth, string address, string email, string phone)
        {
            this.Id = Id;
            this.Name = name;
            this.DateOfBirth = dateOfBirth;
            this.Address = address;
            this.Email = email;
            this.Phone = phone;
            this.CreationDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, DateOfBirth: {DateOfBirth}, Address: {Address}, Email: {Email}, PhoneNumber: {Phone}, CreationDate: {CreationDate}";
        }
    }
}

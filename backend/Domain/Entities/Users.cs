using System.ComponentModel.DataAnnotations;
using Domain.Constants;

namespace Domain.Entities
{
    public abstract class Users
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }

        [MinLength(ModelConstants.UserConst.MinNameLength), MaxLength(ModelConstants.UserConst.MaxNameLength)]
        public string Name { get; set; }

        public DateOnly DateOfBirth { get; set; }

        [MaxLength(ModelConstants.GeneralConst.MaxAddressLength)]
        public string Address { get; set; }

        [MaxLength(256)]
        [EmailAddress(ErrorMessage = "Invalid email format")] 
        public string Email { get; set; }

        [StringLength(9, ErrorMessage = "Phone number must have 9 digits")]
        public string Phone { get; set; }

        public DateTime CreationDate { get; set; }

        // Construtor protegido para uso em classes derivadas
        protected Users() { }

        // Construtor público para inicializar todas as propriedades obrigatórias
        public Users(string Id,string name, DateOnly dateOfBirth, string address, string email, string phone)
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

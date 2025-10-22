/***
 * Entidade que representa um super administrador no sistema.
 */
namespace Domain.Entities
{
    public class SuperAdmin : Users
    {
        protected SuperAdmin() { }

        public SuperAdmin(string name, DateOnly dateOfBirth, string address, string email, 
            string password, int phoneNumber)
            : base(name, dateOfBirth, address, email, password, phoneNumber)
        {

        }

        public override string ToString()
        {
            return $"SuperAdmin: {Name}, Email: {Email}, Phone: {PhoneNumber}";
        }
    }
}

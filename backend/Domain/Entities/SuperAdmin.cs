/***
 * Entidade que representa um super administrador no sistema.
 */
namespace Domain.Entities
{
    public class SuperAdmin : Users
    {
        protected SuperAdmin() { }

        public SuperAdmin(string userId,string name, DateOnly dateOfBirth, string address, string email, 
            string password, string phoneNumber)
            : base(userId,name, dateOfBirth, address, email, phoneNumber)
        {

        }

        public override string ToString()
        {
            return $"SuperAdmin: {Name}, Email: {Email}, Phone: {Phone}";
        }
    }
}

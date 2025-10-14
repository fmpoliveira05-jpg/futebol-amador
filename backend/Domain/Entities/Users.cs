using System.ComponentModel.DataAnnotations;

/***
 * Entidade que representa um utilizador no sistema.
 * 
 * !!!Falta criar validação para a dateOfBirthday, para só receber datas de 18 até 70
 * !!!Falta verificação para a password.
 */
namespace Domain.Entities
{
    public abstract class Users
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MinLength(3), MaxLength(50)]
        public string Name { get; set; }

        public DateOnly DateOfBirth { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        [MaxLength(50)]
        [EmailAddress(ErrorMessage = "O campo Email não está em um formato válido.")] // Adicione este
        public string Email { get; set; }

        [MinLength(8), MaxLength(16)]
        public string Password { get; set; }

        [Range(100000000, 999999999, ErrorMessage = "O número de telefone deve ter 9 dígitos.")]
        public int PhoneNumber { get; set; }

        public DateTime CreationDate { get; set; }

        // Construtor protegido para uso em classes derivadas
        protected Users() { }

        // Construtor público para inicializar todas as propriedades obrigatórias
        public Users(string name, DateOnly dateOfBirth, string address, string email, string password, int phoneNumber)
        {
            this.Name = name;
            this.DateOfBirth = dateOfBirth;
            this.Address = address;
            this.Email = email;
            this.Password = password;
            this.PhoneNumber = phoneNumber;
            this.CreationDate = DateTime.Now;
        }

        /***
         * Metodo que calcula a idade de um jogador, com base no dia atual - a sua data de nascimento
         * 
         * Retorna a idade do jogador
         */
        private int CalculateAge()
        {
            return 0;
        }

        /*
         * Metodo que calcula a idade de um jogador, com base no dia atual - a sua data de nascimento
         *
         * Retorna a idade do jogador
         */
        public int agePalyer()
        {
            return 0;
        }
        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, DateOfBirth: {DateOfBirth}, Address: {Address}, Email: {Email}, PhoneNumber: {PhoneNumber}, CreationDate: {CreationDate}";
        }
    }
}

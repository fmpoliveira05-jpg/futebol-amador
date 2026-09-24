namespace Domain.Entities
{
    /// <summary>
    /// Entidade que representa um Super Administrador no sistema.
    /// 
    /// Esta entidade herda todas as propriedades e funcionalidades da classe base [User]
    /// e é utilizada principalmente para distinguir utilizadores com os mais altos níveis de privilégio
    /// (Administração de Sistema/Plataforma).
    /// </summary>
    public class SuperAdmin : User
    {
        /// <summary>
        /// Construtor padrão exigido pelo Entity Framework (EF).
        /// </summary>
        public SuperAdmin() { }

        /// <summary>
        /// Construtor utilizado para inicializar um novo perfil de Super Administrador.
        /// </summary>
        /// <param name="userId">O ID único do utilizador (herdado de User/Firebase UID).</param>
        /// <param name="name">O nome do administrador.</param>
        /// <param name="dateOfBirth">A data de nascimento.</param>
        /// <param name="address">A morada/localidade.</param>
        /// <param name="email">O email.</param>
        /// <param name="phoneNumber">O número de telefone.</param>
        public SuperAdmin(string userId, string name, DateOnly dateOfBirth, string address, string email,
            string phoneNumber)
            : base(userId, name, dateOfBirth, address, email, phoneNumber)
        {

        }
    }
}
